//FPSPlayer.cs by Azuline Studios© All Rights Reserved
//Controls main player actions such as hitpoints and damage, HUD GUIText/Texture element instantiation and update,
//directs player button mappings to other scripts, handles item detection and pickup, and plays basic player sound effects.
using UnityEngine;
using System.Collections;

public class FPSPlayer : MonoBehaviour {
	[HideInInspector]
	public Ironsights IronsightsComponent;
	[HideInInspector]
	public InputControl InputComponent;
	[HideInInspector]
	public FPSRigidBodyWalker FPSWalkerComponent;
	[HideInInspector]
	public PlayerWeapons PlayerWeaponsComponent;
	[HideInInspector]
	public WorldRecenter WorldRecenterComponent;
	[HideInInspector]
	public WeaponBehavior WeaponBehaviorComponent;
	[HideInInspector]
	public	SmoothMouseLook MouseLookComponent;
	[HideInInspector]
	public	WeaponEffects WeaponEffectsComponent;
	[HideInInspector]
	public WeaponPivot WeaponPivotComponent;
	[HideInInspector]
	public CameraControl CameraControlComponent;
	[HideInInspector]
	public NPCRegistry NPCRegistryComponent;
	[HideInInspector]
	public GameObject NPCMgrObj;
	private AI AIComponent;
 	//other objects accessed by this script
	[HideInInspector]
	public GameObject[] children;//behaviors of these objects are deactivated when restarting the scene
	[HideInInspector]
	public GameObject weaponCameraObj;
	[HideInInspector]
	public GameObject weaponObj;
	[Tooltip("Object reference to the GUITexture object in the project library that renders pain effects on screen.")]
	public GameObject painFadeObj;
	public GameObject levelLoadFadeObj;
	[Tooltip("Object reference to the GUIText object in the project library that renders health amounts on screen.")]
	public GameObject healthGuiObj;
	[HideInInspector]
	public GameObject healthGuiObjInstance;
	[Tooltip("Object reference to the GUIText object in the project library that renders ammo amounts on screen.")]
	public GameObject ammoGuiObj;
	[Tooltip("Object reference to the GUIText object in the project library that renders hunger amounts on screen.")]
	public GameObject hungerGuiObj;
	[HideInInspector]
	public GameObject hungerGuiObjInstance;
	[Tooltip("Object reference to the GUIText object in the project library that renders thirst amounts on screen.")]
	public GameObject thirstGuiObj;
	[HideInInspector]
	public GameObject thirstGuiObjInstance;
	[Tooltip("Object reference to the GUIText object in the project library that renders help text on screen.")]
	public GameObject helpGuiObj;
	[HideInInspector]
	public GameObject helpGuiObjInstance;
	[Tooltip("Object reference to the GUITexture object in the project library that renders crosshair on screen.")]
	public GameObject CrosshairGuiObj;
	[HideInInspector]
	public GameObject CrosshairGuiObjInstance;
	[HideInInspector]
	public GUITexture CrosshairGuiTexture;
	[Tooltip("Object reference to the GUIText object in the project library that renders hitmarker on screen.")]
	public GameObject hitmarkerGuiObj;
	[HideInInspector]
	public GameObject hitmarkerGuiObjInstance;
	[HideInInspector]
	public GUITexture hitmarkerGuiTexture;

	private HealthText HealthText;
	private HealthText[] HealthText2;
	private GUIText healthGuiText;
	
	private HungerText HungerText ;
	private HungerText[] HungerText2;
	private GUIText HungerGUIText;
	
	private ThirstText ThirstText;
	private ThirstText[] ThirstText2;
	private GUIText ThirstGUIText;
		
	[HideInInspector]
	public float crosshairWidth;
	private Rect crossRect;
	[Tooltip("Size of crosshair relative to screen size.")]
	public float crosshairSize;
	private float oldWidth;
	[HideInInspector]
	public float hitTime = -10.0f;
	[HideInInspector]
	public bool hitMarkerState;
	private Transform mainCamTransform;
	[TooltipAttribute("True if the prefab parent object will be removed on scene load.")]
	public bool removePrefabRoot = true;
	
	//player hit points
	public float hitPoints = 100.0f;
	public float maximumHitPoints = 200.0f;
	[Tooltip("True if player's health should be displayed on the screen.")]
	public bool showHealth = true;
	[Tooltip("True if player's ammo should be displayed on the screen.")]
	public bool showAmmo = true;
	[Tooltip("True if negative hitpoint values should be shown.")]
	public bool showHpUnderZero = true;
	[Tooltip("True if player cannot take damage.")]
	public bool invulnerable;
	[Tooltip("True if the player regenerates their health after health regen delay elapses without player taking damage.")]
	public bool regenerateHealth = false;
	[Tooltip("The maximum amount of hitpoints that should be regenerated.")]
	public float maxRegenHealth = 100.0f;
	[Tooltip("Delay after being damaged that the player should start to regenerate health.")]
	public float healthRegenDelay = 7.0f;
	[Tooltip("Rate at which the player should regenerate health.")]
	public float healthRegenRate = 25.0f;
	private float timeLastDamaged;//time that the player was last damaged
	
	//player hunger
	[Tooltip("True if player should have a hunger attribute that increases over time.")]
	public bool usePlayerHunger;
	[HideInInspector]
	public float maxHungerPoints = 100.0f;//maximum amount that hunger will increase to before players starts to starve
	[TooltipAttribute("Seconds it takes for player to accumulate 1 hunger point.")]
	public float hungerInterval = 7.0f;
	[HideInInspector]
	public float hungerPoints = 0.0f;//total hunger points 
	private float lastHungerTime;//time that last hunger point was applied
	private float lastStarveTime;//time that last starve damage was applied
	[TooltipAttribute("Seconds to wait before starve damaging again (should be less than healthRegenDelay to prevent healing of starvation damage).")]
	public float starveInterval = 3.0f;
	[TooltipAttribute("Anount of damage to apply per starve interval.")]
	public float starveDmgAmt = -5.0f;//amount to damage player per starve interval 
	
	//player thirst
	[Tooltip("True if player should have a thirst attribute that increases over time.")]
	public bool usePlayerThirst;
	[HideInInspector]
	public float maxThirstPoints = 100.0f;//maximum amount that thirst will increase to before players starts to take thirst damage
	[TooltipAttribute("Seconds it takes for player to accumulate 1 thirst point.")]
	public float thirstInterval = 7.0f;
	[HideInInspector]
	public float thirstPoints = 0.0f;//total thirst points 
	private float lastThirstTime;//time that last thirst point was applied
	private float lastThirstDmgTime;//time that last thirst damage was applied
	[TooltipAttribute("Seconds to wait before thirst damaging again (should be less than healthRegenDelay to prevent healing of thirst damage).")]
	public float thirstDmgInterval = 3.0f;
	[Tooltip("Amount to damage player per thirst damage interval.")]
	public float thirstDmgAmt = -5.0f;

	[Tooltip("True if player can activate bullet time by pressing button (default T).")]
	public bool allowBulletTime = true;
	[Tooltip("True if help text should be displayed.")]
	public bool showHelpText = true;
	
