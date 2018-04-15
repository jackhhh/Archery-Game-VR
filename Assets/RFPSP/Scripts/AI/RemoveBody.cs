//RemoveBody.cs by Azuline Studios© All Rights Reserved
//Removes NPC ragdolls after bodyStayTime.
using UnityEngine;
using System.Collections;

public class RemoveBody : MonoBehaviour {
	private float startTime = 0;
	[HideInInspector]
	public float bodyStayTime = 15.0f;
	[Tooltip("Weapon item pickup that should be spawned after NPC dies (used for single capsule collider NPCs which instantiate ragdoll on death).")]
	public GameObject GunPickup;
	
	void Start (){
		startTime = Time.time;
	}
	
	void FixedUpdate (){
		if(startTime + bodyStayTime < Time.time){
			if(GunPickup){
				GunPickup.transform.parent = null;//unparent weapon pickup object so it won't be deleted
			}
			//drop arrows if corpse disappears
			ArrowObject[] arrows = gameObject.GetComponentsInChildren<ArrowObject>(true);
			foreach (ArrowObject arr in arrows) {
				arr.transform.parent = null;
				arr.myRigidbody.isKinematic = false;
				arr.myBoxCol.isTrigger = false;
				arr.gameObject.tag = "Usable";
				arr.falling = true;
			}
			Destroy(gameObject);
		}
	}

}