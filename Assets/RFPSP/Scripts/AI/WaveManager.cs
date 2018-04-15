//WaveManager.cs by Azuline Studios© All Rights Reserved
//Spawns NPCs from NPC Spawners for successive waves using several 
//parameters to control spawn timing and amounts.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MultiDimensionalInt
{
	[Tooltip("Total number of NPCs to spawn for this wave.")] 
	public int[] NpcCounts;
	[Tooltip("Maximum number of NPCs from the spawner that can be active in the scene at once.")]
	public int[] NpcLoads;
	[Tooltip("Delay between spawning of NPCs for this wave.")]
	public float[] NpcDelay;
	[Tooltip("The NPC Prefabs that will be spawned for this wave.")]
	public GameObject[] NpcTypes;
}

public class WaveManager : MonoBehaviour {
	private FPSPlayer FPSPlayerComponent;
	[Tooltip("The NPC Spawner objects that the Wave Manager will spawn NPC's from. The Waves list parameters correspond the order of these spawners from top to bottom.")]
	public List<NPCSpawner> NpcSpawners = new List<NPCSpawner>();
	[Tooltip("This list contains information for NPC wave spawning. The array sizes and order correspond with the Npc Spawners list. The Waves list can be expanded to add new waves of varying combinations of NPCs and parameters.")] 
	public MultiDimensionalInt[] waves;
	[Tooltip("Time before wave begins.")]
	public float warmupTime = 30.0f;
	private float startTime = -512;
	private float countDown;
	[HideInInspector]
	public int NpcsToSpawn;
	[HideInInspector]
	public int killedNpcs;
	[HideInInspector]
	public int waveNumber;

	[Tooltip("Sound FX played when wave starts.")]
	public AudioClip waveStartFx;
	[Tooltip("Sound FX played when wave ends.")]
	public AudioClip waveEndFx;
	private AudioSource asource;
	private bool fxPlayed;
	private bool fxPlayed2;
	private bool lastWave;

	[HideInInspector]
	public GameObject waveGuiObj;//this object is instantiated for heath display on hud
	[HideInInspector]
	public GameObject waveGuiObjInstance;
	private WaveText WaveText;
	private WaveText[] WaveText2;
	[HideInInspector]
	public GameObject warmupGuiObj;//this object is instantiated for heath display on hud
	[HideInInspector]
	public GameObject warmupGuiObjInstance;
	private WarmupText WarmupText;
	private WarmupText[] WarmupText2;

	private Color tempColor;
	private Color tempColor2;

	private GUIText WarmupGUIText1;
	private GUIText WarmupGUIText2;
	
	void Start () {
		FPSPlayerComponent =  Camera.main.GetComponent<CameraControl>().playerObj.GetComponent<FPSPlayer>();
		asource = gameObject.AddComponent<AudioSource>();
		asource.spatialBlend = 0.0f;
		//create instance of GUIText to display health amount on hud
		waveGuiObjInstance = Instantiate(waveGuiObj,Vector3.zero,transform.rotation) as GameObject;
		//set reference for main color element of heath GUIText
		WaveText = waveGuiObjInstance.GetComponent<WaveText>();
		//set reference for shadow background color element of GUIText
		//this object is a child of the main GUIText object, so access it as an array
		WaveText2 = waveGuiObjInstance.GetComponentsInChildren<WaveText>();
		
		//initialize health amounts on GUIText objects
		WaveText.waveGui = waveNumber;
		WaveText2[1].waveGui = waveNumber;	
		WaveText.waveGui2 = NpcsToSpawn - killedNpcs;
		WaveText2[1].waveGui2 = NpcsToSpawn - killedNpcs;

		//create instance of GUIText to display health amount on hud
		warmupGuiObjInstance = Instantiate(warmupGuiObj,Vector3.zero,transform.rotation) as GameObject;
		//set reference for main color element of heath GUIText
		WarmupText = warmupGuiObjInstance.GetComponent<WarmupText>();
		//set reference for shadow background color element of GUIText
		//this object is a child of the main GUIText object, so access it as an array
		WarmupText2 = warmupGuiObjInstance.GetComponentsInChildren<WarmupText>();

		tempColor = WarmupText.textColor;
		tempColor2 = WarmupText2[1].textColor; 

		WarmupText.warmupGui = countDown;
		WarmupText2[1].warmupGui = countDown;

		WarmupGUIText1 = WarmupText.GetComponent<GUIText>();
		WarmupGUIText2 = WarmupText2[1].GetComponent<GUIText>();
		
		StartCoroutine(StartWave());
	}

