//WaterZone.cs by Azuline Studios© All Rights Reserved
//Attach to trigger to create a swimmable zone. 
//This script manages swimming/diving behaviors and water related effects. 
using UnityEngine;
using System.Collections;

public class WaterZone : MonoBehaviour {
	private FPSRigidBodyWalker FPSWalkerComponent;
	private Footsteps FootstepsComponent;
	private PlayerWeapons PlayerWeaponsComponent;
	private GameObject playerObj;
	private GameObject weaponObj;
	private bool swimTimeState = false;
	private AudioSource audioSource;
	
	[Tooltip("Sound effect to play underwater.")]
	public AudioClip underwaterSound;
	[Tooltip("Above-water ambient audio sources to pause when submerged.")]
	public AudioSource[] aboveWaterAudioSources;
	
//	[Tooltip("If true, the waterPlane object will be flipped upside down when player is submerged so player can see the surface (depends on water shader if this works).")]
//	public bool flipWaterPlane = true;
	[Tooltip("The mesh to use for the water surface plane (if flip water plane is false, this will just be used for underwater surface plane).")]
	public Transform waterPlane;
	private float underwaterYpos;
	[Tooltip("The top water surface plane (is deactivated when camera is underwater).")]
	public Transform waterPlaneTop;
	private Vector3 waterPlaneRot;
	
	//water particles
	[Tooltip("Particles emitted around player treading water.")]
	public ParticleEmitter rippleEffect;
	[Tooltip("Particles emitted underwater for ambient bubbles/particles.")]
	public ParticleEmitter particlesEffect;
	private float particlesYPos;//to limit y position of underwater particle effect
	private float particleForwardPos;//to limit distance forward of camera for underwater particle effect
	[Tooltip("Splash particles effect to play when object enter water.")]
	public Transform waterSplash;
	private Vector3 splashPos;
	private float lastSplashTime;
	[Tooltip("Particles to emit when player is swimming on water surface.")]
	public Transform splashTrail;
	private Vector3 trailPos;
	private float splashTrailTime;
	
	//underwater lighting
	[Tooltip("True if sunlight color should be changed when underwater.")]
	public bool changeSunlightColor;
	[Tooltip("Sun/directional light that  should be changed to underwaterSunightColor when player is submerged .")]
	public Light SunlightObj;
	[Tooltip("Color of sunlight when underwater.")]
	public Color underwaterSunightColor = new Color(0.15f, 0.32f, 0.4f, 1.0f);
	private Color origSunightColor = new Color(0.15f, 0.32f, 0.4f, 1.0f);
	[Tooltip("True if scene ambient lighting is changed when underwater.")]
	public bool underwaterLightEnabled;
	[Tooltip("Color of underwater scene ambient lighting.")]
	public Color underwaterLightColor = new Color(0.15f, 0.32f, 0.4f, 1.0f);
	
	[Tooltip("True if spotlight with underwter caustics (shimmery lighting) should be active when underwater.")]
	public bool useCausticsLight;
	[Tooltip("Spotlight with caustics cookie to activate when underwater.")]
	public Light causticsLight;
	private int index;
	[Tooltip("Frames per second of underwater caustics animation.")]
	public int fps = 30;
	[Tooltip("Frames of underwater caustic animation.")]
	public Texture2D[] frames;
	
	private bool effectsState;
	
	//vars to apply underwater fog settings if submerged 
	[Tooltip("True if underwater fog settings will be applied when submerged.")]
	public bool underwaterFogEnabled;
	[Tooltip("Fog mode when underwater.")]
	public FogMode underwaterFogMode = FogMode.Linear;//changing fog mode from linear to exponential at runtime might cause small hiccup when first diving
	[Tooltip("Fog color when underwater.")]
	public Color underwaterFogColor = new Color(0.15f, 0.32f, 0.4f, 1.0f);
	[Tooltip("Underwater exponential fog density (if exponential mode is selected).")]
	public float underwaterFogDensity = 0.15f;
	[Tooltip("Underwater linear fog start distance.")]
	public float underwaterLinearFogStart = 0.0f;
	[Tooltip("Underwater linerar fog end distance.")]
	public float underwaterLinearFogEnd = 15.0f;
	
