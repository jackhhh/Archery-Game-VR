//CharacterDamage.cs by Azuline Studios© All Rights Reserved
//Applies damage to NPCs 
using UnityEngine;
using System.Collections;

public class CharacterDamage : MonoBehaviour {
	private AI AIComponent;
	private RemoveBody RemoveBodyComponent;
	[Tooltip("Number of hitpoints for this character or body part.")]
	public float hitPoints = 100.0f;
	private float initialHitPoints;
	[Tooltip("Force to apply to this collider when NPC is killed.")]
	public float attackForce = 2.75f;
	[Tooltip("Item to spawn when NPC is killed.")]
	public GameObject gunItem;
	[Tooltip("Weapon mesh to hide when NPC dies (replaced with usable gun item).")]
	public Transform gunObj;
	private GameObject gunInst;
	
	private Rigidbody[] bodies;
	[Tooltip("True if ragdoll mode is active for this NPC.")]
	public bool ragdollActive;
	private bool ragdollState;
	
	[Tooltip("If NPC only has one capsule collider for hit detection, replace the NPC's character mesh with a ragdoll, instead of transitioning instantly to ragdoll.")]
	public Transform deadReplacement;
	private Transform dead;
	[Tooltip("Sound effect to play when NPC dies.")]
	public AudioClip dieSound;
	[Tooltip("Determine if this object or parent should be removed on death. This is to allow for different hit detection collider types as children of NPC parent.")]
	public bool notParent;
	[Tooltip("Should this NPC's body be removed after Body Stay Time?")]
	public bool  removeBody;
	[Tooltip("Time for body to stay in the scene before it is removed.")]
	public float bodyStayTime = 15.0f;
	[Tooltip("Time for dropped weapon item to stay in scene before it is removed.")]
	public float gunStayTime = -1f;
	
	[Tooltip("Chance between 0 and 1 that death of this NPC will trigger slow motion for a few seconds (regardless of the body part hit).")]
	[Range(0.0f, 1.0f)]
	public float sloMoDeathChance = 0.0f;
	[Tooltip("True if backstabbing this NPC should trigger slow motion for the duration of slo mo backstab time.")]
	public bool sloMoBackstab = true;
	[Tooltip("Duration of slow motion time in seconds if slo mo death chance check is successful.")]
	public float sloMoDeathTime = 0.9f;
	[Tooltip("Duration of slow motion time in seconds if this NPC is backstabbed.")]
	public float sloMoBackstabTime = 0.9f;
	
	//vars related to attacker position (for physics, and other effects)
	private Vector3 attackerPos2;
	private Vector3 attackDir2;
	private Transform myTransform;
	private bool explosionCheck;
	private LayerMask raymask = 1 << 13;

	void OnEnable (){
		myTransform = transform;
		RemoveBodyComponent = GetComponent<RemoveBody>();
		Mathf.Clamp01(sloMoDeathChance);
		if(removeBody && RemoveBodyComponent){//remove body timer starts if RwmoveBody.cs script is enabled
			RemoveBodyComponent.enabled = false;
		}
		if(!AIComponent){
			AIComponent = myTransform.GetComponent<AI>();
		}
		initialHitPoints = hitPoints;
		bodies = GetComponentsInChildren<Rigidbody>();
	}
	
	void Update () {
		if(bodies.Length > 1){
			if(!ragdollActive){//deactivate ragdoll mode
				if(!ragdollState){
					foreach(Rigidbody rb in bodies){
						rb.isKinematic = true;
					}
					if(!AIComponent.useMecanim){
						AIComponent.AnimationComponent.enabled = true;
						AIComponent.AnimationComponent.Play("idle");
					}else{
						AIComponent.AnimatorComponent.enabled = true;
					}
					if(gunObj){
						gunObj.gameObject.SetActive(true);
						if(gunInst){
							Destroy(gunInst);
						}
					}
					ragdollState = true;
				}
			}else{//activate ragdoll mode
				if(ragdollState){
					if(!AIComponent.useMecanim){
						AIComponent.AnimationComponent.Stop();
						AIComponent.AnimationComponent.enabled = false;
					}else{
						AIComponent.AnimatorComponent.enabled = false;
					}
					foreach(Collider col in AIComponent.colliders){
						foreach(Collider col2 in AIComponent.colliders){
							Physics.IgnoreCollision(col, col2, false);//ignore collisions with other body part colliders 
						}
						if(AIComponent.FPSWalker.gameObject.activeInHierarchy){
							Physics.IgnoreCollision(col, AIComponent.FPSWalker.capsule, true);//ignore collisions with player
						}
					}
					foreach(Rigidbody rb in bodies){
						rb.isKinematic = false;
					}
					if(gunObj){//spawn gun pickup object
						gunObj.gameObject.SetActive(false);
						gunInst = Instantiate(gunItem, gunObj.position, gunObj.rotation) as GameObject;
						gunInst.transform.Rotate(0f, 180f, 180f);
						Vector3 tempGunpos = gunInst.transform.position + (transform.forward * 0.35f);
						gunInst.transform.position = tempGunpos;
						if(gunStayTime > 0f && gunInst.GetComponent<WeaponPickup>()){
							gunInst.GetComponent<WeaponPickup>().StartCoroutine(gunInst.GetComponent<WeaponPickup>().DestroyWeapon(gunStayTime));
						}
					}
					ragdollState = false;
				}
			}
		}
	}
	