	//Damage feedback
	private float gotHitTimer = -1.0f;
	private Color PainColor = new Color(0.221f, 0f, 0f, 0.44f);//color of pain screen flash can be selected in editor
	public Texture2D painTexture;
	private Color painFadeColor;//used to modify opacity of pain fade object
	[TooltipAttribute("Amount to kick the player's camera view when damaged.")]
	public float painScreenKickAmt = 0.016f;//magnitude of the screen kicks when player takes damage
	
	//Bullet Time and Pausing
	[TooltipAttribute("Percentage of normal time to use when in bullet time.")]
	[Range(0.0f, 1.0f)]
	public float bulletTimeSpeed = 0.35f;//decrease time to this speed when in bullet time
	[Tooltip("Movement multiplier during bullet time.")]
	public float sloMoMoveSpeed = 2.0f;
	private float pausedTime;//time.timescale value to return to after pausing
	[HideInInspector]
	public bool bulletTimeActive;
	[HideInInspector]
	public float backstabBtTime;
	[HideInInspector]
	public bool backstabBtState;
	private float initialFixedTime;
	[HideInInspector]
	public float usePressTime;
	[HideInInspector]
	public float useReleaseTime;
	private bool useState;
	[HideInInspector]
	public bool pressButtonUpState;
	[HideInInspector]
	public Collider objToPickup;
	
	//zooming
	private bool zoomBtnState = true;
	private float zoomStopTime = 0.0f;//track time that zoom stopped to delay making aim reticle visible again
	[HideInInspector]
	public bool zoomed = false;
	[HideInInspector]
	public float zoomStart = -2.0f;
	[HideInInspector]
	public bool zoomStartState = false;
	[HideInInspector]
	public float zoomEnd = 0.0f;
	[HideInInspector]
	public bool zoomEndState = false;
	private float zoomDelay = 0.4f;
	[HideInInspector]
	public bool dzAiming;
	
	//crosshair 
	[Tooltip("Enable or disable the aiming reticle.")]
	public bool crosshairEnabled = true;
	private bool crosshairVisibleState = true;
	private bool crosshairTextureState = false;
	[Tooltip("Set to true to display swap reticle when item under reticle will replace current weapon.")]
	public bool useSwapReticle = true;
	[Tooltip("The texture used for the aiming crosshair.")]
	public Texture2D aimingReticle;
	[Tooltip("The texture used for the hitmarker.")]
	public Texture2D hitmarkerReticle;
	[Tooltip("The texture used for the pick up crosshair.")]
	public Texture2D pickupReticle;
	[Tooltip("The texture used for when the weapon under reticle will replace current weapon.")]
	public Texture2D swapReticle;
	[Tooltip("The texture used for showing that weapon under reticle cannot be picked up.")]
	public Texture2D noPickupReticle;
	[Tooltip("The texture used for the pick up crosshair.")]
	private Texture2D pickupTex;

	private Color pickupReticleColor = Color.white; 
	[HideInInspector]
	public Color reticleColor = Color.white; 
	private Color hitmarkerColor = Color.white; 
	[Tooltip("Layers to include for crosshair raycast in hit detection.")]
	public LayerMask rayMask;
	[Tooltip("Distance that player can pickup and activate items.")]
	public float reachDistance = 2.1f;

	private RaycastHit hit;
	private RaycastHit hit2;
	private Vector3 camCrosshairPos;
	private Vector3 crosshairPos;
	[HideInInspector]
	public bool raycastCrosshair;
	
	//button and behavior states
	private bool pickUpBtnState = true;
	[HideInInspector]
	public bool restarting = false;//to notify other scripts that level is restarting
	
	//sound effects
	public AudioClip painLittle;
	public AudioClip painBig;
	public AudioClip painDrown;
	public AudioClip gasp;
	public AudioClip catchBreath;
	public AudioClip die;
	public AudioClip dieDrown;
	public AudioClip jumpfx;
	public AudioClip enterBulletTimeFx;
	public AudioClip exitBulletTimeFx;
	public AudioClip hitMarker;
	[Tooltip("Particle effect to play when player blocks attack.")]
	public GameObject blockParticles;
	[Tooltip("Distance from camera to emit blocking particle effect.")]
	public float blockParticlesPos;

	private AudioSource[]aSources;//access the audio sources attatched to this object as an array for playing player sound effects
	[HideInInspector]
	public AudioSource otherfx;
	[HideInInspector]
	public AudioSource hitmarkfx;
	private bool bullettimefxstate;
	[HideInInspector]
	public bool blockState;
	[HideInInspector]
	public float blockAngle;
	[HideInInspector]
	public bool canBackstab;//true if player can backstab an unalerted NPC
	private float moveCommandedTime;//last time that following NPCs were commanded to move (for command cooldown)
	
	[HideInInspector]
	public bool menuDisplayed;
	[HideInInspector]
	public float menuTime;
	[HideInInspector]
	public float pauseTime;
	[HideInInspector]
	public bool paused;
	private MainMenu MainMenuComponent;

