//MineExplosion.cs by Azuline Studios© All Rights Reserved
//setup and detonation for landmine object
using UnityEngine;
using System.Collections;

public class MineExplosion : MonoBehaviour {
	private WeaponEffects WeaponEffectsComponent;
	[Tooltip("Damage dealt by explosion.")]
	public float explosionDamage = 200;
	[Tooltip("Delay before this object applies explosion force and damage to other objects;.")]
	public float damageDelay = 0.2f;
	[Tooltip("Explosive physics force applied to objects in blast radius.")]
	public float blastForce = 15.0f;
	[Tooltip("Explosion sound effect.")]
	public AudioClip explosionFX;
	[Tooltip("mine trigger sound effect.")]
	public AudioClip beepFx;
	[Tooltip("Radius of explosion.")]
	private float radius;
	[Tooltip("True if object is the mine detection radius object (used only for triggering mine, not explosion effects).")]
	public bool isRadiusCollider;
	[Tooltip("Layers that will be hit by explosion.")]
	public LayerMask blastMask;//should be layers 9,10,11,12,13
	[Tooltip("Layers that mine will auto-align angles to surface on scene start.")]
	public LayerMask initPosMask;//should be layer 10
	private Transform myTransform;
	private bool audioPlayed;
	private bool triggered;//true when player steps on mine to allow instant damage, otherwise, wait for damage delay 
	private bool detonated;
	private bool inPosition;
	private RaycastHit hit;
	private RaycastHit hitInit;
	private Vector3 explodePos;
	//private WorldRecenter WorldRecenterComponent;
	
	void Start (){
		WeaponEffectsComponent = Camera.main.GetComponent<CameraControl>().playerObj.GetComponent<FPSPlayer>().WeaponEffectsComponent;
		myTransform = transform;
		//WorldRecenterComponent = Camera.main.GetComponent<CameraControl>().playerObj.transform.GetComponent<WorldRecenter>();
		if(!isRadiusCollider){
			//get radius value from sphere collider radius of child radius object
			radius = myTransform.GetComponentInChildren<SphereCollider>().radius;
			AlignToGround();//automatically align radius collider object's angles and position with the ground underneath it
		}
	}
	
	void AlignToGround (){
		//automatically align radius collider object's angles and position with the ground underneath it
		if (Physics.Raycast(myTransform.position, -transform.up, out hitInit, 2.0f, initPosMask.value)) {
			myTransform.rotation = Quaternion.FromToRotation(Vector3.up, hitInit.normal);
            myTransform.position = hitInit.point;
		}
	}
	
	//if object enters mine detection radius of radius object, detonate mine
	void OnTriggerEnter ( Collider col ){
		if(isRadiusCollider){
			//if(WorldRecenterComponent != null && (WorldRecenterComponent.worldRecenterTime + (0.3f * Time.timeScale)) < Time.time){//prevent mine triggers briefly when world recenters
				if((!col.isTrigger && (col.gameObject.layer == 11 || col.gameObject.layer == 13 || col.attachedRigidbody != null)) && !detonated){//only detonate if player, thrown object, or NPC enters trigger
					detonated = true;
					myTransform.parent.transform.GetComponent<MineExplosion>().triggered = true;
					myTransform.parent.transform.GetComponent<MineExplosion>().detonated = true;
					myTransform.GetComponent<SphereCollider>().enabled = false;
					myTransform.parent.transform.GetComponent<MineExplosion>().StartCoroutine("Detonate");
				}
			//}
		}
	}
	
	IEnumerator DetectDestroyed (){		
		while(true){
			if(!isRadiusCollider && audioPlayed){//destroy object after sound has finished playing
				if(!GetComponent<AudioSource>().isPlaying){
					//prevent attached hitmarks from being destroyed with game object
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
					Destroy(myTransform.gameObject);	
					yield break;
				}
			}
			yield return new WaitForSeconds(0.5f);
		}
	}
	