	//damage NPC
	public void ApplyDamage ( float damage, Vector3 attackDir, Vector3 attackerPos, Transform attacker, bool isPlayer, bool isExplosion, Rigidbody hitBody = null, float bodyForce = 0f ){

		if (hitPoints <= 0.0f){
			return;
		}

		if(!AIComponent.damaged 
		&& !AIComponent.huntPlayer
		&& (((hitPoints / initialHitPoints) < 0.65f))//has NPC been damaged significantly?
		&& attacker
		&& !isExplosion
		){
			if(!isPlayer){
				if(attacker.GetComponent<AI>().factionNum != AIComponent.factionNum){
					AIComponent.target = attacker;
					AIComponent.TargetAIComponent = attacker.GetComponent<AI>();
					AIComponent.targetEyeHeight = AIComponent.TargetAIComponent.eyeHeight;
					AIComponent.damaged = true;
				}
			}else{
				if(!AIComponent.ignoreFriendlyFire){//go hostile on a friendly if they repeatedly attacked us
					AIComponent.target = AIComponent.playerObj.transform;
					AIComponent.targetEyeHeight = AIComponent.FPSWalker.capsule.height * 0.25f;
					AIComponent.playerAttacked = true;
					AIComponent.TargetAIComponent = null;
					AIComponent.damaged = true;
				}
			}
			
		}
		
		//prevent hitpoints from going into negative values
		if(hitPoints - damage > 0.0f){
			if(AIComponent.playerIsBehind && (AIComponent.PlayerWeaponsComponent.CurrentWeaponBehaviorComponent.meleeSwingDelay > 0 || AIComponent.PlayerWeaponsComponent.CurrentWeaponBehaviorComponent.meleeActive)){
				hitPoints -= damage * 32.0f;//backstab npc
				if(sloMoBackstab){
					AIComponent.PlayerWeaponsComponent.FPSPlayerComponent.StartCoroutine(AIComponent.PlayerWeaponsComponent.FPSPlayerComponent.ActivateBulletTime(sloMoBackstabTime));
				}
			}else{
				hitPoints -= damage;
			}
		}else{
			hitPoints = 0.0f;	
		}
		
		attackDir2 = attackDir;
		attackerPos2 = attackerPos;
		explosionCheck = isExplosion;
		
		//to expand enemy search radius if attacked to defend against sniping
		AIComponent.attackedTime = Time.time;
		
		//Kill NPC
		if (hitPoints <= 0.0f){
			AIComponent.vocalFx.Stop();
				
			if(sloMoDeathChance >= Random.value && isPlayer){
				AIComponent.PlayerWeaponsComponent.FPSPlayerComponent.StartCoroutine(AIComponent.PlayerWeaponsComponent.FPSPlayerComponent.ActivateBulletTime(sloMoDeathTime));
			}
			if(bodies.Length < 2){//if NPC is only using one capsule collider for collision, instantiate ragdoll, instead of activating existing body part rigidbodies
				Die();
			}else{
				if(!ragdollActive){
					RagDollDie(hitBody, bodyForce);
				}
			}
		}
	}
	
	//this method called if NPC has died and has more than one capsule collider for collision, so transition to ragdoll
	void RagDollDie(Rigidbody hitBody, float bodyForce) {
		if (dieSound){
			PlayAudioAtPos.PlayClipAt(dieSound, transform.position, 1.0f);
		}
		
		AIComponent.NPCRegistryComponent.UnregisterNPC(AIComponent);//unregister NPC from main NPC registry
		if(AIComponent.spawned && AIComponent.NPCSpawnerComponent){
			AIComponent.NPCSpawnerComponent.UnregisterSpawnedNPC(AIComponent);//unregister NPC from spawner registry
		}
		if(!AIComponent.useMecanim){
			AIComponent.AnimationComponent.enabled = false;
		}else{
			AIComponent.AnimatorComponent.enabled = false;
		}
		ragdollActive = true;
		if(AIComponent.NPCAttackComponent.muzzleFlash){
			AIComponent.NPCAttackComponent.muzzleFlash.enabled = false;
		}
		AIComponent.NPCAttackComponent.enabled = false;
		AIComponent.StopAllCoroutines();
		AIComponent.agent.enabled = false;
		AIComponent.enabled = false;
		StartCoroutine(ApplyForce(hitBody, bodyForce));
		//initialize the RemoveBody.cs script attached to the NPC ragdoll
		if(RemoveBodyComponent){
			if(removeBody){
				RemoveBodyComponent.enabled = true;
				RemoveBodyComponent.bodyStayTime = bodyStayTime;//pass bodyStayTime to RemoveBody.cs script
			}else{
				RemoveBodyComponent.enabled = false;
			}
		}
	}
	