	void Start (){	

		if(removePrefabRoot){
			GameObject prefabRoot = transform.parent.transform.gameObject;
			transform.parent.transform.DetachChildren();
			Destroy(prefabRoot);
		}

		mainCamTransform = Camera.main.transform;
		//set up external script references
		IronsightsComponent = GetComponent<Ironsights>();
		InputComponent = GetComponent<InputControl>();
		FPSWalkerComponent = GetComponent<FPSRigidBodyWalker>();
		WorldRecenterComponent = GetComponent<WorldRecenter>();
		MouseLookComponent = mainCamTransform.parent.transform.GetComponent<SmoothMouseLook>();
		CameraControlComponent = Camera.main.transform.GetComponent<CameraControl>();
		weaponObj = CameraControlComponent.weaponObj;
		WeaponEffectsComponent = weaponObj.GetComponent<WeaponEffects>();
		PlayerWeaponsComponent = weaponObj.GetComponent<PlayerWeapons>();
		
		MainMenuComponent = Camera.main.transform.parent.transform.GetComponent<MainMenu>();
//		MainMenuComponent.enabled = false;
		menuDisplayed = false;
		
		NPCMgrObj = GameObject.Find("NPC Manager");
		NPCRegistryComponent = NPCMgrObj.GetComponent<NPCRegistry>();
			
		aSources = GetComponents<AudioSource>();//Initialize audio source
		otherfx = aSources[0] as AudioSource;
		hitmarkfx = aSources[1] as AudioSource;
		otherfx.spatialBlend = 0.0f;
		hitmarkfx.spatialBlend = 0.0f;
		
		//Set time settings
		Time.timeScale = 1.0f;
		initialFixedTime =  Time.fixedDeltaTime;
		
		usePressTime = 0f;
		useReleaseTime = -8f;
		
		//Physics Layer Management Setup (obsolete, no longer used)
		//these are the layer numbers and their corresponding uses/names accessed by the FPS prefab
		//	Weapon = 8;
		//	Ragdoll = 9;
		//	WorldCollision = 10;
		//	Player = 11;
		//	Objects = 12;
		//	NPCs = 13;
		//	GUICameraLayer = 14;
		//	WorldGeometry = 15;
		//	BulletMarks = 16;
		
		//player object collisions
		Physics.IgnoreLayerCollision(11, 12);//no collisions between player object and misc objects like bullet casings
		Physics.IgnoreLayerCollision (12, 12);//no collisions between bullet shells
		
		//weapon object collisions
		Physics.IgnoreLayerCollision(8, 2);//
		Physics.IgnoreLayerCollision(8, 13);//no collisions between weapon and NPCs
		Physics.IgnoreLayerCollision(8, 12);//no collisions between weapon and Objects
		Physics.IgnoreLayerCollision(8, 11);//no collisions between weapon and Player
		Physics.IgnoreLayerCollision(8, 10);//no collisions between weapon and world collision

		//Call FadeAndLoadLevel fucntion with fadeIn argument set to true to tell the function to fade in (not fade out and (re)load level)
		GameObject llf = Instantiate(levelLoadFadeObj) as GameObject;
		llf.GetComponent<LevelLoadFade>().FadeAndLoadLevel(Color.black, 2.0f, true);
		
		//create instance of GUIText to display health amount on hud
		healthGuiObjInstance = Instantiate(healthGuiObj,Vector3.zero,transform.rotation) as GameObject;
		if(showHelpText){
			//create instance of GUIText to display help text
			helpGuiObjInstance = Instantiate(helpGuiObj,Vector3.zero,transform.rotation) as GameObject;
		}
		//create instance of GUITexture to display crosshair on hud
		CrosshairGuiObjInstance = Instantiate(CrosshairGuiObj,new Vector3(0.5f,0.5f,0.0f),transform.rotation) as GameObject;
		CrosshairGuiTexture = CrosshairGuiObjInstance.GetComponent<GUITexture>();
		CrosshairGuiTexture.texture = aimingReticle;
		hitmarkerGuiObjInstance = Instantiate(hitmarkerGuiObj,new Vector3(0.5f,0.5f,0.0f),transform.rotation) as GameObject;
		hitmarkerGuiTexture = hitmarkerGuiObjInstance.GetComponent<GUITexture>();
		hitmarkerGuiTexture.texture = hitmarkerReticle;
		hitmarkerGuiTexture.enabled = false;
		//set alpha of hand pickup crosshair
		pickupReticleColor.a = 0.5f;
		//set alpha of aiming reticule and make it 100% transparent if crosshair is disabled
		if(crosshairEnabled){
			reticleColor.a = 0.25f;
			hitmarkerGuiTexture.color = hitmarkerColor;
		}else{
			//make alpha of aiming reticle zero/transparent
			reticleColor.a = 0.0f;
			//set alpha of aiming reticle at start to prevent it from showing, but allow item pickup hand reticle 
			CrosshairGuiTexture.color = reticleColor;
			hitmarkerGuiTexture.color = reticleColor;
		}
		
		//set reference for main color element of heath GUIText
		HealthText = healthGuiObjInstance.GetComponent<HealthText>();
		//set reference for shadow background color element of health GUIText
		//this object is a child of the main health GUIText object, so access it as an array
		HealthText2 = healthGuiObjInstance.GetComponentsInChildren<HealthText>();
		
		//initialize health amounts on GUIText objects
		HealthText.healthGui = hitPoints;
		HealthText2[1].healthGui = hitPoints;	
		healthGuiText = HealthText.GetComponent<GUIText>();
		
		if(!showHealth){
			healthGuiObjInstance.gameObject.SetActive(false);
		}
		
		if(usePlayerHunger){
			//create instance of GUIText to display hunger amount on hud
			hungerGuiObjInstance = Instantiate(hungerGuiObj,Vector3.zero,transform.rotation) as GameObject;
			//set reference for main color element of hunger GUIText
			HungerText = hungerGuiObjInstance.GetComponent<HungerText>();
			//set reference for shadow background color element of hunger GUIText
			//this object is a child of the main hunger GUIText object, so access it as an array
			HungerText2 = hungerGuiObjInstance.GetComponentsInChildren<HungerText>();
			
			//initialize hunger amounts on GUIText objects
			HungerText.hungerGui = hungerPoints;
			HungerText2[1].hungerGui = hungerPoints;	
			HungerGUIText = HungerText.GetComponent<GUIText>();
		}
		
		if(usePlayerThirst){
			//create instance of GUIText to display thirst amount on hud
			thirstGuiObjInstance = Instantiate(thirstGuiObj,Vector3.zero,transform.rotation) as GameObject;
			//set reference for main color element of thirst GUIText
			ThirstText = thirstGuiObjInstance.GetComponent<ThirstText>();
			//set reference for shadow background color element of thirst GUIText
			//this object is a child of the main thirst GUIText object, so access it as an array
			ThirstText2 = thirstGuiObjInstance.GetComponentsInChildren<ThirstText>();
			
			//initialize thirst amounts on GUIText objects
			ThirstText.thirstGui = thirstPoints;
			ThirstText2[1].thirstGui = thirstPoints;
			ThirstGUIText = ThirstText.GetComponent<GUIText>();	
		}
		
	}
	
	void LateUpdate () {
	
		if((MouseLookComponent.dzAiming || raycastCrosshair) 
		//&& WeaponBehaviorComponent.recoveryTime + WeaponBehaviorComponent.recoveryTimeAmt + 0.1f < Time.time 
		//&& !FPSWalkerComponent.sprintActive
		/*&& !(FPSWalkerComponent.moving && FPSWalkerComponent.prone)*/){
			if(!WeaponBehaviorComponent.unarmed
			&& Physics.Raycast(mainCamTransform.position, WeaponBehaviorComponent.weaponLookDirection, out hit, 100.0f, rayMask)){
				camCrosshairPos = Camera.main.WorldToScreenPoint(hit.point);
				crosshairPos = new Vector3(camCrosshairPos.x / Screen.width, camCrosshairPos.y / Screen.height, 0.0f);
			}else{
				if(WeaponBehaviorComponent.unarmed){
					crosshairPos = new Vector3(0.5f, 0.5f, 0.0f);
				}else{
					camCrosshairPos = Camera.main.WorldToScreenPoint(WeaponBehaviorComponent.origin + WeaponPivotComponent.childTransform.forward * 2000.0f);
					crosshairPos = new Vector3(camCrosshairPos.x / Screen.width, camCrosshairPos.y / Screen.height, 0.0f);
				}
			}
		}else{
			crosshairPos = new Vector3(0.5f, 0.5f, 0.0f);
		}
		
		CrosshairGuiObjInstance.transform.position = Vector3.Lerp(CrosshairGuiObjInstance.transform.position, crosshairPos, (Time.smoothDeltaTime * 20.0f) * 4.0f);	
		hitmarkerGuiObjInstance.transform.position = CrosshairGuiObjInstance.transform.position;	
		hitmarkerColor.a = 0.2f;
		hitmarkerGuiTexture.color = hitmarkerColor;		
		
	}
	
