//Footsteps.cs by Azuline Studios© All Rights Reserved
//Plays footstep sounds by surface type.
using UnityEngine;
using System.Collections;

public class Footsteps : MonoBehaviour {
	private FPSRigidBodyWalker FPSWalkerComponent;
	[HideInInspector]
	public GameObject playerObj;
	[HideInInspector]
	public string materialType;//tag of object that player is standing on (Metal, Wood, Dirt, Stone, Water)
	private float volumeAmt = 1.0f;//volume of audio clip to be played
	private AudioClip footStepClip;//audio clip to be played 

	[Tooltip("Index number of the terrain texture currently beneath the player, which the various terrain index numbers below can be set to, for footstep sound effects on terrain.")]
	public int terrainIndex;
	[HideInInspector]
	public bool onTerrain;//true if player is on terrain

	//player movement sounds and volume amounts
	[Tooltip("Terrain index number to use for dirt footstep sounds (-1 will play no sounds).")]
	public int dirtTerrainIndex1;
	[Tooltip("Terrain index number to use for dirt footstep sounds (-1 will play no sounds).")]
	public int dirtTerrainIndex2 = -1;
	[Tooltip("Terrain index number to use for dirt footstep sounds (-1 will play no sounds).")]
	public int dirtTerrainIndex3 = -1;
	[Tooltip("Volume of dirt footstep sounds.")]
	public float dirtStepVol = 1.0f;
	[Tooltip("Array containing sound effects to be used for dirt footsteps.")]
	public AudioClip[] dirtSteps;
	[Tooltip("Terrain index number to use for stone footstep sounds (-1 will play no sounds).")]
	public int stoneTerrainIndex1;
	[Tooltip("Terrain index number to use for stone footstep sounds (-1 will play no sounds).")]
	public int stoneTerrainIndex2 = -1;
	[Tooltip("Terrain index number to use for stone footstep sounds (-1 will play no sounds).")]
	public int stoneTerrainIndex3 = -1;
	[Tooltip("Volume of stone footstep sounds.")]
	public float stoneStepVol = 1.0f;
	[Tooltip("Array containing sound effects to be used for stone footsteps.")]
	public AudioClip[] stoneSteps;
	[Tooltip("Terrain index number to use for grass footstep sounds (-1 will play no sounds).")]
	public int grassTerrainIndex1;
	[Tooltip("Terrain index number to use for grass footstep sounds (-1 will play no sounds).")]
	public int grassTerrainIndex2 = -1;
	[Tooltip("Terrain index number to use for grass footstep sounds (-1 will play no sounds).")]
	public int grassTerrainIndex3 = -1;
	[Tooltip("Volume of grass footstep sounds.")]
	public float grassStepVol = 1.0f;
	[Tooltip("Array containing sound effects to be used for grass footsteps.")]
	public AudioClip[] grassSteps;
	[Tooltip("Terrain index number to use for gravel footstep sounds (-1 will play no sounds).")]
	public int gravelTerrainIndex1;
	[Tooltip("Terrain index number to use for gravel footstep sounds (-1 will play no sounds).")]
	public int gravelTerrainIndex2 = -1;
	[Tooltip("Terrain index number to use for gravel footstep sounds (-1 will play no sounds).")]
	public int gravelTerrainIndex3 = -1;
	[Tooltip("Volume of gravel footstep sounds.")]
	public float gravelStepVol = 1.0f;
	[Tooltip("Array containing sound effects to be used for gravel footsteps.")]
	public AudioClip[] gravelSteps;
	[Tooltip("Terrain index number to use for sand footstep sounds (-1 will play no sounds).")]
	public int sandTerrainIndex1;
	[Tooltip("Terrain index number to use for sand footstep sounds (-1 will play no sounds).")]
	public int sandTerrainIndex2 = -1;
	[Tooltip("Terrain index number to use for sand footstep sounds (-1 will play no sounds).")]
	public int sandTerrainIndex3 = -1;
	[Tooltip("Volume of sand footstep sounds.")]
	public float sandStepVol = 1.0f;
	[Tooltip("Array containing sound effects to be used for sand footsteps.")]
	public AudioClip[] sandSteps;