	public IEnumerator ApplyForce (Rigidbody body, float force) {
		yield return new WaitForSeconds(0.02f);
		if(!explosionCheck){
			//apply damage force to the ragdoll rigidbody
			body.AddForce(attackDir2 * attackForce, ForceMode.Impulse);
		}else{
			//apply explosive damage force to the ragdoll rigidbodies
			foreach(Rigidbody rb in bodies) {
				rb.AddForce((myTransform.position - (attackerPos2 + (Vector3.up * -2.5f))).normalized * Random.Range(2.5f, 4.5f), ForceMode.Impulse);
			}
		}
	}
	
	//this method called if the NPC dies and only has one capsule collider for collision
	//which will be instantiated in place of the main NPC object (which is removed from the scene)
	void Die() {
		
		RaycastHit rayHit;
		// Play a dying audio clip
		if (dieSound){
			PlayAudioAtPos.PlayClipAt(dieSound, transform.position, 1.0f);
		}

		AIComponent.NPCRegistryComponent.UnregisterNPC(AIComponent);//unregister NPC from main NPC registry
		if(AIComponent.spawned && AIComponent.NPCSpawnerComponent){
			AIComponent.NPCSpawnerComponent.UnregisterSpawnedNPC(AIComponent);//unregister NPC from spawner registry
		}

		AIComponent.agent.Stop();
		AIComponent.StopAllCoroutines();
	
		// Replace NPC object with the dead body
		if (deadReplacement) {
			
			//drop arrows if corpse disappears
			ArrowObject[] arrows = gameObject.GetComponentsInChildren<ArrowObject>(true);
			foreach (ArrowObject arr in arrows) {
				arr.transform.parent = null;
				arr.myRigidbody.isKinematic = false;
				arr.myBoxCol.isTrigger = false;
				arr.gameObject.tag = "Usable";
				arr.falling = true;
			}
		
			dead = Instantiate(deadReplacement, transform.position, transform.rotation) as Transform;
			RemoveBodyComponent = dead.GetComponent<RemoveBody>();
	
			// Copy position & rotation from the old hierarchy into the dead replacement
			CopyTransformsRecurse(transform, dead);
			
			Collider[] colliders = dead.GetComponentsInChildren<Collider>();
			foreach(Collider col in colliders){
				Physics.IgnoreCollision(col, AIComponent.FPSWalker.capsule, true);
			}
			
			//apply damage force to NPC ragdoll
			if(Physics.SphereCast(attackerPos2, 0.2f, attackDir2, out rayHit, 750.0f, raymask)
			&& rayHit.rigidbody 
			&& attackDir2.x !=0){
				//apply damage force to the ragdoll rigidbody hit by the sphere cast (can be any body part)
				rayHit.rigidbody.AddForce(attackDir2 * 10.0f, ForceMode.Impulse);
			
			}else{//apply damage force to NPC ragdoll if being damaged by an explosive object or other damage source without a specified attack direction
			
				Component[] bodies;
				bodies = dead.GetComponentsInChildren<Rigidbody>();
				foreach(Rigidbody body in bodies) {
					if(explosionCheck){
						//if(body.transform.name == "Chest"){//only apply damage force to the chest of the ragdoll if damage is from non-player source 
							//calculate direction to apply damage force to ragdoll
							body.AddForce((myTransform.position - (attackerPos2 + (Vector3.up * -2.5f))).normalized * Random.Range(4.5f, 7.5f), ForceMode.Impulse);
						//}
					}else{
						if(body.transform.name == "Chest"){//only apply damage force to the chest of the ragdoll if damage is from non-player source 
							//calculate direction to apply damage force to ragdoll
							body.AddForce((myTransform.position - attackerPos2).normalized * 10.0f, ForceMode.Impulse);
						}
					}
				}
			}
			
			//initialize the RemoveBody.cs script attached to the NPC ragdoll
			if(RemoveBodyComponent){
				if(removeBody){
					RemoveBodyComponent.enabled = true;
					RemoveBodyComponent.bodyStayTime = bodyStayTime;//pass bodyStayTime to RemoveBody.cs script
				}else{
					RemoveBodyComponent.enabled = false;
				}
			}
			
			//Determine if this object or parent should be removed.
			//This is to allow for different hit detection collider types as children of NPC parent.
			if(notParent){
				Destroy(transform.parent.gameObject);
			}else{
				Destroy(transform.gameObject);
			}
			
		}
	
	}
	
	static void CopyTransformsRecurse ( Transform src , Transform dst ){
		dst.position = src.position;
		dst.rotation = src.rotation;
		
		foreach(Transform child in dst) {
			// Match the transform with the same name
			Transform curSrc = src.Find(child.name);
			if (curSrc)
				CopyTransformsRecurse(curSrc, child);
		}
	}
}