	//vars to set effects for above water
	private bool fogEnabled;
	private FogMode origFogMode = FogMode.Linear;
	private Color origFogColor = new Color(0.15f, 0.32f, 0.4f, 1.0f);
	private Color origLightColor = new Color(0.15f, 0.32f, 0.4f, 1.0f);
	private float origFogDensity = 0.15f;
	private float origLinearFogStart = 15.0f;
	private float origLinearFogEnd = 30.0f;
	//cache transform for efficiency
	private Transform myTransform;
	private Transform mainCamTransform;

	void Start () {
		myTransform = transform;
		mainCamTransform = Camera.main.transform;
		//assign this item's playerObj and weaponObj value
		playerObj = mainCamTransform.GetComponent<CameraControl>().playerObj;
		weaponObj = mainCamTransform.GetComponent<CameraControl>().weaponObj;
		//assign external script references
		FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		FootstepsComponent = playerObj.GetComponent<Footsteps>();
		PlayerWeaponsComponent = weaponObj.GetComponent<PlayerWeapons>();
		
		Physics.IgnoreCollision(myTransform.GetComponent<Collider>(), FPSWalkerComponent.leanCol, true);
		//set up underwater sound effects
		audioSource = gameObject.AddComponent<AudioSource>();
	    audioSource.clip = underwaterSound;
		audioSource.loop = true;
		audioSource.volume = 0.8f;
		//store original fog values to apply to render settings when player surfaces
		fogEnabled = RenderSettings.fog;
		origFogColor = RenderSettings.fogColor;
		origFogDensity = RenderSettings.fogDensity;
		origFogMode = RenderSettings.fogMode;
		origLinearFogStart = RenderSettings.fogStartDistance;
		origLinearFogEnd = RenderSettings.fogEndDistance;
		origLightColor = RenderSettings.ambientLight;
		
		//automatically assign sunlight
		if(!SunlightObj){
			Light[] lightList = FindObjectsOfType(typeof(Light)) as Light[];
			for(int i = 0; i < lightList.Length; i++)	{
				if(lightList[i].type == LightType.Directional && lightList[i].gameObject.activeInHierarchy){
					SunlightObj = lightList[i];
					break;
				}
			}
		}
		//store original sun/directional light color
		if(SunlightObj){origSunightColor = SunlightObj.color;}
		
		rippleEffect.emit = false;
		if(waterPlane){
			waterPlane.gameObject.SetActive(false);
			underwaterYpos = waterPlane.transform.position.y;
		}

		if(useCausticsLight && causticsLight){
			causticsLight.gameObject.SetActive(false);
		}
		if(waterPlaneTop){waterPlaneTop.gameObject.SetActive(true);}
		
		swimTimeState = false;
		rippleEffect.emit = false;
		particlesEffect.emit = false;
		FPSWalkerComponent.inWater = false;
		FPSWalkerComponent.swimming = false;
		FPSWalkerComponent.belowWater = false;
		FPSWalkerComponent.canWaterJump = true;
		FPSWalkerComponent.holdingBreath = false;
		
		if(frames.Length > 0){
			InvokeRepeating("NextFrame", 0f, 1.0f / fps);
		}
	    
	}
	
