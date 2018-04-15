//Ironsights.cs by Azuline Studios© All Rights Reserved
//Adjusts weapon position and bobbing speeds and magnitudes 
//for various player states like zooming, sprinting, and crouching.
using UnityEngine;
using System.Collections;

public class Ironsights : MonoBehaviour {
	//Set up external script references
	[HideInInspector]
	public SmoothMouseLook SmoothMouseLook;
	private PlayerWeapons PlayerWeaponsComponent;
	private FPSRigidBodyWalker FPSWalker;
	[HideInInspector]
	public VerticalBob VerticalBob;
	[HideInInspector]
	public HorizontalBob HorizontalBob;
	private FPSPlayer FPSPlayerComponent;
	private InputControl InputComponent;
	[HideInInspector]
	public WeaponBehavior WeaponBehaviorComponent;
	[HideInInspector]
	public WeaponPivot WeaponPivotComponent;
	private Animation GunAnimationComponent;
	//other objects accessed by this script
	[HideInInspector]
	public GameObject playerObj;
	[HideInInspector]
	public GameObject weaponObj;
	[HideInInspector]
	public GameObject CameraObj;
	[HideInInspector]
	public Camera WeapCameraObj;
	//weapon object (weapon object child) set by PlayerWeapons.cs script
	[HideInInspector]
	public GameObject gunObj;
	//Var set to sprint animation time of weapon
	[HideInInspector]
	public Transform gun;//this set by PlayerWeapons script to active weapon transform
		
	//weapon positioning	
	private float nextPosX = 0.0f;//weapon x position that is smoothed using smoothDamp function
	private float nextPosY = 0.0f;//weapon y position that is smoothed using smoothDamp function
	private float nextPosZ = 0.0f;//weapon z position that is smoothed using smoothDamp function
	private float zPosRecNext = 0.0f;//weapon recoil z position that is smoothed using smoothDamp function
	private float newPosX = 0.0f;//target weapon x position that is smoothed using smoothDamp function
	private float newPosY = 0.0f;//target weapon y position that is smoothed using smoothDamp function
	private float newPosZ = 0.0f;//target weapon z position that is smoothed using smoothDamp function
	private float zPosRec = 0.0f;//target weapon recoil z position that is smoothed using smoothDamp function
	private Vector3 dampVel = Vector3.zero;//velocities that are used by smoothDamp function
	private float recZDamp = 0.0f;//velocity that is used by smoothDamp function
	private Vector3 tempGunPos = Vector3.zero;

	//camera FOV handling
	[Tooltip("Default camera field of view value.")]
	public float defaultFov = 75.0f;
	[Tooltip("Default camera field of view value while sprinting.")]
	public float sprintFov = 85.0f;
	[Tooltip("Amount to subtract from main camera FOV for weapon camera FOV.")]
	public float weaponCamFovDiff = 20.0f;//amount to subtract from main camera FOV for weapon camera FOV
	[HideInInspector]
	public float nextFov = 75.0f;//camera field of view that is smoothed using smoothDamp
	[HideInInspector]
	public float newFov = 75.0f;//camera field of view that is smoothed using smoothDamp
	private float FovSmoothSpeed = 0.15f;//speed that camera FOV is smoothed
	private float dampFOV = 0.0f;//target weapon z position that is smoothed using smoothDamp function
		
	//zooming
	public enum zoomType{
		hold,
		toggle,
		both
	}
	[HideInInspector]
	public bool dzAiming;//true if deadzone aiming
	[Tooltip("User may set zoom mode to toggle, hold, or both (toggle on zoom button press, hold on zoom button hold).")]
	public zoomType zoomMode = zoomType.both;
	public float zoomSensitivity = 0.5f;//percentage to reduce mouse sensitivity when zoomed
	public AudioClip sightsUpSnd;
	public AudioClip sightsDownSnd;
	[HideInInspector]
	public bool zoomSfxState = true;//var for only playing sights sound effects once
	[HideInInspector]
	public bool reloading = false;//this variable true when player is reloading
	
	[Header ("Bobbing Speeds and Amounts", order = 0)]
	[Space (10, order = 1)]
	[Tooltip("Amount to roll the screen left or right when strafing and sprinting.")]
	public float sprintStrafeRoll = 2.0f;
	[Tooltip("Amount to roll the screen left or right when strafing and walking.")]
	public float walkStrafeRoll = 1.0f;
	[Tooltip("Amount to roll the screen left or right when moving view horizontally.")]
	public float lookRoll = 1f;
	[Tooltip("Amount to roll the screen left or right when moving view horizontally during bullet time.")]
	public float btLookRoll = 1f;
	[Tooltip("Amount to roll the screen left or right when moving view horizontally and underwater.")]
	public float swimLookRoll = 1f;
	[Tooltip("Speed to return to neutral roll values when above water.")]
	public float rollReturnSpeed = 4.0f;
	[Tooltip("Speed to return to neutral roll values when underwater.")]
	public float rollReturnSpeedSwim = 2.0f;
	[Tooltip("Amount the camera should bob vertically to simulate player breathing.")]
	public float idleBobAmt = 1f;
	[Tooltip("Amount the camera should bob vertically to simulate floating in water.")]
	public float swimBobAmt = 1f;//true if camera should bob slightly up and down when player is swimming
	
	private float strafeSideAmt;//amount to move weapon left or right when strafing
	private float pivotBobAmt;
	
	//bobbing speeds and amounts for player movement states	
	public float WalkHorizontalBobAmount = 0.05f;
	public float WalkVerticalBobAmount = 0.11f;
	public float WalkBobPitchAmount = 0.0175f;
	public float WalkBobRollAmount = 0.01f;
	public float WalkBobYawAmount = 0.01f;
	public float WalkBobSpeed = 12f;
	
