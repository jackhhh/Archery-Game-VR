//DragRigidbody.cs by Azuline Studios© All Rights Reserved
//Drags rigidbody objects in fron of player, throws held obejcts, and drops objects.
//Also stores object's original drag and angularDrag values and restores them on drop.
using UnityEngine;
using System.Collections;

public class DragRigidbody : MonoBehaviour {
	private FPSRigidBodyWalker FPSWalkerComponent;
	private Ironsights IronsightsComponent;
	private FPSPlayer FPSPlayerComponent;
	private InputControl InputComponent;
	private float spring = 75.0f;
	private float damper = 1.0f;
	private float drag = 10.0f;
	private float angularDrag = 5.0f;
	private float distance = 0.0f;
	[Tooltip("Max distance to drag objects.")]
	public float reachDistance = 2.5f;//max distance to drag objects (scaled by playerHeightMod amount of FPSRigidBodyWalker.cs)
	private float reachDistanceAmt;//max distance to drag objects
	[Tooltip("Physics force to apply to thrown objects.")]
	public float throwForce = 7.0f;
	private bool attachToCenterOfMass = false;
	private SpringJoint springJoint;
    private float oldDrag;
    private float oldAngularDrag;
    private bool dragState;
    private Vector3 dragDirRayCast;
    private Vector3 dragDirRay;
    
	[Tooltip("If true, dragged object will be dropped if it contacts player object to prevent pushing or lifting player.")]
	public bool dropOnPlayerCollision;
	
	[Tooltip("Only check these layers for draggable objects.")]
	public LayerMask layersToDrag = 0;//
	private Transform mainCamTransform;
	
	void Start(){
		FPSWalkerComponent = GetComponent<FPSRigidBodyWalker>();
		FPSPlayerComponent = GetComponent<FPSPlayer>();
		InputComponent = GetComponent<InputControl>();
		mainCamTransform = Camera.main.transform;
		//proportionately scale reachDistance by playerHeightMod amount
		reachDistanceAmt = reachDistance / (1 - (FPSWalkerComponent.playerHeightMod / FPSWalkerComponent.capsule.height));
	}
       
    void Update(){
			// Make sure the user pressed the mouse down
			if(!InputComponent.useHold 
			|| FPSPlayerComponent.usePressTime + 0.3f > Time.time//drag only after small delay after use button press
		    || FPSPlayerComponent.useReleaseTime + 0.3f > Time.time
			|| dragState 
		    || FPSPlayerComponent.pressButtonUpState
			|| FPSPlayerComponent.zoomed 
			|| FPSPlayerComponent.hitPoints < 1.0f){
				return;
			}
			
			if(!FPSPlayerComponent.CameraControlComponent.thirdPersonActive){
				dragDirRayCast = FPSPlayerComponent.WeaponBehaviorComponent.weaponLookDirection;
			}else{
				dragDirRayCast = ((mainCamTransform.position + mainCamTransform.forward * reachDistanceAmt) - mainCamTransform.position).normalized;
			}
		
			// We need to actually hit an object
	        RaycastHit hit;
			if(!Physics.Raycast(mainCamTransform.position, dragDirRayCast, out hit, reachDistanceAmt + FPSPlayerComponent.CameraControlComponent.zoomDistance, layersToDrag)){
				FPSPlayerComponent.pressButtonUpState = true;
				FPSPlayerComponent.useReleaseTime = -8f;
				return;
	        }
			// We need to hit a rigidbody that is not kinematic
			if(!hit.rigidbody || hit.rigidbody.isKinematic || FPSPlayerComponent.pressButtonUpState){
				return;
			}
			
			if(!springJoint){
				GameObject go = new GameObject("Rigidbody dragger");
				Rigidbody body = go.AddComponent <Rigidbody>() as Rigidbody;
				springJoint = go.AddComponent <SpringJoint>() as SpringJoint;
				body.isKinematic = true;
			}
			
			springJoint.connectedBody = hit.rigidbody;
			springJoint.transform.position = hit.point;
			
			if(attachToCenterOfMass){
				Vector3 anchor = transform.TransformDirection(hit.rigidbody.centerOfMass) + hit.rigidbody.transform.position;
				anchor = springJoint.transform.InverseTransformPoint(anchor);
				springJoint.anchor = anchor;
			}else{
				springJoint.anchor = Vector3.zero;
			}
			
			springJoint.spring = spring;
			springJoint.damper = damper;
			springJoint.maxDistance = distance;
	
			StartCoroutine(DragObject(hit.distance));
	}
	