	[Tooltip("Volume of default footstep sounds.")]
	public float defaultStepVol = 1.0f;
	[Tooltip("Array containing sound effects to be used for default footsteps.")]
	public AudioClip[] defaultSteps;
	[Tooltip("Volume of wood footstep sounds.")]
	public float woodStepVol = 1.0f;
	[Tooltip("Array containing sound effects to be used for wood footsteps.")]
	public AudioClip[] woodSteps;
	[Tooltip("Volume of metal footstep sounds.")]
	public float metalStepVol = 1.0f;
	[Tooltip("Array containing sound effects to be used for metal footsteps.")]
	public AudioClip[] metalSteps;
	[Tooltip("Volume of water footstep sounds.")]
	public float waterSoundVol = 1.0f;
	[Tooltip("Array containing sound effects to be used for water footsteps.")]
	public AudioClip[] waterSounds;

	[Tooltip("Volume of ladder footstep sounds.")]
	public float climbSoundVol = 1.0f;
	[Tooltip("Array containing sound effects to be used for ladder footsteps.")]
	public AudioClip[] climbSounds;
	[Tooltip("Volume of prone shuffle sounds.")]
	public float proneStepVol = 1.0f;
	[Tooltip("Array containing sound effects to be used for prone shuffling.")]
	public AudioClip[] proneSteps;

	[Tooltip("Sound for landing on a dirt surface.")]		
	public AudioClip dirtLand;
	[Tooltip("Sound for landing on a dirt surface.")]	
	public AudioClip metalLand;
	[Tooltip("Sound for landing on a metal surface.")]	
	public AudioClip woodLand;
	[Tooltip("Sound for landing on a water surface.")]	
	public AudioClip waterLand;
	[Tooltip("Sound for landing on a stone surface.")]	
	public AudioClip stoneLand;

	[Tooltip("Volume of foliage rustle sounds.")]
	public float foliageRustleVol = 1.0f;
	[Tooltip("Array containing sound effects to be used for foliage rustling.")]
	public AudioClip[] foliageRustles;

	private AudioSource aSource;

	private Terrain terrain;
	private TerrainData terrainData;
	private Vector3 terrainPos;
	
	void Start () {
		playerObj = transform.gameObject;
		FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		aSource = playerObj.AddComponent<AudioSource>(); 
		aSource.spatialBlend = 0.0f;
		aSource.playOnAwake = false;

		if(Terrain.activeTerrain){//get information of active terrain		
			terrain = Terrain.activeTerrain;
			terrainData = terrain.terrainData;
			terrainPos = terrain.transform.position;
		}
	}
	
