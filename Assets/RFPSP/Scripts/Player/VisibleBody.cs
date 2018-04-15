//VisibleBody.cs by Azuline Studios© All Rights Reserved
//Positions and rotates visible player model and plays animations
//TODO replace with Mecanim animation setup
using UnityEngine;
using System.Collections;

public class VisibleBody : MonoBehaviour {
	
	[HideInInspector]
	public GameObject playerObj;
	private Transform playerTransform;
	[HideInInspector]
	public GameObject weaponObj;
	[TooltipAttribute("Object with animation component to animate.")]
	public GameObject objectWithAnims;
	private Animation AnimationComponent;
	[Tooltip("Object with animation component to animate for shadows in first person mode and as player in third person mode.")]
	public Animation AnimationComponentShadow;
	private InputControl InputComponent;
	private FPSRigidBodyWalker walkerComponent;
	private FPSPlayer FPSPlayerComponent;
	private GunSway GunSwayComponent;
	private SmoothMouseLook SmoothMouseLookComponent;
	private CameraControl CameraControlComponent;
	[TooltipAttribute("Scale of model while standing.")]
	public float modelStandScale = 1.4f;
	[TooltipAttribute("Scale of model while crouching.")]
	public float modelCrouchScale = 1.1f;
	private float modelScaleAmt = 1.1f;
	[Tooltip("Scale of playe model in third person mode.")]
	public float tpMeshScale = 0.95f;
	[Tooltip("Pitch angle of visible body in first person mode.")]
	public float modelPitch = -7.0f;
	[Tooltip("Pitch angle of visible body when sprinting in first person mode.")]
	public float modelPitchRun = -20f;
	private float modelPitchAmt;
	[Tooltip("Amount to add to forward position of visible player model when standing in first person mode.")]
	public float modelForward = -0.35f;
	[Tooltip("Amount to add to forward position of visible player model when crouching in first person mode.")]
	public float modelForwardCrouch = -0.3f;
	[Tooltip("Amount to add to forward position of visible player model when swimming in first person mode.")]
	public float modelForwardSwimming = -0.65f;
	private float modelForwardAmt;
	private float modelRightAmt;
	[Tooltip("Amount to add to upward position of visible player model when standing in first person mode.")]
	public float modelUpFP = 1.8f;
	[Tooltip("Amount to add to upward position of FP shadow and third person player model when standing.")]
	public float modelUp = 1.8f;
	[Tooltip("Amount to add to upward position of visible player model when crouching in first person mode.")]
	public float modelUpCrouchFP = 1.1f;
	[Tooltip("Amount to add to upward position of FP shadow and third person player model when crouching.")]
	public float modelUpCrouch = 1.1f;
	[Tooltip("Amount to add to upward position of FP shadow and third person player model when sprinting.")]
	public float modelUpRun = 1.1f;
	[HideInInspector]
	public float modelUpAmt;
	[HideInInspector]
	public float verticalPos;
	[HideInInspector]
	public float tempPosX;
	[HideInInspector]
	public float tempPosZ;
	[Tooltip("Speed of walking animation.")]
	public float walkAnimSpeed = 1.27f;
	[Tooltip("Speed of crouch animation.")]
	public float crouchWalkAnimSpeed = 0.84f;
	[Tooltip("Speed of sprint animation.")]
	public float sprintAnimSpeed = 2.0f;
	private float speedMax;
	private float speedAmt;
	[TooltipAttribute("Angle to rotate model when strafing.")]
	public float strafeAngle = 40.0f;
	private float rotAngleAmt = 40.0f;
	[TooltipAttribute("Angle to rotate model when jumping.")]
	public float jumpAngle = 30.0f;
	[TooltipAttribute("Angle to rotate model when crouching.")]
	public float crouchAngle = 5.0f;
	[TooltipAttribute("Angle to rotate model when idle.")]
	public float idleAngle = -35.0f;
	[Tooltip("Amount to add to right position of visible player model when standing in first person mode.")]
	public float idleRight = -0.15f;
	private float modelRightMod;
	[Tooltip("Amount to add to right position of visible player model when crouching in first person mode.")]
	public float crouchRight = -0.12f;
	private Vector3 tempBodyAngles;
	[HideInInspector]
	public Vector3 tempBodyPosition;
	private Vector3 tempSmoothedPos;
	
	private float smoothedTurnAmt;
	
