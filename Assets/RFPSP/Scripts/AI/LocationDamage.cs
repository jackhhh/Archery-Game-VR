//LocationDamage.cs by Azuline Studios© All Rights Reserved
//Applies damage to specific NPC body part colliders 
using UnityEngine;
using System.Collections;

public class LocationDamage : MonoBehaviour {
	[Tooltip("Set to Ai.cs component in main NPC object (drag main object from hierachry window into this field).")]
	public AI AIComponent;
	[Tooltip("Amount to increase or decrease base damage of weapon hit on this collider (increase for head shots, decrease for limb hits).")]
	public float damageMultiplier = 1f;
	[Tooltip("Amount of physics force to apply with weapon hit on this collider.")]
	public float damageForce = 2.75f;
	[Tooltip("Sound effect to use for a hit on this collider (doesn't have to be a head shot).")]
	public AudioClip headShot;
	private bool headShotState;
	[Tooltip("Chance between 0 and 1 that killing an NPC with a shot to this collider will trigger slow motion for a few seconds.")]
	[Range(0.0f, 1.0f)]
	public float sloMoKillChance = 0.0f;
	[Tooltip("Duration of slow motion time in seconds if slo mo kill chance check is successful.")]
	public float sloMoTime = 0.9f;

	private Transform myTransform;
	private Rigidbody thisRigidBody;
	
	void OnEnable (){
		myTransform = transform;
		headShotState = false;
		thisRigidBody = myTransform.GetComponent<Rigidbody>();
		Mathf.Clamp01(sloMoKillChance);
	}
	
	//damage NPC
	public void ApplyDamage ( float damage, Vector3 attackDir, Vector3 attackerPos, Transform attacker, bool isPlayer, bool isExplosion  ){
		if(AIComponent && AIComponent.CharacterDamageComponent){
			if(isPlayer){//if attack is from player, pass damage info to main CharacterDamage.cs component
			
				AIComponent.CharacterDamageComponent.ApplyDamage(damage * damageMultiplier, attackDir, attackerPos, attacker, isPlayer, isExplosion, thisRigidBody, damageForce);
				
				if (headShot 
				&& !headShotState 
				&& AIComponent.CharacterDamageComponent.hitPoints <= 0.0f
				&& Vector3.Distance(myTransform.position, attackerPos) < 15f 
				&& !isExplosion){
					PlayAudioAtPos.PlayClipAt(headShot, myTransform.position, 0.6f, 0f);
					if(sloMoKillChance >= Random.value && isPlayer){
						AIComponent.PlayerWeaponsComponent.FPSPlayerComponent.StartCoroutine(AIComponent.PlayerWeaponsComponent.FPSPlayerComponent.ActivateBulletTime(sloMoTime));
					}
					headShotState = true;//only play head shot sound for one shot, so shotguns won't play many sound effects at once for each hit
				}
				
			}else{//attack is not from player, pass damage info to main CharacterDamage.cs component
				AIComponent.CharacterDamageComponent.ApplyDamage(damage, attackDir, attackerPos, attacker, isPlayer, isExplosion, thisRigidBody, damageForce);
			}
		}else{
			//body part collider hit without reference to its main AI.cs component
			Debug.Log("<color=red>LocationDamage.cs:</color> NPC body part hit without reference to its main AI.cs script component, please set reference in inspector.");
		}
	}
	
	void OnCollisionEnter(Collision hit){
		if(AIComponent.enabled){
			LocationDamage hitLocationDamage = hit.collider.GetComponent<LocationDamage>();
			if(hitLocationDamage){
				if(!hitLocationDamage.AIComponent.enabled){
					Physics.IgnoreCollision(hit.collider, myTransform.GetComponent<Collider>(), true);
				}
			}
		}
	}

}