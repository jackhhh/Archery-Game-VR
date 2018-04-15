//WeaponPivot.cs by Azuline Studios© All Rights Reserved
//rotates weapon objects around pivot points for deadzone aiming
using UnityEngine;
using System.Collections;

public class WeaponPivot : MonoBehaviour {

	private GunSway GunSwayComponent;
	private FPSPlayer FPSPlayerComponent;
	private FPSRigidBodyWalker FPSWalkerComponent;
	private WeaponBehavior WeaponBehaviorComponent;
	private SmoothMouseLook SmoothMouseLookComponent;
	private InputControl InputComponent;
	private PlayerWeapons PlayerWeaponsComponent;
	private Ironsights IronsightsComponent;
	private GameObject playerObj;
	
	[Tooltip("Prevent deadzone aiming with this weapon.")]
	public bool noDeadzoneAiming;
	[Tooltip("True if weapon should aim with deadzone (free/unlocked aiming).")]
	public bool deadzoneLooking;
	[Tooltip("True if weapon should zoom with deadzone (free/unlocked zooming).")]
	public bool deadzoneZooming;
	[Tooltip("True if weapon should sway towards view movement direction.")]
	public bool swayLeadingMode;
	[HideInInspector]
	public bool wasDeadzoneZooming;
	[Tooltip("True if crosshair should follow weapon when swaying.")]
	public bool swayLeadingFollowCrosshair = true;
	[Tooltip("Vertical speed of deadzone aiming.")]
	public float verticalSpeed = 1.0f;
	[Tooltip("Horizontal speed of deadzone aiming.")]
	public float horizontalSpeed = 1.0f;
	[Tooltip("Amount of weapon sway in leading mode.")]
	public float swayAmount = 1.0f;
	[Tooltip("Speed that weapon returns to center position after swaying or deadzone zooming.")]
	public float neutralSpeed = 0.2f;
	[Tooltip("Pivot point to rotate weapon around in deadzone aiming mode.")]
	public Transform pivot;
	
	[HideInInspector]
	public Transform childTransform;
	[HideInInspector]
	public Transform myTransform;

	private float angleDiff;
	private float cameraParentYaw;

	[HideInInspector]
	public Vector3 rotateAmtNeutral;
	[HideInInspector]
	public Vector3 mouseOffsetTarg;
	
	private Vector3 velocity2;
	private float velocity3;

	private Animation PivotAnimComponent;
	
	[HideInInspector]
	public float y;
	private float yTarg;
	private float sumY;
	
	[HideInInspector]
	public float x;
	private float xTarg;
	private float sumX;

	private bool init;
	private bool dzSprintState;
	private bool dzProneState;
	private bool dzClimbState;

	private bool dzAimState;
	private bool dzZoomState;
	private float swayAmt;
	
	private Vector2 inputSmoothDampVel;
	
	private float leadRoll;
	private float leadPitch;
	
	[Tooltip("Max camera zoom FOV that weapon can free aim with (allows sniper rifles to zoom normally in free aiming mode)")]
	public float maxFreeAimFOV = 15.0f;

	void Start () {
		GunSwayComponent = transform.parent.transform.GetComponent<GunSway>();
		SmoothMouseLookComponent = GunSwayComponent.cameraObj.GetComponent<SmoothMouseLook>();
		playerObj = Camera.main.transform.GetComponent<CameraControl>().playerObj;
		FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		InputComponent = playerObj.GetComponent<InputControl>();
		PlayerWeaponsComponent = FPSPlayerComponent.PlayerWeaponsComponent;
		IronsightsComponent = FPSPlayerComponent.IronsightsComponent;
		PivotAnimComponent = GetComponent<Animation>();
		myTransform = transform;
		childTransform = myTransform.GetChild(0);
		swayAmt = GunSwayComponent.swayAmount;

		if(deadzoneLooking){
			SmoothMouseLookComponent.dzAiming = true;
		}

	}