	void Update (){
	
		//detect if menu display button was pressed
		if (InputComponent.menuPress){
			if(!menuDisplayed){
				MainMenuComponent.enabled = true;
				menuDisplayed = true;
			}else{
				MainMenuComponent.enabled = false;
				paused = false;
				menuDisplayed = false;
			}
			if(Time.timeScale > 0.0f || paused){
				if(!paused){
					menuTime = Time.timeScale;
				}
				Time.timeScale = 0.0f;
			}else{
				Time.timeScale = menuTime;	
			}
		}
		
		if(InputComponent.pausePress && !menuDisplayed){
			if(Time.timeScale > 0.0f){
				paused = true;
				pauseTime = Time.timeScale;
				Time.timeScale = 0.0f;
			}else{
				paused = false;
				Time.timeScale = pauseTime;	
			}
		}
			
		if(allowBulletTime){//make bullet time an optional feature
			if(InputComponent.bulletTimePress){//set bulletTimeActive to true or false based on button input
				if(!bulletTimeActive){
					FPSWalkerComponent.moveSpeedMult = Mathf.Clamp(sloMoMoveSpeed, 1f, sloMoMoveSpeed);
					bulletTimeActive = true;
				}else{
					FPSWalkerComponent.moveSpeedMult = 1.0f;
					bulletTimeActive = false;
				}
			}
					
			otherfx.pitch = Time.timeScale;//sync pitch of bullet time sound effects with Time.timescale
			hitmarkfx.pitch = Time.timeScale;
		
			if(Time.timeScale > 0 && !restarting){//decrease or increase Time.timescale when bulletTimeActive is true
				Time.fixedDeltaTime = initialFixedTime * Time.timeScale;
				if(bulletTimeActive){
					if(!bullettimefxstate){
						otherfx.clip = enterBulletTimeFx;
						otherfx.PlayOneShot(otherfx.clip, 1.0f);//play enter bullet time sound effect
						bullettimefxstate = true;
					}
					Time.timeScale = Mathf.MoveTowards(Time.timeScale, bulletTimeSpeed, Time.deltaTime * 3.0f);
				}else{
					if(bullettimefxstate){
						otherfx.clip = exitBulletTimeFx;
						otherfx.PlayOneShot(otherfx.clip, 1.0f);//play exit bullet time sound effect
						FPSWalkerComponent.moveSpeedMult = 1.0f;
						bullettimefxstate = false;
					}
					if(1.0f - Mathf.Abs(Time.timeScale) > 0.05f){//make sure that timescale returns to exactly 1.0f 
						Time.timeScale = Mathf.MoveTowards(Time.timeScale, 1.0f, Time.deltaTime * 3.0f);
					}else{
						Time.timeScale = 1.0f;
					}
				}
			}
		}
		
		//set zoom mode to toggle, hold, or both, based on inspector setting
		switch (IronsightsComponent.zoomMode){
			case Ironsights.zoomType.both:
				zoomDelay = 0.4f;
			break;
			case Ironsights.zoomType.hold:
				zoomDelay = 0.0f;
			break;
			case Ironsights.zoomType.toggle:
				zoomDelay = 999.0f;
			break;
		}
		
		//regenerate player health if regenerateHealth var is true
		if(regenerateHealth){
			if(hitPoints < maxRegenHealth && timeLastDamaged + healthRegenDelay < Time.time){
				HealPlayer(healthRegenRate * Time.deltaTime);	
			}
		}
		
		//apply player hunger if usePlayerHunger var is true
		if(usePlayerHunger){
			thirstGuiObjInstance.SetActive(true);
			//increase player hunger 
			if(lastHungerTime + hungerInterval < Time.time){
				UpdateHunger(1.0f);
			}
			//calculate and apply starvation damage to player
			if(hungerPoints == maxHungerPoints 
			&& lastStarveTime + starveInterval < Time.time
			&& hitPoints > 0.0f){
				//use a negative heal amount to prevent unneeded damage effects of ApplyDamage function
				HealPlayer(starveDmgAmt, true);//
				//fade screen red when taking starvation damage
				GameObject pf = Instantiate(painFadeObj) as GameObject;//Create instance of painFadeObj
				pf.GetComponent<PainFade>().FadeIn(PainColor, painTexture, 0.75f);//Call FadeIn function in painFadeObj to fade screen red when damage taken
				//Call Die function if player's hitpoints have been depleted
				if (hitPoints < 1.0f){
					SendMessage("Die");//use SendMessage() to allow other script components on this object to detect player death
				}
				//update starvation timers
				timeLastDamaged = Time.time;
				lastStarveTime = Time.time;
			}
			
		}else{
			if(thirstGuiObjInstance){
				thirstGuiObjInstance.SetActive(false);
			}
		}
		
		//apply player thirst if usePlayerThirst var is true
		if(usePlayerThirst){
			hungerGuiObjInstance.SetActive(true);
			//increase player hunger 
			if(lastThirstTime + thirstInterval < Time.time){
				UpdateThirst(1.0f);
			}
			//calculate and apply starvation damage to player
			if(thirstPoints == maxThirstPoints 
			&& lastThirstDmgTime + thirstDmgInterval < Time.time
			&& hitPoints > 0.0f){
				//use a negative heal amount to prevent unneeded damage effects of ApplyDamage function
				HealPlayer(thirstDmgAmt, true);
				//fade screen red when taking starvation damage
				GameObject pf = Instantiate(painFadeObj) as GameObject;//Create instance of painFadeObj
				pf.GetComponent<PainFade>().FadeIn(PainColor, painTexture, 0.75f);//Call FadeIn function in painFadeObj to fade screen red when damage taken
				//Call Die function if player's hitpoints have been depleted
				if (hitPoints < 1.0f){
					Die();
				}
				//update starvation timers
				timeLastDamaged = Time.time;
				lastThirstDmgTime = Time.time;
			}
			
		}else{
			if(hungerGuiObjInstance){
				hungerGuiObjInstance.SetActive(false);
			}
		}
		
		WeaponBehavior WeaponBehaviorComponent = PlayerWeaponsComponent.CurrentWeaponBehaviorComponent;	
		
		//toggle or hold zooming state by determining if zoom button is pressed or held
		if(InputComponent.zoomHold
		&& WeaponBehaviorComponent.canZoom 
		&& !blockState
		&& !IronsightsComponent.reloading
		&& !FPSWalkerComponent.proneMove//no zooming while crawling
		&& !FPSWalkerComponent.hideWeapon){
			if(!zoomStartState){
				zoomStart = Time.time;//track time that zoom button was pressed
				zoomStartState = true;//perform these actions only once
				zoomEndState = false;
				if(zoomEnd - zoomStart < zoomDelay * Time.timeScale){//if button is tapped, toggle zoom state
					if(!zoomed){
						zoomed = true;
					}else{
						zoomed = false;	
					}
				}
			}
		}else{
			if(!InputComponent.zoomHold){blockState = false;}//reset block after a hit, so player needs to press block/zoom button again
			if(!zoomEndState){
				zoomEnd = Time.time;//track time that zoom button was released
				zoomEndState = true;
				zoomStartState = false;
				if(zoomEnd - zoomStart > zoomDelay * Time.timeScale){//if releasing zoom button after holding it down, stop zooming
					zoomed = false;	
				}
			}
		}
		
		//cancel zooming while crawling
		if(FPSWalkerComponent.proneMove){
			zoomEndState = true;
			zoomStartState = false;
			zoomed = false;	
		}
		
		//track when player stopped zooming to allow for delay of reticle becoming visible again
		if (zoomed){
			zoomBtnState = false;//only perform this action once per button press
		}else{
			if(!zoomBtnState){
				zoomStopTime = Time.time;
				zoomBtnState = true;
			}
		}
		
		//scale crosshair size with screen size by crosshairSize value
		if(oldWidth != Screen.width || crossRect.width != Screen.width * crosshairSize){
			crossRect.width = Screen.width * crosshairSize;
			crossRect.height = Screen.width * crosshairSize;
			crossRect.x = -crossRect.width * 0.5f;
			crossRect.y = -crossRect.height * 0.5f;
			CrosshairGuiTexture.pixelInset = crossRect;
			hitmarkerGuiTexture.pixelInset = crossRect;
			oldWidth = Screen.width;
		}
		
		UpdateHitmarker();
		
		//enable and disable crosshair based on various states like reloading and zooming
		if((IronsightsComponent.reloading || (zoomed && (!dzAiming || WeaponBehaviorComponent.zoomIsBlock) && !WeaponBehaviorComponent.showZoomedCrosshair)) 
		&& !CameraControlComponent.thirdPersonActive){
			//don't disable reticle if player is using a melee weapon or if player is unarmed
			if((WeaponBehaviorComponent.meleeSwingDelay == 0 || WeaponBehaviorComponent.zoomIsBlock) && !WeaponBehaviorComponent.unarmed){
				if(crosshairVisibleState){
					//disable the GUITexture element of the instantiated crosshair object
					//and set state so this action will only happen once.
					CrosshairGuiTexture.enabled = false;
					crosshairVisibleState = false;
				}
			}
		}else{
			//Because of the method that is used for non magazine reloads, an additional check is needed here
			//to make the reticle appear after the last bullet reload time has elapsed. Proceed with no check
			//for magazine reloads.
			if((WeaponBehaviorComponent.bulletsPerClip != WeaponBehaviorComponent.bulletsToReload 
				&& WeaponBehaviorComponent.reloadLastStartTime + WeaponBehaviorComponent.reloadLastTime < Time.time)
			|| WeaponBehaviorComponent.bulletsPerClip == WeaponBehaviorComponent.bulletsToReload){
				//allow a delay before enabling crosshair again to let the gun return to neutral position
				//by checking the zoomStopTime value
				if(zoomStopTime + 0.2f < Time.time){
					if(!crosshairVisibleState){
						CrosshairGuiTexture.enabled = true;
						crosshairVisibleState = true;
					}
				}
			}
		}
		
		if(crosshairEnabled){
			if(WeaponBehaviorComponent.showAimingCrosshair){
				if(!WeaponPivotComponent.deadzoneZooming){
					if(!WeaponPivotComponent.deadzoneLooking){
						reticleColor.a = 0.25f;
					}else{
						reticleColor.a = 0.5f;
					}
				}else{
					if(!CameraControlComponent.thirdPersonActive){
						if(zoomed){
							reticleColor.a = 0.5f;
						}else{
							if(WeaponPivotComponent.swayLeadingMode){
								reticleColor.a = 0.25f;
							}else{
								reticleColor.a = 0.0f;//no crosshair for goldeneye/perfect dark style, non-zoomed aiming
							}
						}
					}else{
						reticleColor.a = 0.25f;
					}
				}
				CrosshairGuiTexture.color = reticleColor;
			}else{
				//make alpha of aiming reticle zero/transparent
				reticleColor.a = 0.0f;
				//set alpha of aiming reticle at start to prevent it from showing, but allow item pickup hand reticle 
				CrosshairGuiTexture.color = reticleColor;
			}
		}else{
			reticleColor.a = 0.0f;
			CrosshairGuiTexture.color = reticleColor;
		}
				
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Pick up or activate items	
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		if(InputComponent.useHold){
			if(!useState){
				usePressTime = Time.time;
				objToPickup = hit.collider;
				useState = true;
			}
		}else{
			if(useState){
				useReleaseTime = Time.time;
				useState = false;
			}
			pressButtonUpState = false;
		}
			
		if(!IronsightsComponent.reloading//no item pickup when reloading
		&& !WeaponBehaviorComponent.lastReload//no item pickup when when reloading last round in non magazine reload
		&& !PlayerWeaponsComponent.switching//no item pickup when switching weapons
		&& !FPSWalkerComponent.holdingObject//don't pick up objects if player is dragging them
		&& (!FPSWalkerComponent.canRun || FPSWalkerComponent.inputY == 0)//no item pickup when sprinting
			//there is a small delay between the end of canRun and the start of sprintSwitching (in PlayerWeapons script),
			//so track actual time that sprinting stopped to avoid the small time gap where the pickup hand shows briefly
		&& ((FPSWalkerComponent.sprintStopTime + 0.4f) < Time.time)){
			//raycast a line from the main camera's origin using a point extended forward from camera position/origin as a target to get the direction of the raycast
			//and scale the distance of the raycast based on the playerHeightMod value in the FPSRigidbodyWalker script 
			if((!CameraControlComponent.thirdPersonActive && Physics.Raycast(mainCamTransform.position, 
                                                   WeaponBehaviorComponent.weaponLookDirection, 
												   out hit, 
												   reachDistance + FPSWalkerComponent.playerHeightMod, 
												   rayMask))
			//thirdperson item detection for use button and crosshair
			||(CameraControlComponent.thirdPersonActive && Physics.Raycast(mainCamTransform.position + mainCamTransform.forward * (CameraControlComponent.zoomDistance + CameraControlComponent.currentDistance + 0.5f), 
												 mainCamTransform.forward, out hit, 
												 reachDistance + FPSWalkerComponent.playerHeightMod, 
												 rayMask))
			){
				
				//Detect if player can backstab NPCs
				if(WeaponBehaviorComponent.meleeSwingDelay > 0 && hit.collider.gameObject.layer == 13){
					if(hit.collider.gameObject.GetComponent<AI>() || hit.collider.gameObject.GetComponent<LocationDamage>()){
						if(hit.collider.gameObject.GetComponent<AI>()){
							AIComponent = hit.collider.gameObject.GetComponent<AI>();
						}else{
							AIComponent = hit.collider.gameObject.GetComponent<LocationDamage>().AIComponent;
						}
						if(AIComponent.playerIsBehind && AIComponent.CharacterDamageComponent.hitPoints > 0){
							canBackstab = true;  
						}else{
							canBackstab = false;  
						}
					}else{
						canBackstab = false;  
					}
				}else{
					canBackstab = false; 
				}
				
				if(hit.collider.gameObject.tag == "Usable"){//if the object hit by the raycast is a pickup item and has the "Usable" tag
					
					if (pickUpBtnState && usePressTime - useReleaseTime < 0.4f && usePressTime + 0.4f > Time.time && objToPickup == hit.collider){
						//run the PickUpItem function in the pickup object's script
						hit.collider.SendMessageUpwards("PickUpItem", transform.gameObject, SendMessageOptions.DontRequireReceiver);
						//run the ActivateObject function of this object's script if it has the "Usable" tag
						hit.collider.SendMessageUpwards("ActivateObject", SendMessageOptions.DontRequireReceiver);
						pickUpBtnState = false;
						FPSWalkerComponent.cancelSprint = true;
						usePressTime = -8f;
						objToPickup = null;
					}
					
					//determine if pickup item is using a custom pickup reticle and if so set pickupTex to custom reticle
					if(pickUpBtnState){//check pickUpBtnState to prevent reticle from briefly showing custom/general pickup icon briefly when picking up last weapon before maxWeapons are obtained
						
						//determine if item under reticle is a weapon pickup
						if(hit.collider.gameObject.GetComponent<WeaponPickup>()){
							//set up external script references
							WeaponBehavior PickupWeaponBehaviorComponent = PlayerWeaponsComponent.weaponOrder[hit.collider.gameObject.GetComponent<WeaponPickup>().weaponNumber].GetComponent<WeaponBehavior>();
							WeaponPickup WeaponPickupComponent = hit.collider.gameObject.GetComponent<WeaponPickup>();
							
							if(PlayerWeaponsComponent.totalWeapons == PlayerWeaponsComponent.maxWeapons//player has maximum weapons
							&& PickupWeaponBehaviorComponent.addsToTotalWeaps){//weapon adds to total inventory
								
								//player does not have weapon under reticle
								if(!PickupWeaponBehaviorComponent.haveWeapon
								//and weapon under reticle hasn't been picked up from an item with removeOnUse set to false
								&& !PickupWeaponBehaviorComponent.dropWillDupe){	
									
									if(!useSwapReticle){//if useSwapReticle is true, display swap reticle when item under reticle will replace current weapon
										if(WeaponPickupComponent.weaponPickupReticle){
											//display custom weapon pickup reticle if the weapon item has one defined
											pickupTex = WeaponPickupComponent.weaponPickupReticle;	
										}else{
											//weapon has no custom pickup reticle, just show general pickup reticle 
											pickupTex = pickupReticle;
										}
									}else{
										//display weapon swap reticle if player has max weapons and can swap held weapon for pickup under reticle
										pickupTex = swapReticle;
									}
									
								}else{
									
									//weapon under reticle is not removed on use and is in player's inventory, so show cantPickup reticle
									if(!WeaponPickupComponent.removeOnUse){
										
										pickupTex = noPickupReticle;
										
									}else{//weapon is removed on use, so show standard or custom pickup reticle
										
										if(WeaponPickupComponent.weaponPickupReticle){
											//display custom weapon pickup reticle if the weapon item has one defined
											pickupTex = WeaponPickupComponent.weaponPickupReticle;	
										}else{
											//weapon has no custom pickup reticle, just show general pickup reticle 
											pickupTex = pickupReticle;
										}
										
									}
									
								}
							}else{//total weapons not at maximum and weapon under reticle does not add to inventory
								
								if(!PickupWeaponBehaviorComponent.haveWeapon
								&& !PickupWeaponBehaviorComponent.dropWillDupe
								|| WeaponPickupComponent.removeOnUse){
									
									if(WeaponPickupComponent.weaponPickupReticle){
										//display custom weapon pickup reticle if the weapon item has one defined
										pickupTex = WeaponPickupComponent.weaponPickupReticle;	
									}else{
										//weapon has no custom pickup reticle, just show general pickup reticle 
										pickupTex = pickupReticle;
									}
									
								}else{
									pickupTex = noPickupReticle;
								}
								
							}
						//determine if item under reticle is a health pickup	
						}else if(hit.collider.gameObject.GetComponent<HealthPickup>()){
							//set up external script references
							HealthPickup HealthPickupComponent = hit.collider.gameObject.GetComponent<HealthPickup>();
							
							if(HealthPickupComponent.healthPickupReticle){
								pickupTex = HealthPickupComponent.healthPickupReticle;	
							}else{
								pickupTex = pickupReticle;
							}
						//determine if item under reticle is an ammo pickup
						}else if(hit.collider.gameObject.GetComponent<AmmoPickup>()){
							//set up external script references
							AmmoPickup AmmoPickupComponent = hit.collider.gameObject.GetComponent<AmmoPickup>();
							
							if(AmmoPickupComponent.ammoPickupReticle){
								pickupTex = AmmoPickupComponent.ammoPickupReticle;	
							}else{
								pickupTex = pickupReticle;
							}
						}else{
							pickupTex = pickupReticle;
						}
					}
					
					UpdateReticle(false);//show pickupReticle if raycast hits a pickup item

				}else{
					objToPickup = null;//cancel use press if player moves away
					if(hit.collider.gameObject.layer == 13){//switch to pickup reticle if this NPC can be interacted with
						if(hit.collider.gameObject.GetComponent<AI>() 
						|| hit.collider.gameObject.GetComponent<LocationDamage>()){
							if(hit.collider.gameObject.GetComponent<AI>()){
								AIComponent = hit.collider.gameObject.GetComponent<AI>();
							}else{
								AIComponent = hit.collider.gameObject.GetComponent<LocationDamage>().AIComponent;
							}
							if(AIComponent.factionNum == 1 && AIComponent.followOnUse && AIComponent.enabled){
								pickupTex = pickupReticle;
								UpdateReticle(false);
								if (pickUpBtnState && InputComponent.useHold){
									AIComponent.CommandNPC();//command NPC to follow or stay put
									pickUpBtnState = false;
									FPSWalkerComponent.cancelSprint = true;
								}
							}else{
								UpdateReticle(true);//show aiming reticle crosshair if item is not a pickup item
							}
						}else{
							if(crosshairTextureState){
								UpdateReticle(true);//show aiming reticle crosshair if item is not a pickup item
							}
						}
					}else{
						if(crosshairTextureState){
							UpdateReticle(true);//show aiming reticle crosshair if item is not a pickup item
						}
					}
				}
			}else{
				canBackstab = false; 
				if(crosshairTextureState){
					UpdateReticle(true);//show aiming reticle crosshair if raycast hits nothing
				}
				//Command NPCs to move to location under crosshair
				if(moveCommandedTime + 0.5f < Time.time && 
				((!CameraControlComponent.thirdPersonActive && Physics.Raycast(mainCamTransform.position, WeaponBehaviorComponent.weaponLookDirection, out hit2, 500f, rayMask))
				||(CameraControlComponent.thirdPersonActive && Physics.Raycast(mainCamTransform.position, mainCamTransform.forward, out hit2, 500f, rayMask)))){
					if(hit2.collider.gameObject.layer == 10 || hit2.collider.gameObject.layer == 0){
						if (pickUpBtnState && InputComponent.useHold){
							NPCRegistryComponent.MoveFolowingNpcs(hit2.point);
							moveCommandedTime = Time.time;
							pickUpBtnState = false;
						}
					}
				}
			}
		}else{
			canBackstab = false; 
			if(crosshairTextureState){
				UpdateReticle(true);//show aiming reticle crosshair if reloading, switching weapons, or sprinting
			}
		}
		
		//only register one press of E key to make player have to press button again to pickup items instead of holding E
		if (InputComponent.useHold){
			pickUpBtnState = false;
		}else{
			pickUpBtnState = true;	
		}
	
	}
	
//	void OnDrawGizmos() {
		//draw red dot at crosshair raycast position
//		Gizmos.color = Color.red;
//		Gizmos.DrawSphere(hit2.point, 0.2f);
//	}
	
