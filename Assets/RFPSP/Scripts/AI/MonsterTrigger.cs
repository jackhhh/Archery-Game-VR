//MonsterTrigger.cs by Azuline Studios© All Rights Reserved
//used to spawn NPCs after player enters trigger to set up traps and ambushes
using UnityEngine;
using System.Collections;

public class MonsterTrigger : MonoBehaviour {
	[Tooltip("NPC objects to deactivate on level load and activate when player walks into trigger.")]
	public GameObject[] npcsToTrigger;
	
	void Start () {
		//deactivate the npcs in the npcsToTrigger array on start up
		for (int i = 0; i < npcsToTrigger.Length; i++){
			if(npcsToTrigger[i]){
				npcsToTrigger[i].SetActive(false);
			}
		}
	}

	void OnTriggerEnter ( Collider col  ){
		if(col.gameObject.tag == "Player"){
			//activate the npcs in the npcsToTrigger array when object is picked up/used by player
			for (int i = 0; i < npcsToTrigger.Length; i++){
				if(npcsToTrigger[i]){
					npcsToTrigger[i].SetActive(true);
				}
			}			
			Destroy(transform.gameObject);
		}
	}
}
