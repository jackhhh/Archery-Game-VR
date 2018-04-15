//NPCAttack.cs by Azuline Studios© All Rights Reserved
//Manages timers for enemy attacks, damage application to other objects, and sound effects.
using UnityEngine;
using System.Collections;

public class NPCAttack : MonoBehaviour {
	private AI AIComponent;
	private	WeaponEffects WeaponEffectsComponent;
	private FPSRigidBodyWalker FPSWalker;
	private GameObject playerObj;

	private Transform myTransform;
	private Vector3 targetPos;
	[Tooltip("Maximum range of NPC attack.")]
	public float range = 100.0f;
	[Tooltip("Random range in units around target that NPC attack will hit (so NPCs won't have perfect aim).")]
	public float inaccuracy = 0.5f;
	[Tooltip("Fire rate of NPC attack.")]
	public float fireRate = 0.097f;
	[Tooltip("Number of attacks to fire in quick succession during NPC attack (for automatic weapons).")]
	public int burstShots = 0;
	[Tooltip("Maximum number of random shots to add to end of attack (so automatic weapons will fire different number of bullets per attack).")]
	public int randomShots = 0;
	[Tooltip("Physics force to apply to collider hit by NPC attack.")]
	public float force = 20.0f;
	[Tooltip("Damage to inflict per NPC attack.")]
	public float damage = 10.0f;
	[Tooltip("True if this is a melee attack (so actions like blocking can identify attack type).")]
	public bool isMeleeAttack;
	private float damageAmt;
//	public int bulletsPerClip = 50;
//	public int ammo = 150;
//	public float reloadTime = 1.75f;
	
	private bool doneShooting = true;
	private int shotsFired;
	private bool randBurstState;
	private int randShotsAmt;
	private bool  shooting = false;
	private bool  reloading = false;
	private bool  mFlashState = false;
	//private bool  noAmmoState = false;

	private Vector3 rayOrigin;
	private Vector3 targetDir;
	private RaycastHit[] hits;
	private bool hitObject;

	[Tooltip("Muzzle flash object to display during NPC attacks.")]
	public Renderer muzzleFlash;
	private Transform muzzleFlashTransform;
	
	[Tooltip("Sound effect for NPC attack.")]
	public AudioClip firesnd;
	[Tooltip("Random value to add to NPC attack sound for variety.")]
	public float fireFxRandPitch = 0.86f;
	private AudioSource aSource;
//	public AudioClip reloadsnd;
//	public AudioClip noammosnd;
	
//	private int bulletsLeft = 0;
	
	private float shootStartTime = 0.0f;
	
	void OnEnable (){
		
		myTransform = transform;
		if (muzzleFlash){
			muzzleFlashTransform = muzzleFlash.transform;
		}
		AIComponent = myTransform.GetComponent<AI>();
		WeaponEffectsComponent = AIComponent.playerObj.GetComponent<FPSPlayer>().weaponObj.GetComponent<WeaponEffects>();
		playerObj = Camera.main.transform.GetComponent<CameraControl>().playerObj;
		FPSWalker = playerObj.GetComponent<FPSRigidBodyWalker>();
		aSource = GetComponent<AudioSource>();

		//bulletsLeft = bulletsPerClip;
		shootStartTime = -fireRate * 2;
	}
	
	void Update (){
	
		if(shootStartTime + fireRate < Time.time){ 
			shooting = false;
		}
		
		//fire more shots per attack if burstShots or randomShots is greater than zero
		if(!doneShooting && shotsFired < (burstShots + randShotsAmt) 
		   && (AIComponent.target == AIComponent.playerTransform || AIComponent.target == AIComponent.FPSWalker.leanObj.transform || (AIComponent.TargetAIComponent && AIComponent.TargetAIComponent.enabled))){
			Fire();	
			if(!randBurstState){//get random number of shots to add to this burst for variation
				randShotsAmt = Random.Range(0,randomShots);
				randBurstState = true;
			}
		}else{//reset burst shooting vars
			doneShooting = true;
			shotsFired = 0;
			randBurstState = false;
		}
		
	}
	