	private Transform myTransform;
	private Vector3 myScale;
	[Tooltip("Object with mesh renderer component for the shadow and third person player model.")]
	public SkinnedMeshRenderer shadowSkinnedMesh;
	[Tooltip("Object of an additional accessory mesh for third person model, like a visor.")]
	public GameObject accMesh;
	[Tooltip("Position of third person player model.")]
	public Vector3 tpMeshPos;
	[Tooltip("Position of first person player model.")]
	public Vector3 fpShadowMeshPos;
	[Tooltip("Object with mesh renderer component for the weapon model.")]
	public MeshRenderer weaponMesh;
	[Tooltip("Object with mesh renderer component for the first person player model.")]
	public SkinnedMeshRenderer fpSkinnedMesh;
	private bool meshState1;
	private bool meshState2;
	
	private Vector3 dampvel;
	[Tooltip("Speed to smooth character model yaw angles (facing direction) in third person mode.")]
	private float slerpSpeed = 14f;
	private float slerpSpeedAmt;
	[HideInInspector]
	public float camModeSwitchedTime;
	[Tooltip("Time to allow smoothing of character model yaw using slerp speed.")]
	private float smoothTime = 0.01f;
	[Tooltip("Speed to reduce smoothing of character model yaw when smooth time has elapsed.")]
	private float speedUpTime = 30f;
	
	void Start () {
		CameraControlComponent = Camera.main.transform.GetComponent<CameraControl>();
		playerObj = CameraControlComponent.playerObj;
		weaponObj = CameraControlComponent.weaponObj;
		playerTransform = playerObj.transform;
		InputComponent = playerObj.GetComponent<InputControl>();
		FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		walkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		GunSwayComponent = weaponObj.GetComponent<GunSway>();
		SmoothMouseLookComponent = GunSwayComponent.cameraObj.GetComponent<SmoothMouseLook>();
		AnimationComponent = objectWithAnims.GetComponent<Animation>();
		AnimationComponent.wrapMode = WrapMode.Loop;
		AnimationComponentShadow.wrapMode = WrapMode.Loop;
		myTransform = transform;
		verticalPos = playerTransform.position.y - modelUpAmt;
		tempPosX = playerTransform.position.x;
		tempPosZ = playerTransform.position.z;
		meshState1 = false;
		meshState2 = false;
		fpSkinnedMesh.gameObject.SetActive(true);
	}
	
