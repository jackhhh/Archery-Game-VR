//GunSway.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class GunSway : MonoBehaviour {
		
	//set up external script references
	private FPSRigidBodyWalker FPSWalkerComponent;
	private Ironsights IronsightsComponent;
	private WeaponBehavior WeaponBehaviorComponent;
	private HorizontalBob HorizontalBob;
	private FPSPlayer FPSPlayerComponent;
	private InputControl InputComponent;
	private CameraControl CameraControlComponent;
	
	//other objects accessed by this script
	public GameObject cameraObj;
	public GameObject playerObj;
	private Transform myTransform;
	//velocities for smoothing functions
	private float dampSpeed = 0.001f;
	private float dampVelocity1;
	private float dampVelocity2;
	private float dampVelocity6;
	private float dampVelocity7;

	//angle target vectors
	[HideInInspector]
	public Vector3 targetRotation;
	[HideInInspector]
	public Vector3 targetRotation2;
	[HideInInspector]
	public float targetRotationRoll;
	[HideInInspector]
	public float targetRotationPitch;
	[HideInInspector]
	public Vector3 tempEulerAngles;
	//passed on to ironsights script to sway gun
	[HideInInspector]
	public float localSide;
	[HideInInspector]
	public float localRaise;
	private float swingAmt = 0.035f;
	private float swingSpeed = 9.0f;
	//rolling of weapon
	[HideInInspector]
	public float localRoll;
	[HideInInspector]
	public float localPitch;
	private float rollSpeed;
	//to manage Z axis
	private float zAxis1;
	private float zAxis2;
	//set amounts of gun angle bobbing
	private float gunBobRoll = 10.0f;
	private float gunBobYaw = 16.0f;
	//vars to change swaying and bobbing amounts from the inspector
	[Tooltip("Amount to sway weapon with mouse movement.")]
	public float swayAmount = 1.0f;
	private float swayVal= 1.0f;
	[Tooltip("Amount to sway weapon roll angle with mouse movement.")]
	public float rollSwayAmount = 1.0f;
	[Tooltip("Amount to bob weapon yaw angles when player is walking.")]
	public float walkBobYawAmount = 1.0f;
	[Tooltip("Amount to bob weapon roll angles when player is walking.")]
	public float walkBobRollAmount = 1.0f;
	[Tooltip("Amount to bob weapon yaw angles when player is sprinting.")]
	public float sprintBobYawAmount = 1.0f;
	[Tooltip("Amount to bob weapon roll angles when player is sprinting.")]
	public float sprintBobRollAmount = 1.0f;

	[HideInInspector]
	public bool dzAiming;//used by other script to detect deadzone zooming mode

	private float zDampVel;
	private float zDamp;
	
	void Start (){
		myTransform = transform;//define transform for efficiency		
		//set up external script references
		FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		IronsightsComponent = playerObj.GetComponent<Ironsights>();
		HorizontalBob = playerObj.GetComponent<HorizontalBob>();
		FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		InputComponent = playerObj.GetComponent<InputControl>();
		CameraControlComponent = Camera.main.GetComponent<CameraControl>();
		//initialize bobbing amounts 
		walkBobYawAmount = Mathf.Clamp01(walkBobYawAmount);
		walkBobRollAmount = Mathf.Clamp01(walkBobRollAmount);
		sprintBobYawAmount = Mathf.Clamp01(sprintBobYawAmount);
		sprintBobRollAmount = Mathf.Clamp01(sprintBobRollAmount);
		gunBobRoll *= walkBobRollAmount;
		gunBobYaw *= walkBobYawAmount;
	}
	
	void Update (){
		WeaponBehaviorComponent = FPSPlayerComponent.WeaponBehaviorComponent;

		if(FPSPlayerComponent.zoomed){swayVal = swayAmount * 0.5f;}else{swayVal = swayAmount;}

		if(Time.timeScale > 0 && Time.deltaTime > 0){//allow pausing by setting timescale to 0
		
			if(FPSWalkerComponent.canRun){//sprinting
				//set sway amounts for sprinting
				swingAmt = 0.02f * swayVal * WeaponBehaviorComponent.swayAmountUnzoomed;
				swingSpeed = 0.000025f * swayVal * WeaponBehaviorComponent.swayAmountUnzoomed;
				rollSpeed = 0.0f * rollSwayAmount * WeaponBehaviorComponent.swayAmountUnzoomed;
				//smoothly change bobbing amounts for sprinting
				if(gunBobYaw < 12.0f * sprintBobYawAmount){gunBobYaw += 60.0f * Time.deltaTime;}
				if(gunBobRoll < 15.0f * sprintBobRollAmount){gunBobRoll += 60.0f * Time.deltaTime;}
				
			}else{//walking
				//smoothly change bobbing amounts for walking
				if(gunBobYaw > -16.0f * walkBobYawAmount){gunBobYaw -= 60.0f  *Time.deltaTime;}
				if(gunBobRoll > 15.0f * walkBobRollAmount){gunBobRoll -= 60.0f * Time.deltaTime;}
				//set sway amounts based on player state
				if(!FPSPlayerComponent.zoomed || IronsightsComponent.reloading || WeaponBehaviorComponent.meleeSwingDelay != 0){
					swingAmt = 0.035f * swayVal * WeaponBehaviorComponent.swayAmountUnzoomed;
					swingSpeed = 0.00015f * swayVal * WeaponBehaviorComponent.swayAmountUnzoomed;
					rollSpeed = 0.025f * rollSwayAmount * WeaponBehaviorComponent.swayAmountUnzoomed;
				}else{
					swingAmt = 0.025f * swayVal * WeaponBehaviorComponent.swayAmountZoomed;
					swingSpeed = 0.0001f * swayVal * WeaponBehaviorComponent.swayAmountZoomed;
					rollSpeed = 0.075f * rollSwayAmount * WeaponBehaviorComponent.swayAmountZoomed;
				}
			}

			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Weapon angles are set here, can be decoupled from camera movment by changing line below
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			//use both these z axes for independent animation of camera X and Y position and bobbing of camera roll 
			zAxis1 = Camera.main.transform.localEulerAngles.z;
			zAxis2 = cameraObj.transform.localEulerAngles.z;
			//Set target angles to main camera angles
			targetRotation.x =  cameraObj.transform.eulerAngles.x;
			targetRotation.y =  cameraObj.transform.eulerAngles.y;
			targetRotation.z = Mathf.MoveTowardsAngle(zAxis1, zAxis2, Time.deltaTime / 16);
			
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			//sway gun
			if(CameraControlComponent.MouseLookComponent.playerMovedTime + (0.2f * Time.timeScale) < Time.time){
				if(!dzAiming){
					//find angle delta and reduce to smaller value
					localRaise = Mathf.DeltaAngle(targetRotation2.x,targetRotation.x) * (swingSpeed / Time.deltaTime);
	
					//find angle delta and reduce to smaller value
					localSide = Mathf.DeltaAngle(targetRotation2.y,targetRotation.y) * -(swingSpeed / Time.deltaTime);
				}else{
					localRaise = -InputComponent.lookY * 0.002f;
					localSide = -InputComponent.lookX * 0.002f;
				}
			}

			//pass sway value to ironsights script where gun position is calculated
			IronsightsComponent.side = Mathf.Clamp(localSide, -swingAmt, swingAmt);
			//pass sway value to ironsights script where gun position is calculated
			IronsightsComponent.raise = Mathf.Clamp(localRaise, -swingAmt, swingAmt);
			//calculate roll swaying of weapon and smooth with lerpAngle to minimize snapping of gun during extreme rotations
			localRoll = Mathf.LerpAngle(localRoll, Mathf.DeltaAngle(targetRotationRoll,targetRotation.y)* -rollSpeed * 3.0f, Time.deltaTime * 5.0f);
			localPitch = Mathf.LerpAngle(localPitch, Mathf.DeltaAngle(targetRotationPitch,targetRotation.x)* -rollSpeed * 3.0f, Time.deltaTime * 5.0f);
			
			//dampen angles of gun with main camera angles for swaying effects
			targetRotation2.x = Mathf.SmoothDampAngle(targetRotation2.x, targetRotation.x, ref dampVelocity1, dampSpeed, Mathf.Infinity, Time.deltaTime);
			targetRotation2.y = Mathf.SmoothDampAngle(targetRotation2.y, targetRotation.y, ref dampVelocity2, dampSpeed, Mathf.Infinity, Time.deltaTime);
			targetRotationRoll = Mathf.SmoothDampAngle(targetRotationRoll, targetRotation.y, ref dampVelocity6, 0.075f, Mathf.Infinity, Time.deltaTime);
			targetRotationPitch = Mathf.SmoothDampAngle(targetRotationPitch, targetRotation.x, ref dampVelocity7, 0.075f, Mathf.Infinity, Time.deltaTime);

			if(WeaponBehaviorComponent.shooting){
				zDamp = Mathf.SmoothDampAngle(zDamp, WeaponBehaviorComponent.randZkick, ref zDampVel, 0.075f, Mathf.Infinity, Time.deltaTime);
			}else{
				zDamp = Mathf.SmoothDampAngle(zDamp, 0.0f, ref zDampVel, 0.075f, Mathf.Infinity, Time.deltaTime);
			}
			
			//set gun angles
			//set temporary vector to smoothed angles of gun and add yaw and roll bobbing and roll swaying
			tempEulerAngles = new Vector3(
				(targetRotation.x),
				(targetRotation.y + (HorizontalBob.dampOrg * gunBobYaw)),
				(targetRotation.z + (HorizontalBob.dampOrg * -gunBobRoll) - (FPSWalkerComponent.leanPos * 10.0f) + zDamp)
			);
			//finally, set actual weapon angles to the temporary vector's angles
			myTransform.localRotation = Quaternion.Euler(tempEulerAngles);
			
		}
	}
	
}