//MovePlayerAndCamera.cs by Azuline Studios© All Rights Reserved
//Contains methods to teleport the player or release main camera for cinema scenes or other uses
using UnityEngine;
using System.Collections;

public class MovePlayerAndCamera : MonoBehaviour {

	[Tooltip("Position and angles of main camera will be set to this one's when toggling main camera.")]
	public GameObject CinemaCameraObj;
	[Tooltip("Position that player will be teleported to.")]
	public Transform movePos;
	[Tooltip("Pitch to set camera to after moving player.")]
	public float moveCamPitch;
	[Tooltip("Yaw to set camera to after moving player.")]
	public float moveCamYaw;
//	public AI testAI;
	
	private bool toggleState;
	private bool toggleGuiState;
	private bool noCrosshair;
	public GameObject FPSWeaponsObj;
	public GameObject FPSPlayerObj;
	public GameObject FPSCameraObj;
	public GameObject MainCameraObj;

	public GameObject VisibleBodyObj;
	private CameraControl CameraControlComponent;
	private SmoothMouseLook MouseLookComponent;
	private FPSPlayer FPSPlayerComponent;
	private FPSRigidBodyWalker FPSWalkerComponent;
	private PlayerWeapons PlayerWeaponsComponent;
	private Ironsights IronsightsComponent;
	private VisibleBody VisibleBodyComponent;

	void Start () {
		//set up component references
		if(CinemaCameraObj){
			CinemaCameraObj.SetActive(false);
		}
		CameraControlComponent = MainCameraObj.GetComponent<CameraControl>();
		MouseLookComponent = FPSCameraObj.GetComponent<SmoothMouseLook>();
		FPSPlayerComponent = FPSPlayerObj.GetComponent<FPSPlayer>();
		FPSWalkerComponent = FPSPlayerObj.GetComponent<FPSRigidBodyWalker>();
		PlayerWeaponsComponent = FPSWeaponsObj.GetComponent<PlayerWeapons>();
		IronsightsComponent = FPSPlayerObj.GetComponent<Ironsights>();
		VisibleBodyComponent = FPSWalkerComponent.VisibleBody.GetComponent<VisibleBody>();
		if(!FPSPlayerComponent.crosshairEnabled){
			noCrosshair = true;//don't reactivate crosshair if it wasn't active to start
		}
	}
	
	//Detect keypresses for camera and player positioning tests (testing)
	void Update () {
		if(Time.timeSinceLevelLoad > 0.2f){
//			if(testAI && movePos && Input.GetKeyDown(KeyCode.Comma)){
//				testAI.GoToPosition(movePos.position);//move npc to a particular place
//			}
			if(movePos && Input.GetKeyDown(KeyCode.Insert)){
				MovePlayer(movePos.position + (Vector3.up * FPSWalkerComponent.capsule.height * 0.5f), moveCamYaw, moveCamPitch);//move player with camera
			}
			if(Input.GetKeyDown(KeyCode.Delete) && CinemaCameraObj){
				ReleaseMainCamera();//release main camera from RFPSP control, for cinematics or other purposes
			}
			if(movePos && Input.GetKeyDown(KeyCode.End) && CinemaCameraObj){
				ReleaseMainCameraAndMovePlayer();//release main camera from RFPSP control, for cinematics or other purposes, then reposition player and set camera angles afterwards
			}
		}
	}
	
	//move player with camera
	public void MovePlayer (Vector3 position, float camYaw = 0f, float camPitch = 0f){
	
		if(!FPSPlayerObj.activeInHierarchy){
			return;//don't move player if it isn't active
		}
	
		CancelPlayerActions();
	
		//set the time that the player moved in these scripts, to reduce lerp/smoothing times for instant transition
		CameraControlComponent.movingTime = Time.time;
		MouseLookComponent.playerMovedTime = Time.time;
		
		//move camera to position, and add player height
		Vector3 tempCamPosition = position + (Vector3.up * FPSWalkerComponent.standingCamHeight);
		
		//change camera position
		CameraControlComponent.tempLerpPos = tempCamPosition;
		CameraControlComponent.camPos = tempCamPosition;
		CameraControlComponent.targetPos = tempCamPosition;
		
		//change player object positions
		FPSPlayerObj.transform.position = position;
		FPSCameraObj.transform.position = tempCamPosition;
		FPSWeaponsObj.transform.position = FPSCameraObj.transform.position;
		
		VisibleBodyObj.transform.position = position - (Vector3.up * FPSWalkerComponent.capsule.height * 0.5f);
		VisibleBodyComponent.tempBodyPosition = position - (Vector3.up * FPSWalkerComponent.capsule.height * 0.5f);
		VisibleBodyComponent.transform.rotation = FPSPlayerObj.transform.rotation;
		VisibleBodyComponent.tempPosX = position.x;
		VisibleBodyComponent.verticalPos = position.y;
		VisibleBodyComponent.modelUpAmt = VisibleBodyComponent.modelUp;
		VisibleBodyComponent.tempPosZ = position.z;
		
		//set camera angles to those defined by the MovePlayer "camYaw" and "camPitch" arguments
		//so player can be looking in a certain direction after moving
		if(camYaw != 0f){//don't modify camera angles if MovePlayer was called with none specified
			//Set view angles to those defined by camYaw and camPitch parameters
			FPSCameraObj.transform.rotation = Quaternion.Euler(-camPitch, camYaw, 0f);
			FPSWeaponsObj.transform.rotation = FPSCameraObj.transform.rotation;
			MouseLookComponent.rotationX = camYaw;
			MouseLookComponent.rotationY = camPitch;
			//Reset angle amounts below to zero, to allow view movement to resume at new angles
			MouseLookComponent.horizontalDelta = 0f;
			MouseLookComponent.recoilY = 0f;
			MouseLookComponent.recoilX = 0f;
			MouseLookComponent.xQuaternion = Quaternion.Euler(0f, 0f, 0f);
			MouseLookComponent.yQuaternion = Quaternion.Euler(0f, 0f, 0f);
			MouseLookComponent.originalRotation = Quaternion.Euler(0f, 0f, 0f);
		}
	}
	
