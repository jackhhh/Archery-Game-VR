//EdgeClimbTriggerActivate.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;
//Reactivate Vault Triggers when player is in position to use them because
//VaultTrigger.cs script deactivates its box collider/trigger to prevent
//player from falling down on it and hovering upwards. This script can
//be placed on the floor below the original Vault Trigger, or accross a
//gap that the player might jump over and vault up the opposite ledge
public class EdgeClimbTriggerActivate : MonoBehaviour {
	[Tooltip("The box collider to reactivate (set in inspector by dragging trigger object over this field).")]
	public BoxCollider triggerToActivate;
	
	void OnTriggerEnter ( Collider other  ){
		//on start of a collision with this trigger, reactivate triggerToActivate collider
		triggerToActivate.enabled = true;
	}
}

