//ShellEjection.cs by Azuline Studios© All Rights Reserved
//Rotates and moves instantiated rigidbody shell object and lerps mesh shell object.
using UnityEngine;
using System.Collections;

public class ShellEjection : MonoBehaviour{
	//set up external script references
	[HideInInspector]
	public FPSPlayer FPSPlayerComponent;
	private FPSRigidBodyWalker FPSWalkerComponent;
	private PlayerWeapons PlayerWeaponsComponent;
	[HideInInspector]
	public WeaponBehavior WeaponBehaviorComponent;
	[HideInInspector]
	public Rigidbody RigidbodyComponent;
	[HideInInspector]
	public Rigidbody PlayerRigidbodyComponent;

	//objects accessed by this script
	[HideInInspector]
	public GameObject playerObj;
	[HideInInspector]
	public GameObject gunObj;
	private Transform gunObjTransform;
	[HideInInspector]
	public Transform lerpShell;//the mesh shell object that lerps the rigidbody shell's position and rotation
	private Vector3 tempPos;
	private Vector3 tempRot;
	private bool rotated;
	private Transform myTransform;
	private Transform FPSMainTransform;
	[Tooltip("Sound effects to play when shell lands on a surface")]
	public AudioClip[] shellSounds;//shell bounce sounds
	//shell states and settings
	private bool parentState = true;
	private bool soundState = true;
	//shell rotation
	private float rotateAmt = 0.0f;//amount that the shell rotates, scaled up after ejection
	[HideInInspector]
	public float shellRotateUp = 0.0f;//amount of vertical shell rotation
	[HideInInspector]
	public float shellRotateSide = 0.0f;//amount of horizontal shell rotation	
	//timers and shell lifetime duration
	private float shellRemovalTime = 0.0f;//time that this shell will be removed from the level
	[HideInInspector]
	public int shellDuration = 0;//time in seconds that shells persist in the world before being removed	
	private float startTime = 0.0f;//time that the shell instance was created in the world

	[HideInInspector]
	public int shellPoolIndex;
	[HideInInspector]
	public int RBPoolIndex;

	[HideInInspector]
	public bool dzAiming;
	
	void Start(){

	}

	public void InitializeShell(){
	
		//set up external script references
		FPSPlayerComponent = Camera.main.transform.GetComponent<CameraControl>().FPSPlayerComponent;
		PlayerWeaponsComponent = FPSPlayerComponent.PlayerWeaponsComponent;
		FPSWalkerComponent = FPSPlayerComponent.FPSWalkerComponent;
		myTransform = transform;//manually set transform for efficiency
	
		FPSWalkerComponent = FPSPlayerComponent.FPSWalkerComponent;

		//shell states and settings
		parentState = true;
		soundState = true;

		//initialize shell rotation amounts
		shellRotateUp = WeaponBehaviorComponent.shellRotateUp;
		shellRotateSide = WeaponBehaviorComponent.shellRotateSide;
		shellDuration = WeaponBehaviorComponent.shellDuration;
		//track the time that the shell was ejected
		startTime = Time.time;
		shellRemovalTime = Time.time + shellDuration;//time that shell will be removed
		RigidbodyComponent.maxAngularVelocity = 100;//allow shells to spin faster than default
		//determine if shell rotates clockwise or counter-clockwise at random
		if(Random.value < 0.5f){shellRotateUp *= -1;} 
		RigidbodyComponent.velocity = Vector3.zero;
		RigidbodyComponent.angularVelocity = Vector3.zero;
		//rotate shell
		rotateAmt = 0.1f;
		//apply torque to rigidbody
		RigidbodyComponent.AddRelativeTorque(Vector3.up * (Random.Range (0.175f, rotateAmt) * shellRotateSide), ForceMode.Impulse);
		RigidbodyComponent.AddRelativeTorque(Vector3.right * (Random.Range (0.4f, rotateAmt * 6) * shellRotateUp), ForceMode.Impulse);

		StartCoroutine(CalcShellPos());
	}
	
	void Update (){		
		if(Time.time > shellRemovalTime){
			AzuObjectPool.instance.RecyclePooledObj(WeaponBehaviorComponent.shellRBPoolIndex,gameObject);
		}
	}
	
	IEnumerator CalcShellPos(){

		while(true){
			
			//Check if the player is on a moving platform to determine how to handle shell parenting and velocity
			if(!FPSWalkerComponent.playerParented){//if player is not on a moving platform
				//Make the shell's parent the weapon object for a short time after ejection
				//to the link shell ejection position with weapon object for more consistent movement,
				if(parentState
				&& ((startTime + PlayerWeaponsComponent.shellParentTime < Time.time && !FPSPlayerComponent.bulletTimeActive)
				|| (startTime + PlayerWeaponsComponent.shellBtParentTime < Time.time && FPSPlayerComponent.bulletTimeActive)
				|| (startTime + PlayerWeaponsComponent.shellDzParentTime < Time.time && dzAiming)
				//don't parent shell if switching weapon
				|| PlayerWeaponsComponent.switching
				//don't parent shell if moving weapon to sprinting position or moving while prone
				|| (FPSWalkerComponent.sprintActive && !FPSWalkerComponent.cancelSprint)
				|| (FPSWalkerComponent.prone && FPSWalkerComponent.moving))){
					Vector3 tempVelocity = PlayerRigidbodyComponent.velocity;
					tempVelocity.y = 0.0f;
					myTransform.parent = null;
					//add player velocity to shell when unparenting from player object to prevent shell from suddenly changing direction
					if(!FPSWalkerComponent.sprintActive && !FPSWalkerComponent.canRun && FPSWalkerComponent.moving){//don't inherit parent velocity if sprinting to prevent visual glitches
						RigidbodyComponent.AddForce(tempVelocity, ForceMode.VelocityChange);
					}
					parentState = false;
					yield break;
				}
			}else{//if player is on elevator, keep gun object as parent for a longer time to prevent strange shell movements
				if((startTime + PlayerWeaponsComponent.shellParentTime < Time.time && !FPSPlayerComponent.bulletTimeActive) 
			    || (startTime + PlayerWeaponsComponent.shellBtParentTime < Time.time && FPSPlayerComponent.bulletTimeActive)){
					myTransform.parent = null;
					//add player velocity to shell when unparenting from player object to prevent shell from suddenly changing direction
					RigidbodyComponent.AddForce(PlayerRigidbodyComponent.velocity, ForceMode.VelocityChange);	
					parentState = false;
					yield break;
				}		
			}
			yield return null;
		}

	}

	void OnCollisionEnter(Collision collision){
		//play a bounce sound when shell object collides with a surface
		if(soundState){
			if (shellSounds.Length > 0){
				PlayAudioAtPos.PlayClipAt(shellSounds[(int)Random.Range(0, (shellSounds.Length))], myTransform.position, 0.75f);
			}
			soundState = false;
		}
	}

}


	