	public void ToggleGuiDisplay(){
		if(!toggleGuiState){
			if(!noCrosshair){FPSPlayerComponent.CrosshairGuiTexture.enabled = false;}
////			FPSPlayerComponent.healthGuiObjInstance.SetActive(false);
//			if(FPSPlayerComponent.thirstGuiObjInstance){
//				FPSPlayerComponent.thirstGuiObjInstance.SetActive(false);
//			}
//			if(FPSPlayerComponent.hungerGuiObjInstance){
//				FPSPlayerComponent.hungerGuiObjInstance.SetActive(false);
//			}
			FPSPlayerComponent.helpGuiObjInstance.SetActive(false);
//			PlayerWeaponsComponent.ammoGuiObjInstance.SetActive(false);
			
			toggleGuiState = true;
		}else{
			//don't toggle crosshair if it wasn't active to start
			if(!noCrosshair){FPSPlayerComponent.CrosshairGuiTexture.enabled = true;}
////			FPSPlayerComponent.healthGuiObjInstance.SetActive(true);
//			if(FPSPlayerComponent.thirstGuiObjInstance){
//				FPSPlayerComponent.thirstGuiObjInstance.SetActive(true);
//			}
//			if(FPSPlayerComponent.hungerGuiObjInstance){
//				FPSPlayerComponent.hungerGuiObjInstance.SetActive(true);
//			}
			FPSPlayerComponent.helpGuiObjInstance.SetActive(true);
//			PlayerWeaponsComponent.ammoGuiObjInstance.SetActive(true);
			
			toggleGuiState = false;
		}
	} 
	
	//toggle a camera other than the main camera
	public void ToggleCinemaCamera(){
		if(!toggleState){
		
			ToggleGuiDisplay();
			
			FPSWeaponsObj.SetActive(false);
			FPSPlayerObj.SetActive(false);
			FPSCameraObj.SetActive(false);
			VisibleBodyObj.SetActive(false);
			
			CinemaCameraObj.SetActive(true);

			toggleState = true;
		}else{

			CinemaCameraObj.SetActive(false);
			
			CameraControlComponent.movingTime = Time.time;
			MouseLookComponent.playerMovedTime = Time.time;
		
			FPSCameraObj.SetActive(true);
			FPSWeaponsObj.SetActive(true);
			FPSPlayerObj.SetActive(true);
			
			VisibleBodyObj.SetActive(true);
			
			CancelPlayerActions();
			ToggleGuiDisplay();

			toggleState = false;
		}
	} 
	
	//release control of the main camera for other uses like cinema scenes
	public void ReleaseMainCamera(){
		if(!toggleState){
		
			ToggleGuiDisplay();
			FPSWeaponsObj.SetActive(false);
			FPSPlayerObj.SetActive(false);
			VisibleBodyObj.SetActive(false);
			
			CameraControlComponent.enabled = false;
			MouseLookComponent.enabled = false;
			MainCameraObj.transform.position = CinemaCameraObj.transform.position;
			MainCameraObj.transform.rotation = CinemaCameraObj.transform.rotation;
			
			Camera.main.fieldOfView = IronsightsComponent.defaultFov;
			
			toggleState = true;
		}else{
			
			MainCameraObj.transform.rotation = FPSCameraObj.transform.rotation;
			MainCameraObj.transform.position = FPSCameraObj.transform.position;
			CameraControlComponent.enabled = true;
			MouseLookComponent.enabled = true;
			
			FPSWeaponsObj.SetActive(true);
			FPSPlayerObj.SetActive(true);
			
			VisibleBodyObj.SetActive(true);
			
			CancelPlayerActions();
			ToggleGuiDisplay();
			
			toggleState = false;
		}
	} 
	