	void LateUpdate (){
	
		//enable muzzle flash
		if (muzzleFlash){
			if(Time.time - shootStartTime < fireRate * 0.33){ 
				if(mFlashState){
					muzzleFlash.enabled = true;
					mFlashState = false;
				}
			}else{
				if(!mFlashState){
					// We didn't, disable the muzzle flash
					muzzleFlash.enabled = false;
				}
			}
		}
	
	}
	
	public void Fire (){
	
//		if (bulletsLeft == 0){
//			return;
//		}
		
		//fire weapon
		if(!reloading){
			if(!shooting){
				FireOneShot();
				shootStartTime = Time.time;
				shooting = true;
				doneShooting = false;
			}else{
				if(shootStartTime + fireRate < Time.time){ 
					shooting = false;
				}
			}
		}else{
			shooting = false;
		}
	
	}
	
	void FireOneShot (){
		RaycastHit hit;
		if(AIComponent.TargetAIComponent){
			if(Vector3.Distance(myTransform.position, AIComponent.lastVisibleTargetPosition) > 2.5f){
				targetPos = new Vector3(AIComponent.lastVisibleTargetPosition.x + Random.Range(-inaccuracy, inaccuracy), 
										AIComponent.lastVisibleTargetPosition.y + Random.Range(-inaccuracy, inaccuracy), 
										AIComponent.lastVisibleTargetPosition.z + Random.Range(-inaccuracy, inaccuracy));
			}else{
				targetPos = new Vector3(AIComponent.lastVisibleTargetPosition.x, 
		                                AIComponent.lastVisibleTargetPosition.y, 
		                                AIComponent.lastVisibleTargetPosition.z);
			}
		}else{
			if(Vector3.Distance(myTransform.position, AIComponent.lastVisibleTargetPosition) > 2.5f){//only calculate inaccuracy if target is not in close range
				if(FPSWalker.crouched || FPSWalker.prone){
					targetPos = new Vector3(AIComponent.lastVisibleTargetPosition.x + Random.Range(-inaccuracy, inaccuracy), 
					                        AIComponent.lastVisibleTargetPosition.y + Random.Range(-inaccuracy, inaccuracy), 
					                        AIComponent.lastVisibleTargetPosition.z + Random.Range(-inaccuracy, inaccuracy));
				}else{
					targetPos = new Vector3(AIComponent.lastVisibleTargetPosition.x + Random.Range(-inaccuracy, inaccuracy), 
					                        AIComponent.lastVisibleTargetPosition.y + Random.Range(-inaccuracy, inaccuracy), 
					                        AIComponent.lastVisibleTargetPosition.z + Random.Range(-inaccuracy, inaccuracy))
						+ (AIComponent.playerObj.transform.up * AIComponent.targetEyeHeight);
				}
			}else{
				targetPos = new Vector3(AIComponent.lastVisibleTargetPosition.x, 
				                        AIComponent.lastVisibleTargetPosition.y, 
				                        AIComponent.lastVisibleTargetPosition.z);
			}
		}

		rayOrigin = new Vector3(myTransform.position.x, myTransform.position.y + AIComponent.eyeHeight, myTransform.position.z);
		targetDir = (targetPos - rayOrigin).normalized;
		if (muzzleFlashTransform){
			WeaponEffectsComponent.BulletTracers(targetDir, muzzleFlashTransform.position, 0f, 0f, false);
		}

		hitObject = false;
		hit = new RaycastHit();
		
		// Did we hit anything?
		hits = Physics.RaycastAll (rayOrigin, targetDir, Vector3.Distance(rayOrigin, targetPos), AIComponent.searchMask);
		
		//check if NPC can see their target
		for(int i = 0; i < hits.Length; i++){
			if(!hits[i].transform.IsChildOf(myTransform) ){
				hitObject = true;
				hit = hits[i];
				break;
			}
		}
		
		if (hitObject) {
			// Apply a force to the rigidbody we hit
			if (hit.rigidbody){
				hit.rigidbody.AddForceAtPosition(force * targetDir / (Time.fixedDeltaTime * 100.0f), hit.point);
			}
		
			if(hit.collider){
				//draw impact effects where the weapon hit
				if(hit.collider.gameObject.layer != 20 && hit.collider.gameObject.layer != 11){
					if(!isMeleeAttack){
						WeaponEffectsComponent.ImpactEffects(hit.collider, hit.point, true, false, hit.normal);
					}else{
						WeaponEffectsComponent.ImpactEffects(hit.collider, hit.point, true, true, hit.normal);
					}
				}
				
				//calculate damage amount
				damageAmt = Random.Range(damage, damage + damage);	
				
				//call the ApplyDamage() function in the script of the object hit
				switch(hit.collider.gameObject.layer){
					case 0://hit object is an apple
						if(hit.collider.gameObject.GetComponent<AppleFall>()){
							hit.collider.gameObject.GetComponent<AppleFall>().ApplyDamage(damageAmt);
						}	
						//hit object is a breakable or explosive object
						if(hit.collider.gameObject.GetComponent<BreakableObject>()){
							hit.collider.gameObject.GetComponent<BreakableObject>().ApplyDamage(damageAmt);
						}else if(hit.collider.gameObject.GetComponent<ExplosiveObject>()){
							hit.collider.gameObject.GetComponent<ExplosiveObject>().ApplyDamage(damageAmt);
						}else if(hit.collider.gameObject.GetComponent<MineExplosion>()){
							hit.collider.gameObject.GetComponent<MineExplosion>().ApplyDamage(damageAmt);
						}
						break;
					case 13://hit object is an NPC
						if(hit.collider.gameObject.GetComponent<CharacterDamage>()){
							hit.collider.gameObject.GetComponent<CharacterDamage>().ApplyDamage(damageAmt, Vector3.zero, myTransform.position, myTransform, false, false);
						}
						if(hit.collider.gameObject.GetComponent<LocationDamage>()){
							hit.collider.gameObject.GetComponent<LocationDamage>().ApplyDamage(damageAmt, Vector3.zero, myTransform.position, myTransform, false, false);
						}
						break;
					case 11://hit object is player
						if(hit.collider.gameObject.GetComponent<FPSPlayer>()){
							hit.collider.gameObject.GetComponent<FPSPlayer>().ApplyDamage(damageAmt, myTransform, isMeleeAttack);
						}	
						if(hit.collider.gameObject.GetComponent<LeanColliderDamage>()){
							hit.collider.gameObject.GetComponent<LeanColliderDamage>().ApplyDamage(damageAmt);
						}	
						break;
					case 20://hit object is player lean collider
						if(hit.collider.gameObject.GetComponent<FPSPlayer>()){
							hit.collider.gameObject.GetComponent<FPSPlayer>().ApplyDamage(damageAmt, myTransform, isMeleeAttack);
						}	
						if(hit.collider.gameObject.GetComponent<LeanColliderDamage>()){
							hit.collider.gameObject.GetComponent<LeanColliderDamage>().ApplyDamage(damageAmt);
						}	
						break;
					default:
						break;	
				}
			}
			
		}

		aSource.clip = firesnd;
		aSource.pitch = Random.Range(fireFxRandPitch, 1);
		if(aSource.volume > 0){
			aSource.PlayOneShot(aSource.clip, 0.8f / aSource.volume);
		}
		
		//track ammo and fired shots amount
		//bulletsLeft--;
		shotsFired++;

		mFlashState=true;
		enabled = true;
		
		// Reload gun in reload Time		
//		if (bulletsLeft == 0){
//			Reload();	
//		}
		
	}
	
//	IEnumerator Reload (){
//		
//		if(ammo > 0){
//			audio.volume = 1.0f;
//			audio.pitch = 1.0f;
//			audio.PlayOneShot(reloadsnd, 1.0f / audio.volume);
//			
//			reloading = true;
//			// Wait for reload time first, then proceed
//			yield return new WaitForSeconds(reloadTime);
//			//set reloading var in ironsights script to true after reloading delay
//			reloading = false;
//	
//			// We have ammo left to reload
//			if(ammo >= bulletsPerClip){
//				ammo -= bulletsPerClip - bulletsLeft;
//				bulletsLeft = bulletsPerClip;
//			}else{
//				bulletsLeft += ammo;
//				ammo = 0;
//			}
//		}
//		
//	}
	
//	private int GetBulletsLeft(){
//		return bulletsLeft;
//	}
}