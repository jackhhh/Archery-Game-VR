//DrinkPickup.cs by Azuline Studios© All Rights Reserved
//script for drink pickups
using UnityEngine;
using System.Collections;

public class DrinkPickup : MonoBehaviour {
	
	private GameObject playerObj;//the GameObject that is a child of FPS Weapons which has the WeaponBehavior script attatched
	private Transform myTransform;
	
	[Tooltip("True if this pickup should disappear when used/activated by player.")]
	public bool removeOnUse = true;
	
	[Tooltip("Sound to play when picking up this item.")]
	public AudioClip pickupSound;
	[Tooltip("Sound to play when thirst is zero and item cannot be used.")]
	public AudioClip fullSound;
	
	[Tooltip("Amount of thirst to remove when picking up this drink item.")]
	public int thirstToRemove = 15;
	[Tooltip("Amount of health to restore when picking up this drink item.")]
	public int healthToRestore = 5;
	
	private FPSPlayer FPSPlayerComponent;

	void Start () {
		myTransform = transform;//manually set transform for efficiency
		playerObj = Camera.main.transform.GetComponent<CameraControl>().playerObj;
		FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		Physics.IgnoreCollision(myTransform.GetComponent<Collider>(), FPSPlayerComponent.FPSWalkerComponent.capsule, true);
	}
	
	public void PickUpItem(){
		//if player is thirsty, apply thirstToRemove to thirstPoints
		if (FPSPlayerComponent.thirstPoints > 0.0f && FPSPlayerComponent.usePlayerThirst) {
			
			if(FPSPlayerComponent.thirstPoints - thirstToRemove > 0.0){
				FPSPlayerComponent.UpdateThirst(-thirstToRemove);
			}else{
				FPSPlayerComponent.UpdateThirst(-FPSPlayerComponent.thirstPoints);	
			}
			
			//restore player health by healthToRestore amount
			if(FPSPlayerComponent.hitPoints + healthToRestore < FPSPlayerComponent.maximumHitPoints){
				FPSPlayerComponent.HealPlayer(healthToRestore);	
			}else{
				FPSPlayerComponent.HealPlayer(FPSPlayerComponent.maximumHitPoints - FPSPlayerComponent.hitPoints);
			}
			
			//play pickup sound
			if(pickupSound){PlayAudioAtPos.PlayClipAt(pickupSound, myTransform.position, 0.75f);}
			
			if(removeOnUse){
				FreePooledObjects();
				//remove this drink pickup
				Object.Destroy(gameObject);
			}
		}else{
			//if player not thirsty, just play beep sound
			if(fullSound){PlayAudioAtPos.PlayClipAt(fullSound, myTransform.position, 0.75f);}	
		}
	}
	
	//return pooled objects back to object pool to prevent them from being destroyed when this object is destroyed after use
	private void FreePooledObjects(){
		FadeOutDecals[] decals = gameObject.GetComponentsInChildren<FadeOutDecals>(true);
		foreach (FadeOutDecals dec in decals) {
			dec.parentObjTransform.parent = AzuObjectPool.instance.transform;
			dec.parentObj.SetActive(false);
		}
		//drop arrows if object is destroyed
		ArrowObject[] arrows = gameObject.GetComponentsInChildren<ArrowObject>(true);
		foreach (ArrowObject arr in arrows) {
			arr.transform.parent = null;
			arr.myRigidbody.isKinematic = false;
			arr.myBoxCol.isTrigger = false;
			arr.gameObject.tag = "Usable";
			arr.falling = true;
		}
	}
	
}