	//release main camera from RFPSP control, for cinematics or other purposes, then reposition player and set camera angles when reactivating camera
	public void ReleaseMainCameraAndMovePlayer(){
		if(!toggleState){
			
			ToggleGuiDisplay();
			FPSWeaponsObj.SetActive(false);
			FPSPlayerObj.SetActive(false);
			VisibleBodyObj.SetActive(false);
			
			CameraControlComponent.enabled = false;
			MouseLookComponent.enabled = false;
			MainCameraObj.transform.position = CinemaCameraObj.transform.position;
			MainCameraObj.transform.rotation = CinemaCameraObj.transform.rotation;
			
			Camera.main.fieldOfView = IronsightsComponent.defaultFov;
			
			toggleState = true;
		}else{
			
			MainCameraObj.transform.rotation = FPSCameraObj.transform.rotation;
			MainCameraObj.transform.position = FPSCameraObj.transform.position;
			CameraControlComponent.enabled = true;
			MouseLookComponent.enabled = true;
			
			FPSWeaponsObj.SetActive(true);
			FPSPlayerObj.SetActive(true);
			
			VisibleBodyObj.SetActive(true);
			
			CancelPlayerActions();
			ToggleGuiDisplay();
			MovePlayer(movePos.position + (Vector3.up * FPSWalkerComponent.capsule.height * 0.5f), moveCamYaw, moveCamPitch);
			
			toggleState = false;
		}
	}
	
	//cancel actions like jumping, crouching, sprinting, reloading, and weapon switching 
	//so player resumes after camera switch in a neutral state
	void CancelPlayerActions(){
	
		WeaponBehavior CurrentWeaponBehaviorComponent = PlayerWeaponsComponent.CurrentWeaponBehaviorComponent;
	
		PlayerWeaponsComponent.StopAllCoroutines();
		CurrentWeaponBehaviorComponent.StopAllCoroutines();
		
		FPSWalkerComponent.jumping = false;
		FPSWalkerComponent.landState = false;
		FPSWalkerComponent.jumpfxstate = true;
		FPSWalkerComponent.CameraAnimationComponent.Rewind("CameraLand");
		
		FPSWalkerComponent.crouched = false;
		FPSWalkerComponent.crouchState = false;
		FPSWalkerComponent.crouchRisen = true;
		
		FPSWalkerComponent.prone = false;
		FPSWalkerComponent.proneState = false;
		FPSWalkerComponent.proneRisen = true;
		
		if(CurrentWeaponBehaviorComponent && !CurrentWeaponBehaviorComponent.PistolSprintAnim){
			CurrentWeaponBehaviorComponent.AnimationComponent.Rewind("RifleSprinting");
			CurrentWeaponBehaviorComponent.AnimationComponent["RifleSprinting"].normalizedTime = 0;
		}else{
			CurrentWeaponBehaviorComponent.AnimationComponent.Rewind("PistolSprinting");
			CurrentWeaponBehaviorComponent.AnimationComponent["PistolSprinting"].normalizedTime = 0;
		}
		
		CurrentWeaponBehaviorComponent.gunAnglesTarget = Vector3.zero;
		
		if(PlayerWeaponsComponent.currentWeapon == PlayerWeaponsComponent.grenadeWeapon && !CurrentWeaponBehaviorComponent.cycleSelect){
			PlayerWeaponsComponent.grenadeThrownState = true;
		}else{
			PlayerWeaponsComponent.grenadeThrownState = false;
		}
		
		PlayerWeaponsComponent.offhandThrowActive = false;
		PlayerWeaponsComponent.pullGrenadeState = false;
		CurrentWeaponBehaviorComponent.pullAnimState = false;
		CurrentWeaponBehaviorComponent.fireOnReleaseState = false;
		CurrentWeaponBehaviorComponent.doReleaseFire = false;
		CurrentWeaponBehaviorComponent.releaseAnimState = false;
		CurrentWeaponBehaviorComponent.fireHoldTimer = 0.0f;
		
		IronsightsComponent.switchMove = 0.0f;
		IronsightsComponent.reloading = false;
		
		FPSPlayerComponent.zoomed = false;
		IronsightsComponent.newFov = IronsightsComponent.defaultFov;
		IronsightsComponent.nextFov = IronsightsComponent.defaultFov;
		
		FPSWalkerComponent.cancelSprint = true;
		FPSWalkerComponent.sprintActive = false;
		FPSWalkerComponent.jumping = false;
		FPSWalkerComponent.landState = false;
		FPSWalkerComponent.jumpfxstate = true;
		FPSWalkerComponent.CameraAnimationComponent.Rewind("CameraLand");
		
		if(CurrentWeaponBehaviorComponent.WeaponAnimationComponent){
			CurrentWeaponBehaviorComponent.WeaponAnimationComponent.Stop();
			CurrentWeaponBehaviorComponent.WeaponAnimationComponent.CrossFade("Fire",0.01f,PlayMode.StopSameLayer);
			CurrentWeaponBehaviorComponent.WeaponAnimationComponent["Fire"].normalizedTime = 1f;
		}
	
	}
	
}