	void Update(){
	
		if(!playerObj.activeInHierarchy){
			swimTimeState = false;
			rippleEffect.emit = false;
			particlesEffect.emit = false;
			FPSWalkerComponent.inWater = false;
			FPSWalkerComponent.swimming = false;
			FPSWalkerComponent.belowWater = false;
			FPSWalkerComponent.canWaterJump = true;
			FPSWalkerComponent.holdingBreath = false;
			StopUnderwaterEffects();
		}
		
		if(!myTransform.GetComponent<Collider>().bounds.Contains(mainCamTransform.position - (mainCamTransform.up * 0.2f))){	
			StopUnderwaterEffects();
		}else{
			StartUnderwaterEffects ();
		}
		
	}
	
	void NextFrame(){
		if(useCausticsLight && causticsLight && causticsLight.isActiveAndEnabled){
			index = (index + 1) % frames.Length;
			causticsLight.cookie = frames[index];
		}
	}
	
	void OnTriggerStay(Collider col){
		EnterWater(col);
	}
	
	void OnTriggerEnter(Collider col){
		string colTag = col.gameObject.tag;
		Transform colTransform = col.gameObject.transform;

		//play splash effects for player and objects thrown into water
		if((colTag == "Player" && (FPSWalkerComponent.jumping || !FPSWalkerComponent.grounded))//play splash effects for player if they jumped into water
		|| ((colTag == "Usable"//play splash effects for objects if they hit water
		|| colTag == "Metal" 
		|| colTag == "Wood" 
		|| colTag == "Glass" 
		|| colTag == "Flesh"
		|| col.gameObject.name == "Chest") 
		&& colTransform.position.y > myTransform.GetComponent<Collider>().bounds.max.y - 0.4f)
		&& lastSplashTime + 0.6f < Time.time){
		
			if(colTag == "Player"){
				EnterWater(col);
				if(FootstepsComponent.waterLand){
					PlayAudioAtPos.PlayClipAt(FootstepsComponent.waterLand, col.gameObject.transform.position, 1.0f, 0.0f);//play splash sound
				}
			}else{
				if(FootstepsComponent.waterLand){
					PlayAudioAtPos.PlayClipAt(FootstepsComponent.waterLand, colTransform.position, 1.0f);//play splash sound
				}
			}

			if(waterSplash){
			    foreach (Transform child in waterSplash.transform){//emit all particles in the particle effect game object group stored in impactObj var
					
					if(child.name == "FastSplash"){
						splashPos = new Vector3(colTransform.position.x, myTransform.GetComponent<Collider>().bounds.max.y - 0.15f, colTransform.position.z);
						child.GetComponent<ParticleEmitter>().transform.position = splashPos;
					}else{
						splashPos = new Vector3(colTransform.position.x, myTransform.GetComponent<Collider>().bounds.max.y + 0.01f, colTransform.position.z);
						child.GetComponent<ParticleEmitter>().transform.position = splashPos;	
					}
					child.GetComponent<ParticleEmitter>().transform.rotation = Quaternion.FromToRotation(Vector3.up, waterSplash.transform.up);//rotate impact effects so they are perpendicular to surface hit
					child.GetComponent<ParticleEmitter>().Emit();//emit the particle(s)
				}
			}
			
			lastSplashTime = Time.time;
			
		}
	}
	