    IEnumerator DragObject ( float distance  ){
		
        if(!dragState){
            oldDrag = springJoint.connectedBody.drag;
            oldAngularDrag = springJoint.connectedBody.angularDrag;
            dragState = true;
        }
		
		//allow floating objects like apples to fall once "picked" by dragging
		if(!springJoint.connectedBody.useGravity){
			if(springJoint.connectedBody.GetComponent<AudioSource>() && springJoint.connectedBody.GetComponent<AudioSource>().enabled){//play "picking" sound effect
				springJoint.connectedBody.GetComponent<AudioSource>().pitch = Random.Range(0.75f * Time.timeScale, 1.0f * Time.timeScale);//add slight random value to sound pitch for variety
				springJoint.connectedBody.GetComponent<AudioSource>().Play();
			}
			springJoint.connectedBody.useGravity = true;
		}
		
        springJoint.connectedBody.drag = drag;
        springJoint.connectedBody.angularDrag = angularDrag;
		
		while((InputComponent.useHold && FPSPlayerComponent.usePressTime + 0.3f < Time.time)
		&& springJoint.connectedBody 
		&& !FPSPlayerComponent.zoomed
		&& FPSPlayerComponent.hitPoints > 0.0f){
		
			if(!FPSPlayerComponent.CameraControlComponent.thirdPersonActive){
				dragDirRay = FPSPlayerComponent.WeaponBehaviorComponent.weaponLookDirection;
			}else{
				dragDirRay = ((mainCamTransform.position + mainCamTransform.forward * (reachDistanceAmt + FPSPlayerComponent.CameraControlComponent.zoomDistance)) - mainCamTransform.position).normalized;
			}
			
			Ray ray = new Ray (mainCamTransform.position, dragDirRay);
            springJoint.transform.position = ray.GetPoint(distance);
            if(!InputComponent.firePress){
				//let go of object if we are out of grabbing range
				if( Vector3.Distance(springJoint.connectedBody.transform.position, mainCamTransform.position) < (reachDistanceAmt + FPSPlayerComponent.CameraControlComponent.zoomDistance) * 1.4f ){
					FPSWalkerComponent.holdingObject = true;
					yield return null;
				}else{
					break;
				}
			}else{//throw object
				float throwForceAmt;
				if(springJoint.connectedBody.mass < 1){
					throwForceAmt = throwForce/2;
				}else{
					throwForceAmt = throwForce * springJoint.connectedBody.mass;
				}
				if(!FPSPlayerComponent.CameraControlComponent.thirdPersonActive){
					springJoint.connectedBody.AddForceAtPosition((throwForceAmt * FPSPlayerComponent.WeaponBehaviorComponent.weaponLookDirection), springJoint.transform.position,ForceMode.Impulse);
				}else{
					springJoint.connectedBody.AddForceAtPosition((throwForceAmt * mainCamTransform.forward), springJoint.transform.position,ForceMode.Impulse);
				}
				
				FPSPlayerComponent.WeaponBehaviorComponent.shootStartTime = Time.time;//prevent weapons from shooting after throwing with fire button
				
				break;
			}
		}
		if (springJoint.connectedBody){//stop dragging object
			DropObject();
		}else{
			FPSWalkerComponent.holdingObject = false;//raise weapon if dragged object is destroyed	
			FPSWalkerComponent.dropTime = Time.time;
		}
		
		dragState = false;
		FPSPlayerComponent.pressButtonUpState = true;
	}

	//if dragged object contacts player object, stop dragging to prevent pushing or lifting player
	void OnCollisionStay(Collision col){
		if(dropOnPlayerCollision && springJoint){
			if(springJoint.connectedBody){//stop dragging object
				if(col.gameObject.GetComponent<Rigidbody>() == springJoint.connectedBody){
					DropObject();
				}		
			}
		}
    }
	
	void DropObject(){
		FPSWalkerComponent.holdingObject = false;
		FPSWalkerComponent.dropTime = Time.time;
		springJoint.connectedBody.drag = oldDrag;
        springJoint.connectedBody.angularDrag = oldAngularDrag;
        springJoint.connectedBody = null;
        dragState = false;
	}

}