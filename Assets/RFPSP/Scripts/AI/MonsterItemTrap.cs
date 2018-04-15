//MonsterItemTrap.cs by Azuline Studios© All Rights Reserved
//used to spawn NPCs after player picks up an item to set up traps and ambushes
using UnityEngine;
using System.Collections;

public class MonsterItemTrap : MonoBehaviour {
	[Tooltip("NPC objects to deactivate on level load and activate when player picks up this item.")]
	public GameObject[] npcsToTrigger;
	
	void Start () {
		//deactivate the npcs in the npcsToTrigger array on start up
		for (int i = 0; i < npcsToTrigger.Length; i++){
			if(npcsToTrigger[i]){
				npcsToTrigger[i].SetActive(false);
			}
		}
	}
	
	//ActivateObject is called by every object with the "Usable" tag that the player activates/picks up by pressing the use key
	void ActivateObject () {
		//activate the npcs in the npcsToTrigger array when object is picked up/used by player
		for(int i = 0; i < npcsToTrigger.Length; i++){
			if(npcsToTrigger[i]){
				npcsToTrigger[i].SetActive(true);
			}
		}
	}
}