	public float CrouchHorizontalBobAmount = 0.075f;
	public float CrouchVerticalBobAmount = 0.11f;
	public float CrouchBobPitchAmount = 0.025f;
	public float CrouchBobRollAmount = 0.055f;
	public float CrouchBobYawAmount = 0.055f;
	public float CrouchBobSpeed = 8f;
	
	public float ProneHorizontalBobAmount = 0.25f;
	public float ProneVerticalBobAmount = 0.075f;
	public float ProneBobPitchAmount = 0.04f;
	public float ProneBobRollAmount = 0.03f;
	public float ProneBobYawAmount = 0.1f;
	public float ProneBobSpeed = 8f;
	
	public float ZoomHorizontalBobAmount = 0.016f;
	public float ZoomVerticalBobAmount = 0.0075f;
	public float ZoomBobPitchAmount = 0.001f;
	public float ZoomBobRollAmount = 0.008f;
	public float ZoomBobYawAmount = 0.008f;
	public float ZoomBobSpeed = 8f;
		
	public float SprintHorizontalBobAmount = 0.135f;
	public float SprintVerticalBobAmount = 0.16f;
	public float SprintBobPitchAmount = 0.12f;
	public float SprintBobRollAmount = 0.075f;
	public float SprintBobYawAmount = 0.075f;
	public float SprintBobSpeed = 19f;
		
	//gun X position amount for tweaking ironsights position
	private float horizontalGunPosAmt = -0.02f;
	private float weaponSprintXPositionAmt = 0.0f;
	//vars to scale up bob speeds slowly to prevent jerky transitions
	private float HBamt = 0.075f;//dynamic head bobbing variable
	private float HRamt = 0.125f;//dynamic head rolling variable
	private float HYamt = 0.125f;//dynamic head yawing variable
	private float HPamt = 0.1f;//dynamic head pitching variable
	private float GBamt = 0.075f;//dynamic gun bobbing variable
	//weapon sprinting positioning
	private float gunup = 0.015f;//amount to move weapon up while sprinting
	private float gunRunSide = 1.0f;//to control horizontal bobbing of weapon during sprinting
	private float gunRunUp = 1.0f;//to control vertical bobbing of weapon during sprinting
	private float sprintBob = 0.0f;//to modify weapon bobbing speeds when sprinting
	private float sprintBobAmtX = 0.0f;//actual horizontal weapon bobbing speed when sprinting
	private float sprintBobAmtY = 0.0f;//actual vertical weapon bobbing speed when sprinting
	//weapon positioning
	private float yDampSpeed= 0.0f;//this value used to control speed that weapon Y position is smoothed
	private float zDampSpeed= 0.0f;//this value used to control speed that weapon Z position is smoothed
	private float bobDir = 0.0f;//positive or negative direction of bobbing
	private float bobMove = 0.0f;
	private float sideMove = 0.0f;
	[HideInInspector]
	public float switchMove = 0.0f;//for moving weapon down while switching weapons
	[HideInInspector]
	public float climbMove = 0.0f;//for moving weapon down while climbing
	private float jumpmove = 0.0f;//for moving weapon down while jumping
	[HideInInspector]
	public float jumpAmt = 0.0f;
	private float idleX = 0.0f;//amount of weapon movement when idle
	private float idleY = 0.0f;
	[HideInInspector]
	public float side = 0.0f;//amount to sway weapon position horizontally
	[HideInInspector]
	public float raise = 0.0f;//amount to sway weapon position vertically
	[HideInInspector]
	public float gunAnimTime = 0.0f;
	
	private float proneSwayX;
	private float proneSwayY;

	private AudioSource aSource;
	[Tooltip("Point to rotate weapon models for vertical bobbing effect.")]
	public Transform pivot;
	private float pivotAmt;
	private float dampVel2;
	private float rotateAmtNeutral;
	
	[HideInInspector]
	public float gunPosBack;
	[HideInInspector]
	public float gunPosUp;
	[HideInInspector]
	public float gunPosSide;
	
	void Start(){
		//Set up external script references
		SmoothMouseLook = CameraObj.GetComponent<SmoothMouseLook>();
		PlayerWeaponsComponent = weaponObj.GetComponent<PlayerWeapons>();
		FPSWalker = playerObj.GetComponent<FPSRigidBodyWalker>();
		VerticalBob = playerObj.GetComponent<VerticalBob>();
		HorizontalBob = playerObj.GetComponent<HorizontalBob>();
		FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		InputComponent = playerObj.GetComponent<InputControl>();
		WeaponPivotComponent = FPSPlayerComponent.WeaponPivotComponent;

		aSource = playerObj.AddComponent<AudioSource>(); 
		aSource.spatialBlend = 0.0f;
		aSource.playOnAwake = false;
		
	}
	