	void FixedUpdate(){	
		if(WaveText.waveGui2 != NpcsToSpawn - killedNpcs){
			WaveText.waveGui2 = NpcsToSpawn - killedNpcs;
			WaveText2[1].waveGui2 = NpcsToSpawn - killedNpcs;
		}
	}

	public IEnumerator StartWave(){

		countDown = warmupTime;
		WarmupText.warmupGui = countDown;
		WarmupText2[1].warmupGui = countDown;	
		killedNpcs = 0;
		NpcsToSpawn = 0;
		if(waveNumber <= waves.Length){
			if(waveNumber < waves.Length){
				waveNumber ++;
			}else{
				//start again from first wave if last wave was completed
				lastWave = true;
				waveNumber = 1;
			}
		}else{
			waveNumber = 1;
		}
		WaveText.waveGui = waveNumber;
		WaveText2[1].waveGui = waveNumber;	

		tempColor.a = 1.0f;
		tempColor2.a = 1.0f;

		WarmupText.waveBegins = false;
		WarmupText2[1].waveBegins = false;

		if(waveNumber > 1 || lastWave){
			startTime = Time.time;
			WarmupText.waveComplete = true;
			WarmupText2[1].waveComplete = true;
			if(waveEndFx && !fxPlayed2){
				asource.PlayOneShot(waveEndFx, 1.0f);
				FPSPlayerComponent.StartCoroutine(FPSPlayerComponent.ActivateBulletTime(1.0f));
				fxPlayed2 = true;
			}
			if(lastWave){lastWave = false;}
		}

		//initialize NPC Spawner objects for spawning of this wave
		for(int i = 0; i < NpcSpawners.Count; i++){
			NpcSpawners[i].NPCPrefab = waves[waveNumber - 1].NpcTypes[i];
			NpcSpawners[i].NpcsToSpawn = waves[waveNumber - 1].NpcCounts[i];
			NpcSpawners[i].maxActiveNpcs = waves[waveNumber - 1].NpcLoads[i];
			NpcSpawners[i].spawnDelay = waves[waveNumber - 1].NpcDelay[i];
			NpcsToSpawn += NpcSpawners[i].NpcsToSpawn;
			NpcSpawners[i].pauseSpawning = true;
			NpcSpawners[i].spawnedNpcAmt = 0;
			NpcSpawners[i].huntPlayer = true;
			NpcSpawners[i].unlimitedSpawning = false;
		}

		//spawn wave
		while(true){

			WarmupText.verticalOffsetAmt = WarmupText.verticalOffset;
			WarmupText2[1].verticalOffsetAmt = WarmupText2[1].verticalOffset;

			if(startTime + 3.00 < Time.time){
				WarmupText.waveComplete = false;
				WarmupText2[1].waveComplete = false;
				countDown -= Time.deltaTime;
				WarmupText.warmupGui = countDown;
				WarmupText2[1].warmupGui = countDown;
			}

			WarmupGUIText1.material.color = tempColor; 
			WarmupGUIText2.material.color = tempColor2; 

			//start spawning NPCs for this wave
			if(countDown <= 0.0f){
				if(waveStartFx && !fxPlayed){
					for(int i = 0; i < NpcSpawners.Count; i++){
						NpcSpawners[i].pauseSpawning = false;
					}
	
					WarmupText.waveBegins = true;
					WarmupText2[1].waveBegins = true;

					fxPlayed = true;
					fxPlayed2 = false;
					asource.PlayOneShot(waveStartFx, 1.0f);
				}
			}

			if(countDown <= -2.75f){
				StartCoroutine(FadeWarmupText());
				fxPlayed = false;
				yield break;
			}

			yield return null;

		}

	}

	IEnumerator FadeWarmupText(){

		while(true){

			tempColor.a -= Time.deltaTime;
			tempColor2.a -= Time.deltaTime;
			WarmupText.verticalOffsetAmt -= Time.deltaTime * 0.05f;
			WarmupText2[1].verticalOffsetAmt -= Time.deltaTime * 0.05f;
			WarmupGUIText1.pixelOffset = new Vector2 (Screen.width * WarmupText.horizontalOffsetAmt, Screen.height * WarmupText.verticalOffsetAmt);
			WarmupGUIText2.pixelOffset = new Vector2 (Screen.width * WarmupText2[1].horizontalOffsetAmt, Screen.height * WarmupText2[1].verticalOffsetAmt);
			
			WarmupGUIText1.material.color = tempColor; 
			WarmupGUIText2.material.color = tempColor2; 
			
			if(tempColor.a <= 0.0f && tempColor2.a <= 0.0f){
				yield break;
			}

			yield return null;
			
		}
	}

}
	