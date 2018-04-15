//ReconfigurePrefab.cs by Azuline Studios© All Rights Reserved
//Allows user to toggle between single and dual camera setups.
using UnityEngine;
using System.Collections;

public class ReconfigurePrefab : MonoBehaviour {

	[Tooltip("True if dual camera setup for player prefab should be used, false to use single camera setup. Dual cameras are better for large scenes, and the single camera setup is better for small scenes.")]
	public bool TwoCameraSetup = true;

	//image effects to toggle on and off
	private UnityStandardAssets.ImageEffects.SunShafts SunShaftsComponent;
	private UnityStandardAssets.ImageEffects.ColorCorrectionCurves ColorCorrectionCurvesComponent;
	private UnityStandardAssets.ImageEffects.BloomOptimized BloomOptimizedComponent;
//	private UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration ChromaticAberrationComponent;
//	private UnityStandardAssets.ImageEffects.Antialiasing AntiAliasingComponent;
	
	private FPSPlayer FPSPlayerComponent;
	[Tooltip("Reference to weapon camera object.")]	
	public GameObject WeaponCamera;
	[Tooltip("Reference to main camera object.")]	
	public GameObject MainCamera;
	[Tooltip("Reference to weapon parent object.")]	
	public GameObject WeaponObj;
	[Tooltip("Scale of weapons for the dual camera setup. Larger scale reduces jittering of models farther from scene origin.")]	
	public float twoCamWeaponScale = 20.0f;
	[Tooltip("Scale of weapons for the single camera setup. Smaller scale allows weapon models to fit in player collider and receive scene shadows.")]	
	public float oneCamWeaponScale = 1.0f;
	[Tooltip("Near clip plane for the single camera setup. Lower value prevents clipping of smaller weapon models, but increases z fighting.")]	
	public float oneCamNearPlane = 0.02f;
	[Tooltip("Near clip plane for the dual camera setup. Larger value decreases z fighting.")]	
	public float twoCamNearPlane = 0.22f;
	[Tooltip("Near clip plane for the weapon camera. Larger value decreases z fighting.")]	
	public float weaponCamNearPlane = 0.45f;
	[Tooltip("True if the albedo color of the weapon model materials should should be dimmed to simulate shadows from geometry obstructing the sun (weapon models don't receive scene shadows with dual camera setup).")]	
	public bool shadeWeaponByRaycast = true;
	
	private WeaponBehavior[] WeaponBehaviorComponents;
	[Tooltip("Layers for the main camera to render using dual camera setup (excludes gun and GUI layers).")]	
	public LayerMask mainTwoCamMask;
	[Tooltip("Layers for the main camera to render using single camera setup (renders everything).")]	
	public LayerMask mainOneCamMask;

	private bool TwoCamState;
	private bool OneCamState;
	
	void Start () {
		//set up object and script references
		SunShaftsComponent = MainCamera.GetComponent<UnityStandardAssets.ImageEffects.SunShafts>();
		ColorCorrectionCurvesComponent = MainCamera.GetComponent<UnityStandardAssets.ImageEffects.ColorCorrectionCurves>();
		BloomOptimizedComponent = MainCamera.GetComponent<UnityStandardAssets.ImageEffects.BloomOptimized>();
//		ChromaticAberrationComponent = MainCamera.GetComponent<UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration>();
//		AntiAliasingComponent = MainCamera.GetComponent<UnityStandardAssets.ImageEffects.Antialiasing>();
		WeaponBehaviorComponents = WeaponObj.GetComponentsInChildren<WeaponBehavior>(true);
		FPSPlayerComponent = MainCamera.GetComponent<CameraControl>().FPSPlayerComponent;
	}
	
	void Update () {
	
		if(TwoCameraSetup && !TwoCamState){//set up dual camera prefab
			
			Camera.main.cullingMask = mainTwoCamMask;
			Camera.main.nearClipPlane = twoCamNearPlane;
			WeaponObj.transform.localScale = new Vector3(twoCamWeaponScale, twoCamWeaponScale, twoCamWeaponScale);
			
			FPSPlayerComponent.FPSWalkerComponent.sphereCol.enabled = false;
			
			SunShaftsComponent.enabled = false;
			ColorCorrectionCurvesComponent.enabled = false;
			BloomOptimizedComponent.enabled = false;
//			ChromaticAberrationComponent.enabled = false;
//			AntiAliasingComponent.enabled = false;
			
			foreach(WeaponBehavior weap in WeaponBehaviorComponents){
				weap.shadeWeapon = shadeWeaponByRaycast;
			}
			
			WeaponCamera.GetComponent<Camera>().nearClipPlane = weaponCamNearPlane;
			WeaponCamera.SetActive(true);

			OneCamState = false;
			TwoCamState = true;

		}else if(!TwoCameraSetup && !OneCamState){//set up single camera prefab
		
			
			Camera.main.cullingMask = mainOneCamMask;
			Camera.main.nearClipPlane = oneCamNearPlane;
			WeaponObj.transform.localScale = new Vector3(oneCamWeaponScale, oneCamWeaponScale, oneCamWeaponScale);
			
			//use an additional sphere collider around weapon models to prevent them from cliping into scene geometry
			FPSPlayerComponent.FPSWalkerComponent.sphereCol.enabled = true;
			
			SunShaftsComponent.enabled = true;
			ColorCorrectionCurvesComponent.enabled = true;
			BloomOptimizedComponent.enabled = true;
//			ChromaticAberrationComponent.enabled = true;
//			AntiAliasingComponent.enabled = true;
			
			foreach(WeaponBehavior weap in WeaponBehaviorComponents){
				weap.shadeWeapon = false;
			}

			TwoCamState = false;
			OneCamState = true;
			
			WeaponCamera.SetActive(false);
		
		}
	
	}
}