	#if UNITY_EDITOR || UNITY_WEBPLAYER
	void OnGUI(){//lock cursor - don't use OnGUI in standalone for performance reasons
		if(Time.timeScale > 0.0f){
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}
	#endif

	void Update () {

		WeaponBehaviorComponent =  PlayerWeaponsComponent.CurrentWeaponBehaviorComponent;
		
		if(Time.timeScale > 0.0f && Time.smoothDeltaTime > 0.0f ){
		
			if(!noDeadzoneAiming && !SmoothMouseLookComponent.thirdPerson){
				
				//detect deadzone zooming button press

				if(!FPSPlayerComponent.zoomed){
					if(InputComponent.deadzonePress){
						if(deadzoneLooking){
							if(wasDeadzoneZooming){
								deadzoneZooming = true;//resume deadzone zooming if we started deadzone looking (unzoomed) when deadzone zooming was true
								FPSPlayerComponent.reticleColor.a = 0.0f;//hide crosshair instantly to avoid brief blink in center position
								FPSPlayerComponent.CrosshairGuiTexture.color = FPSPlayerComponent.reticleColor;
							}
							deadzoneLooking = false;
						}else{
							deadzoneLooking = true;
							deadzoneZooming = false;
						}
					}
				}
				
				
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				//Update zooming settings based on which aiming/zooming mode is active
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
				if(deadzoneLooking && FPSWalkerComponent.sprintActive){
					deadzoneLooking = false;
					dzSprintState = true;
				}
	
				if(!FPSWalkerComponent.sprintActive 
				&& WeaponBehaviorComponent.recoveryTime + WeaponBehaviorComponent.recoveryTimeAmt + 0.1f < Time.time 
				&& dzSprintState){
					deadzoneLooking = true;
					dzSprintState = false;
				}
	
				if(deadzoneLooking && FPSWalkerComponent.moving && FPSWalkerComponent.prone){
					deadzoneLooking = false;
					dzProneState = true;
				}
	
				if(((!FPSWalkerComponent.moving 
				&& FPSWalkerComponent.prone 
				&& WeaponBehaviorComponent.recoveryTime + WeaponBehaviorComponent.recoveryTimeAmt + 0.1f < Time.time) 
				|| !FPSWalkerComponent.prone) && dzProneState){
					deadzoneLooking = true;
					dzProneState = false;
				}
	
				if(deadzoneLooking && FPSWalkerComponent.climbing){
					deadzoneLooking = false;
					dzClimbState = true;
				}
				
				if(!FPSWalkerComponent.climbing && dzClimbState){
					deadzoneLooking = true;
					dzClimbState = false;
				}
	
	
				if(!deadzoneZooming){
					IronsightsComponent.dzAiming = false;//make sure other scripts are updated with deadzone aiming states
					FPSPlayerComponent.dzAiming = false;
					if(deadzoneLooking && FPSPlayerComponent.zoomed){
						deadzoneLooking = false;
						dzAimState = true;
					}
					
					if(!FPSPlayerComponent.zoomed && dzAimState){
						if(!dzZoomState){
							deadzoneLooking = true;//deadzone was not active when zooming, so resume unzoomed deadzone looking
							dzAimState = false;
						}else{
							dzZoomState = false;//don't resume deadzone looking if we canceled deadzone zooming mode while zoomed
							dzAimState = false;
						}
					}
				}else{
					if(FPSPlayerComponent.zoomed && !WeaponBehaviorComponent.zoomIsBlock && WeaponBehaviorComponent.zoomFOV > maxFreeAimFOV){
						deadzoneLooking = true;
						IronsightsComponent.dzAiming = true;
						FPSPlayerComponent.dzAiming = true;
					}else{
						deadzoneLooking = false;
						IronsightsComponent.dzAiming = false;
						FPSPlayerComponent.dzAiming = false;
					}
					dzZoomState = true;
					wasDeadzoneZooming = true;
				}
				
			}else{
				deadzoneLooking = false;
			}


			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
			//Calculate deadzone zooming angles
			/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

			//weapon neutral angles

			if(myTransform.localEulerAngles.y < 180.0f){
				rotateAmtNeutral.y = -myTransform.localEulerAngles.y;
			}else{
				rotateAmtNeutral.y = 360.0f - myTransform.localEulerAngles.y;
			}

			if(childTransform.localEulerAngles.x < 180.0f){
				rotateAmtNeutral.x = -childTransform.localEulerAngles.x;
			}else{
				rotateAmtNeutral.x = 360.0f - childTransform.localEulerAngles.x;
			}

			angleDiff = Mathf.DeltaAngle(SmoothMouseLookComponent.myTransform.eulerAngles.y, myTransform.eulerAngles.y);

			if(childTransform.localEulerAngles.x > 180.0f){
				cameraParentYaw = (360.0f - childTransform.localEulerAngles.x) * -1.0f;
			}else{
				cameraParentYaw = childTransform.localEulerAngles.x;
			}
			
			//deadzone zooming angle calculations

			if(deadzoneLooking && !WeaponBehaviorComponent.unarmed && !SmoothMouseLookComponent.thirdPerson){

				if(init){
					SmoothMouseLookComponent.dzAiming = true;
					GunSwayComponent.swayAmount = swayAmt;
					GunSwayComponent.dzAiming = true;
					
					init = false;
				}

				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;

				if(angleDiff < -WeaponBehaviorComponent.horizontalDZ + 5.0f && InputComponent.lookX < 0.0f){
					SmoothMouseLookComponent.horizontalDelta = SmoothMouseLookComponent.rotationX;//old rotationX
					SmoothMouseLookComponent.rotationX += InputComponent.lookX * SmoothMouseLookComponent.sensitivityAmt * Time.timeScale;
					SmoothMouseLookComponent.horizontalDelta = Mathf.DeltaAngle(SmoothMouseLookComponent.horizontalDelta, SmoothMouseLookComponent.rotationX);
				}

				if(angleDiff > WeaponBehaviorComponent.horizontalDZ -5.0f && InputComponent.lookX > 0.0f){
					SmoothMouseLookComponent.horizontalDelta = SmoothMouseLookComponent.rotationX;//old rotationX
					SmoothMouseLookComponent.rotationX += InputComponent.lookX * SmoothMouseLookComponent.sensitivityAmt * Time.timeScale;
					SmoothMouseLookComponent.horizontalDelta = Mathf.DeltaAngle(SmoothMouseLookComponent.horizontalDelta, SmoothMouseLookComponent.rotationX);
				}
				
				x = InputComponent.lookX * horizontalSpeed;//deadzone looking horizontal input amount


				if(cameraParentYaw < -WeaponBehaviorComponent.verticalDZUpper + 5.0f && -InputComponent.lookY < 0.0f ){
					SmoothMouseLookComponent.rotationY += InputComponent.lookY * SmoothMouseLookComponent.sensitivityAmt * Time.timeScale;
				}

				if(cameraParentYaw > WeaponBehaviorComponent.verticalDZLower - 5.0f && -InputComponent.lookY > 0.0f){
					SmoothMouseLookComponent.rotationY += InputComponent.lookY * SmoothMouseLookComponent.sensitivityAmt * Time.timeScale;
				}

				y = -InputComponent.lookY * verticalSpeed;//deadzone looking vertical input amount

			}else{
				if(!init){
					if(FPSPlayerComponent.zoomed){
						SmoothMouseLookComponent.rotationY += -cameraParentYaw;
						SmoothMouseLookComponent.rotationX += myTransform.localEulerAngles.y;
					}
					SmoothMouseLookComponent.dzAiming = false;
					GunSwayComponent.dzAiming = false;
					
					init = true;
				}

				//compensate for floating point imprecision in RotateAround when player is a large distance from scene origin
				myTransform.localPosition = Vector3.MoveTowards(myTransform.localPosition, Vector3.zero, 0.005f * Time.smoothDeltaTime);
				childTransform.localPosition = Vector3.MoveTowards(childTransform.localPosition, Vector3.zero, 0.005f * Time.smoothDeltaTime);

				if(swayLeadingMode){
					if(swayLeadingFollowCrosshair && !SmoothMouseLookComponent.thirdPerson){
						FPSPlayerComponent.raycastCrosshair = true;
					}else{
						FPSPlayerComponent.raycastCrosshair = false;
					}
					
					//calculate sway leading angles
					if(!FPSPlayerComponent.zoomed && !FPSWalkerComponent.sprintActive){
						GunSwayComponent.swayAmount = 0.0f;
						leadRoll = Mathf.SmoothDampAngle(leadRoll, InputComponent.lookX, ref inputSmoothDampVel.x, Time.smoothDeltaTime);
						leadPitch = Mathf.SmoothDampAngle(leadPitch, -InputComponent.lookY, ref inputSmoothDampVel.y, Time.smoothDeltaTime);
						x = Mathf.SmoothDampAngle(x, Mathf.DeltaAngle(x,(InputComponent.lookX * swayAmount) + rotateAmtNeutral.y * neutralSpeed), ref velocity2.y, Time.smoothDeltaTime);
						y = Mathf.SmoothDampAngle(y, Mathf.DeltaAngle(y,(-InputComponent.lookY * swayAmount) + rotateAmtNeutral.x * neutralSpeed), ref velocity3, Time.smoothDeltaTime);
					}else{
						GunSwayComponent.swayAmount = swayAmt;
						x = Mathf.SmoothDampAngle(x, rotateAmtNeutral.y * neutralSpeed, ref velocity2.y, Time.smoothDeltaTime);
						y = Mathf.SmoothDampAngle(y, rotateAmtNeutral.x * neutralSpeed, ref velocity3, Time.smoothDeltaTime);
					}
				}else{//return weapon angles to neutral (deadzone aiming/sway leading off)
					FPSPlayerComponent.raycastCrosshair = false;
					GunSwayComponent.swayAmount = swayAmt;
					if(!FPSPlayerComponent.zoomed){
						x = Mathf.SmoothDampAngle(x, rotateAmtNeutral.y * neutralSpeed, ref velocity2.y, Time.smoothDeltaTime);
						y = Mathf.SmoothDampAngle(y, rotateAmtNeutral.x * neutralSpeed, ref velocity3, Time.smoothDeltaTime);
					}else{
						x = Mathf.SmoothDampAngle(x, rotateAmtNeutral.y * neutralSpeed, ref velocity2.y, Time.smoothDeltaTime);
						y = Mathf.SmoothDampAngle(y, rotateAmtNeutral.x * neutralSpeed, ref velocity3, Time.smoothDeltaTime);
					}
				}

			}
			
			//calculate if deadzone aiming input amounts will exceed max allowed values and clamp them if they do

			if (sumX + x < -WeaponBehaviorComponent.horizontalDZ) {
				x =  -WeaponBehaviorComponent.horizontalDZ - sumX;//undo last move and reset to deadzone limit
				sumX = -WeaponBehaviorComponent.horizontalDZ;                 
			}
			else if (sumX + x > WeaponBehaviorComponent.horizontalDZ) {
				x =  WeaponBehaviorComponent.horizontalDZ - sumX;//undo last move and reset to deadzone limit
				sumX = WeaponBehaviorComponent.horizontalDZ;
			}
			else{
				sumX += x;//increase deadzone aiming angle normally (not exceeding limit)
			}


			if (sumY + y < -WeaponBehaviorComponent.verticalDZUpper) {
				y =  -WeaponBehaviorComponent.verticalDZUpper - sumY;//undo last move and reset to deadzone limit
				sumY = -WeaponBehaviorComponent.verticalDZUpper;                 
			}
			else if (sumY + y > WeaponBehaviorComponent.verticalDZLower) {
				y =  WeaponBehaviorComponent.verticalDZLower - sumY;//undo last move and reset to deadzone limit
				sumY = WeaponBehaviorComponent.verticalDZLower;
			}
			else{
				sumY += y;//increase deadzone aiming angle normally (not exceeding limit)
			}

			//now actually rotate weapon parent objects around pivot position with x & y values
			myTransform.RotateAround(pivot.position, myTransform.up, x);
			childTransform.RotateAround(pivot.position, myTransform.right, y);

		}
	}

	//play animation for vertical bobbing along pivot point (rotateAround actually applied in Ironsights.cs)
	public void PlayAnim() {
		PivotAnimComponent.Rewind("WeaponPivotVert");
		if(deadzoneZooming && deadzoneLooking){
			PivotAnimComponent["WeaponPivotVert"].speed = 0.75f;
		}else{ 
			PivotAnimComponent["WeaponPivotVert"].speed = WeaponBehaviorComponent.pivotBobSpeed;
		}
		PivotAnimComponent.CrossFade("WeaponPivotVert", 0.01f,PlayMode.StopSameLayer);//play weapon swing animation
	}
	
	public void ToggleDeadzoneZooming() {//used by main menu toggle button
		if(deadzoneZooming){
			wasDeadzoneZooming = false;//set this var to allow deadzone zooming to resume after deadzone looking without zooming
			deadzoneZooming = false;
		}else{
			wasDeadzoneZooming = true;
			deadzoneZooming = true;
			FPSPlayerComponent.reticleColor.a = 0.0f;//hide crosshair instantly to avoid brief blink in center position
			FPSPlayerComponent.CrosshairGuiTexture.color = FPSPlayerComponent.reticleColor;
			deadzoneLooking = false;
		}
	}
	
	public void ToggleSwayLeadingMode() {
		if(swayLeadingMode){
			swayLeadingMode = false;
		}else{
			swayLeadingMode = true;
		}
	}

	public static float ClampAngle (float angle, float min, float max){
		angle = angle % 360;
		if((angle >= -360F) && (angle <= 360F)){
			if(angle < -360F){
				angle += 360F;
			}
			if(angle > 360F){
				angle -= 360F;
			}         
		}
		return Mathf.Clamp (angle, min, max);
	}

}