	//find objects in blast radius, apply damage and physics force, and remove mine object
    IEnumerator Detonate() {

		if(triggered){//if player stepped on mine, play beep sound effect and detonate after damageDelay
			if(beepFx){
				GetComponent<AudioSource>().clip = beepFx;
				GetComponent<AudioSource>().Play();
			}
			yield return new WaitForSeconds(damageDelay);
		}
		
		//play explosion effects
		WeaponEffectsComponent.ExplosionEffect(myTransform.position);
		
		myTransform.GetComponent<MeshRenderer>().enabled = false;
		GetComponent<AudioSource>().clip = explosionFX;
		GetComponent<AudioSource>().pitch = Random.Range(0.75f * Time.timeScale, 1.0f * Time.timeScale);//add slight random value to sound pitch for variety
		GetComponent<AudioSource>().Play();
		audioPlayed = true;
		
		if(!triggered){//unless player stepped on mine, apply explosion damage and force after damageDelay
			yield return new WaitForSeconds(damageDelay);
		}
		
        //find surrounding objects to be damaged by explosion
		Collider[] hitColliders = Physics.OverlapSphere(myTransform.position, radius * 1.5f, blastMask);
		//apply damage and force to surrounding objects
		for(int i = 0; i < hitColliders.Length; i++){
			Transform hitTransform = hitColliders[i].transform;
			if((hitTransform != myTransform)//do not call ApplyDamage on this mine object
			//don't damage or apply force to object if it is shielded/hidden from blast by other object
			&& (Physics.Linecast(hitTransform.position, myTransform.position, out hit, blastMask) 
			&& hit.collider == myTransform.GetComponent<Collider>())){
				
				//call ApplyDamage() function of objects in radius	
				switch(hitColliders[i].GetComponent<Collider>().gameObject.layer){
					case 0://hit object is a breakable or explosive object
						if(hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<BreakableObject>()){
							hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<BreakableObject>().ApplyDamage(explosionDamage);
						}else if(hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<ExplosiveObject>()){
							hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<ExplosiveObject>().ApplyDamage(explosionDamage);
						}else if(hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<MineExplosion>()){
							hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<MineExplosion>().ApplyDamage(explosionDamage);
						}else if(hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<AppleFall>()){
							hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<AppleFall>().ApplyDamage(explosionDamage);
						}	
						break;
					case 11://hit object is player
						if(hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<FPSPlayer>()){
							hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<FPSPlayer>().ApplyDamage(explosionDamage);
						}	
						break;
					case 13://hit object is an NPC
						if(hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<CharacterDamage>()){
							hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<CharacterDamage>().ApplyDamage(explosionDamage, Vector3.zero, myTransform.position, null, false, true);
						}
						if(hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<LocationDamage>()){
							hitColliders[i].GetComponent<Collider>().gameObject.GetComponent<LocationDamage>().ApplyDamage(explosionDamage, Vector3.zero, myTransform.position, null, false, true);
						}
					break;
					default:
						break;	
				}
				
				//apply explosion force
				if (hitTransform.GetComponent<Rigidbody>()){
                	hitTransform.GetComponent<Rigidbody>().AddExplosionForce(blastForce * hitTransform.GetComponent<Rigidbody>().mass, myTransform.position, radius, 3.0F, ForceMode.Impulse);
				}
			}
			
			if(i < hitColliders.Length - 1){
				continue;
			}else{//if all objects have been damaged by blast, disable collider (it was needed for the line cast above)
				myTransform.GetComponent<BoxCollider>().enabled = false;
			}
		}
    }
	
	//if mine object is shot or damaged by nearby explosion, detonate mine
	public void ApplyDamage( float damage ){
		if(!isRadiusCollider && !detonated){
			detonated = true;//this line must be before call to Detonate() otherwise stack overflow occurs
			myTransform.GetComponentInChildren<SphereCollider>().enabled = false;
			StartCoroutine(Detonate());
			StartCoroutine(DetectDestroyed());
		}
	}
	
}