	void Update (){
		
		if(Time.timeScale > 0.0f && Time.deltaTime > 0.0f && Time.smoothDeltaTime > 0.0f){//allow pausing by setting timescale to 0

			GunAnimationComponent = gunObj.GetComponent<Animation>();
				
			if(SmoothMouseLook.playerMovedTime + 0.1 < Time.time || FPSWalker.moving){
				//main weapon position smoothing happens here
				newPosX = Mathf.SmoothDamp(newPosX, nextPosX, ref dampVel.x, yDampSpeed, Mathf.Infinity, Time.deltaTime);
				newPosY = Mathf.SmoothDamp(newPosY, nextPosY, ref dampVel.y, yDampSpeed, Mathf.Infinity, Time.deltaTime);
				newPosZ = Mathf.SmoothDamp(newPosZ, nextPosZ, ref dampVel.z, zDampSpeed, Mathf.Infinity, Time.deltaTime);
				zPosRec = Mathf.SmoothDamp(zPosRec, zPosRecNext, ref recZDamp, 0.25f, Mathf.Infinity, Time.deltaTime);//smooth recoil kick back of weapon
			}else{
				//main weapon position smoothing happens here
				newPosX = nextPosX;
				newPosY = nextPosY;
				newPosZ = nextPosZ;
				zPosRec = zPosRecNext;
			}
			
			newFov = Mathf.SmoothDamp(Camera.main.fieldOfView, nextFov, ref dampFOV, FovSmoothSpeed, Mathf.Infinity, Time.deltaTime);//smooth camera FOV
			
			//Sync camera FOVs
			if(WeapCameraObj){
				WeapCameraObj.fieldOfView = Camera.main.fieldOfView - weaponCamFovDiff;
			}
			Camera.main.fieldOfView = newFov;
			//Get input from player movement script
			float horizontal = FPSWalker.inputX;
			float vertical = FPSWalker.inputY;
			
			//move gun back if collision is detected on view model - usused
//			if(WeaponBehaviorComponent.doBarrelCheck){
////				if(SmoothMouseLook.inputY > -75){
//					if(WeaponBehaviorComponent.hitDist > 0f /*&& !FPSPlayerComponent.zoomed*/ && !FPSWalker.sprintActive){
//						gunPosBack = WeaponBehaviorComponent.forwardDetectAmt * WeaponBehaviorComponent.barrelForwardAmt;
//						gunPosUp = gunPosBack * WeaponBehaviorComponent.barrelUpAmt;
//						gunPosSide = gunPosBack * WeaponBehaviorComponent.barrelRightAmt;
//					}else{
//						gunPosBack = 0f;
//						gunPosUp = 0f;
//						gunPosSide = 0f;
//					}
////				}else{//move gun back if looking down
////					gunPosBack = (WeaponBehaviorComponent.barrelCheckDist * 0.6f) * WeaponBehaviorComponent.barrelForwardAmt;
////					gunPosUp = gunPosBack * WeaponBehaviorComponent.barrelUpAmt;
////					gunPosSide = gunPosBack * WeaponBehaviorComponent.barrelRightAmt;
////				}
//			}
			
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Adjust weapon position and bobbing amounts dynamicly based on movement and player states
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			//move weapon back towards camera based on kickBack amount in WeaponBehavior.cs					
			if(WeaponBehaviorComponent.shootStartTime + 0.1f > Time.time){
				if(FPSPlayerComponent.zoomed){
					zPosRecNext = WeaponBehaviorComponent.kickBackAmtZoom;	
				}else{
					zPosRecNext = WeaponBehaviorComponent.kickBackAmtUnzoom;	
				}
			}else{
				if(WeaponBehaviorComponent.pullBackAmt != 0.0f){
					zPosRecNext = WeaponBehaviorComponent.pullBackAmt * WeaponBehaviorComponent.fireHoldMult;
				}else{
					zPosRecNext = 0.0f;
				}
			}
				
			if(FPSWalker.moving){
				idleY = 0;
				idleX = 0;
				//check for sprinting or prone
				if(
					(
						(//player has stood up from crouch or prone and is sprinting
							FPSWalker.sprintActive
							&& !FPSWalker.crouched
							&& !FPSWalker.cancelSprint
							&& (FPSWalker.midPos >= FPSWalker.standingCamHeight && FPSWalker.proneRisen)	
						) 
						|| (!reloading && (!FPSWalker.proneRisen && !FPSWalker.crouched) 
					    && (FPSWalker.prone || FPSWalker.sprintActive))//player is prone
					)
				&& FPSWalker.fallingDistance < 0.75f	
				&& !FPSPlayerComponent.zoomed
				&& !FPSWalker.jumping
				){
					
					sprintBob = 128.0f;
					
					if (!FPSWalker.cancelSprint
					&& (!reloading || FPSWalker.sprintReload)
					&& FPSWalker.fallingDistance < 0.75f
					&& !FPSWalker.jumping){//actually sprinting now
						//set the camera's fov back to normal if the player has sprinted into a wall, but the sprint is still active
						if(((FPSWalker.inputY != 0 && FPSWalker.forwardSprintOnly) || (!FPSWalker.forwardSprintOnly && FPSWalker.moving))
						&& !FPSWalker.prone){
							nextFov = sprintFov;
						}else{
							nextFov = defaultFov;	
						}
						
						if(!reloading){
							//gradually move weapon more towards center while sprinting
							weaponSprintXPositionAmt = Mathf.MoveTowards(weaponSprintXPositionAmt, WeaponBehaviorComponent.weaponSprintXPosition, Time.deltaTime * 16);
							horizontalGunPosAmt = WeaponBehaviorComponent.weaponUnzoomXPosition + weaponSprintXPositionAmt;
							gunRunSide = 2.0f;
							
							if(gunRunUp < 1.4f){gunRunUp += Time.deltaTime / 4.0f;}//gradually increase for smoother transition
							bobMove = gunup + WeaponBehaviorComponent.weaponSprintYPosition;//raise weapon while sprinting
							sideMove = 0.0f;
						}else{//weapon positioning for sprinting and reloading
							gunRunSide = 1.0f;
							gunRunUp = 1.0f;
							sprintBob = 216;
							bobMove = 0.0f;
							sideMove = 0.0f;
							weaponSprintXPositionAmt = Mathf.MoveTowards(weaponSprintXPositionAmt, 0, Time.deltaTime * 16);
							horizontalGunPosAmt = WeaponBehaviorComponent.weaponUnzoomXPosition + weaponSprintXPositionAmt;
						}
						
					}else{//not sprinting
						nextFov = defaultFov;
						gunRunSide = 1.0f;
						gunRunUp = 1.0f;
						bobMove = -0.01f;
						//make this check to prevent weapon occasionally not lowering during switch while prone and moving 
						if(!FPSWalker.prone){
							switchMove = 0.0f;
						}
					}
				}else{//walking
					gunRunSide = 1.0f;
					gunRunUp = 1.0f;
					//reset horizontal weapon positioning var and make sure it returns to zero when not sprinting to prevent unwanted side movement
					weaponSprintXPositionAmt = Mathf.MoveTowards(weaponSprintXPositionAmt, 0, Time.deltaTime * 16);
					horizontalGunPosAmt = WeaponBehaviorComponent.weaponUnzoomXPosition + weaponSprintXPositionAmt;
					if(reloading){//move weapon position up when reloading and moving for full view of animation
						nextFov = defaultFov;
						sprintBob = 216;
						bobMove = 0.0f;
						sideMove = 0.0f;
					}else{
						nextFov = defaultFov;
						if(FPSPlayerComponent.zoomed && WeaponBehaviorComponent.meleeSwingDelay == 0) {//zoomed and not melee weapon
							sprintBob = 96.0f;
							bobMove = 0.0F;//move weapon to idle
						}else{//not zoomed
							sprintBob = 216.0f;
							if(FPSWalker.moving){
								//move weapon down and left when crouching
								if (FPSWalker.crouched || FPSWalker.midPos < FPSWalker.standingCamHeight * 0.85f) {
									bobMove = -0.01f;
									sideMove = -0.0125f;
								}else{
									bobMove = -0.008f;
									sideMove = 0.00f;
								}
							}else{
								//move weapon to idle
								bobMove = 0.0F;
								sideMove = 0.0F;
							}
						}
					}
				}
			}else{//if not moving (no player movement input)
				nextFov = defaultFov;
				horizontalGunPosAmt = WeaponBehaviorComponent.weaponUnzoomXPosition;
				if(weaponSprintXPositionAmt > 0){weaponSprintXPositionAmt -= Time.deltaTime / 4;}
				sprintBob = 96.0f;
				if(reloading){
					nextFov = defaultFov;
					sprintBob = 96.0f;
					bobMove = 0.0F;
					sideMove = 0.0f;
				}else{
					//move weapon to idle
					if((FPSWalker.crouched || FPSWalker.midPos < FPSWalker.standingCamHeight * 0.85f) && !FPSPlayerComponent.zoomed) {
						bobMove = -0.005f;
						sideMove = -0.0125f;
					}else{
						bobMove = 0.0f;
						sideMove = 0.0f;
					}
				}
				//weapon idle motion
				if(FPSPlayerComponent.zoomed && WeaponBehaviorComponent.meleeSwingDelay == 0) {
					idleX = Mathf.Sin(Time.time * 1.25f) * 0.0005f * WeaponBehaviorComponent.zoomIdleSwayAmt;
					idleY = Mathf.Sin(Time.time * 1.5f) * 0.0005f * WeaponBehaviorComponent.zoomIdleSwayAmt;
				}else{
					if(!FPSWalker.swimming){
						idleX = Mathf.Sin(Time.time * 1.25f) * 0.0012f * WeaponBehaviorComponent.idleSwayAmt;
						idleY = Mathf.Sin(Time.time * 1.5f) * 0.0012f * WeaponBehaviorComponent.idleSwayAmt;
					}else{
						idleX = Mathf.Sin(Time.time * 1.25f) * 0.003f * WeaponBehaviorComponent.swimIdleSwayAmt;
						idleY = Mathf.Sin(Time.time * 1.5f) * 0.003f * WeaponBehaviorComponent.swimIdleSwayAmt;	
					}
				}
			}
			
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Weapon Swaying/Bobbing while moving
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			
			//track X axis while walking for side to side bobbing effect	
			if(HorizontalBob.waveslice != 0){bobDir = 1;}else{bobDir = -1;}
				
			//Reduce weapon bobbing by sprintBobAmount value defined in WeaponBehavior script.
			//This is for fine tuning of weapon bobbing. Pistols look better with less sprint bob 
			//because they use a different sprinting anim and have a different sprinting position 
			//than the animation used by rifle-type weapons.
			if(!FPSWalker.canRun){
				sprintBobAmtX = sprintBob / WeaponBehaviorComponent.walkBobAmountX;
				sprintBobAmtY = sprintBob / WeaponBehaviorComponent.walkBobAmountY;
			}else{
				sprintBobAmtX = sprintBob / WeaponBehaviorComponent.sprintBobAmountX;
				sprintBobAmtY = sprintBob / WeaponBehaviorComponent.sprintBobAmountY;
			}
			
			//increase weapon bobbing amounts for prone
			if(FPSWalker.allowProne
			&& FPSWalker.midPos <= FPSWalker.crouchingCamHeight 
			&& FPSWalker.prone 
			&& FPSWalker.moving){
				if(!WeaponBehaviorComponent.PistolSprintAnim){
					proneSwayX = 3.5f;//x
					proneSwayY = 4.5f;//y
				}else{//less prone bobbing for pistol-type weapons
					proneSwayX = 1.5f;
					proneSwayY = 2.5f;	
				}
			}else{
				proneSwayX = 1.0f;
				proneSwayY = 1.0f;
			}
				
			//set smoothed weapon position to actual gun position vector
			tempGunPos.x = newPosX;
			tempGunPos.y = newPosY;
			tempGunPos.z = newPosZ + zPosRec;//add weapon z position and recoil kick back
			
			//apply temporary vector to gun's transform position
			gun.localPosition = tempGunPos;
			
			
			if(gun.transform.parent.transform.localEulerAngles.x < 180.0f){
				rotateAmtNeutral = -gun.transform.parent.transform.localEulerAngles.x;
			}else{
				rotateAmtNeutral = 360.0f - gun.transform.parent.transform.localEulerAngles.x;
			}
			
			//compensate for floating point imprecision in RotateAround when player is a large distance from scene origin
			gun.transform.parent.transform.localPosition = Vector3.MoveTowards(gun.transform.parent.transform.localPosition, Vector3.zero, 0.005f * Time.smoothDeltaTime);
				
			if(!FPSPlayerComponent.zoomed || WeaponPivotComponent.deadzoneZooming){
				pivotBobAmt = WeaponBehaviorComponent.pivotBob;
			}else{
				pivotBobAmt = 0f;
			}
			
			//rotate weapon vertically along pivot for bobbing effect
			pivotAmt = Mathf.SmoothDampAngle(pivotAmt, ((-WeaponPivotComponent.mouseOffsetTarg.x * pivotBobAmt)) + rotateAmtNeutral * 0.2f, ref dampVel2, Time.smoothDeltaTime);
			gun.transform.parent.transform.RotateAround(pivot.position, gun.transform.parent.transform.right, pivotAmt);
			
		
			//lower weapon when jumping, falling, or slipping off ledge
			if(FPSWalker.jumping || FPSWalker.fallingDistance > 1.25f){
				//lower weapon less when zoomed
				if (!FPSPlayerComponent.zoomed){
					//raise weapon when jump is ascending and lower when descending
					if((FPSWalker.airTime + 0.175f) > Time.time){
						jumpmove = 0.015f;
					}else{
						jumpmove = -0.025f;
					}
				}else{
					jumpmove = -0.01f;
				}
			}else{
				jumpmove = 0.0f;
			}
		   
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Adjust vars for zoom and other states
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				
			float deltaAmount = Time.smoothDeltaTime * 100;//define delta for framerate independence
			float bobDeltaAmount = 0.12f / Time.smoothDeltaTime;//define bobbing delta for framerate independence
		  	
			if(!WeaponBehaviorComponent.PistolSprintAnim || !FPSWalker.canRun){
				gunAnimTime = GunAnimationComponent["RifleSprinting"].normalizedTime;//Track playback position of rifle sprinting animation
			}else{
				gunAnimTime = GunAnimationComponent["PistolSprinting"].normalizedTime;//Track playback position of pistol sprinting animation	
			}
		   
			//if zoomed
			//check time of weapon sprinting anim to make weapon return to center, then zoom normally 
			if((FPSPlayerComponent.zoomed || (FPSPlayerComponent.canBackstab && !WeaponBehaviorComponent.shooting))
			&& FPSPlayerComponent.hitPoints > 1.0f
			&& PlayerWeaponsComponent.switchTime + WeaponBehaviorComponent.readyTime < Time.time//don't raise sights when readying weapon 
			&& !reloading 
			&& gunAnimTime < 0.35f 
			//&& WeaponBehaviorComponent.meleeSwingDelay == 0//not a melee weapon
			&& PlayerWeaponsComponent.currentWeapon != 0
			//move weapon to zoom values if zoomIsBlock is true, also
			&& (FPSPlayerComponent.canBackstab || (((WeaponBehaviorComponent.zoomIsBlock && ((WeaponBehaviorComponent.shootStartTime + WeaponBehaviorComponent.fireRate < Time.time && !WeaponBehaviorComponent.shootFromBlock) || WeaponBehaviorComponent.shootFromBlock)) || !WeaponBehaviorComponent.zoomIsBlock)))
			&& WeaponBehaviorComponent.reloadLastStartTime + WeaponBehaviorComponent.reloadLastTime < Time.time){

				if(!dzAiming){
					if(!reloading){
						strafeSideAmt = Mathf.MoveTowards(strafeSideAmt, WeaponBehaviorComponent.strafeSideZoom, Time.deltaTime * 16);
					}else{
						strafeSideAmt = Mathf.MoveTowards(strafeSideAmt, 0f, Time.deltaTime * 16);
					}
					if(!FPSPlayerComponent.canBackstab){
						//X pos with idle movement
						nextPosX = WeaponBehaviorComponent.weaponZoomXPosition + (side / 1.5f) + idleX + gunPosSide + (FPSWalker.inputX * 0.1f * strafeSideAmt)
							//also add X bobbing amounts here so they get smoothed to remove any small glitches in position
							+ (((HorizontalBob.translateChange / sprintBobAmtX) * bobDeltaAmount) * gunRunSide * bobDir);
						//Y pos with idle movement
						nextPosY = WeaponBehaviorComponent.weaponZoomYPosition + (raise / 1.5f) + idleY + (bobMove + switchMove + climbMove + jumpAmt + jumpmove + gunPosUp) 
							//also add Y bobbing amounts here so they get smoothed to remove any small glitches in position
							+ (((VerticalBob.translateChange / sprintBobAmtY) * bobDeltaAmount) * gunRunUp * bobDir);
						//Z pos
						nextPosZ = WeaponBehaviorComponent.weaponZoomZPosition + gunPosBack;
					}else{
						//X pos with idle movement
						nextPosX = WeaponBehaviorComponent.weaponBackstabXPosition + (side / 1.5f) + idleX + gunPosSide + (FPSWalker.inputX * 0.1f * strafeSideAmt)
							//also add X bobbing amounts here so they get smoothed to remove any small glitches in position
							+ (((HorizontalBob.translateChange / sprintBobAmtX) * bobDeltaAmount) * gunRunSide * bobDir);
						//Y pos with idle movement
						nextPosY = WeaponBehaviorComponent.weaponBackstabYPosition + (raise / 1.5f) + idleY + (bobMove + switchMove + climbMove + jumpAmt + jumpmove + gunPosUp) 
							//also add Y bobbing amounts here so they get smoothed to remove any small glitches in position
							+ (((VerticalBob.translateChange / sprintBobAmtY) * bobDeltaAmount) * gunRunUp * bobDir);
						//Z pos
						nextPosZ = WeaponBehaviorComponent.weaponBackstabZPosition + gunPosBack;
					}

					nextFov = WeaponBehaviorComponent.zoomFOV;

					//If not a melee weapon, play sound effect when raising sights
					if(zoomSfxState && WeaponBehaviorComponent.meleeSwingDelay == 0 && !WeaponBehaviorComponent.unarmed){
						aSource.clip = sightsUpSnd;
						aSource.volume = 1.0f;
						aSource.pitch = 1.0f * Time.timeScale;
						aSource.Play();
						zoomSfxState = false;
					}
				}else{//zooming with deadzone (goldeneye/perfect dark style)
					if(!reloading){
						strafeSideAmt = Mathf.MoveTowards(strafeSideAmt, WeaponBehaviorComponent.strafeSideUnzoom, Time.deltaTime * 16);
					}else{
						strafeSideAmt = Mathf.MoveTowards(strafeSideAmt, 0f, Time.deltaTime * 16);
					}
					//X pos with idle movement
					nextPosX = side + idleX + gunPosSide + sideMove + horizontalGunPosAmt + (FPSWalker.leanAmt / 60.0f) + (FPSWalker.inputX * 0.1f * strafeSideAmt)
						//also add X bobbing amounts here so they get smoothed to remove any small glitches in position
						+ (((HorizontalBob.translateChange / sprintBobAmtX * proneSwayX) * bobDeltaAmount) * gunRunSide * bobDir);
					//Y pos with idle movement
					nextPosY = raise + idleY + (bobMove + climbMove + switchMove + jumpAmt + jumpmove + gunPosUp) + WeaponBehaviorComponent.weaponUnzoomYPosition
						//also add Y bobbing amounts here so they get smoothed to remove any small glitches in position
						+ (((VerticalBob.translateChange / sprintBobAmtY * proneSwayY) * bobDeltaAmount) * gunRunUp * bobDir);
					//Z pos
					nextPosZ = WeaponBehaviorComponent.weaponUnzoomZPosition + gunPosBack;
					nextFov = WeaponBehaviorComponent.zoomFOVDz;
				}

				//adjust FOV and weapon position for zoom
				FovSmoothSpeed = 0.09f;//faster FOV zoom speed when zooming in
				yDampSpeed = 0.09f;
				zDampSpeed = 0.15f;
				
				if(!FPSPlayerComponent.canBackstab){
					//slow down turning and movement speed for zoom
					FPSWalker.zoomSpeed = true;
				
					if(!WeaponBehaviorComponent.zoomIsBlock){
						//Reduce mouse sensitivity when zoomed, but maintain sensitivity set in SmoothMouseLook script
						SmoothMouseLook.sensitivityAmt = SmoothMouseLook.sensitivity * WeaponBehaviorComponent.zoomSensitivity;
					}
					
					//Gradually increase or decrease bobbing amounts for smooth transitions between movement states
						
					////zoomed bobbing amounts////
					//horizontal bobbing 
					GBamt = Mathf.MoveTowards(GBamt, ZoomHorizontalBobAmount, Time.smoothDeltaTime * 0.5f);
					//vertical bobbing	
					HBamt = Mathf.MoveTowards(HBamt, ZoomVerticalBobAmount, Time.smoothDeltaTime * 0.5f);
					//pitching	
					HPamt = Mathf.MoveTowards(HPamt, ZoomBobPitchAmount, Time.smoothDeltaTime * 0.75f);
					//rolling	
					HRamt = Mathf.MoveTowards(HRamt, ZoomBobRollAmount, Time.smoothDeltaTime * 0.75f);
					//yawing	
					HYamt = Mathf.MoveTowards(HYamt, ZoomBobYawAmount, Time.smoothDeltaTime * 0.75f);
					
					if(!FPSWalker.swimming){
						//Set bobbing speeds and amounts in other scripts to these smoothed values
						VerticalBob.bobbingSpeed = ZoomBobSpeed;
						//make horizontal bob speed half as slow as vertical bob speed for synchronization of bobbing motions
						HorizontalBob.bobbingSpeed = ZoomBobSpeed / 2.0f;
					}else{
						//Set bobbing speeds and amounts in other scripts to these smoothed values
						VerticalBob.bobbingSpeed = ZoomBobSpeed / 2.0f;
						//make horizontal bob speed half as slow as vertical bob speed for synchronization of bobbing motions
						HorizontalBob.bobbingSpeed = ZoomBobSpeed / 4.0f;	
					}
				}
				VerticalBob.bobbingAmount = HBamt * deltaAmount;//apply delta at this step for framerate independence
				VerticalBob.rollingAmount = HRamt * deltaAmount;
				VerticalBob.yawingAmount = HYamt * deltaAmount;
				VerticalBob.pitchingAmount = HPamt * deltaAmount;
				HorizontalBob.bobbingAmount = GBamt * deltaAmount;
				
			}else{//not zoomed
				
				FovSmoothSpeed = 0.18f;//slower FOV zoom speed when zooming out
				
				//adjust weapon Y position smoothing speed for unzoom and switching weapons
				if(!PlayerWeaponsComponent.switching){
					yDampSpeed = 0.18f;//weapon swaying speed
				}else{
					yDampSpeed = 0.2f;//weapon switch raising speed
				}
				zDampSpeed = 0.1f;
				//X pos with idle movement
				nextPosX = side + idleX + gunPosSide + sideMove + horizontalGunPosAmt + (FPSWalker.leanAmt / 60.0f) + (FPSWalker.inputX * 0.1f * strafeSideAmt)
					//also add X bobbing amounts here so they get smoothed to remove any small glitches in position
					+ (((HorizontalBob.translateChange / sprintBobAmtX * proneSwayX) * bobDeltaAmount) * gunRunSide * bobDir);
				//Y pos with idle movement
				nextPosY = raise + idleY + (bobMove + climbMove + switchMove + jumpAmt + jumpmove + gunPosUp) + WeaponBehaviorComponent.weaponUnzoomYPosition
					//also add Y bobbing amounts here so they get smoothed to remove any small glitches in position
					+ (((VerticalBob.translateChange / sprintBobAmtY * proneSwayY) * bobDeltaAmount) * gunRunUp * bobDir);
				
				   //Z pos
				if(!FPSWalker.prone){
					nextPosZ = WeaponBehaviorComponent.weaponUnzoomZPosition + (((HorizontalBob.translateChange * bobDeltaAmount) * bobDir) * 0.003f * WeaponBehaviorComponent.zBobWalk) + gunPosBack;
				}else{
					nextPosZ = WeaponBehaviorComponent.weaponUnzoomZPosition + gunPosBack;
				}
				//Set turning and movement speed for unzoom
				FPSWalker.zoomSpeed = false;	
				//If not a melee weapon, play sound effect when lowering sights	
				if(!zoomSfxState && WeaponBehaviorComponent.meleeSwingDelay == 0 && !WeaponBehaviorComponent.unarmed){
					aSource.clip = sightsDownSnd;
					aSource.volume = 1.0f;
					aSource.pitch = 1.0f * Time.timeScale;
					aSource.Play();
					zoomSfxState = true;
				}
				//Return mouse sensitivity to normal
				SmoothMouseLook.sensitivityAmt = SmoothMouseLook.sensitivity;
				
				//Set weapon and view bobbing amounts
				if (FPSWalker.sprintActive
				&& !(FPSWalker.forwardSprintOnly && (Mathf.Abs(horizontal) != 0.0f) && (Mathf.Abs(vertical) < 0.75f))
				&& (Mathf.Abs(vertical) != 0.0f || (!FPSWalker.forwardSprintOnly && FPSWalker.moving))
				&& !FPSWalker.cancelSprint
				&& !FPSWalker.crouched
				&& !FPSWalker.prone
				&& FPSWalker.midPos >= FPSWalker.standingCamHeight
				&& !FPSPlayerComponent.zoomed
				&& !InputComponent.fireHold){

					if(!reloading){
						strafeSideAmt = Mathf.MoveTowards(strafeSideAmt, WeaponBehaviorComponent.strafeSideSprint, Time.deltaTime * 16);
					}else{
						strafeSideAmt = Mathf.MoveTowards(strafeSideAmt, 0f, Time.deltaTime * 16);
					}
					
					//scale up bob speeds slowly to prevent jerky transition
					if (FPSWalker.grounded){
						////sprinting bobbing amounts////
						//horizontal bobbing 
						GBamt = Mathf.MoveTowards(GBamt, SprintHorizontalBobAmount, Time.smoothDeltaTime * 0.5f);
						//vertical bobbing
						HBamt = Mathf.MoveTowards(HBamt, SprintVerticalBobAmount, Time.smoothDeltaTime * 0.5f);
						//pitching
						HPamt = Mathf.MoveTowards(HPamt, SprintBobPitchAmount, Time.smoothDeltaTime * 0.75f);
						//rolling
						HRamt = Mathf.MoveTowards(HRamt, SprintBobRollAmount, Time.smoothDeltaTime * 0.75f);
						//yawing
						HYamt = Mathf.MoveTowards(HYamt, SprintBobYawAmount, Time.smoothDeltaTime * 0.75f);
						
					}else{
						//reduce bobbing amounts for smooth jumping/landing transition
						HBamt = Mathf.MoveTowards(HBamt, 0f, Time.smoothDeltaTime);
						HRamt = Mathf.MoveTowards(HRamt, 0f, Time.smoothDeltaTime * 2f);
						HYamt = Mathf.MoveTowards(HYamt, 0f, Time.smoothDeltaTime * 2f);
						HPamt = Mathf.MoveTowards(HPamt, 0f, Time.smoothDeltaTime * 2f);
						GBamt = Mathf.MoveTowards(GBamt, 0f, Time.smoothDeltaTime);
					}
					//Set bobbing speeds and amounts in other scripts to these smoothed values
					VerticalBob.bobbingSpeed = SprintBobSpeed;
					//make horizontal bob speed half as slow as vertical bob speed for synchronization of bobbing motions
					HorizontalBob.bobbingSpeed = SprintBobSpeed / 2.0f;
					HorizontalBob.bobbingAmount = GBamt * deltaAmount;//apply delta at this step for framerate independence
					VerticalBob.rollingAmount = HRamt * deltaAmount;
					VerticalBob.yawingAmount = HYamt * deltaAmount;
					VerticalBob.pitchingAmount = HPamt * deltaAmount;
					VerticalBob.bobbingAmount = HBamt * deltaAmount;
					if(!reloading){
						//move weapon toward or away from camera while sprinting
						nextPosZ = WeaponBehaviorComponent.weaponSprintZPosition + (((HorizontalBob.translateChange * bobDeltaAmount) * bobDir) * 0.003f * WeaponBehaviorComponent.zBobSprint) + gunPosBack;
					}

				}else{

					if(!reloading){
						strafeSideAmt = Mathf.MoveTowards(strafeSideAmt, WeaponBehaviorComponent.strafeSideUnzoom, Time.deltaTime * 16);
					}else{
						strafeSideAmt = Mathf.MoveTowards(strafeSideAmt, 0f, Time.deltaTime * 16);
					}
					
					//scale up bob speeds slowly to prevent jerky transition
					if (FPSWalker.grounded) {
						if (!FPSWalker.crouched && !FPSWalker.prone && FPSWalker.midPos >= FPSWalker.standingCamHeight){
							////walking bob amounts///
							//horizontal bobbing 
							GBamt = Mathf.MoveTowards(GBamt, WalkVerticalBobAmount, Time.smoothDeltaTime * 0.5f);
							//vertical bobbing
							HBamt = Mathf.MoveTowards(HBamt, WalkHorizontalBobAmount, Time.smoothDeltaTime * 0.5f);
							//pitching
							HPamt = Mathf.MoveTowards(HPamt, WalkBobPitchAmount, Time.smoothDeltaTime * 0.75f);
							//rolling
							if(!FPSWalker.swimming){
								HRamt = Mathf.MoveTowards(HRamt, WalkBobRollAmount, Time.smoothDeltaTime * 0.75f);
							}else{
								HRamt = Mathf.MoveTowards(HRamt, WalkBobRollAmount * 2.0f, Time.smoothDeltaTime * 0.75f);	
							}
							//yawing
							HYamt = Mathf.MoveTowards(HYamt, WalkBobYawAmount, Time.smoothDeltaTime * 0.75f);	
										
							if(!FPSWalker.swimming){
								VerticalBob.bobbingSpeed = WalkBobSpeed;
								//make horizontal bob speed half as slow as vertical bob speed for synchronization of bobbing motions
								HorizontalBob.bobbingSpeed = WalkBobSpeed / 2.0f;
							}else{//bobbing is slower while swimming
								VerticalBob.bobbingSpeed = WalkBobSpeed/2;
								HorizontalBob.bobbingSpeed = WalkBobSpeed / 4.0f;
							}
						}else{
							if(FPSWalker.crouched){
								////crouching bob amounts////
								//horizontal bobbing 
								GBamt = Mathf.MoveTowards(GBamt, CrouchVerticalBobAmount, Time.smoothDeltaTime * 0.5f);
								//vertical bobbing
								HBamt = Mathf.MoveTowards(HBamt, CrouchHorizontalBobAmount, Time.smoothDeltaTime * 0.5f);
								//pitching
								HPamt = Mathf.MoveTowards(HPamt, CrouchBobPitchAmount, Time.smoothDeltaTime * 0.75f);
								//rolling
								HRamt = Mathf.MoveTowards(HRamt, CrouchBobRollAmount, Time.smoothDeltaTime * 0.75f);
								//yawing
								HYamt = Mathf.MoveTowards(HYamt, CrouchBobYawAmount, Time.smoothDeltaTime * 0.75f);
					
								VerticalBob.bobbingSpeed = CrouchBobSpeed;
								HorizontalBob.bobbingSpeed = CrouchBobSpeed / 2.0f;
							}else if(FPSWalker.prone){
								////prone bob amounts////
								//horizontal bobbing 
								HBamt = Mathf.MoveTowards(HBamt, ProneHorizontalBobAmount, Time.smoothDeltaTime * 0.5f);
								//vertical bobbing
								GBamt = Mathf.MoveTowards(GBamt, ProneVerticalBobAmount, Time.smoothDeltaTime * 0.5f);
								//pitching
								HPamt = Mathf.MoveTowards(HPamt, ProneBobPitchAmount, Time.smoothDeltaTime * 0.75f);
								//rolling
								HRamt = Mathf.MoveTowards(HRamt, ProneBobRollAmount, Time.smoothDeltaTime * 0.75f);
								//yawing
								HYamt = Mathf.MoveTowards(HYamt, ProneBobYawAmount, Time.smoothDeltaTime * 0.75f);
					
								VerticalBob.bobbingSpeed = ProneBobSpeed;
								HorizontalBob.bobbingSpeed = ProneBobSpeed / 2.0f;
								//move weapon toward or away from camera while prone
//								if((Mathf.Abs(horizontal) > 0.15f) || (Mathf.Abs(vertical) > 0.15f)){
//									nextPosZ = 0.02f;
//								}else{
//									nextPosZ = 0.0f;
//								}
							}
						}
					}else{
						//reduce bobbing amounts for smooth jumping/landing transition
						HBamt = Mathf.MoveTowards(HBamt, 0f, Time.smoothDeltaTime);
						HRamt = Mathf.MoveTowards(HRamt, 0f, Time.smoothDeltaTime * 2f);
						HYamt = Mathf.MoveTowards(HYamt, 0f, Time.smoothDeltaTime * 2f);
						HPamt = Mathf.MoveTowards(HPamt, 0f, Time.smoothDeltaTime * 2f);
						GBamt = Mathf.MoveTowards(GBamt, 0f, Time.smoothDeltaTime);
					}
					VerticalBob.bobbingAmount = GBamt * deltaAmount;//apply delta at this step for framerate independence
					VerticalBob.rollingAmount = HRamt * deltaAmount;
					VerticalBob.yawingAmount = HYamt * deltaAmount;
					VerticalBob.pitchingAmount = HPamt * deltaAmount;
					HorizontalBob.bobbingAmount = HBamt * deltaAmount;
				}
			}
		}
	}
	
}