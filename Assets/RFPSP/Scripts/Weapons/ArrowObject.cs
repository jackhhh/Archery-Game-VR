//ArrowObject.cs by Azuline Studios© All Rights Reserved
//Detects hits for flying arrows, sets up collision and arrow retrieval
using UnityEngine;
using System.Collections;

public class ArrowObject : MonoBehaviour {
	[HideInInspector]
	public Rigidbody myRigidbody;
	private MeshRenderer myMeshRenderer;
	[HideInInspector]
	public bool hit;
	[HideInInspector]
	public bool falling;
	[Tooltip("Base damage of arrow without damage add amount.")]
	public float damage = 50f;
	[Tooltip("Maximum additional damage of arrow to inflict with full strength pull.")]
	public float damageAdd = 20f;//how much to increase damage with full strength pull
	[Tooltip("Force to apply to rigidbody that is hit with arrow.")]
	public float force = 3f;
	[HideInInspector]
	public float damageAddAmt;
	private FPSPlayer FPSPlayerComponent;
	private AmmoPickup AmmoPickupComponent;
	private float hitTime;
	private float startTime;
	private bool startState;
	[Tooltip("Time that arrow object stays in scene after hitting object.")]
	public float waitDuration = 30f;
	[HideInInspector]
	public int objectPoolIndex;
	private Collider hitCol;
	[HideInInspector]
	public BoxCollider myBoxCol;
	private GameObject emptyObject;
	[Tooltip("Scale of the arrow object.")]
	public Vector3 scale;
	[Tooltip("Initial size of the arrow collider (increased after hit to make pick up easier).")]
	public Vector3 initialColSize;
	[Tooltip("True if helper gizmos for arrow object should be shown to assist setting script values.")]
	public bool drawHelperGizmos;
	public RaycastHit arrowRayHit;
	[Tooltip("Distance in front of arrow to check for hits (scaled up at higher velocities).")]
	public float hitCheckDist = 0.4f;
	[Tooltip("Layers that the arrow will collide with.")]
	public LayerMask rayMask;
	//set from WeaponBehavior.cs FireOneShot() function to scale forward hit detection raycast based on arrow release velocity
	//to large of a raycast distance at low velocities makes arrow "jump" forward to impact point
	[HideInInspector]
	public float velFactor;
	[HideInInspector]
	public float visibleDelay;

	void Start () {
		FPSPlayerComponent = Camera.main.transform.GetComponent<CameraControl>().playerObj.transform.GetComponent<FPSPlayer>();
	}
	
	public void InitializeProjectile(){
		hit = false;
		falling = false;
		AmmoPickupComponent = GetComponent<AmmoPickup>();
		AmmoPickupComponent.enabled = false;
		myRigidbody = GetComponent<Rigidbody>();
		myBoxCol = GetComponent<BoxCollider>();
		myMeshRenderer = GetComponent<MeshRenderer>();
		myMeshRenderer.enabled = false;
		//reset collider size to original, smaller arrow sized value for more realistic, narrower collisions
		myBoxCol.size = initialColSize;
		startTime = Time.time; 
		myRigidbody.isKinematic = false;
		myBoxCol.isTrigger = true;
		transform.gameObject.tag = "Untagged";//don't allow arrow to be grabbed in flight
		
		transform.parent = AzuObjectPool.instance.transform;
		transform.localScale = scale;
		DeleteEmptyObj();//delete the empty object used last time for this arrow (empty parent obj prevents arrow from inheriting hit collider's scale)
	}
	
	public void DeleteEmptyObj () {//delete empty parent object
		if(emptyObject != null){
			transform.parent = AzuObjectPool.instance.transform;
			DestroyImmediate(emptyObject);
		}
	}
	
	void OnDrawGizmos() {
		if(drawHelperGizmos){
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(transform.position, 0.04f);
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(transform.position + transform.forward * (hitCheckDist + ((hitCheckDist * 3f) * velFactor)), 0.04f);
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(arrowRayHit.point, 0.06f);
		}
	}

	void Update () {
		if(startTime + visibleDelay < Time.time){
			myMeshRenderer.enabled = true;//enable mesh renderer after delay to prevent arrow from blocking player view if near clip plane is small
		}
		if(!hit){
			//Check for arrow hit
			if(Physics.Raycast(transform.position, transform.forward, out arrowRayHit, hitCheckDist + ((hitCheckDist * 3f) * velFactor), rayMask, QueryTriggerInteraction.Ignore)){
				HitTarget();
			}
			if(!falling && myRigidbody.velocity.magnitude > 0.01f){
				transform.rotation = Quaternion.LookRotation(myRigidbody.velocity);//make arrow always point in direction of movement
			}
		}else{
			if(hitTime + waitDuration < Time.time){//wait duration has elapsed, recycle arrow object
				transform.parent = AzuObjectPool.instance.transform;
				DestroyImmediate(emptyObject);
				AzuObjectPool.instance.RecyclePooledObj(objectPoolIndex, transform.gameObject);
			}
			if(hitCol && !hitCol.enabled){//make arrow fall to ground if hit collider is removed or disabled
				myRigidbody.isKinematic = false;
				myBoxCol.isTrigger = false;
				transform.gameObject.tag = "Usable";
				falling = true;
			}
		}
	}
	