	void Update (){
		
		if(Time.timeScale > 0f && Time.smoothDeltaTime > 0f){
		
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//First Person Mode
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
			if(!CameraControlComponent.thirdPersonActive){
				
				if(!CameraControlComponent.thirdPersonActive && !meshState1){
					shadowSkinnedMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
					weaponMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
					if(accMesh){accMesh.SetActive(false);}
					shadowSkinnedMesh.transform.parent.transform.localScale = Vector3.one;
					shadowSkinnedMesh.transform.parent.transform.localPosition = fpShadowMeshPos;

					verticalPos = playerTransform.position.y - modelUpAmt;
					tempPosX = playerTransform.position.x;
					tempPosZ = playerTransform.position.z;
	
					meshState1 = true;
					meshState2 = false;
				}

				tempSmoothedPos = new Vector3(playerTransform.position.x, playerTransform.position.y - modelUpAmt, playerTransform.position.z); 
				
				tempBodyPosition = Vector3.SmoothDamp(tempBodyPosition, tempSmoothedPos, ref dampvel, CameraControlComponent.lerpSpeedAmt, Mathf.Infinity, Time.smoothDeltaTime);
				
				myTransform.position = tempBodyPosition + (playerTransform.forward * modelForwardAmt) + (playerTransform.right * modelRightMod);
				
				tempBodyAngles = new Vector3(playerTransform.eulerAngles.x, playerTransform.eulerAngles.y + rotAngleAmt, 0f);
				myTransform.eulerAngles = tempBodyAngles;
				
				tempBodyAngles = new Vector3(modelPitchAmt, playerTransform.eulerAngles.y + rotAngleAmt, 0f);
				myTransform.eulerAngles = tempBodyAngles;
				
				//adjust model scale for better visual tweaking for standing and crouching
				if(walkerComponent.crouched){
					modelScaleAmt = Mathf.MoveTowards(modelScaleAmt, modelCrouchScale, Time.smoothDeltaTime * 5.0f);
				}else{
					modelScaleAmt = Mathf.MoveTowards(modelScaleAmt, modelStandScale, Time.smoothDeltaTime * 5.0f);
				}
				
				myTransform.localScale = new Vector3(modelScaleAmt, modelScaleAmt, modelScaleAmt);
				
				if(walkerComponent.prone || FPSPlayerComponent.hitPoints <= 0.0f){
					//move model back and out of sight if the player's legs shouldn't be visible in this state
					modelForwardAmt = Mathf.Lerp(modelForwardAmt, -1.75f, Time.smoothDeltaTime * 7.0f);
					
				}else if(!walkerComponent.grounded || walkerComponent.holdingBreath || walkerComponent.swimming ){//play jumping animation and rotate to jumpAngle amount
					rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, jumpAngle, 220.0f * Time.smoothDeltaTime);
					modelRightMod = Mathf.Lerp(modelRightMod, 0.0f, Time.smoothDeltaTime * 7.0f);
					if(!walkerComponent.holdingBreath && !walkerComponent.swimming){
						modelForwardAmt = Mathf.Lerp(modelForwardAmt, modelForward, Time.smoothDeltaTime * 7.0f);
					}else{
						modelForwardAmt = Mathf.Lerp(modelForwardAmt, modelForwardSwimming, Time.smoothDeltaTime * 7.0f);
					}
					modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpFP, Time.smoothDeltaTime * 2.0f);
					AnimationComponent["jump"].speed = 0.3f;
					AnimationComponentShadow["jump"].speed = 0.3f;
					AnimationComponent.CrossFade("jump", 0.2f);
					AnimationComponentShadow.CrossFade("jump", 0.2f);
				}else{
					//position model forward
					if(walkerComponent.proneRisen || walkerComponent.crouchRisen){
						if(!walkerComponent.crouched){
							modelForwardAmt = Mathf.Lerp(modelForwardAmt, modelForward, Time.smoothDeltaTime * 7.0f);
						}else{
							modelForwardAmt = Mathf.Lerp(modelForwardAmt, modelForwardCrouch, Time.smoothDeltaTime * 7.0f);
						}
					}
					
					if(FPSPlayerComponent.zoomed){
						speedAmt = 0.67f;
					}else{
						speedAmt = 1.0f;
					}
					//calculate movement speed to scale animation speed when using a joystick
					speedMax = Mathf.Max(Mathf.Abs(walkerComponent.inputY), Mathf.Abs(walkerComponent.inputX)) * speedAmt;
					
					if(Mathf.Abs(walkerComponent.inputY) > 0.1f){
						modelRightMod = Mathf.Lerp(modelRightMod, 0.0f, Time.smoothDeltaTime * 7.0f);
						if(walkerComponent.crouched){//play walking crouch animation
							modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpCrouchFP, Time.smoothDeltaTime * 1.5f);
							modelPitchAmt = Mathf.MoveTowards(modelPitchAmt, modelPitch, 2f * Time.timeScale);
							if(walkerComponent.inputY > 0.1f){
								AnimationComponent["crouchwalk"].speed = crouchWalkAnimSpeed;
								AnimationComponentShadow["crouchwalk"].speed = crouchWalkAnimSpeed;
							}else if(walkerComponent.inputY < 0.1f){
								AnimationComponent["crouchwalk"].speed = -crouchWalkAnimSpeed;
								AnimationComponentShadow["crouchwalk"].speed = -crouchWalkAnimSpeed;
							}
							AnimationComponent.CrossFade("crouchwalk", 0.3f);
							AnimationComponentShadow.CrossFade("crouchwalk", 0.3f);
						}else{//play walking animation
							AnimationComponent.CrossFade("walk", 0.25f);
							if(walkerComponent.inputY > 0.1f){
								if(!walkerComponent.sprintActive){
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpFP, Time.smoothDeltaTime * 2.0f);
									modelPitchAmt = Mathf.MoveTowards(modelPitchAmt, modelPitch, 0.4f * Time.timeScale);
									AnimationComponent["walk"].speed = walkAnimSpeed * speedMax;
									AnimationComponentShadow["walk"].speed = walkAnimSpeed * speedMax;
									AnimationComponentShadow.CrossFade("walk", 0.25f);
									AnimationComponentShadow["walk"].normalizedTime = AnimationComponent["walk"].normalizedTime;
								}else{
									modelUpAmt = Mathf.MoveTowards(modelUpRun, modelUpFP, Time.smoothDeltaTime * 2.0f);
									modelPitchAmt = Mathf.MoveTowards(modelPitchAmt, modelPitchRun, 2f * Time.timeScale);
									AnimationComponent["run"].speed = sprintAnimSpeed * speedMax;
									AnimationComponent.CrossFade("run", 0.25f);
									AnimationComponentShadow["run"].speed = sprintAnimSpeed * speedMax;
									AnimationComponentShadow.CrossFade("run", 0.25f);
									AnimationComponentShadow["run"].normalizedTime = AnimationComponent["run"].normalizedTime;
								}
							}else if(walkerComponent.inputY < -0.1f){
								if(!walkerComponent.sprintActive){
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpFP, Time.smoothDeltaTime * 2.0f);
									modelPitchAmt = Mathf.MoveTowards(modelPitchAmt, modelPitch, 0.4f * Time.timeScale);
									AnimationComponent["walk"].speed = -walkAnimSpeed * speedMax;
									AnimationComponentShadow["walk"].speed = -walkAnimSpeed * speedMax;
									AnimationComponentShadow.CrossFade("walk", 0.25f);
									AnimationComponentShadow["walk"].normalizedTime = AnimationComponent["walk"].normalizedTime;
								}else{
									modelUpAmt = Mathf.MoveTowards(modelUpRun, modelUpFP, Time.smoothDeltaTime * 2.0f);
									modelPitchAmt = Mathf.MoveTowards(modelPitchAmt, modelPitchRun, 2f * Time.timeScale);
									AnimationComponent["run"].speed = -sprintAnimSpeed * speedMax;
									AnimationComponent.CrossFade("run", 0.25f);
									AnimationComponentShadow["run"].speed = -sprintAnimSpeed * speedMax;
									AnimationComponentShadow.CrossFade("run", 0.25f);
									AnimationComponentShadow["run"].normalizedTime = AnimationComponent["run"].normalizedTime;
								}
							}
						}
						if(InputComponent.moveX > 0.1f){//play strafing animations and rotate to strafing angle
							if(walkerComponent.inputY > 0.1f){
								rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, strafeAngle, 220.0f * Time.smoothDeltaTime);
							}else{
								rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, -strafeAngle, 220.0f * Time.smoothDeltaTime);
							}
						}else if(InputComponent.moveX < -0.1f){
							if(walkerComponent.inputY > 0.1f){
								rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, -strafeAngle, 220.0f * Time.smoothDeltaTime);
							}else{
								rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, strafeAngle, 220.0f * Time.smoothDeltaTime);
							}
						}else{
							rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, 0.0f, 220.0f * Time.smoothDeltaTime);
						}
					}else{
						modelPitchAmt = Mathf.MoveTowards(modelPitchAmt, modelPitch, 0.4f * Time.timeScale);
						if(InputComponent.moveX > 0.1f){
							modelRightMod = Mathf.Lerp(modelRightMod, 0.0f, Time.smoothDeltaTime * 7.0f);
							rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, 0.0f, 220.0f * Time.smoothDeltaTime);
							if(walkerComponent.crouched){
								AnimationComponent.CrossFade("crouchstraferight", 0.3f);
								AnimationComponentShadow.CrossFade("crouchstraferight", 0.3f);
								AnimationComponent["crouchstraferight"].speed = crouchWalkAnimSpeed * speedMax;
								AnimationComponentShadow["crouchstraferight"].speed = crouchWalkAnimSpeed * speedMax;
								modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpCrouchFP, Time.smoothDeltaTime * 1.5f);
							}else{
								AnimationComponent.CrossFade("straferight", 0.3f);
								AnimationComponentShadow.CrossFade("straferight", 0.3f);
								if(!walkerComponent.sprintActive){
									AnimationComponent["straferight"].speed = 1.27f * speedMax;
									AnimationComponentShadow["straferight"].speed = 1.27f * speedMax;
								}else{
									AnimationComponent["straferight"].speed = 2.0f * speedMax;
									AnimationComponentShadow["straferight"].speed = 2.0f * speedMax;
								}
								modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpFP, Time.smoothDeltaTime * 2.0f);
							}
						}else if(InputComponent.moveX < -0.1f){
							modelRightMod = Mathf.Lerp(modelRightMod, 0.0f, Time.smoothDeltaTime * 7.0f);
							rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, 0.0f, 220.0f * Time.smoothDeltaTime);
							if(walkerComponent.crouched){
								AnimationComponent.CrossFade("crouchstrafeleft", 0.3f);
								AnimationComponentShadow.CrossFade("crouchstrafeleft", 0.3f);
								AnimationComponent["crouchstrafeleft"].speed = crouchWalkAnimSpeed * speedMax;
								AnimationComponentShadow["crouchstrafeleft"].speed = crouchWalkAnimSpeed * speedMax;
								modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpCrouchFP, Time.smoothDeltaTime * 1.5f);
							}else{
								AnimationComponent.CrossFade("strafeleft", 0.3f);
								AnimationComponentShadow.CrossFade("strafeleft", 0.3f);
								if(!walkerComponent.sprintActive){
									AnimationComponent["strafeleft"].speed = 1.27f * speedMax;
									AnimationComponentShadow["strafeleft"].speed = 1.27f * speedMax;
								}else{
									AnimationComponent["strafeleft"].speed = 2.0f * speedMax;
									AnimationComponentShadow["strafeleft"].speed = 2.0f * speedMax;
								}
								modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpFP, Time.smoothDeltaTime * 2.0f);
							}
						}else{
							smoothedTurnAmt = Mathf.Lerp(smoothedTurnAmt, SmoothMouseLookComponent.horizontalDelta, Time.smoothDeltaTime * 10.0f);
							if(smoothedTurnAmt < -0.4){
								modelRightMod = Mathf.Lerp(modelRightMod, idleRight, Time.smoothDeltaTime * 7.0f);
								rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, 0.0f, 2.0f);
								if(walkerComponent.crouched){
									AnimationComponent["crouchstraferight"].speed = 0.7f;
									AnimationComponentShadow["crouchstraferight"].speed = 0.7f;
									AnimationComponent.CrossFade("crouchstraferight", 0.4f);
									AnimationComponentShadow.CrossFade("crouchstraferight", 0.4f);
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpCrouchFP, Time.smoothDeltaTime * 1.5f);
								}else{
									AnimationComponent["straferight"].speed = 1.0f;
									AnimationComponentShadow["straferight"].speed = 1.0f;
									AnimationComponent.CrossFade("straferight", 0.4f);
									AnimationComponentShadow.CrossFade("straferight", 0.4f);
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpFP, Time.smoothDeltaTime * 2.0f);
								}
							}else if(smoothedTurnAmt > 0.4){
								modelRightMod = Mathf.Lerp(modelRightMod, idleRight, Time.smoothDeltaTime * 7.0f);
								rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, 0.0f, 1.0f);
								if(walkerComponent.crouched){
									AnimationComponent["crouchstrafeleft"].speed = 0.7f;
									AnimationComponentShadow["crouchstrafeleft"].speed = 0.7f;
									AnimationComponent.CrossFade("crouchstrafeleft", 0.4f);
									AnimationComponentShadow.CrossFade("crouchstrafeleft", 0.4f);
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpCrouchFP, Time.smoothDeltaTime * 1.5f);
								}else{
									AnimationComponent["strafeleft"].speed = 1.0f;
									AnimationComponentShadow["strafeleft"].speed = 1.0f;
									AnimationComponent.CrossFade("strafeleft", 0.4f);
									AnimationComponentShadow.CrossFade("strafeleft", 0.4f);
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpFP, Time.smoothDeltaTime * 2.0f);
								}
							}else{//play idle animation and rotate to idle angles
								if(walkerComponent.crouched){
									modelRightMod = Mathf.Lerp(modelRightMod, crouchRight, Time.smoothDeltaTime * 7.0f);
									rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, crouchAngle, 2.0f);
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpCrouchFP, Time.smoothDeltaTime * 1.5f);
									AnimationComponent.CrossFade("crouchidle", 0.4f);
									AnimationComponentShadow.CrossFade("crouchidle", 0.4f);
								}else{
									modelRightMod = Mathf.Lerp(modelRightMod, idleRight, Time.smoothDeltaTime * 7.0f);
									rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, idleAngle, 2.0f);
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpFP, Time.smoothDeltaTime * 2.0f);
									AnimationComponent.CrossFade("idle2", 0.4f);
									AnimationComponentShadow.CrossFade("idle2", 0.4f);
								}
							}
						}
					}
				}
			}else{
			
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				//Third Person Mode
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				
				if(CameraControlComponent.thirdPersonActive && !meshState2){
	
					shadowSkinnedMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
					shadowSkinnedMesh.transform.parent.transform.localPosition = tpMeshPos;
					weaponMesh.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
					fpSkinnedMesh.enabled = false;
					if(accMesh){accMesh.SetActive(true);}
					shadowSkinnedMesh.transform.parent.transform.localScale = new Vector3(tpMeshScale, tpMeshScale, tpMeshScale);
					
					tempBodyPosition = new Vector3(playerTransform.position.x, playerTransform.position.y - modelUpAmt, playerTransform.position.z); 
	
					
					meshState1 = false;
					meshState2 = true;
				}
						
				tempSmoothedPos = new Vector3(playerTransform.position.x, playerTransform.position.y - modelUpAmt, playerTransform.position.z); 
				
				tempBodyPosition = Vector3.SmoothDamp(tempBodyPosition, tempSmoothedPos, ref dampvel, CameraControlComponent.lerpSpeedAmt, Mathf.Infinity, Time.smoothDeltaTime);
				
				myTransform.position = tempBodyPosition;
				
				tempBodyAngles = new Vector3(0.0f, playerTransform.eulerAngles.y + rotAngleAmt, 0.0f);
				
				if(camModeSwitchedTime + smoothTime > Time.time){
					slerpSpeedAmt = slerpSpeed;
				}else{
					slerpSpeedAmt = Mathf.MoveTowards(slerpSpeedAmt, 64f, Time.smoothDeltaTime * speedUpTime);
				}
				
				myTransform.rotation = Quaternion.Slerp(myTransform.rotation, Quaternion.Euler(tempBodyAngles), Time.smoothDeltaTime * slerpSpeedAmt);
				
				myTransform.localScale = new Vector3(modelStandScale, modelStandScale, modelStandScale);
				
				if(walkerComponent.prone || FPSPlayerComponent.hitPoints <= 0.0f){
					
				}else if(!walkerComponent.grounded || walkerComponent.holdingBreath || walkerComponent.swimming ){//play jumping animation and rotate to jumpAngle amount
					rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, jumpAngle, 220.0f * Time.smoothDeltaTime);
					modelRightMod = Mathf.Lerp(modelRightMod, 0.0f, Time.smoothDeltaTime * 7.0f);
					modelForwardAmt = Mathf.Lerp(modelForwardAmt, modelForward, Time.smoothDeltaTime * 7.0f);
					modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUp, Time.smoothDeltaTime * 2.0f);
					AnimationComponent["jump"].speed = 0.3f;
					AnimationComponentShadow["jump"].speed = 0.3f;
					AnimationComponent.CrossFade("jump", 0.2f);
					AnimationComponentShadow.CrossFade("jump", 0.2f);
				}else{
					
					if(FPSPlayerComponent.zoomed){
						speedAmt = 0.67f;
					}else{
						speedAmt = 1.0f;
					}
					//calculate movement speed to scale animation speed when using a joystick
					speedMax = Mathf.Max(Mathf.Abs(walkerComponent.inputY), Mathf.Abs(walkerComponent.inputX)) * speedAmt;
					
					if(Mathf.Abs(walkerComponent.inputY) > 0.1f){
						modelRightMod = Mathf.Lerp(modelRightMod, 0.0f, Time.smoothDeltaTime * 7.0f);
						if(walkerComponent.crouched){//play walking crouch animation
							modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpCrouch, Time.smoothDeltaTime * 1.5f);
							modelPitchAmt = Mathf.MoveTowards(modelPitchAmt, modelPitch, 2f * Time.timeScale);
							if(walkerComponent.inputY > 0.1f){
								AnimationComponent["crouchwalk"].speed = crouchWalkAnimSpeed;
								AnimationComponentShadow["crouchwalk"].speed = crouchWalkAnimSpeed;
							}else if(walkerComponent.inputY < 0.1f){
								AnimationComponent["crouchwalk"].speed = -crouchWalkAnimSpeed;
								AnimationComponentShadow["crouchwalk"].speed = -crouchWalkAnimSpeed;
							}
							AnimationComponent.CrossFade("crouchwalk", 0.3f);
							AnimationComponentShadow.CrossFade("crouchwalk", 0.3f);
						}else{//play walking animation
							AnimationComponent.CrossFade("walk", 0.25f);
							if(walkerComponent.inputY > 0.1f){
								if(!walkerComponent.sprintActive){
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUp, Time.smoothDeltaTime * 2.0f);
									modelPitchAmt = Mathf.MoveTowards(modelPitchAmt, modelPitch, 2f * Time.timeScale);
									AnimationComponent["walk"].speed = walkAnimSpeed * speedMax;
									AnimationComponentShadow["walk"].speed = walkAnimSpeed * speedMax;
									AnimationComponentShadow.CrossFade("walk", 0.25f);
									AnimationComponentShadow["walk"].normalizedTime = AnimationComponent["walk"].normalizedTime;
								}else{
									modelUpAmt = Mathf.MoveTowards(modelUpRun, modelUp, Time.smoothDeltaTime * 2.0f);
									modelPitchAmt = Mathf.MoveTowards(modelPitchAmt, modelPitch, 6f * Time.timeScale);
									AnimationComponent["run"].speed = sprintAnimSpeed * speedMax;
									AnimationComponent.CrossFade("run", 0.25f);
									AnimationComponentShadow["run"].speed = sprintAnimSpeed * speedMax;
									AnimationComponentShadow.CrossFade("run", 0.25f);
									AnimationComponentShadow["run"].normalizedTime = AnimationComponent["run"].normalizedTime;
								}
							}else if(walkerComponent.inputY < -0.1f){
								if(!walkerComponent.sprintActive){
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUp, Time.smoothDeltaTime * 2.0f);
									modelPitchAmt = Mathf.MoveTowards(modelPitchAmt, modelPitch, 2f * Time.timeScale);
									AnimationComponent["walk"].speed = -walkAnimSpeed * speedMax;
									AnimationComponentShadow["walk"].speed = -walkAnimSpeed * speedMax;
									AnimationComponentShadow.CrossFade("walk", 0.25f);
									AnimationComponentShadow["walk"].normalizedTime = AnimationComponent["walk"].normalizedTime;
								}else{
									modelUpAmt = Mathf.MoveTowards(modelUpRun, modelUp, Time.smoothDeltaTime * 2.0f);
									modelPitchAmt = Mathf.MoveTowards(modelPitchAmt, modelPitch, 6f * Time.timeScale);
									AnimationComponent["run"].speed = -sprintAnimSpeed * speedMax;
									AnimationComponent.CrossFade("run", 0.25f);
									AnimationComponentShadow["run"].speed = -sprintAnimSpeed * speedMax;
									AnimationComponentShadow.CrossFade("run", 0.25f);
									AnimationComponentShadow["run"].normalizedTime = AnimationComponent["run"].normalizedTime;
								}
							}
						}
						if(InputComponent.moveX > 0.1f){//play strafing animations and rotate to strafing angle
							if(walkerComponent.inputY > 0.1f){
								rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, strafeAngle, 220.0f * Time.smoothDeltaTime);
							}else{
								rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, -strafeAngle, 220.0f * Time.smoothDeltaTime);
							}
						}else if(InputComponent.moveX < -0.1f){
							if(walkerComponent.inputY > 0.1f){
								rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, -strafeAngle, 220.0f * Time.smoothDeltaTime);
							}else{
								rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, strafeAngle, 220.0f * Time.smoothDeltaTime);
							}
						}else{
							rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, 0.0f, 220.0f * Time.smoothDeltaTime);
						}
					}else{
						modelPitchAmt = Mathf.MoveTowards(modelPitchAmt, modelPitch, 2f * Time.timeScale);
						if(InputComponent.moveX > 0.1f){
							modelRightMod = Mathf.Lerp(modelRightMod, 0.0f, Time.smoothDeltaTime * 7.0f);
							rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, 0.0f, 220.0f * Time.smoothDeltaTime);
							if(walkerComponent.crouched){
								AnimationComponent.CrossFade("crouchstraferight", 0.3f);
								AnimationComponentShadow.CrossFade("crouchstraferight", 0.3f);
								AnimationComponent["crouchstraferight"].speed = crouchWalkAnimSpeed * speedMax;
								AnimationComponentShadow["crouchstraferight"].speed = crouchWalkAnimSpeed * speedMax;
								modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpCrouch, Time.smoothDeltaTime * 1.5f);
							}else{
								AnimationComponent.CrossFade("straferight", 0.3f);
								AnimationComponentShadow.CrossFade("straferight", 0.3f);
								if(!walkerComponent.sprintActive){
									AnimationComponent["straferight"].speed = 1.27f * speedMax;
									AnimationComponentShadow["straferight"].speed = 1.27f * speedMax;
								}else{
									AnimationComponent["straferight"].speed = 2.0f * speedMax;
									AnimationComponentShadow["straferight"].speed = 2.0f * speedMax;
								}
								modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUp, Time.smoothDeltaTime * 2.0f);
							}
						}else if(InputComponent.moveX < -0.1f){
							modelRightMod = Mathf.Lerp(modelRightMod, 0.0f, Time.smoothDeltaTime * 7.0f);
							rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, 0.0f, 220.0f * Time.smoothDeltaTime);
							if(walkerComponent.crouched){
								AnimationComponent.CrossFade("crouchstrafeleft", 0.3f);
								AnimationComponentShadow.CrossFade("crouchstrafeleft", 0.3f);
								AnimationComponent["crouchstrafeleft"].speed = crouchWalkAnimSpeed * speedMax;
								AnimationComponentShadow["crouchstrafeleft"].speed = crouchWalkAnimSpeed * speedMax;
								modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpCrouch, Time.smoothDeltaTime * 1.5f);
							}else{
								AnimationComponent.CrossFade("strafeleft", 0.3f);
								AnimationComponentShadow.CrossFade("strafeleft", 0.3f);
								if(!walkerComponent.sprintActive){
									AnimationComponent["strafeleft"].speed = 1.27f * speedMax;
									AnimationComponentShadow["strafeleft"].speed = 1.27f * speedMax;
								}else{
									AnimationComponent["strafeleft"].speed = 2.0f * speedMax;
									AnimationComponentShadow["strafeleft"].speed = 2.0f * speedMax;
								}
								modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUp, Time.smoothDeltaTime * 2.0f);
							}
						}else{
							if(!SmoothMouseLookComponent.tpIdleCamRotate){
								smoothedTurnAmt = Mathf.Lerp(smoothedTurnAmt, SmoothMouseLookComponent.horizontalDelta, Time.smoothDeltaTime * 10.0f);
							}else{
								smoothedTurnAmt = 0.0f;//don't play strafing anims for turning if player is rotating camera around in third person mode and not moving
							}
							if(smoothedTurnAmt < -0.4){
								modelRightMod = Mathf.Lerp(modelRightMod, idleRight, Time.smoothDeltaTime * 7.0f);
								rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, 0.0f, 2.0f);
								if(walkerComponent.crouched){
									AnimationComponent["crouchstraferight"].speed = 0.7f;
									AnimationComponentShadow["crouchstraferight"].speed = 0.7f;
									AnimationComponent.CrossFade("crouchstraferight", 0.4f);
									AnimationComponentShadow.CrossFade("crouchstraferight", 0.4f);
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpCrouch, Time.smoothDeltaTime * 1.5f);
								}else{
									AnimationComponent["straferight"].speed = 1.0f;
									AnimationComponentShadow["straferight"].speed = 1.0f;
									AnimationComponent.CrossFade("straferight", 0.4f);
									AnimationComponentShadow.CrossFade("straferight", 0.4f);
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUp, Time.smoothDeltaTime * 2.0f);
								}
							}else if(smoothedTurnAmt > 0.4){
								modelRightMod = Mathf.Lerp(modelRightMod, idleRight, Time.smoothDeltaTime * 7.0f);
								rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, 0.0f, 1.0f);
								if(walkerComponent.crouched){
									AnimationComponent["crouchstrafeleft"].speed = 0.7f;
									AnimationComponentShadow["crouchstrafeleft"].speed = 0.7f;
									AnimationComponent.CrossFade("crouchstrafeleft", 0.4f);
									AnimationComponentShadow.CrossFade("crouchstrafeleft", 0.4f);
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpCrouch, Time.smoothDeltaTime * 1.5f);
								}else{
									AnimationComponent["strafeleft"].speed = 1.0f;
									AnimationComponentShadow["strafeleft"].speed = 1.0f;
									AnimationComponent.CrossFade("strafeleft", 0.4f);
									AnimationComponentShadow.CrossFade("strafeleft", 0.4f);
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUp, Time.smoothDeltaTime * 2.0f);
								}
							}else{//play idle animation and rotate to idle angles
								if(walkerComponent.crouched){
									modelRightMod = Mathf.Lerp(modelRightMod, crouchRight, Time.smoothDeltaTime * 7.0f);
									rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, crouchAngle, 4.0f);
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUpCrouch, Time.smoothDeltaTime * 1.5f);
									AnimationComponent.CrossFade("crouchidle", 0.4f);
									AnimationComponentShadow.CrossFade("crouchidle", 0.4f);
								}else{
									modelRightMod = Mathf.Lerp(modelRightMod, idleRight, Time.smoothDeltaTime * 7.0f);
									rotAngleAmt = Mathf.MoveTowards(rotAngleAmt, idleAngle, 4.0f);
									modelUpAmt = Mathf.MoveTowards(modelUpAmt, modelUp, Time.smoothDeltaTime * 2.0f);
									AnimationComponent.CrossFade("idle2", 0.4f);
									AnimationComponentShadow.CrossFade("idle2", 0.4f);
								}
							}
						}
					}
				}
			}
		}
	}
	
}