	void EnterWater(Collider col){

		if(col.gameObject.tag == "Player"){

			FPSWalkerComponent.inWater = true;
			//check if player is at wading depth in water (water line at chest) after wading into water
			if(col.gameObject.GetComponent<Collider>().bounds.max.y - 0.95f <= myTransform.GetComponent<Collider>().bounds.max.y){
				
				FPSWalkerComponent.swimming = true;
				
				if(!swimTimeState){
					FPSWalkerComponent.swimStartTime = Time.time;//track time that swimming started
					swimTimeState = true;
				}
				//check if player is at treading water depth (water line at shoulders/neck) after surfacing from dive
				if(col.gameObject.GetComponent<Collider>().bounds.max.y - 0.9f <= myTransform.GetComponent<Collider>().bounds.max.y){
					FPSWalkerComponent.belowWater = true;
				}else{
					FPSWalkerComponent.belowWater = false;
				}
				
			}else{
				FPSWalkerComponent.swimming = false;
			}
			
			//check if view height is under water line
			if(FPSWalkerComponent.eyePos.y <= myTransform.GetComponent<Collider>().bounds.max.y){

				if(!FPSWalkerComponent.holdingBreath){
					rippleEffect.emit = false;
					FPSWalkerComponent.diveStartTime = Time.time;
					FPSWalkerComponent.holdingBreath = true;
				}
				
			}else{

				FPSWalkerComponent.holdingBreath = false;

				//check if treading water ripples or player wake/ripple trail should be emitted
				if(FPSWalkerComponent.inputY == 0 && FPSWalkerComponent.inputX == 0){//player is treading water
					//play idle treading water ripples around player
					rippleEffect.emit = true;
					//emit the particles slightly above the water surface so they are not hidden by the visual water effect plane
					Vector3 tempRipplePos = new Vector3(playerObj.transform.position.x, myTransform.GetComponent<Collider>().bounds.max.y + 0.0005f, playerObj.transform.position.z);
					rippleEffect.transform.position = tempRipplePos;
					
				}else{//player is swimming on surface
					//stop idle treading water ripples around player
					rippleEffect.emit = false;
					//emit player wake particle group at a set interval
					if(splashTrailTime + 0.075f < Time.time){
						if(splashTrail){
						    foreach (Transform child in splashTrail.transform){//emit all particles in the particle effect game object group stored in impactObj var
								//emit the particles slightly above the water surface so they are not hidden by the visual water effect plane
								trailPos = new Vector3(playerObj.transform.position.x, myTransform.GetComponent<Collider>().bounds.max.y + 0.0005f, playerObj.transform.position.z);
								splashTrail.transform.position = trailPos;
								child.GetComponent<ParticleEmitter>().transform.position = trailPos;	
								//rotate impact effects so they are perpendicular to surface hit
								child.GetComponent<ParticleEmitter>().Emit();//emit the particle(s)
							}
						}
						splashTrailTime = Time.time;//store last emitted time to set particle emission interval
					}	
				}		
			}
		}	
	}
	
	void OnTriggerExit(Collider col){
		//player has exited water, so reset swimming related variables
		if(col.gameObject.tag == "Player"){
			swimTimeState = false;
			rippleEffect.emit = false;
			particlesEffect.emit = false;
			FPSWalkerComponent.inWater = false;
			FPSWalkerComponent.swimming = false;
			FPSWalkerComponent.belowWater = false;
			FPSWalkerComponent.canWaterJump = true;
			FPSWalkerComponent.holdingBreath = false;
		}
	}
	