	//set reticle type based on the boolean value passed to this function
	public void UpdateReticle( bool reticleType ){
		if(!reticleType){
			CrosshairGuiTexture.texture = pickupTex;
			CrosshairGuiTexture.color = pickupReticleColor;
			crosshairTextureState = true;
		}else{
			CrosshairGuiTexture.texture = aimingReticle;
			CrosshairGuiTexture.color = reticleColor;
			crosshairTextureState = false;	
		}
	}
	
	void UpdateHitmarker(){
		if(hitTime + 0.3f > Time.time){
			if(!hitMarkerState){
				if(WeaponBehaviorComponent.meleeSwingDelay == 0 && !WeaponBehaviorComponent.meleeActive){
					hitmarkerGuiTexture.enabled = true;
					hitmarkfx.clip = hitMarker;
					hitmarkfx.PlayOneShot(hitmarkfx.clip, 1.0f);
					hitMarkerState = true;
				}
			}
		}else{
			if(hitMarkerState){
				hitMarkerState = false;
			}
			hitmarkerGuiTexture.enabled = false;
		}
	}
	
	public void UpdateHitTime(){
		hitTime = Time.time;//used for hitmarker
		hitMarkerState = false;	
	}
	
	//Activate bullet time for a specific duration
	public IEnumerator ActivateBulletTime (float duration){
		if(!bulletTimeActive){
			bulletTimeActive = true;
		}else{
			yield break;
		}
		float startTime = Time.time;
		while(true){
			if(startTime + duration < Time.time){
				bulletTimeActive = false;
				yield break;
			}
			yield return new WaitForSeconds(0.1f);
		}	
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Update player attributes
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	//add hitpoints to player health
	public void HealPlayer( float healAmt, bool isHungryThirsty = false ){
			
		if (hitPoints < 1.0f){//Don't add health if player is dead
			return;
		}
		
		//Apply healing
		if(hitPoints + healAmt > maximumHitPoints){ 
			hitPoints = maximumHitPoints;
		}else{
			//Call Die function if player's hitpoints have been depleted
			if(healAmt < 0){
				if(!isHungryThirsty){
					ApplyDamage(healAmt);//allow items that cause damage when consumed
				}else{
					hitPoints += healAmt;//waste away from hunger or thirst
				}
			}else{
				hitPoints += healAmt;
			}
		}
			
		//set health hud value to hitpoints remaining
		HealthText.healthGui = Mathf.Round(hitPoints);
		HealthText2[1].healthGui = Mathf.Round(hitPoints);
			
		//change color of hud health element based on hitpoints remaining
		if (hitPoints <= 25.0f){
			healthGuiText.material.color = Color.red;
		}else if (hitPoints <= 40.0f){
			healthGuiText.material.color = Color.yellow;	
		}else{
			healthGuiText.material.color = HealthText.textColor;	
		}

	}
	
	//update the hunger amount for the player
	public void UpdateHunger( float hungerAmt ){
		
		if (hitPoints < 1.0f){//Don't add hunger if player is dead
			return;
		}
		
		//Apply hungerAmt
		if(hungerPoints + hungerAmt > maxHungerPoints){ 
			hungerPoints = maxHungerPoints;
		}else{
			hungerPoints += hungerAmt;
		}
		
		hungerPoints = Mathf.Clamp(hungerPoints, 0.0f, hungerPoints);
			
		//set hunger hud value to hunger points remaining
		HungerText.hungerGui = Mathf.Round(hungerPoints);
		HungerText2[1].hungerGui = Mathf.Round(hungerPoints);
			
		//change color of hud hunger element based on hunger points
		if (hungerPoints <= 65.0f){
			HungerGUIText.material.color = HungerText.textColor;
		}else if (hungerPoints <= 85.0f){
				HungerGUIText.material.color = Color.yellow;	
		}else{
			HungerGUIText.material.color = Color.red;	
		}
		
		lastHungerTime = Time.time;	
	}
	
	//update the thirst amount for the player
	public void UpdateThirst( float thirstAmt ){
		
		if (hitPoints < 1.0f){//Don't add thirst if player is dead
			return;
		}
		
		//Apply thirstAmt
		if(thirstPoints + thirstAmt > maxThirstPoints){ 
			thirstPoints = maxThirstPoints;
		}else{
			thirstPoints += thirstAmt;
		}
		
		thirstPoints = Mathf.Clamp(thirstPoints, 0.0f, thirstPoints);
			
		//set thirst hud value to thirst points remaining
		ThirstText.thirstGui = Mathf.Round(thirstPoints);
		ThirstText2[1].thirstGui = Mathf.Round(thirstPoints);
			
		//change color of hud thirst element based on thirst points
		if (thirstPoints <= 65.0f){
			ThirstGUIText.material.color = ThirstText.textColor;
		}else if (thirstPoints <= 85.0f){
				ThirstGUIText.material.color = Color.yellow;	
		}else{
			ThirstGUIText.material.color = Color.red;	
		}
		
		lastThirstTime = Time.time;
	}
	
	//remove hitpoints from player health
	public void ApplyDamage ( float damage, Transform attacker = null, bool isMeleeAttack = false){

		float appliedPainKickAmt;
			
		if (hitPoints < 1.0f){//Don't apply damage if player is dead
			if(!showHpUnderZero){hitPoints = 0.0f;}
			return;
		}
		
		//detect if attacker is inside player block zone
		if(attacker != null 
		&& WeaponBehaviorComponent.zoomIsBlock 
		&& WeaponBehaviorComponent.blockDefenseAmt > 0f
		&& zoomed
		&& ((WeaponBehaviorComponent.onlyBlockMelee && isMeleeAttack) || !WeaponBehaviorComponent.onlyBlockMelee)
		&& WeaponBehaviorComponent.shootStartTime + WeaponBehaviorComponent.fireRate < Time.time){
		
			Vector3 toTarget = (attacker.position - transform.position).normalized;
			blockAngle = Vector3.Dot(toTarget, transform.forward);
			
			if(Vector3.Dot(toTarget, transform.forward) > WeaponBehaviorComponent.blockCoverage){
			
				damage *= 1f - WeaponBehaviorComponent.blockDefenseAmt;
				otherfx.clip = WeaponBehaviorComponent.blockSound;
				otherfx.PlayOneShot(otherfx.clip, 1.0f);
				
				if(blockParticles){
					blockParticles.transform.position = Camera.main.transform.position + Camera.main.transform.forward * (blockParticlesPos + CameraControlComponent.zoomDistance + CameraControlComponent.currentDistance);
					if(blockParticles.GetComponent<ParticleEmitter>()){blockParticles.GetComponent<ParticleEmitter>().Emit();}
					foreach (Transform child in blockParticles.transform){//emit all particles in the particle effect game object group stored in blockParticles var
						child.GetComponent<ParticleEmitter>().Emit();//emit the particle(s)
					}
				}
				blockState = true;
			}
		}
		
		timeLastDamaged = Time.time;

	    Quaternion painKickRotation;//Set up rotation for pain view kicks
	    int painKickUpAmt = 0;
	    int painKickSideAmt = 0;
		
		if(!invulnerable){
			hitPoints -= damage;//Apply damage
		}
	
		//set health hud value to hitpoints remaining
		HealthText.healthGui = Mathf.Round(hitPoints);
		HealthText2[1].healthGui = Mathf.Round(hitPoints);
			
		//change color of hud health element based on hitpoints remaining
		if (hitPoints <= 25.0f){
			healthGuiText.material.color = Color.red;
		}else if (hitPoints <= 40.0f){
			healthGuiText.material.color = Color.yellow;	
		}else{
			healthGuiText.material.color = HealthText.textColor;	
		}
		
		if(!blockState){
			GameObject pf = Instantiate(painFadeObj) as GameObject;//Create instance of painFadeObj
			painFadeColor = PainColor;
			painFadeColor.a = (damage / 5.0f);//fade pain overlay based on damage amount
			pf.GetComponent<PainFade>().FadeIn(painFadeColor, painTexture, 0.75f);//Call FadeIn function in painFadeObj to fade screen red when damage taken
		}
			
		if(!FPSWalkerComponent.holdingBreath){
			//Play pain sound when getting hit
			if(!blockState){//don't play hit sound if blocking attack
				if (Time.time > gotHitTimer && painBig && painLittle) {
					// Play a big pain sound
					if (hitPoints < 40.0f || damage > 30.0f) {
						otherfx.clip = painBig;
						otherfx.PlayOneShot(otherfx.clip, 1.0f);
						gotHitTimer = Time.time + Random.Range(.5f, .75f);
					} else {
						//Play a small pain sound
						otherfx.clip = painLittle;
						otherfx.PlayOneShot(otherfx.clip, 1.0f);
						gotHitTimer = Time.time + Random.Range(.5f, .75f);
					}
				}
			}
		}else{
			if (Time.time > gotHitTimer && painDrown) {
				//Play a small pain sound
				otherfx.clip = painDrown;
				otherfx.PlayOneShot(otherfx.clip, 1.0f);
				gotHitTimer = Time.time + Random.Range(.5f, .75f);
			}	
		}
		
		if(!CameraControlComponent.thirdPersonActive){
			painKickUpAmt = Random.Range(100, -100);//Choose a random view kick up amount
			if(painKickUpAmt < 50 && painKickUpAmt > 0){painKickUpAmt = 50;}//Maintain some randomness of the values, but don't make it too small
			if(painKickUpAmt < 0 && painKickUpAmt > -50){painKickUpAmt = -50;}
			
			painKickSideAmt = Random.Range(100, -100);//Choose a random view kick side amount
			if(painKickSideAmt < 50 && painKickSideAmt > 0){painKickSideAmt = 50;}
			if(painKickSideAmt < 0 && painKickSideAmt > -50){painKickSideAmt = -50;}
			
			//create a rotation quaternion with random pain kick values
			painKickRotation = Quaternion.Euler(mainCamTransform.localRotation.eulerAngles - new Vector3(painKickUpAmt, painKickSideAmt, 0));
			
			//make screen kick amount based on the damage amount recieved
			if(zoomed && !WeaponBehaviorComponent.zoomIsBlock){
				appliedPainKickAmt = (damage / (painScreenKickAmt * 10)) / 3;	
			}else{
				appliedPainKickAmt = (damage / (painScreenKickAmt * 10));			
			}
			
			if(blockState){
				appliedPainKickAmt = 0.025f;
			}
			
			//make sure screen kick is not so large that view rotates past arm models 
			appliedPainKickAmt = Mathf.Clamp(appliedPainKickAmt, 0.0f, 0.15f); 
			
			//smooth current camera angles to pain kick angles using Slerp
			mainCamTransform.localRotation = Quaternion.Slerp(mainCamTransform.localRotation, painKickRotation, appliedPainKickAmt );
		}
		
		if(WeaponBehaviorComponent.zoomIsBlock){
			if(!WeaponBehaviorComponent.hitCancelsBlock){
				blockState = false;
			}else{
			 	zoomed = false;
			}
		}
	
		//Call Die function if player's hitpoints have been depleted
		if (hitPoints < 1.0f){
			SendMessage("Die");//use SendMessage() to allow other script components on this object to detect player death
		}
	}
	
	void Die () {
		
		bulletTimeActive = false;//set bulletTimeActive to false so fadeout wont take longer if bullet time is active
		
		if(!FPSWalkerComponent.drowning){
			//play normal player death sound effect if the player is on land 
			otherfx.clip = die;
			otherfx.PlayOneShot(otherfx.clip, 1.0f);

		}else{
			//play drowning sound effect if the player is underwater 	
			otherfx.clip = dieDrown;
			otherfx.PlayOneShot(otherfx.clip, 1.0f);
		}
		
		//disable player control and sprinting on death
		FPSWalkerComponent.inputX = 0;
		FPSWalkerComponent.inputY = 0;
		FPSWalkerComponent.cancelSprint = true;
			
		GameObject llf = Instantiate(levelLoadFadeObj) as GameObject;//Create instance of levelLoadFadeObj
		//call FadeAndLoadLevel function with fadein argument set to false 
		//in levelLoadFadeObj to restart level and fade screen out from black on level load
		llf.GetComponent<LevelLoadFade>().FadeAndLoadLevel(Color.black, 2.0f, false);
		
	}
	
	public void RestartMap () {
		Time.timeScale = 1.0f;//set timescale to 1.0f so fadeout wont take longer if bullet time is active
		GameObject llf = Instantiate(levelLoadFadeObj) as GameObject;//Create instance of levelLoadFadeObj
		//call FadeAndLoadLevel function with fadein argument set to false 
		//in levelLoadFadeObj to restart level and fade screen out from black on level load
		llf.GetComponent<LevelLoadFade>().FadeAndLoadLevel(Color.black, 2.0f, false);
		//set restarting var to true to be accessed by FPSRigidBodyWalker script to stop rigidbody movement
		restarting = true;
		// Disable all scripts to deactivate player control upon player death
		FPSWalkerComponent.inputX = 0;
		FPSWalkerComponent.inputY = 0;
		FPSWalkerComponent.cancelSprint = true;
		WeaponBehaviorComponent.shooting = false;
	}

}