	void HitTarget(){	
		
		if(!hit 
		&& arrowRayHit.collider != FPSPlayerComponent.FPSWalkerComponent.capsule
		&& !arrowRayHit.collider.gameObject.GetComponent<ArrowObject>()){
		
			hitCol = arrowRayHit.collider;
			myRigidbody.isKinematic = true;
			transform.gameObject.tag = "Usable";//allow arroe to be picked up
			//increase collider size to make is easier to retrieve later
			myBoxCol.size = new Vector3(initialColSize.x * 2f, initialColSize.y * 2f, initialColSize.z);
			AmmoPickupComponent.enabled = true;
			transform.position = arrowRayHit.point;
					
			if(hitCol.GetComponent<Rigidbody>() || (hitCol.transform.parent != null && hitCol.transform.parent.GetComponent<Rigidbody>())){//or other moving objects?
				if(hitCol.GetComponent<Rigidbody>()){
					hitCol.GetComponent<Rigidbody>().AddForce(transform.forward * force, ForceMode.Impulse);
				}else if(hitCol.transform.parent != null && hitCol.transform.parent.GetComponent<Rigidbody>()){//do additional check for rigidbody on parent object if one not found on hit collider
					hitCol.transform.parent.GetComponent<Rigidbody>().AddForce(transform.forward * force, ForceMode.Impulse);				
				}
				//Create empty parent object for arrow object to prevent arrow from inheriting scale of hit collider if it is skewed or uneven
				emptyObject = new GameObject();
				emptyObject.transform.position = arrowRayHit.point;
				emptyObject.transform.rotation =  hitCol.transform.rotation;
				emptyObject.transform.parent = hitCol.transform;
				transform.parent = emptyObject.transform;
				//empty obj will be destroyed after waitDuration, but obj pool might reuse the prefab and leave the parent obj stranded
				Destroy(emptyObject.gameObject, waitDuration + 1f);
			}
			
			switch(hitCol.gameObject.layer){//apply damage to hit object
			case 0://hit object
				if(hitCol.gameObject.GetComponent<AppleFall>()){
					hitCol.gameObject.GetComponent<AppleFall>().ApplyDamage(damage + damageAddAmt);
					FPSPlayerComponent.UpdateHitTime();//used for hitmarker
				}else if(hitCol.gameObject.GetComponent<BreakableObject>()){
					hitCol.gameObject.GetComponent<BreakableObject>().ApplyDamage(damage + damageAddAmt);
					FPSPlayerComponent.UpdateHitTime();//used for hitmarker
				}else if(hitCol.gameObject.GetComponent<ExplosiveObject>()){
					hitCol.gameObject.GetComponent<ExplosiveObject>().ApplyDamage(damage + damageAddAmt);
					FPSPlayerComponent.UpdateHitTime();//used for hitmarker
				}else if(hitCol.gameObject.GetComponent<MineExplosion>()){
					hitCol.gameObject.GetComponent<MineExplosion>().ApplyDamage(damage + damageAddAmt);
					FPSPlayerComponent.UpdateHitTime();//used for hitmarker
				}
				break;
			case 1://hit object is an object with transparent effects like a window
				if(hitCol.gameObject.GetComponent<BreakableObject>()){
					hitCol.gameObject.GetComponent<BreakableObject>().ApplyDamage(damage + damageAddAmt);
					FPSPlayerComponent.UpdateHitTime();//used for hitmarker
				}	
				break;
			case 13://hit object is an NPC
				if(hitCol.gameObject.GetComponent<CharacterDamage>() && hitCol.gameObject.GetComponent<AI>().enabled){
					hitCol.gameObject.GetComponent<CharacterDamage>().ApplyDamage(damage + damageAddAmt, transform.forward, Camera.main.transform.position, transform, true, false);
					FPSPlayerComponent.UpdateHitTime();//used for hitmarker
				}
				if(hitCol.gameObject.GetComponent<LocationDamage>() && hitCol.gameObject.GetComponent<LocationDamage>().AIComponent.enabled){
					hitCol.gameObject.GetComponent<LocationDamage>().ApplyDamage(damage + damageAddAmt, transform.forward,  Camera.main.transform.position, transform, true, false);
					FPSPlayerComponent.UpdateHitTime();//used for hitmarker
				}
				//move arrow more towards center of collider to compensate for body part colliders that are slightly larger than character mesh (prevents floating, stuck arrows)
				transform.position = hitCol.transform.position - ((hitCol.transform.position - arrowRayHit.point).normalized * 0.15f);
				break;
			default:
				break;	
			}
			//draw impact effects where the weapon hit
			FPSPlayerComponent.WeaponEffectsComponent.ImpactEffects(hitCol, arrowRayHit.point, false, true, arrowRayHit.normal);
	
			hitTime = Time.time;
			hit = true;
			
		}

	}
}