	private void StartUnderwaterEffects(){
	
		if(!effectsState){
			
			//play underwater sound effect and pause above-water ambient audio sources
			audioSource.Play();
			foreach (AudioSource aSource in aboveWaterAudioSources){
				if(aSource != null){
					aSource.Pause();	
				}
			}
			//emit ambient underwater particles/bubbles
			particlesEffect.emit = true;
			particlesEffect.transform.GetComponent<ParticleRenderer>().enabled = true;
			//don't emit swimming water rings if submerged
			rippleEffect.emit = false;
			//set color of underwater muzzle flash to underwaterFogColor
			PlayerWeaponsComponent.waterMuzzleFlashColor.r = underwaterFogColor.r;
			PlayerWeaponsComponent.waterMuzzleFlashColor.g = underwaterFogColor.g;
			PlayerWeaponsComponent.waterMuzzleFlashColor.b = underwaterFogColor.b;
			
			if(underwaterFogEnabled){
				//settings for underwater fog
				RenderSettings.fog = underwaterFogEnabled;
				RenderSettings.fogColor = underwaterFogColor;
				RenderSettings.fogDensity = underwaterFogDensity;
				RenderSettings.fogMode = underwaterFogMode;
				RenderSettings.fogStartDistance = underwaterLinearFogStart;
				RenderSettings.fogEndDistance = underwaterLinearFogEnd;
			}
			if(underwaterLightEnabled){
				RenderSettings.ambientLight = underwaterLightColor;
			}
			//change original sun/directional light color to underwaterSunightColor
			if(SunlightObj && changeSunlightColor){SunlightObj.color = underwaterSunightColor;}
			
			if(waterPlane /*&& flipWaterPlane*/){//flip the water plane so we can see the surface from underwater
				//						waterPlaneRot.z = 180.0f;
				//						waterPlane.localEulerAngles = waterPlaneRot;
				waterPlane.gameObject.SetActive(true);
			}
			
			if(useCausticsLight && causticsLight){
				causticsLight.gameObject.SetActive(true);
			}
			
			if(waterPlaneTop){
				waterPlaneTop.gameObject.SetActive(false);
			}
			
			FPSWalkerComponent.FPSPlayerComponent.CameraControlComponent.viewUnderwater = true;
			//perform above actions only once at start of dive
			effectsState = true;
		}
		
		if(waterPlane && FPSWalkerComponent.holdingBreath){
			waterPlane.position = new Vector3(FPSWalkerComponent.myTransform.position.x, 
			                                  underwaterYpos, 
			                                  FPSWalkerComponent.myTransform.position.z);
		}
		
		//Make sure that water particles don't rise past the surface of the water by subtracting the 
		//particle emitter ellipsoid y amount from the top of the water zone collilder/trigger.
		//Otherwise, just emit at the same height/water depth as the camera
		if(myTransform.GetComponent<Collider>().bounds.max.y - 2.04f > mainCamTransform.position.y){
			particlesYPos = mainCamTransform.position.y;	
			particleForwardPos = 3.25f;
		}else{
			particlesYPos = myTransform.GetComponent<Collider>().bounds.max.y - 2.04f;	
			particleForwardPos = 0.0f;
		}
		//make underwater particles/bubbles follow player position
		particlesEffect.transform.position = new Vector3(mainCamTransform.position.x, particlesYPos, mainCamTransform.position.z) + (mainCamTransform.forward * particleForwardPos);
		
	}
	
	private void StopUnderwaterEffects(){
		if(effectsState){
			//pause underwater sound effect and resume playing above-water ambient audio sources
			audioSource.Pause();
			if(aboveWaterAudioSources.Length > 0){
				foreach (AudioSource aSource in aboveWaterAudioSources){
					if(aSource){
						aSource.Play();
					}	
				}
			}
			//stop emitting underwater particles/bubbles
			particlesEffect.emit = false;
			particlesEffect.transform.GetComponent<ParticleRenderer>().enabled = false;
			//apply original fog settings when above water
			RenderSettings.fog = fogEnabled;
			RenderSettings.fogColor = origFogColor;
			RenderSettings.fogDensity = origFogDensity;
			RenderSettings.fogMode = origFogMode;
			RenderSettings.fogStartDistance = origLinearFogStart;
			RenderSettings.fogEndDistance = origLinearFogEnd;
			RenderSettings.ambientLight = origLightColor;
			//change original sun/directional light color to origSunightColor
			if(SunlightObj && changeSunlightColor){SunlightObj.color = origSunightColor;}
			
			if(waterPlane /*&& flipWaterPlane*/){
				//						waterPlaneRot.z = 0.0f;
				//						waterPlane.localEulerAngles = waterPlaneRot;
				waterPlane.gameObject.SetActive(false);
			}
			
			if(useCausticsLight && causticsLight){
				causticsLight.gameObject.SetActive(false);
			}
			
			if(waterPlaneTop){
				waterPlaneTop.gameObject.SetActive(true);
			}
			
			FPSWalkerComponent.FPSPlayerComponent.CameraControlComponent.viewUnderwater = false;
			effectsState = false;
		}
	}
	
}
