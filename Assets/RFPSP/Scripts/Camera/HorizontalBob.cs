//HorizontalBob.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class HorizontalBob : MonoBehaviour {
	//set up external script references
	private FPSRigidBodyWalker FPSWalkerComponent;
	private CameraControl CameraControlComponent;
	[HideInInspector]
	public GameObject playerObj;
	//variables for horizontal sine bob of camera and weapons
	private float timer = 0.0f;
	[HideInInspector]
	public float bobbingSpeed = 0.0f;
	[HideInInspector]
	public float bobbingAmount = 0.0f;
	[HideInInspector]
	public float translateChange = 0.0f;
	[HideInInspector]
	public float waveslice = 0.0f;
	private float dampVelocity = 0.0f;
	private float totalAxes;
	[HideInInspector]
	public float dampOrg = 0.0f;//Smoothed view postion to be passed to CameraControl script
	private float dampTo = 0.0f;
	private float horizontal;
	private float vertical;
	private float inputSpeed;
	
	void Start(){
		//set up external script references
		playerObj = Camera.main.transform.GetComponent<CameraControl>().playerObj;
		FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		CameraControlComponent = Camera.main.GetComponent<CameraControl>();	
	}
	
	void Update (){
	
		if(Time.timeScale > 0 && Time.smoothDeltaTime > 0){//allow pausing by setting timescale to 0
			
			waveslice = 0.0f;
			horizontal = FPSWalkerComponent.inputX;//get input from player movement script
			vertical = FPSWalkerComponent.inputY;	
		
			if(FPSWalkerComponent.moving && FPSWalkerComponent.grounded){//Perform bob only when moving and grounded
				waveslice = Mathf.Sin(timer);

				if(Mathf.Abs(FPSWalkerComponent.inputY) > Mathf.Abs(FPSWalkerComponent.inputX)){
					inputSpeed = Mathf.Abs(FPSWalkerComponent.inputY);
				}else{
					inputSpeed = Mathf.Abs(FPSWalkerComponent.inputX);
				}
				
				timer = timer + (bobbingSpeed) * (inputSpeed) * Time.smoothDeltaTime;
				if(timer > Mathf.PI * 2.0f) {
				  timer = timer - (Mathf.PI * 2.0f);
				}
			}else{
				timer = 0.0f;//reset timer when stationary to start bob cycle from neutral position
			}
		
			if(waveslice != 0){
			   translateChange = waveslice * bobbingAmount;
			   totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
			   totalAxes = Mathf.Clamp (totalAxes, 0.0f, 1.0f);
			   translateChange = totalAxes * translateChange;
				//set position for smoothing function
				dampTo = translateChange / Time.smoothDeltaTime * 0.01f;//divide position by smoothDeltaTime for framerate independence
			}else{
				//reset variables to prevent view twitching when falling
				dampTo = 0.0f;
				totalAxes = 0.0f;
				translateChange = 0.0f;
			}
			//use SmoothDamp to smooth position and remove any small twitches in bob amount 
			dampOrg = Mathf.SmoothDamp(dampOrg, dampTo, ref dampVelocity, 0.1f, Mathf.Infinity, Time.smoothDeltaTime);
			//pass bobbing amount to the camera kick script in the camera object after smoothing
			CameraControlComponent.dampOriginX = dampOrg;
		}
	}
}