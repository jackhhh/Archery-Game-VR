//MoveTrigger.cs by Azuline Studios© All Rights Reserved
//Commands an NPC to move to a position when player enters trigger,
//and sets up next move trigger in sequence 
//(for NPCs that lead the player through the scene)
using UnityEngine;
using System.Collections;

public class MoveTrigger : MonoBehaviour {
	[Tooltip("The NPC to move when the Player enters this trigger.")]
	public AI npcToMove;
	[Tooltip("The position to move the NPC to.")]
	public Transform movePosition;
	[Tooltip("True if NPC should run to the move position, false if they should walk.")]
	public bool runToGoal;
	private bool moved;
	private bool rotated;
	[Tooltip("The next MoveTrigger.cs component to activate after this one.")]
	public MoveTrigger nextMoveTrigger;
	[Tooltip("True if this trigger should be active at scene start, instead of waiting to be activated by other MoveTrigger.cs components when NPC reaches goal.")]
	public bool isStartingTrigger;
	
	[Tooltip("Sound effects to play when NPC starts traveling to move position (following vocals).")]
	public AudioClip[] followSnds;
	[Tooltip("Volume of follow sound effects.")]
	public float followVol = 0.7f;
	
	void Start () {
		if(!isStartingTrigger){
			gameObject.SetActive(false);
		}
		if(npcToMove){
			npcToMove.leadPlayer = true;
			npcToMove.followOnUse = false;
		}
	}
	
	void OnTriggerStay (Collider col) {
		if(npcToMove 
		&& npcToMove.enabled
		&& col.gameObject.tag == "Player" 
		&& !moved 
		&& !npcToMove.followed){//don't trigger a new destination if NPC is already moving to one
			if(!runToGoal){
				npcToMove.GoToPosition(movePosition.position, false);
			}else{
				npcToMove.GoToPosition(movePosition.position, true);
			}
			
			if(followSnds.Length > 0){//play NPC leading vocal sound effects
				npcToMove.vocalFx.volume = followVol;
				npcToMove.vocalFx.pitch = Random.Range(0.94f, 1f);
				npcToMove.vocalFx.spatialBlend = 1.0f;
				npcToMove.vocalFx.clip = followSnds[Random.Range(0, followSnds.Length)];
				npcToMove.vocalFx.PlayOneShot(npcToMove.vocalFx.clip);
			}
			
			if(nextMoveTrigger){//reset/initialize next move trigger
				nextMoveTrigger.gameObject.SetActive(true);
				nextMoveTrigger.moved = false;
				nextMoveTrigger.rotated = false;
			}	
			npcToMove.followed = true;
			moved = true;
		}
	}

	void Update () {
		//detect if NPC reached destination
		if(moved && !rotated && Vector3.Distance(movePosition.position, npcToMove.myTransform.position) < npcToMove.pickNextDestDist){
			npcToMove.cancelRotate = false;
			npcToMove.StartCoroutine(npcToMove.RotateTowards(npcToMove.playerTransform.position, 10.0f, 2.0f, false));
			npcToMove.followed = false;
			gameObject.SetActive(false);
			rotated = true;
		}
	}
	
}
