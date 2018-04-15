//NPCSpawner.cs by Azuline Studios© All Rights Reserved
//Spawns NPCs, using several parameters to control spawn timing and amounts.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCSpawner : MonoBehaviour {

	[Tooltip("Set to the wave manager object if this spawner should be controled by the wave manager")]
	public WaveManager WaveManager;
	[Tooltip("If not linked to a wave manager to spawn NPC waves, this is the NPC prefab that will be spawned")]
	public GameObject NPCPrefab;
	[Tooltip("Delay until spawning next NPC from this spawner.")]
	public float spawnDelay = 30.0f;
	private float spawnTime;
	private GameObject NPCInstance = null;
	private List<AI> Npcs = new List<AI>(); 
	private float timeLeft;
	[Tooltip("The waypoint group that this NPC should patrol after spawning.")]
	public WaypointGroup waypointGroup;
	[Tooltip("The number of the waypoint in the waypoint group that should be traveled to first.")]
	public int firstWaypoint = 1;
	private AI AIcomponent;
	[Tooltip("True if spawner should continuously spawn NPCs.")]
	public bool unlimitedSpawning = true;
	[Tooltip("True if this NPC should hunt the player across the map")]
	public bool huntPlayer;
	[Tooltip("Maximuim number of NPCs from this spawner that can be active in the scene at a time.")]
	public int maxActiveNpcs = 3;
	[Tooltip("The number of NPCs to spawn if not spawning unlimited NPCs.")]
	public int NpcsToSpawn = 5;
	[HideInInspector]
	public int spawnedNpcAmt;
	[HideInInspector]
	public bool pauseSpawning;

	void Start (){
		spawnTime = -1024.0f;
	}

	void Update (){

		if(!pauseSpawning){

			if(Npcs.Count < maxActiveNpcs
			&& (unlimitedSpawning || (!unlimitedSpawning && spawnedNpcAmt < NpcsToSpawn))){
				if(spawnTime + spawnDelay < Time.time){
					Spawn(NPCPrefab);
				}
			}

		}

	}

	void Spawn (GameObject NpcPrefab){
		// Make an instance of the NPC
		if(NPCPrefab){
			NPCInstance = Instantiate(NpcPrefab,transform.position,transform.rotation) as GameObject;
			
			AI AIcomponent = NPCInstance.GetComponent<AI>();
			Npcs.Add(AIcomponent);
			
			AIcomponent.NPCSpawnerComponent = transform.GetComponent<NPCSpawner>();
			if(huntPlayer){
				AIcomponent.huntPlayer = true;
			}
			AIcomponent.spawned = true;
			AIcomponent.waypointGroup = waypointGroup;
			AIcomponent.firstWaypoint = firstWaypoint;
			AIcomponent.walkOnPatrol = false;
			AIcomponent.standWatch = false;
			AIcomponent.SpawnNPC();
			spawnTime = Time.time;
			spawnedNpcAmt ++;
		}
	}

	public void UnregisterSpawnedNPC(AI NpcAI){
		for(int i = 0; i < Npcs.Count; i++){
			if(Npcs[i] == NpcAI){
				Npcs.RemoveAt(i);
				if(WaveManager && WaveManager.enabled){
					WaveManager.killedNpcs ++;
					if(WaveManager.killedNpcs >= WaveManager.NpcsToSpawn){
						StartCoroutine(WaveManager.StartWave());
					}
				}
				break;
			}
		}
	}

}