	public void FootstepSfx (){
		//play footstep sound effects
		if(!FPSWalkerComponent.prone){
			if(!FPSWalkerComponent.climbing){
				if(FPSWalkerComponent.inWater){//play swimming/wading footstep effects
					if(waterSounds.Length > 0){
						footStepClip = waterSounds[Random.Range(0, waterSounds.Length)];//select random water step effect from waterSounds array
						if(!FPSWalkerComponent.holdingBreath){
							volumeAmt = waterSoundVol;//set volume of audio clip to customized amount
						}else{
							volumeAmt = waterSoundVol/2;//set volume of audio clip to customized amount
						}
						aSource.clip = footStepClip;
						aSource.volume = volumeAmt;
						aSource.Play();
					}
				}else{
					//Make a short delay before playing footstep sounds to allow landing sound to play
					if (FPSWalkerComponent.grounded && (FPSWalkerComponent.landStartTime + 0.3f) < Time.time){

						if(!onTerrain){
							switch(materialType){//determine which material the player is standing on and select random footstep effect for surface type
							case "Wood":
								if(woodSteps.Length > 0){
									footStepClip = woodSteps[Random.Range(0, woodSteps.Length)];
									volumeAmt = woodStepVol;
								}
								break;
							case "Metal":
								if(metalSteps.Length > 0){
									footStepClip = metalSteps[Random.Range(0, metalSteps.Length)];
									volumeAmt = metalStepVol;
								}
								break;
							case "Dirt":
								if(dirtSteps.Length > 0){
									footStepClip = dirtSteps[Random.Range(0, dirtSteps.Length)];
									volumeAmt = dirtStepVol;
								}
								break;
							case "Stone":
								if(stoneSteps.Length > 0){
									footStepClip = stoneSteps[Random.Range(0, stoneSteps.Length)];
									volumeAmt = stoneStepVol;
								}
								break;
							case "Sand":
								if(sandSteps.Length > 0){
									footStepClip = sandSteps[Random.Range(0, sandSteps.Length)];
									volumeAmt = sandStepVol;
								}
								break;
							case "Gravel":
								if(gravelSteps.Length > 0){
									footStepClip = gravelSteps[Random.Range(0, gravelSteps.Length)];
									volumeAmt = gravelStepVol;
								}
								break;
							case "Grass":
								if(grassSteps.Length > 0){
									footStepClip = grassSteps[Random.Range(0, grassSteps.Length)];
									volumeAmt = grassStepVol;
								}
								break;
							default:
								if(defaultSteps.Length > 0){
									footStepClip = defaultSteps[Random.Range(0, defaultSteps.Length)];
									volumeAmt = defaultStepVol;
								}
								break;	
							}
						}else{
							if(Terrain.activeTerrain){
								terrainIndex = GetTerrainTextureIndex(transform.position);
								if(terrainIndex == dirtTerrainIndex1
								|| terrainIndex == dirtTerrainIndex2
								|| terrainIndex == dirtTerrainIndex3){
									if(dirtSteps.Length > 0){
										footStepClip = dirtSteps[Random.Range(0, dirtSteps.Length)];
										volumeAmt = dirtStepVol;
									}
								}else if(terrainIndex == stoneTerrainIndex1
								|| terrainIndex == stoneTerrainIndex2
								|| terrainIndex == stoneTerrainIndex3){
									if(stoneSteps.Length > 0){
										footStepClip = stoneSteps[Random.Range(0, stoneSteps.Length)];
										volumeAmt = stoneStepVol;
									}
								}else if(terrainIndex == grassTerrainIndex1
								|| terrainIndex == grassTerrainIndex2
								|| terrainIndex == grassTerrainIndex3){
									if(grassSteps.Length > 0){
										footStepClip = grassSteps[Random.Range(0, grassSteps.Length)];
										volumeAmt = grassStepVol;
									}
								}else if(terrainIndex ==  gravelTerrainIndex1
								|| terrainIndex ==  gravelTerrainIndex2
								|| terrainIndex ==  gravelTerrainIndex3){
									if(gravelSteps.Length > 0){
										footStepClip = gravelSteps[Random.Range(0, gravelSteps.Length)];
										volumeAmt = gravelStepVol;
									}
								}else if(terrainIndex == sandTerrainIndex1
								|| terrainIndex == sandTerrainIndex2
								|| terrainIndex == sandTerrainIndex3){
									if(sandSteps.Length > 0){
										footStepClip = sandSteps[Random.Range(0, sandSteps.Length)];
										volumeAmt = sandStepVol;
									}
								}else{
									if(defaultSteps.Length > 0){
										footStepClip = defaultSteps[Random.Range(0, defaultSteps.Length)];
										volumeAmt = defaultStepVol;
									}
								}
							}
						}
						if(footStepClip){
							//play the sound effect
							aSource.clip = footStepClip;
							aSource.volume = volumeAmt;
							aSource.Play();
						}
					}
				}
			}else{//play climbing footstep effects
				if(climbSounds.Length > 0){
					footStepClip = climbSounds[Random.Range(0, climbSounds.Length)];
					volumeAmt = climbSoundVol;
					aSource.clip = footStepClip;
					aSource.volume = volumeAmt;
					aSource.Play();
				}
			}
		}else{
			if(proneSteps.Length > 0){
				footStepClip = proneSteps[Random.Range(0, proneSteps.Length)];
				volumeAmt = proneStepVol;
				aSource.clip = footStepClip;
				aSource.volume = volumeAmt;
				aSource.Play();
			}
		}
	}
	
//	void OnGUI () {
		//display index of terrain texture below player on screen for testing
//		GUI.Box(new Rect( 100, 100, 200, 25 ), terrainIndex.ToString());
//	}

	private float[] GetTexturesAtPoint(Vector3 WorldPos){
		//find the splat map cell the worldPos correlates with
		int mapX = (int)(((WorldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
		int mapZ = (int)(((WorldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);
		
		//get the alpha map data for worldPos
		float[,,] splatmapData = terrainData.GetAlphamaps( mapX, mapZ, 1, 1 );
		float[] alphaMix = new float[ splatmapData.GetUpperBound(2) + 1 ];
		
		for(int n=0; n<alphaMix.Length; n++){
			alphaMix[n] = splatmapData[ 0, 0, n ];
		}
		return alphaMix;
	}
	
	private int GetTerrainTextureIndex(Vector3 WorldPos){
		//returns the index of the most dominant texture at this world position.
		float[] mix = GetTexturesAtPoint(WorldPos);
		
		float maxMix = 0;
		int maxIndex = 0;
		
		//loop through each alpha value and find the most visible texture and this world position
		for(int n=0; n<mix.Length; n++){
			if ( mix[n] > maxMix ){
				maxIndex = n;
				maxMix = mix[n];
			}
		}
		return maxIndex;
	}

}
