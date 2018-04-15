//WeaponBehavior.cs by Azuline Studios© All Rights Reserved
//Runs weapon animations and initializes, fires, and reloads weapons. 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class WeaponBehavior : MonoBehaviour {
	//Other objects accessed by this script
	[HideInInspector]
	public GameObject playerObj;
	[HideInInspector]
	public GameObject weaponObj;
	
	[HideInInspector]
	public GameObject ammoGuiObj;
	private AmmoText AmmoText1;//set reference for main color element of ammo GUIText
	private AmmoText[] AmmoText2;//set reference for shadow background color element of heath GUIText
	
	private Transform myTransform;
	private Transform mainCamTransform;
	
	private FPSRigidBodyWalker FPSWalkerComponent;
	[HideInInspector]
	public PlayerWeapons PlayerWeaponsComponent;
	private Ironsights IronsightsComponent;
	[HideInInspector]
	public FPSPlayer FPSPlayerComponent;
	private InputControl InputComponent;
	[HideInInspector]
	public	SmoothMouseLook MouseLookComponent;
	private	WeaponEffects WeaponEffectsComponent;
	private CameraControl CameraControlComponent;
	private WeaponPivot WeaponPivotComponent;
	[HideInInspector]
	public VisibleBody VisibleBodyComponent;
	
	
	[Header ("Inventory and Ammo", order = 0)]
	[Space (10, order = 1)]
	[Tooltip("Reference to weapon pickup object to drop for this weapon.")]
	public GameObject weaponDropObj;
	[Tooltip("Weapon Mesh object which will be animated and positioned by this script.")]
	public GameObject weaponMesh;
	private Vector3 initialWeaponMeshScale;
	private bool thirdpersonState;
	private Vector3 weaponMeshInitalPos;
	[HideInInspector]
	public Animation AnimationComponent;
	[HideInInspector]
	public Animation WeaponAnimationComponent;
	[HideInInspector]
	public Animation CameraAnimationComponent;
	[Tooltip("True if player has this weapon in their inventory.")]
	public bool haveWeapon = false;
	[Tooltip("Should this weapon be selected in normal wepaon selection cycle? (false for grenades and offhand weapons).")]
	public bool cycleSelect = true;
	[Tooltip("Can this weapon be dropped? False for un-droppable weapons like fists or sidearm.")]
	public bool droppable = true;
	[Tooltip("Does this weapon count toward weapon total? False for weapons like fists or sidearm.")]
	public bool addsToTotalWeaps = true;
	[HideInInspector]
	public int weaponNumber = 0;//number of this weapon in the weaponOrder array in playerWeapons script.
	[Tooltip("Ammo amount for this weapon in player's inventory.")]
	public int ammo = 150;
	[Tooltip("Bullets left in magazine.")]
	public int bulletsLeft = 0;
	[Tooltip("Maximum amount of bullets per magazine.")]
	public int bulletsPerClip  = 30;
	[Tooltip("Number of bullets to reload per reload cycle (when < bulletsPerClip, allows reloading one or more bullets at a time).")]
	public int bulletsToReload  = 50;
	[Tooltip("Maximum ammo amount player's inventory can hold for this weapon.")]
	public int maxAmmo = 999;
	
	[Header ("Damage and Firing", order = 2)]
	[Space (10, order = 3)]
	[Tooltip("/Damage to inflict on objects with ApplyDamage(); function.")]
	public int damage  = 10;
	private float damageAmt;
	[Tooltip("Amount of physics push to apply to rigidbodies on contact.")]
	public int force = 200;
	[Tooltip("Range that weapon can hit targets.")]
	public float range  = 100;
	private float rangeAmt;
	[Tooltip("Time between shots.")]
	public float fireRate = 0.097f;
	
	//Shooting
	[Tooltip("Amount of projectiles to be fired per shot ( > 1 for shotguns).")]
	public int projectileCount  = 1;
	private int hitCount = 0;//track number of hits on a surface to optimize/reduce impact effects played for shotguns
	[Tooltip("If > 0, projectile from object pool with this index will be fired from weapon, instead of raycast-based firing.")]
	public int projectilePoolIndex = 0;
	[Tooltip("Amount forward of camera to spawn projectile.")]
	public float projSpawnForward = 0.5f;
	private GameObject projectile = null;
	private Rigidbody projBody;
	[Tooltip("Force to apply to projectile after firing (shot velocity).")]
	public float projectileForce;
	[Tooltip("True if forward velocity of the projectile should be tied to how long fire button is held.")]
	public bool pullProjectileForce;
	public float minimumProjForce = 1.5f;
	private float projectileForceAmt;
	[Tooltip("Vertical rotation to add to fired projectile.")]
	public float projRotUp = 0;
	[Tooltip("Horizontal rotation to add to fired projectile.")]
	public float projRotSide = 0;
	
	[Tooltip("True if weapon should fire after releasing fire button.")]
	public bool fireOnRelease = false;
	[HideInInspector]
	public bool fireOnReleaseState;
	[HideInInspector]
	public bool doReleaseFire;
	[Tooltip("Play optional camera animation 1 when firing after release of fire button.")]
	public bool doCamReleaseAnim1;
	public float CamReleaseAnim1speed = 1f;
	[Tooltip("Play optional camera animation 2 when firing after release of fire button.")]
	public bool doCamReleaseAnim2;
	public float CamReleaseAnim2speed = 1f;
	[Tooltip("Play optional camera animation 1 when holding fire button before release fire.")]
	public bool doCamPullAnim1;
	public float CamPullAnim1speed = 1f;
	[Tooltip("Play optional camera animation 2 when holding fire button before release fire.")]
	public bool doCamPullAnim2;
	public float CamPullAnim2speed = 1f;
	[Tooltip("Make view kick when release firing.")]
	public bool useWeaponKick = true;
	[Tooltip("Time needed to pull weapon back for release fire (holding fire button).")]
	public float pullTime = 0.5f;
	private float pullTimer = 0.5f;
	[Tooltip("Time after fire button release to spawned projectile.")]
	public float releaseTime = 0.5f;
	private float releaseTimer = 0.5f;
	[Tooltip("Maximum time needed to hold release fire (pulling weapon back) for maximum shot charge.")]
	public float maxHoldTime = 3.0f;
	[HideInInspector]
	public float fireHoldTimer;
	[HideInInspector]
	public float fireHoldMult;
	public float pullBackAmt;//amount to move when back when pulling shot (visually indicates fully charged shot)
	[HideInInspector]
	public bool pullAnimState;
	[HideInInspector]
	public bool releaseAnimState;
	[HideInInspector]
	public float fuseTime = 2.0f;
	
	[Tooltip("True if weapon can switch between burst and semi-auto.")]
	public bool fireModeSelectable;
	private bool fireModeState;
	[Tooltip("True when weapon is in semi-auto mode.")]
	public bool semiAuto;
	private bool semiState;
	[Tooltip("True when weapon is in burst mode.")]
	public bool burstFire;
	[Tooltip("If true, weapon will cycle all three burst modes.")]
	public bool burstAndAuto;
	[Tooltip("Amount of bullets to fire per burst.")]
	public int burstShots = 3;
	private bool burstState;
	private int burstShotsFired;
	private bool burstHold;
	[Tooltip("If true, enemies will not hear weapon shooting.")]
	public bool silentShots;
	private bool initialSilentShots;
	[Tooltip("True if weapon can be fired underwater.")]
	public bool fireableUnderwater = true;
	//true when a weapon that cant be fired under water has attempted to fire
	//used to make player have to press fire button again when surfacing to fire instead of holding button down
	private bool waterFireState;
	[Tooltip("Should this weapon be null/unarmed? (true for first weapon in Weapon Order array of PlayerWeapons.cs).")]
	public bool unarmed;
	
	[Tooltip("False if weapon only needs ammo to fire, for grenades and other disposable, one-shot weapons.")]
	public bool doReload = true;
	[Tooltip("This weapon does not reload (doReload = false) but a weapon is still needed to fire ammo (like bow & arrow).")]
	public bool nonReloadWeapon;
	private bool swingSide;//to control which direction to swing melee weapon
	[HideInInspector]
	public float shootStartTime = -16.0f;//time that shot started

	[Tooltip("False if aiming reticule should not be displayed when not zoomed, used for weapons like sniper rifles.")]
	public bool showAimingCrosshair = true;
	[Tooltip("True if crosshair should be shown when zoomed.")]
	public bool showZoomedCrosshair;
	
	[Header ("Melee Attacks", order = 4)]
	[Space (10, order = 5)]
	[Tooltip("Delay after firing to check for hit, to simulate time taken for melee weapon to reach target (this weapon will be treated as a melee weapon when this value is > 0).")]
	public float meleeSwingDelay = 0.0f;
	[Tooltip("True if this weapon has an offhand melee attack like pistol whip.")]
	public bool offhandMeleeAttack;
	[Tooltip("Total time of offhand melee attack untill another can be done.")]
	public float meleeAttackTime = 0.3f;
	[Tooltip("Delay between offhand melee start time and hit time.")]
	public float offhandMeleeDelay = 0.15f;
	[Tooltip("Range of offhand melee attack.")]
	public float offhandMeleeRange = 1.5f;
	[Tooltip("Damage of offhand melee attack.")]
	public float offhandMeleeDamage = 100f;
	[Tooltip("False if melee attack shouldn't be allowed when looking straight down, to prevent clipping of large weapons into player model.")]
	public bool allowDownwardMelee = true;
	[Tooltip("True if a melee attack should be performed with fire button if this is a ranged weapon without ammo.")]
	public bool meleeIfNoAmmo;
	private float lastMeleeTime;
	private bool meleeBlendState; 
	[HideInInspector]
	public bool meleeActive;

	
	[HideInInspector]
	public bool dropWillDupe;//true when weapon has been picked up from an item that is not destroyed on use to prevent dropping this weapon 
	//and creating duplicated ammo by picking up the weapon again from the non-destroyable pickup item
	[Header ("View Model Positioning", order = 6)]
	[Space (10, order = 7)]
	//Gun Position Amounts
	[Tooltip("Horizontal modifier of gun position when not zoomed.")]
	[Range(-0.3f, 0.3f)]
	public float weaponUnzoomXPosition = -0.02f;
	[Tooltip("Vertical modifier of gun position when not zoomed.")]
	[Range(-0.3f, 0.3f)]
	public float weaponUnzoomYPosition = 0.0127f;
	[Tooltip("Forward modifier of gun position when not zoomed.")]
	[Range(-0.3f, 0.3f)]
	public float weaponUnzoomZPosition = 0.0f;
	[Tooltip("Horizontal modifier of gun position when zoomed.")]
	[Range(-0.3f, 0.3f)]
	public float weaponZoomXPosition = -0.07f;
	[Tooltip("Vertical modifier of gun position when zoomed.")]
	[Range(-0.3f, 0.3f)]
	public float weaponZoomYPosition = 0.032f;
	[Tooltip("Forward modifier of gun position when zoomed.")]
	[Range(-0.3f, 0.3f)]
	public float weaponZoomZPosition = 0.0f;
	[Tooltip("Weapon roll angle when blocking.")]
	[Range(-180f, 180f)]
	public float blockRoll = 30f;
	[Tooltip("Horizontal modifier of gun position when backstab is ready.")]
	[Range(-0.3f, 0.3f)]
	public float weaponBackstabXPosition = -0.07f;
	[Tooltip("Vertical modifier of gun position when backstab is ready.")]
	[Range(-0.3f, 0.3f)]
	public float weaponBackstabYPosition = 0.032f;
	[Tooltip("Forward modifier of gun position when backstab is ready.")]
	[Range(-0.3f, 0.3f)]
	public float weaponBackstabZPosition = 0.0f;
	[Tooltip("Weapon roll angle when backstab is ready.")]
	[Range(-180f, 180f)]
	public float backstabRoll = 30f;
	private float weapRollAmt = 1f;
	private float strafeSmoothed;
	[Tooltip("Horizontal modifier of gun position when sprinting.")]
	[Range(-0.3f, 0.3f)]
	public float weaponSprintXPosition = 0.075f;
	[Tooltip("Vertical modifier of gun position when sprinting.")]
	[Range(-0.3f, 0.3f)]
	public float weaponSprintYPosition = 0.0075f;
	[Tooltip("Forward modifier of gun position when sprinting.")]
	[Range(-0.3f, 0.3f)]
	public float weaponSprintZPosition = 0.0f;
	[Header ("Camera and Zooming", order = 8)]
	[Space (10, order = 9)]
	[Tooltip("Upper vertical limit for deadzone aiming.")]
	public float verticalDZUpper = 31.0f;	
	[Tooltip("Lower vertical limit for deadzone aiming.")]
	public float verticalDZLower = 12.0f;	
	[Tooltip("Horizontal limit for deadzone aiming.")]
	public float horizontalDZ = 48.0f;
	
	[Tooltip("True if zoom can be used with this weapon.")]
	public bool canZoom = true;
	[Tooltip("True if zooming action should block instead of activate ironsights (mostly for melee weapons).")]
	public bool zoomIsBlock;
	[Tooltip("True if jumping while zoomed is allowed.")]
	public bool canJumpZoom;
	[Tooltip("True if blocking an attack cancels block (true for parrying with swords, false for shield weapons that don't cancel guard after block).")]
	public bool hitCancelsBlock = true;
	[Tooltip("True if a backstab attack should trigger slow motion/bullet time for a few seconds .")]
	public bool slomoBackstab = true;
	[Tooltip("Percentage of blocked damage that should be ignored.")]
	[Range(0.0f, 1.0f)]
	public float blockDefenseAmt = 0.5f;
	[Tooltip("Angle in front of player where attacks will be blocked.")]
	[Range(-1.0f, 1.0f)]
	public float blockCoverage = 0.45f;
	[Tooltip("Blocking with this weapon will only block melee attacks, ranged attacks still cause full damage (shield block vs sword block/parry).")]
	public bool onlyBlockMelee;
	[Tooltip("If true, weapon will shoot from blocked position, instead of returning to unzoomed position to fire (for firing guns or swords over shields).")]
	public bool shootFromBlock;
	[Tooltip("FOV value to use when zoomed, lower values with scoped weapons for more zoom.")]
	public float zoomFOV = 55.0f;
	[Tooltip("FOV value for zooming when also deadzone aiming (goldeneye/perfect dark style).")]
	public float zoomFOVDz = 53.0f;
	[Tooltip("Sensitivity of view movement when zooming.")]
	public float zoomSensitivity = 0.3f;
	
	[Header ("Bobbing and Sway Amounts", order = 10)]
	[Space (10, order = 11)]
	[Tooltip("View sway amount for this weapon when not zoomed.")]
	public float swayAmountUnzoomed = 1.0f;
	[Tooltip("View sway amount for this weapon when zoomed.")]
	public float swayAmountZoomed = 1.0f;
	[Tooltip("Amount the weapon moves randomly when idle.")]
	public float idleSwayAmt = 1.0f;
	[Tooltip("Amount the weapon moves randomly when swimming.")]
	public float swimIdleSwayAmt = 1.0f;
	[Tooltip("Amount the weapon moves randomly when zoomed.")]
	public float zoomIdleSwayAmt = 1.0f;
	[Tooltip("True if weapon should bob vertically around pivot when zoomed.")]
	public bool pivotWhenZoomed = true;
	[Tooltip("Set to true to use alternate sprinting animation with pistols or one handed weapons.")]
	public bool PistolSprintAnim ;
	[Tooltip("True if this weapon extends further up the screen and will be moved further down when switching so it goes completely offscreen (for swords and bows).")]
	public bool verticalWeapon;
	[Tooltip("To reset weapon anim after unhiding weapon (mostly for fire on release weapons that were pulled before hiding).")]
	private bool hideAnimState;
	[Tooltip("To fine tune horizontal weapon sprint bobbing amounts.")]
	public float sprintBobAmountX = 1.0f;
	[Tooltip("To fine tune vertical weapon sprint bobbing amounts.")]
	public float sprintBobAmountY = 1.0f;
	[Tooltip("To fine tune horizontal weapon walking bobbing amounts.")]
	public float walkBobAmountX = 1.0f;
	[Tooltip("To fine tune vertical weapon walking bobbing amounts.")]
	public float walkBobAmountY = 1.0f;
	
	[Tooltip("Amount to bob weapon forward and backward when walking.")]
	public float zBobWalk = 0.5f;
	[Tooltip("Amount to bob weapon forward and backward when sprinting.")]
	public float zBobSprint = 0.5f;
	[Tooltip("Amount to bob weapon vertically around pivot point (uses animation value from WeaponPivot.cs).")]
	public float pivotBob = 2f;
	[Tooltip("Speed to bob weapon vertically around pivot point.")]
	public float pivotBobSpeed = 1f;

	//Weapon rolling angles
	[Tooltip("True if weapon roll angle should be modified by this script (toggle roll angle bobbing and swaying).")]
	public bool doWeaponRoll = true;
	private float rollRot; 
	private float rollNeutralAngle; 
	private float rollVel;
	private float rollBobAmt;
	private float initialRollAngle;
	[Tooltip("Speed to smooth weapon roll angles.")]
	public float rollSmoothSpeed = 0.15f;
	private float rollSmoothSpeedAmt;
	[Tooltip("Amount to roll weapon left or right when moving view.")]
	public float rollSwayAmt = 30f;
	[Tooltip("Amount to bob weapon roll angle left and right when zoomed.")]
	public float zoomRollBob = 6f;
	[Tooltip("Amount to bob weapon roll angle left and right when walking.")]
	public float walkRollBob = 1.5f;
	[Tooltip("Amount to bob weapon roll angle left and right when sprinting.")]
	public float sprintRollBob = 3.5f;
	
	//strafing feedback
	[Tooltip("Amount to roll weapon when pressing left or right sprint button.")]
	public float strafeRoll = 0.5f;
	[Tooltip("Amount to move weapon left or right when pressing left or right strafe button while unzoomed.")]
	public float strafeSideUnzoom = 0.5f;
	[Tooltip("Amount to move weapon left or right when pressing left or right strafe button while zoomed.")]
	public float strafeSideZoom = 0.2f;
	[Tooltip("Amount to move weapon left or right when pressing left or right strafe button while sprinting.")]
	public float strafeSideSprint = 0.2f;
	
	[Header ("Animation Timings and Effects", order = 12)]
	[Space (10, order = 13)]
	[Tooltip("Speed to play the firing animation.")]
	public float fireAnimSpeed = 1.0f;
	[Tooltip("Speed to crossfade fire animation (lower for shorter animations).")]
	public float fireAnimFadeSpeed = 0.35f;
	[Tooltip("Speed of the offhand melee attack animations.")]
	public float meleeAnimSpeed = 1.0f;
	[Tooltip("Speed of animation that pulls weapon back for release fire.")]
	public float PullAnimSpeed = 1.0f;
	[Tooltip("Only layers to include in bullet hit detection (for efficiency).")]
	public LayerMask bulletMask;
	private LayerMask liquidMask;//only layers to include in underwater bullet hit detection (for efficiency)
	
	//Weapon Animation Smoothing
	private Vector3 gunAngleVel = Vector3.zero;
	[HideInInspector]
	public Vector3 gunAnglesTarget = Vector3.zero;
	[HideInInspector]
	public Vector3 gunAngles = Vector3.zero; 
	
	//Sprinting and Player States
	private bool canShoot = true;//true when player is allowed to shoot
	[HideInInspector]
	public bool shooting;//true when shooting
	[HideInInspector]
	public bool sprintAnimState;//to control playback of sprinting animation
	[HideInInspector]
	public bool sprintState;//to control timing of weapon recovery after sprinting
	[HideInInspector]
	public float recoveryTime;//time that sprint animation started playing
	private float horizontal = 0;//player movement
	private float vertical = 0;//player movement
	
	//Reloading
	private int bulletsNeeded = 0;//number of bullets absent in magazine
	[HideInInspector]
	public int bulletsReloaded = 0;//number of bullets reloaded during this reloading cycle
	[Tooltip("Time per reload cycle, should be shorter if reloading one bullet at a time and longer if reloading magazine.")]
	public float reloadTime = 1.75f;
	private	float reloadStartTime = 0.0f;
	[HideInInspector]
	public bool reloadState;
	private bool sprintReloadState;
	private	float reloadEndTime = 0.0f;//used to allow fire button to cancel a reload if not reloading a magazine and bulletsLeft > 1
	[Tooltip("Speed of reload animation.")]
	public float reloadAnimSpeed = 1.15f;
	[Tooltip("Speed of single shell/bullet reload animation.")]
	public float shellRldAnimSpeed = 0.7f;
	[Tooltip("Speed of weapon readying animation.")]
	public float readyAnimSpeed = 1.0f;
	[Tooltip("Amount of time needed to finish the ready anim after weapon has just been switched to/selected.")]
	public float readyTime = 0.6f;
	[Tooltip("Percentage of total ready time for offhand throw (usually shorter than ready time).")]
	public float offhandThrowReadyMod = 0.4f;
	public float readyTimeAmt;
	[HideInInspector]
	public bool isOffhandThrow;//true if weapon is readying from offhand grenade throw, so don't play readying animation or sound
	[HideInInspector]
	public float recoveryTimeAmt = 0.0f;//amount of time needed to recover weapon center position after sprinting
	[HideInInspector]
	public float startTime = 0.0f;//track time that weapon was selected to calculate readyTime
	[HideInInspector]
	public float reloadLastTime = 1.2f;//to track when last bullet is reloaded if not reloading magazine, to play chambering animation and sound
	[HideInInspector]
	public	float reloadLastStartTime = 0.0f;
	[HideInInspector]
	public bool lastReload;//true when last bullet of a non -magazine reload is being loaded, to play chambering animation and sound 
	private bool cantFireState;//to track ammo depletion and to play out of ammo sound	
	
	public Transform shotOrigin;//the game object that will be used as shot origin
	[HideInInspector]
	public Vector3 origin;
	[HideInInspector]
	public Vector3 direction;
	[Tooltip("Distance from muzzle flash to start shot (closer if using two cameras and weapon models are larger than player capsule).")]
	public float shotOriginDist = 0.5f;
	[HideInInspector]
	public Ray weaponRay;//ray pointed in weapon facing direction
	[HideInInspector]
	public Vector3 weaponLookDirection;//direction that weapon is facing
	private Vector3 lookDirection;
	
	//Muzzle Flash
	[Tooltip("The game object that will be used as a muzzle flash.")]
	public Transform muzzleFlash;
	private float muzzleFlashReduction = 6.5f;//value to control time that muzzle flash is on screen (lower value makes muzzle flash fade slower)
	[HideInInspector]
	public Color muzzleFlashColor = new Color(1, 1, 1, 0.0f);//transparent white as starting color to allow muzzle flash fade in
	[Tooltip("The game object with a light component that will be used for muzzle light.")]
	public GameObject muzzleLightObj;
	[Tooltip("Time to wait until the muzzle light starts fading out.")]
	public float muzzleLightDelay = 0.1f;
	[Tooltip("Speed of muzzle light fading.")]
	public float muzzleLightReduction = 100.0f;
	private Renderer muzzleRendererComponent;
	private Light muzzleLightComponent;
	private Renderer muzzleSmokeComponent;
	
	//Barrel Smoke
	[Tooltip("True if barrel smoke should be emitted (long trail from end of barrel).")]
	public bool useBarrelSmoke = true;
	[Tooltip("Number of consecutive shots required for barrel smoke to emit.")]
	public int barrelSmokeShots;
	[Tooltip("Distance forward to emit smoke effects.")]
	public float smokeForward = 0.15f;
	[Tooltip("Particle effect for smoke rising from barrel after firing more bullets than barrelSmokeShots amount.")]
	public ParticleEmitter barrelSmokeParticles;
	[Tooltip("Horizontal and vertical offset for emitting barrel smoke.")]
	public Vector3 barrelSmokeOffset;
	private int bulletsJustFired;
	private bool emitBarrelSmoke;//barrel smoke emission state
	private float barrelSmokeTime;//time that barrel smoke emission should start
	
	//Muzzle Smoke
	[Tooltip("True if muzzle smoke should be emitted (short cloud from end of barrel).")]
	public bool useMuzzleSmoke = true;//true if barrel smoke should be emitted
	[Tooltip("Particle effect for puff of smoke when firing.")]
	public ParticleEmitter muzzleSmokeParticles;
	[Tooltip("Horizontal and vertical offset for emitting muzzle smoke.")]
	public Vector3 muzzleSmokeOffset;
	private Color muzzleSmokeColor = Color.white;//initialize muzzle smoke color
	[Tooltip("Alpha transparency of muzzle smoke.")]
	public float muzzleSmokeAlpha = 0.25f;
	[Tooltip("True if tracers should be emitted for bullet shots.")]
	public bool useTracers = true;
	[Tooltip("Offset from shot origin to emit tracers.")]
	public Vector3 tracerOffset;
	[Tooltip("Distance from shot origin to emit tracers.")]
	public float tracerDist;
	[Tooltip("Distance from shot origin to emit tracers when swimming.")]
	public float tracerSwimDist;
	[Tooltip("Distance from shot origin to emit tracers when in third person mode.")]
	public float tracerDistTp;
	
	private CapsuleCollider capsule;
	
	//weapon shading
	[Tooltip("True if the albedo color of the weapon model materials should should be dimmed to simulate shadows from geometry obstructing the sun (used by two camera setup).")]	
	public bool shadeWeapon;
	[Tooltip("True if shading (shade boolean value) should be handled manually by triggers, not by linecast to sun object, to allow for bright interiors.")]
	public bool manualShadingMode;
	[HideInInspector]
	public bool shaded;
	private Transform sunLightObj;
	private Vector3 litPos;
	[Tooltip("Do not change the albedo color for shading on this mesh renderer (for scope lenses or other effects).")]
	public Renderer ignoreMeshShading;
	private Renderer[] meshRenderers;
	private Material[] tempWeapMaterials;
	private List<Material> materialList = new List<Material>();
	private float updateInterval = 0.25f;
	private float lastUpdate = 0.0f;
	private Color shadeColor;
	private Color smoothedColor = Color.white;
	private float initSpec;
	private float smoothedSpec;
	
	[Header ("Recoil", order = 14)]
	[Space (10, order = 15)]
	[Tooltip("Amount that shot accuracy will decrease over time.")]
	public float shotSpread = 0.0f;
	private	float shotSpreadAmt = 0.0f;//actual accuracy amount
	[HideInInspector]
	public Quaternion kickRotation;//rotation used for screen kicks
	[Tooltip("Amount to kick view up when firing.")]
	public float kickUp = 7.0f;
	[Tooltip("Amount to kick view sideways when firing.")]
	public float kickSide = 2.0f;
	[Tooltip("Amount to kick view up when firing.")]
	public float kickRoll = 15.0f;
	private bool kickRollState;
	private float kickUpAmt = 0.0f;//actual amount to kick view up when firing
	private float kickSideAmt = 0.0f;//actual amount to kick view sideways when firing
	private float kickRollAmt = 20.0f;//amount to kick view up when firing (set in editor)
	[Tooltip("Distance that gun pushes back when firing and not zoomed.")]
	public float kickBackAmtUnzoom = -0.025f;
	[Tooltip("Distance that gun pushes back when firing and zoomed.")]
	public float kickBackAmtZoom = -0.0175f;
	[Tooltip("True if view should climb with weapon fire (player has to compensate by moving view the opposite direction).")]
	public bool useViewClimb;
	[Tooltip("Amount that view climbs upwards with non-recovering recoil.")]
	public float viewClimbUp = 1.0f;
	[Tooltip("Amount that view moves side to side with non-recovering recoil.")]
	public float viewClimbSide = 1.0f;
	[Tooltip("Amount that view moves right with non-recovering recoil.")]
	public float viewClimbRight = 0.75f;
	[Tooltip("True if weapon accuracy should decrease with sustained fire.")]
	public bool useRecoilIncrease;
	[Tooltip("Number of shots before weapon recoil increases with sustained fire.")]
	public int shotsBeforeRecoil = 4;
	[Tooltip("Growth rate of sustained fire recoil for view angles/input.")]
	public float viewKickIncrease = 1.75f;
	[Tooltip("Growth Rate of sustained fire recoil for weapon accuracy.")]
	public float aimDirRecoilIncrease = 2.0f;
	[HideInInspector]
	public float randZkick;
	
	[Header ("Shell Ejection", order = 16)]
	[Space (10, order = 17)]
	[Tooltip("True if weapon should eject shell casing when firing.")]
	public bool spawnShell;
	[Tooltip("Object pool index for the shell object with rigidbody (physics for ejected shell).")]
	public int shellRBPoolIndex;
	
	private GameObject shell;
	private GameObject shell2;
	
	private Vector3 shellEjectDirection;//direction of ejected shell casing
	[Tooltip("Position shell is ejected from when not zoomed.")]
	public Transform shellEjectPosition;
	[Tooltip("Position shell is ejected from when zoomed.")]
	public Transform shellEjectPositionZoom;
	private Transform shellEjectPos;
	[Tooltip("Scale of shell, can be used to make different shaped shells from one model.")]
	public Vector3 shellScale = new Vector3(1.0f, 1.0f, 1.0f);
	[Tooltip("Delay before ejecting shell (used for bolt action rifles and pump shotguns).")]
	public float shellEjectDelay = 0.0f;
	[Tooltip("Overall movement force of ejected shell.")]
	public float shellForce = 0.2f;
	[Tooltip("Vertical amount to apply to shellForce.")]
	public float shellUp = 0.75f;
	[Tooltip("Horizontal amount to apply to shellForce.")]
	public float shellSide = 1.0f;
	[Tooltip("Forward amount to apply to shellForce.")]
	public float shellForward = 0.1f;
	[Tooltip("Amount of vertical shell rotation.")]
	public float shellRotateUp = 0.25f;
	[Tooltip("Amount of horizontal shell rotation.")]
	public float shellRotateSide = 0.25f;
	[Tooltip("Time in seconds that shells persist in the world before being removed.")]
	public int shellDuration = 5;
	
	//audio sources
	private AudioSource []aSources;//Initialize audio sources
	private AudioSource firefx;//use multiple audio sources to play weapon sfx without skipping
	[HideInInspector]
	public AudioSource otherfx;
	
	[Header ("Sound Effects", order = 18)]
	[Space (10, order = 19)]
	[Tooltip("Weapon firing sound effect.")]
	public AudioClip fireSnd;
	[Tooltip("Offhand melee attack sound effect.")]
	public AudioClip meleeSnd;
	[Tooltip("Volume of firing sound effect.")]
	public float fireVol = 0.9f;
	[Tooltip("Weapon reloading sound effect.")]
	public AudioClip reloadSnd;
	[Tooltip("Usually shell reload sound + shotgun pump or rifle chambering sound.")]
	public AudioClip reloadLastSnd;
	[Tooltip("Sound effect for when weapon has no ammo.")]
	public AudioClip noammoSnd;
	[Tooltip("Sound effect for wepaon readying animation.")]
	public AudioClip readySnd;
	[Tooltip("Sound effect for pulling weapon back if firing on release of fire button.")]
	public AudioClip pullSnd;
	[Tooltip("Sound effect for blocking of attack.")]
	public AudioClip blockSound;
	
	//Precise firing timing
	private double nextFireTime;
	private double schedulingTime = 0.1; //time to allow system to arrange shot fire scheduling
	private double lastFireTime = 0;
	private double firingPauseMinimumTime = 0.05; //how fast the weapon can be be fired by rapid fire key pressing

	private AudioSource autoFireAsource1;
	private AudioSource autoFireAsource2;
	private bool curAutofireAsource;
	private float firePitch;
	
	[Header ("Flashlight", order = 20)]
	[Space (10, order = 21)]
	[Tooltip("True if this weapon has a flashlight attachment.")]
	public bool useLight;
	[Tooltip("Object that flashlight should inherit rotation from.")]
	public Transform lightBaseObj;
	[Tooltip("Should the facing of the light be reversed if this model was exported with yaw rotated 180 degrees? (Some weapon models need this).")]
	public bool flipLightFacing;
	private bool lightOnState;
	[Tooltip("True if the zoom key should toggle the flashlight in addition to the toggle light key.")]
	public bool useZoomSwitch;
	[Tooltip("True if the flashlight is on.")]
	public bool lightOn;
	[Tooltip("Mesh Renderer/Object of the flashlight cone effect that will be toggled on/off.")]
	public MeshRenderer lightConeMesh;
	[Tooltip("Spotlight object for flashlight.")]
	public Light spot;
	[Tooltip("Point light object for flashlight (shines on player's hand and weapon when flashlight is on).")]
	public Light point;
	
	//unused
//	[Header ("Barrel Check", order = 22)]
//	[Space (10, order = 23)]
//	[Tooltip("True if weapon should be moved back from collision with walls (for upscaled weapons and single camera setup to allow for better shadows on weapons from world geometry).")]
//	public bool doBarrelCheck;
//	private RaycastHit hitForward;
//	[HideInInspector]
//	public bool forwardDetect;
//	[HideInInspector]
//	public float forwardDetectAmt;
//	public float hitDist;
//	public float barrelForwardAmt = -0.4f;
//	public float barrelUpAmt = 0.12f;
//	public float barrelRightAmt = -0.1f;
//	public Vector3 barrelCheckOffset;
//	public float barrelCheckRadius = 0.2f;
//	public float barrelCheckDist = 1.25f;
//	public LayerMask forwardMask;

	void Start (){

		myTransform = transform;//cache transform for efficiency
		mainCamTransform = Camera.main.transform;
		
		playerObj = mainCamTransform.GetComponent<CameraControl>().playerObj;
		weaponObj = mainCamTransform.GetComponent<CameraControl>().weaponObj;
		
		AnimationComponent = GetComponent<Animation>();
		WeaponAnimationComponent = weaponMesh.GetComponent<Animation>();
		CameraAnimationComponent = Camera.main.GetComponent<Animation>();
		
		//define external script references (grab from FPSPlayer.cs to reduce GetComponent calls on init)
		FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		FPSWalkerComponent = FPSPlayerComponent.FPSWalkerComponent;
		IronsightsComponent = FPSPlayerComponent.IronsightsComponent;
		PlayerWeaponsComponent = FPSPlayerComponent.PlayerWeaponsComponent;
		InputComponent = FPSPlayerComponent.InputComponent;
		MouseLookComponent = FPSPlayerComponent.MouseLookComponent;
		WeaponEffectsComponent = FPSPlayerComponent.WeaponEffectsComponent;
		//VisibleBodyComponent = FPSWalkerComponent.VisibleBody.GetComponent<VisibleBody>();
		CameraControlComponent = Camera.main.GetComponent<CameraControl>();
		WeaponPivotComponent = FPSPlayerComponent.WeaponPivotComponent;
		
		weaponMeshInitalPos = weaponMesh.transform.localPosition;
		initialWeaponMeshScale = weaponMesh.transform.localScale;

		//Access GUIText instance that was created by the PlayerWeapons script
		ammoGuiObj = PlayerWeaponsComponent.ammoGuiObjInstance;
		AmmoText1 = ammoGuiObj.GetComponent<AmmoText>();//set reference for main color element of ammo GUIText
		AmmoText2 = ammoGuiObj.GetComponentsInChildren<AmmoText>();//set reference for shadow background color element of heath GUIText
		if(!FPSPlayerComponent.showAmmo){
			AmmoText1.guiTextComponent.enabled = false; 
			AmmoText2[1].guiTextComponent.enabled = false; 
		}

		//Initialize audio sources
		aSources = weaponObj.GetComponents<AudioSource>();
		otherfx = aSources[1];
		firefx = aSources[0];
		firefx.clip = fireSnd;//play fire sound
		autoFireAsource1 = aSources[0];
		autoFireAsource2 = aSources[2];

		//weapon shading
		shadeColor = PlayerWeaponsComponent.shadeColor;
		sunLightObj = PlayerWeaponsComponent.sunLightObj;
		
		meshRenderers = weaponMesh.GetComponentsInChildren<Renderer>();
		
		for(int i = 0; i < meshRenderers.Length; i++){
			if(meshRenderers[i] != ignoreMeshShading){
				tempWeapMaterials = meshRenderers[i].materials;
				for(int n = 0; n < tempWeapMaterials.Length; n++){
					materialList.Add(tempWeapMaterials[n]);
				}
			}
		}
		
		capsule = playerObj.GetComponent<CapsuleCollider>();
		
		if(shotOrigin){
			origin = shotOrigin.position;
		}else{
			if(muzzleFlash){
				origin = muzzleFlash.position;
				origin = origin + muzzleFlash.forward * shotOriginDist;
				muzzleFlash.localEulerAngles = Vector3.zero;
			}
		}
		
		if(muzzleFlash){
			muzzleRendererComponent = muzzleFlash.GetComponent<Renderer>();
			if(muzzleLightObj){
				muzzleLightComponent = muzzleLightObj.GetComponent<Light>();
			}
		}
		
		if(muzzleSmokeParticles){
			muzzleSmokeComponent = muzzleSmokeParticles.GetComponent<Renderer>();
		}
		
		//do not perform weapon actions if this is an unarmed/null weapon
		if(!unarmed){
			
			if(meleeSwingDelay == 0){//initialize muzzle flash color if not a melee weapon
				if(muzzleFlash){
					muzzleRendererComponent.enabled = false;
					muzzleFlashColor = muzzleRendererComponent.material.GetColor("_TintColor");
					if(muzzleLightObj){
						muzzleLightComponent.enabled = false;
					}
				}
				//clamp initial ammo amount in clip for non melee weapons
				bulletsLeft = Mathf.Clamp(bulletsLeft,0,bulletsPerClip);
			}else{
				//initial ammo amount in clip for melee weapons
				bulletsLeft = bulletsPerClip;	
			}
			
			if(semiAuto){//make muzzle flash fade out slower when gun is semiAuto
				if(projectileCount < 2){//make muzzle flash last slightly longer for shotguns
					muzzleFlashReduction = 3.5f;
				}else{
					muzzleFlashReduction = 2.0f;		
				}
			}else{
				if(projectileCount < 2){//make muzzle flash last slightly longer for shotguns
					muzzleFlashReduction = 6.5f;
				}else{
					muzzleFlashReduction = 2.0f;		
				}
			}
			
			//exclude layer 18, the liquidCollision layer, from bulletMask so second raycast 
			//of shots fired from above water don't continue to collide with water trigger
			liquidMask = bulletMask & ~(1 << 18);
			
			//initialize shot timers and animation settings
			shootStartTime = -1.0f;
			shotSpreadAmt = shotSpread;
			
			gunAngles = gunAnglesTarget;//initialize gun position damp angles
			initialRollAngle = weaponMesh.transform.localEulerAngles.z;
			myTransform.localEulerAngles = gunAngles;
			
			AnimationComponent["RifleSprinting"].speed = -1.5f;//init at this speed for correct rifle switching anim
			if(PistolSprintAnim){AnimationComponent["PistolSprinting"].speed = -1.5f;}//init at this speed for correct pistol switching anim
			
			//initialize weapon mesh animation speeds and wrap modes
			WeaponAnimationComponent["Fire"].speed = fireAnimSpeed;
			WeaponAnimationComponent["Ready"].speed = readyAnimSpeed;
			if(WeaponAnimationComponent["Reload"]){
				WeaponAnimationComponent["Reload"].speed = reloadAnimSpeed;
			}
			
			//If weapon reloads one bullet at a time, use anim called "Neutral" of hand returning to idle position
			//from reloading position to allow smooth anims when single bullet reloading is cancelled by sprinting.
			//The "Neutral" animation's wrap mode also needs to be set to "clamp forever" in the animation import settings. 
			if(bulletsToReload != bulletsPerClip){
				WeaponAnimationComponent["Neutral"].speed = 1.5f;
			}
			
			initialSilentShots = silentShots;//store original silent shots value
			
			//limit ammo to maxAmmo value
			ammo = Mathf.Clamp(ammo, 0, maxAmmo);
			//limit bulletsToReload value to bulletsPerClip value
			bulletsToReload = Mathf.Clamp(bulletsToReload, 0, bulletsPerClip);
			
			//make weapon recover faster from sprinting if using the pistol sprint anim 
			//because the gun/rifle style anims have more yaw movement and take longer to return to center
			if(!PistolSprintAnim){recoveryTimeAmt = 0.4f;}else{recoveryTimeAmt = 0.2f;}
			
		}
		
	}
	
	public void InitializeWeapon () {
		myTransform = transform;//cache transform for efficiency
		mainCamTransform = Camera.main.transform;
		
		playerObj = mainCamTransform.GetComponent<CameraControl>().playerObj;
		weaponObj = mainCamTransform.GetComponent<CameraControl>().weaponObj;
		//define external script references
		FPSPlayerComponent = playerObj.GetComponent<FPSPlayer>();
		IronsightsComponent = FPSPlayerComponent.IronsightsComponent;
		PlayerWeaponsComponent = weaponObj.GetComponent<PlayerWeapons>();
		WeaponAnimationComponent = weaponMesh.GetComponent<Animation>();
		FPSWalkerComponent = FPSPlayerComponent.FPSWalkerComponent;
		
		//Initialize audio sources
		aSources = weaponObj.GetComponents<AudioSource>();
		otherfx = aSources[1];
		firefx = aSources[0];
		firefx.clip = fireSnd;//play fire sound
		firefx.spatialBlend = 0.0f;
		firefx.panStereo = 0.0f;
		firefx.velocityUpdateMode = AudioVelocityUpdateMode.Auto;
		autoFireAsource1 = aSources[0];
		autoFireAsource2 = aSources[2];
		autoFireAsource2.clip = fireSnd;

		firingPauseMinimumTime = fireRate;

		CancelWeaponPull();//cancel weapon pull after weapon switching if fire button was held when switching
		
		capsule = playerObj.GetComponent<CapsuleCollider>();
		
		if(!unarmed){
			if((!doReload && !nonReloadWeapon) && ammo <= 0){//inventory of disposable, one-shot weapon has been depleted, don't show view model
				FPSWalkerComponent.hideWeapon = true;
			}
		}
		
		if(useLight){//initialize light state
			if(!lightOn){
				if(lightConeMesh){lightConeMesh.enabled = false;}
				if(spot){spot.enabled = false;}
				if(point){point.enabled = false;}
			}else{
				if(lightConeMesh){lightConeMesh.enabled = true;}
				if(spot){spot.enabled = true;}
				if(point){point.enabled = true;}
			}
		}
		
		//do not perform weapon actions if this is an unarmed/null weapon
		if(!unarmed && ((ammo > 0 || doReload) || meleeSwingDelay > 0.0f)){
			
			if(Time.timeSinceLevelLoad > 2 && PlayerWeaponsComponent.switching){//don't ready weapon on level load, just when switching weapons
				
				StopCoroutine("Reload");//stop reload coroutine if interrupting a non-magazine reload
				IronsightsComponent.reloading = false;//update reloading var in Ironsights script if cancelling reload to fire
				
				//track time that weapon was made active to calculate readyTime for syncing ready anim with weapon firing
				startTime = Time.time;
				
				myTransform = transform;//cache transforms for efficiency
				gunAngles = gunAnglesTarget;//initialize gun position damp angles
				myTransform.localEulerAngles = gunAngles;
				
				if(fireOnRelease || bulletsToReload != bulletsPerClip){
					WeaponAnimationComponent.playAutomatically = true;//to allow proper animation blending of weapons using fire on release (grenades, bows)
					WeaponAnimationComponent["Ready"].normalizedTime = 1.0f;
					WeaponAnimationComponent["Fire"].normalizedTime = 1.0f;
				}

				if(!isOffhandThrow){
					readyTimeAmt = readyTime;

					//play weapon readying sound
					otherfx.volume = 1.0f;
					otherfx.clip = readySnd;
					otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);

					//play weapon readying animation after it has just been selected
					if(!PlayerWeaponsComponent.offhandThrowActive){
						WeaponAnimationComponent.Rewind("Ready");
						WeaponAnimationComponent["Ready"].speed = readyAnimSpeed;
						WeaponAnimationComponent.CrossFade("Ready",0.35f,PlayMode.StopSameLayer);
					}
				}else{
					//reduce weapon readying time by offhandThrowReadyMod after offhand grenade throw to account for weapon readying animation not playing
					readyTimeAmt = readyTime * offhandThrowReadyMod;
					isOffhandThrow = false;
				}
			}
		}
		
	}
	
	//for testing - usused
//	void OnDrawGizmos() {
//
////				Gizmos.DrawLine (myPos, target.position + (transform.up * targetEyeHeight));
//
//		Vector3 startPos = Camera.main.transform.position 
//						+ (Camera.main.transform.right * barrelCheckOffset.x)
//						+ (Camera.main.transform.up * barrelCheckOffset.y)
//						+ (Camera.main.transform.forward * barrelCheckOffset.z);
//                            
//		Gizmos.color = Color.green;
//		Gizmos.DrawSphere(startPos, barrelCheckRadius);
//		Gizmos.color = Color.cyan;
//		Gizmos.DrawSphere(startPos + (weaponLookDirection * barrelCheckDist), barrelCheckRadius);
//		Gizmos.color = Color.red;
//		Gizmos.DrawSphere(hitForward.point, barrelCheckRadius * 0.2f);
//		
//	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Update Actions
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void LateUpdate(){
		if(doWeaponRoll && Time.timeScale > 0 && Time.deltaTime > 0){//allow pausing by setting timescale to 0
			//set roll bobbing amounts
			if(FPSWalkerComponent.canRun){//sprinting
				rollBobAmt = sprintRollBob;
			}else{//walking
				if(!FPSPlayerComponent.zoomed || IronsightsComponent.reloading || meleeSwingDelay != 0){
					rollBobAmt = walkRollBob;
				}else{
					rollBobAmt = zoomRollBob;
				}
			}
			
			if(meleeSwingDelay != 0f && FPSPlayerComponent.zoomed){
				rollSmoothSpeedAmt = 0.2f;
			}else{
				rollSmoothSpeedAmt = rollSmoothSpeed;
			}
			
			//set neutral roll angle targets
			if(shootStartTime + fireRate < Time.time){
				if(zoomIsBlock && FPSPlayerComponent.zoomed){
					if(FPSPlayerComponent.canBackstab){
						weapRollAmt = backstabRoll;
					}else{
						weapRollAmt = blockRoll;
					}
				}else{
					if(FPSPlayerComponent.canBackstab){
						weapRollAmt = backstabRoll;
					}else{
						weapRollAmt = 0f;
					}
				}
			}else{
				weapRollAmt = 0f;
			}
			
			float deltaAmt = Time.smoothDeltaTime * 60f;
			
			//calculate smoothed strafing amount for strafe feedback roll angles
			strafeSmoothed = Mathf.Lerp(strafeSmoothed, FPSWalkerComponent.inputX * 1.0f, Time.smoothDeltaTime * 7f);
			//calculate angle distance from neutral roll angle 
			rollNeutralAngle = Mathf.DeltaAngle(initialRollAngle + weapRollAmt, weaponMesh.transform.localEulerAngles.z);
			
			if(!FPSPlayerComponent.zoomed || zoomIsBlock){
				rollRot = Mathf.SmoothDampAngle(rollRot, (((rollNeutralAngle * rollSmoothSpeedAmt) * deltaAmt)
				                                	   + ((IronsightsComponent.side * rollSwayAmt) * deltaAmt)
				                                       - ((strafeSmoothed * deltaAmt) * strafeRoll)
				                                       - ((IronsightsComponent.HorizontalBob.dampOrg * 2.5f * rollBobAmt) * deltaAmt)), ref rollVel, Time.smoothDeltaTime);
			}else{
				rollRot = Mathf.SmoothDampAngle(rollRot, ((rollNeutralAngle * rollSmoothSpeedAmt) * deltaAmt), ref rollVel, Time.smoothDeltaTime);
				//compensate for floating point imprecision in RotateAround when player is a large distance from scene origin
				weaponMesh.transform.localPosition = Vector3.MoveTowards(weaponMesh.transform.localPosition, weaponMeshInitalPos, 0.05f * Time.smoothDeltaTime);
			}
			
			//rotate weapon around shotOrigin or muzzleFlash position 
			if(muzzleFlash){
				weaponMesh.transform.RotateAround(muzzleFlash.position, muzzleFlash.forward, rollRot);//define delta for framerate independence);
			}else if(shotOrigin){
				weaponMesh.transform.RotateAround(shotOrigin.position, shotOrigin.forward, rollRot);
			}

		}
		
	}
	
	void Update (){

		if(Time.timeScale > 0 && Time.deltaTime > 0){//allow pausing by setting timescale to 0
			
			horizontal = FPSWalkerComponent.inputX;//Get input from player movement script
			vertical = FPSWalkerComponent.inputY;
			
			weaponRay = new Ray(mainCamTransform.position, myTransform.parent.transform.forward);
			weaponLookDirection = (weaponRay.GetPoint(20) - mainCamTransform.position).normalized;
			
			//scale weapons down to the point of them being invisible so they will continue playing animations in third person mode
			//and scale them up when back in first person mode, where they will be playing the correct animation
			if(CameraControlComponent.thirdPersonActive){
				if(!thirdpersonState){
					if(WeaponAnimationComponent){
						weaponMesh.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
					}
					thirdpersonState = true;
				}
			}else{
				if(thirdpersonState){
					if(WeaponAnimationComponent){
						weaponMesh.transform.localScale = initialWeaponMeshScale;
					}
					thirdpersonState = false;
				}
			}
			
			if((!doReload && !nonReloadWeapon) && ammo <= 0){//inventory of disposable, one-shot weapon has been depleted, don't show view model
				FPSWalkerComponent.hideWeapon = true;
			}
			
			if(shadeWeapon){
				//weapon shading
				if(sunLightObj){
					if(Time.time > lastUpdate + updateInterval){
						litPos = mainCamTransform.position + (mainCamTransform.up * -0.25f);
						if(!manualShadingMode){
							if(Physics.Linecast(litPos, sunLightObj.position, bulletMask)) {
								shaded = true;
							}else{
								shaded = false;
							}
						}
						lastUpdate = Time.time;
					}
				}	
				
				if(shaded){
					if(smoothedColor.r > shadeColor.r){
						smoothedColor = Color.Lerp(smoothedColor, shadeColor, Time.smoothDeltaTime * 5.0f);
						for(int i = 0; i < materialList.Count; i++){
							materialList[i].SetColor("_Color", smoothedColor);
						}
					}
				}else{
					if(smoothedColor.r < 255.0f){
						smoothedColor = Color.Lerp(smoothedColor, Color.white, Time.smoothDeltaTime * 5.0f);
						for(int i = 0; i < materialList.Count; i++){
							materialList[i].SetColor("_Color", smoothedColor);
						}
					}
				}
				
			}else{
				if(smoothedColor.r < 255.0f){
					smoothedColor = Color.Lerp(smoothedColor, Color.white, Time.smoothDeltaTime * 5.0f);
					for(int i = 0; i < materialList.Count; i++){
						materialList[i].SetColor("_Color", smoothedColor);
					}
					shaded = false;
				}
			}
			
			//calculate shot origin from muzzleflash position or shotOrigin position
			if(shotOrigin){
				origin = shotOrigin.position;
			}else{
				if(muzzleFlash){
					origin = muzzleFlash.position;
					origin = origin + muzzleFlash.forward * -shotOriginDist;
					muzzleFlash.localEulerAngles = Vector3.zero;
				}
			}
			
			//pass ammo amounts to the ammo GuiText object if not a melee weapon or unarmed
			if(meleeSwingDelay == 0 && !unarmed){
				//pass ammo amount to Gui object to be rendered on screen
				AmmoText1.ammoGui = bulletsLeft;//main color
				AmmoText1.ammoGui2 = ammo;
				AmmoText2[1].ammoGui = bulletsLeft;//shadow background color
				AmmoText2[1].ammoGui2 = ammo;
				AmmoText1.horizontalOffsetAmt = AmmoText1.horizontalOffset;//normal position on screen
				AmmoText1.verticalOffsetAmt = AmmoText1.verticalOffset;
				AmmoText2[1].horizontalOffsetAmt = AmmoText2[1].horizontalOffset;
				AmmoText2[1].verticalOffsetAmt = AmmoText2[1].verticalOffset;
				if(!doReload){
					AmmoText1.showMags = false;
					AmmoText2[1].showMags = false;
				}else{
					AmmoText1.showMags = true;
					AmmoText2[1].showMags = true;
				}
			}else{
				//pass ammo amount to Gui object to be rendered on screen
				AmmoText1.ammoGui = bulletsLeft;//main color
				AmmoText1.ammoGui2 = ammo;
				AmmoText2[1].ammoGui = bulletsLeft;//shadow background color
				AmmoText2[1].ammoGui2 = ammo;
				AmmoText1.horizontalOffsetAmt = 5;//make ammo GUIText move off screen if using a melee weapon
				AmmoText1.verticalOffsetAmt = 5;
				AmmoText2[1].horizontalOffsetAmt = 5;
				AmmoText2[1].verticalOffsetAmt = 5;	
			}
			
			//do not perform weapon actions if this is an unarmed/null weapon
			if(!unarmed){
				
				if(FPSPlayerComponent.hitPoints >= 1.0f){
					
					//Determine if player is reloading last round during a non-magazine reload. 
					if(reloadLastStartTime + reloadLastTime > Time.time){
						lastReload = true;	
					}else{
						lastReload = false;		
					}

					if(doReload && !meleeActive){
						//cancel auto and manual reload if player starts sprinting
						if((FPSWalkerComponent.sprintActive && !FPSWalkerComponent.sprintReload)//cancel reloading while sprinting if sprintReload is false
						&& !InputComponent.fireHold
						//&& !lastReload//allow player to finish chambering last round of a non-magazine reload
						&& !FPSWalkerComponent.cancelSprint
						&& FPSWalkerComponent.moving
						//cancel auto and manual reload if player presses fire button during a non magazine reload and player has loaded at least 2 shells/bullets 	
						
						||(InputComponent.fireHold
						&& bulletsToReload != bulletsPerClip
						&& bulletsReloaded >= (bulletsToReload * 2.0f)
						&& reloadEndTime + reloadTime < Time.time)

						|| FPSWalkerComponent.hideWeapon){//cancel reload if player is climbing, swimming, or holding object and weapon is lowered
							if(IronsightsComponent.reloading){
								IronsightsComponent.reloading = false;
								//use StopCoroutine to completely stop reload() function and prevent
								//"yield return new WaitForSeconds(reloadTime);" from continuing to excecute
								StopCoroutine("Reload");
								//reset reloading vars for non-magazine reloads
								if(bulletsToReload != bulletsPerClip){
									bulletsReloaded = 0;
									reloadStartTime = -16f;
									reloadEndTime = -16f;
									reloadLastStartTime = -16f;
									//rewind Neutral animation when sprinting
									WeaponAnimationComponent["Neutral"].speed = 1.5f;
									WeaponAnimationComponent.Play("Neutral", PlayMode.StopSameLayer);//play Neutral animation	
								}	
								//fast forward camera animations to stop playback if sprinting
								CameraAnimationComponent["CameraReloadMP5"].normalizedTime = 1.0f;
								CameraAnimationComponent["CameraReloadAK47"].normalizedTime = 1.0f;
								CameraAnimationComponent["CameraReloadPistol"].normalizedTime = 1.0f;
								CameraAnimationComponent["CameraReloadSingle"].normalizedTime = 1.0f;
								CameraAnimationComponent["CameraSwitch"].normalizedTime = 1.0f;
								//if sprint interrupts reload more than half-way through, just give bulletsNeeded
								if(bulletsToReload == bulletsPerClip && reloadStartTime + reloadTime / 2 < Time.time && !sprintReloadState){
									bulletsNeeded = bulletsPerClip - bulletsLeft;
									//we have ammo left to reload
									if(ammo >= bulletsNeeded){
										ammo -= bulletsNeeded;//subtract bullets needed from total ammo
										bulletsLeft = bulletsPerClip;//add bullets to magazine 
									}else{
										bulletsLeft += ammo;//if ammo left is less than needed to reload, just load all remaining bullets
										ammo = 0;//out of ammo for this weapon now
									}
									sprintReloadState = true;//only preform this action once at beginning of sprint/reload check
								}else{//if we are less than half way through reload before sprint interrupted, cancel reload
									//stop reload sound from playing
									otherfx.clip = null;
									if(bulletsToReload == bulletsPerClip){
										//rewind reload animation when sprinting
										WeaponAnimationComponent["Reload"].speed = -reloadAnimSpeed * 1.5f;
										WeaponAnimationComponent.CrossFade("Reload", 0.35f, PlayMode.StopSameLayer);//play reloading animation backwards
									}		
								}
							}
						}else{
							//Start automatic reload if player is out of ammo and firing time has elapsed to allow finishing of firing animation and sound
							if (bulletsLeft <= 0 
						    && Time.time - shootStartTime > fireRate 
						    && (canShoot || FPSWalkerComponent.sprintReload)
						    && doReload){
								if( ammo > 0 
								&& !IronsightsComponent.reloading
								&& !PlayerWeaponsComponent.switching 
								&& ((startTime + readyTimeAmt) < Time.time)){
									StartCoroutine("Reload");
									//set animation speeds
									//make this check to prevent slow playing of non magazine anim for last bullet in inventory
									if(bulletsToReload == bulletsPerClip){
										WeaponAnimationComponent["Reload"].speed = reloadAnimSpeed;	
									}
									WeaponAnimationComponent["Ready"].speed = readyAnimSpeed;
								}
							}	
						}
					}
					
					//don't spawn shell if player started sprinting to avoid unrealistic movement of shell if sprint stops
					//					if(FPSWalkerComponent.canRun){
					//						StopCoroutine("SpawnShell");	
					//					}
					
					if(!FPSWalkerComponent.hideWeapon){//dont shoot if player is climbing or weapon is hidden
						//reset weapon pull anim after switching if fire button was held
						if(hideAnimState && WeaponAnimationComponent){
							WeaponAnimationComponent.CrossFade("Ready",0.001f, PlayMode.StopAll);
							WeaponAnimationComponent["Ready"].normalizedTime = 1.0f;
							hideAnimState = false;	
						}
						IronsightsComponent.climbMove = 0.0f;
						//enable/disable shooting based on various player states
						if(((!FPSWalkerComponent.sprintActive && !FPSWalkerComponent.prone) || (IronsightsComponent.reloading && !FPSWalkerComponent.sprintReload))
						   || FPSWalkerComponent.crouched
						   || (FPSPlayerComponent.zoomed && meleeSwingDelay == 0)
						   || ((Mathf.Abs(horizontal) > 0.75f) && (Mathf.Abs(vertical) < 0.0f) && !FPSWalkerComponent.prone && FPSWalkerComponent.forwardSprintOnly)
						   || FPSWalkerComponent.cancelSprint
						   || (!FPSWalkerComponent.grounded && FPSWalkerComponent.jumping)//don't play sprinting anim while jumping
						   || (FPSWalkerComponent.fallingDistance > 0.75f)//don't play sprinting anim while falling  
						   || (InputComponent.fireHold && !FPSWalkerComponent.prone)){
							//not sprinting
							//set sprint recovery timer so gun only shoots after returning to neutral
							if(!sprintState){
								recoveryTime = Time.time;
								sprintState = true;
							}
							canShoot = true;
							sprintReloadState = false;//reset sprintReloadState to allow another sprint reload cancel check
						}else{
							//sprinting (account for forwardSprintOnly var in sprinting check)
							if(((Mathf.Abs(vertical) > 0.0f && FPSWalkerComponent.forwardSprintOnly) || (!FPSWalkerComponent.forwardSprintOnly && FPSWalkerComponent.moving))
							|| (Mathf.Abs(horizontal) > 0.0f && FPSWalkerComponent.prone)){
								sprintState = false;
								if(IronsightsComponent.reloading && !FPSWalkerComponent.sprintReload){
									canShoot = false;
								}else{
									if(FPSPlayerComponent.zoomed && meleeSwingDelay == 0){
										canShoot = true;
									}else{
										canShoot = false;
									}
								}
							}else{
								//set sprint recovery timer so gun only shoots after returning to center
								if(!sprintState){
									recoveryTime = Time.time;
									sprintState = true;
								}
								canShoot = true;
							}
						}
					}else{
						CancelWeaponPull();//cancel weapon pull after weapon switching if fire button was held when switching
						hideAnimState = true;	
						if(!FPSWalkerComponent.lowerGunForClimb){
							if(!sprintState){
								recoveryTime = Time.time;
								sprintState = true;
							}
							canShoot = true;
						}else{
							if(meleeSwingDelay == 0){
								IronsightsComponent.climbMove = -0.4f;
							}else{
								IronsightsComponent.climbMove = -1.4f;
							}
							canShoot = false;
							sprintState = false;
						}
					}
					
					//Play noammo sound and manage other states where player can't fire weapon
					if (InputComponent.fireHold){
						if(cantFireState
						   && canShoot
						   && (bulletsLeft <= 0 && doReload)
						   && ammo <= 0 
						   && !meleeIfNoAmmo
						   && ((!PistolSprintAnim && AnimationComponent["RifleSprinting"].normalizedTime < 0.35f)//only play sound when weapon is centered
						    ||(PistolSprintAnim && AnimationComponent["PistolSprinting"].normalizedTime < 0.35f))){
							otherfx.volume = 1.0f;
							otherfx.clip = noammoSnd;
							otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
							shooting = false;
							cantFireState = false;
						}
					}else{
						cantFireState = true;
						waterFireState = false;//allow player to fire again after surfacing from underwater if they released the fire button
					}
					
					//Change fire mode
					if (InputComponent.fireModePress || InputComponent.xboxDpadUpPress){
						if(fireModeState
						   && canShoot
						   && !IronsightsComponent.reloading
						   && ((!PistolSprintAnim && AnimationComponent["RifleSprinting"].normalizedTime < 0.35f)//only change fire mode when weapon is centered
						    ||(PistolSprintAnim && AnimationComponent["PistolSprinting"].normalizedTime < 0.35f))){
							
							burstState = false;
							burstShotsFired = 0;
							doReleaseFire = false;
							
							if(fireModeSelectable && semiAuto){
								
								semiAuto  = false;
								fireModeState = false;
								if(projectileCount < 2){//make muzzle flash last slightly longer for semiAuto
									muzzleFlashReduction = 6.5f;
								}else{
									muzzleFlashReduction = 2.0f;		
								}
								otherfx.volume = 1.0f;
								otherfx.clip = noammoSnd;
								otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
								
								if(burstAndAuto){//if all three fire modes are available, choose burst fire after semi auto
									if(!burstFire){
										burstFire = true;
									}
								}
								
							}else if(fireModeSelectable && !semiAuto){
								
								if(!burstAndAuto){
									semiAuto  = true;
								}else{//if all three fire modes are available, choose auto fire after burst
									if(burstFire){
										burstFire = false;	
									}else{
										semiAuto  = true;//if auto mode is active, switch back to semi auto
									}
								}
								
								fireModeState = false;
								if(projectileCount < 2){//make muzzle flash last slightly longer for shotguns
									muzzleFlashReduction = 3.5f;
								}else{
									muzzleFlashReduction = 2.0f;		
								}
								otherfx.volume = 1.0f;
								otherfx.clip = noammoSnd;
								otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);	
							}
						}
					}else{
						fireModeState = true;
					}
					
					//emit barrel smoke particles after firing if player has fired more than barrelSmokeShots
					if(useBarrelSmoke && !FPSWalkerComponent.holdingBreath && !CameraControlComponent.thirdPersonActive){
						if(!emitBarrelSmoke){
							if(Time.time - shootStartTime > 0.25f){					
								if(barrelSmokeParticles && bulletsJustFired >= barrelSmokeShots ){
									emitBarrelSmoke = true;
									barrelSmokeTime = Time.time;
								}
							}
						}else{
							if(barrelSmokeTime + 0.5f > Time.time){
								if((!InputComponent.fireHold 
								   || IronsightsComponent.reloading
								   //emit particles if player is still holding fire button and fire time has elapsed for semi auto
								   || ((Time.time - shootStartTime > 0.1f) && InputComponent.fireHold))
								   && lastMeleeTime + (meleeAttackTime * 2f) < Time.time
								   && !FPSWalkerComponent.sprintActive){
									if(!FPSPlayerComponent.zoomed || WeaponPivotComponent.deadzoneZooming){
										barrelSmokeParticles.transform.position = mainCamTransform.position + (mainCamTransform.right * barrelSmokeOffset.x) + (mainCamTransform.up * barrelSmokeOffset.y) + (mainCamTransform.forward * smokeForward) + (weaponLookDirection * barrelSmokeOffset.z);//muzzleFlash.position;
									}else{
										barrelSmokeParticles.transform.position = mainCamTransform.position + (mainCamTransform.forward * smokeForward) + (weaponLookDirection * barrelSmokeOffset.z);//muzzleFlash.position;
									}
									barrelSmokeParticles.Emit();
								}
							}else{
								emitBarrelSmoke = false;	
							}
						}
					}
					
					//reset bulletsJustFired value when not firing after a time
					if(Time.time - shootStartTime > fireRate + 0.2f){	
						bulletsJustFired = 0;	
					}
					
				}
				
				//move gun back if collision is detected on view model - unused
//				RaycastHit hit2;
//				forwardDetect = Physics.Raycast(mainCamTransform.position + mainCamTransform.right * barrelCheckOffset, mainCamTransform.forward, out hit2, 1f, bulletMask);
//				if((startTime + 0.2f) < Time.time){
//					forwardDetect = Physics.CheckCapsule(mainCamTransform.position, mainCamTransform.position + mainCamTransform.forward * 1f, 0.15f, bulletMask);
//				}else{
//					forwardDetect = true;
//				}

//				Vector3 startPos = mainCamTransform.position 
//						+ (mainCamTransform.right * barrelCheckOffset.x)
//						+ (mainCamTransform.up * barrelCheckOffset.y)
//						+ (mainCamTransform.forward * barrelCheckOffset.z);
//						
//						
//				if(doBarrelCheck 
//				&& !CameraControlComponent.thirdPersonActive 
//				&& Physics.SphereCast(startPos, barrelCheckRadius, weaponLookDirection, out hitForward, barrelCheckDist, forwardMask)
////				&& Physics.CheckSphere(startPos + (weaponLookDirection * barrelCheckDist), barrelCheckRadius, bulletMask)
//				){
//					forwardDetect = true;
////					if(MouseLookComponent.inputY > -70){
//						hitDist = hitForward.distance - barrelCheckRadius;
//						forwardDetectAmt = barrelCheckDist - hitDist;
////					}else{
////						hitDist = 1f;
////						forwardDetectAmt = -barrelForwardAmt;
////					}
//				}else{
//					forwardDetect = false;
////					if(MouseLookComponent.inputY > -70){
//						hitDist = 0f;
//						forwardDetectAmt = 0f;
////					}else{
////						hitDist = 1f;
////						forwardDetectAmt = -barrelForwardAmt;
////					}
//				}
				
				
				//Run weapon sprinting animations
				if((((canShoot || FPSWalkerComponent.hideWeapon)//allow gun to stay centered and lowered when climbing, swimming, or holding object
				|| FPSWalkerComponent.crouched
				|| (FPSWalkerComponent.midPos < FPSWalkerComponent.standingCamHeight && FPSWalkerComponent.proneRisen)//player is crouching
				|| IronsightsComponent.reloading
				|| FPSWalkerComponent.cancelSprint)
				|| !FPSWalkerComponent.moving) 
				/*&& !forwardDetect*/){
					if(sprintAnimState){//animate weapon up
						//store time that sprint anim started to disable weapon switching during transition
						PlayerWeaponsComponent.sprintSwitchTime = Time.time;
						
						if(!PistolSprintAnim){
							//keep playback at last frame of animation to prevent it from being interrupted and to allow 
							//instant reversal of playback intstead of continuing past an animation playback time of 1
							if(AnimationComponent["RifleSprinting"].normalizedTime > 1){AnimationComponent["RifleSprinting"].normalizedTime = 1;}
							//reverse animation speed for smooth changing of direction/reversal
							//animation will need to finish before recoveryTime has elapsed to prevent twisting of view when recovering from sprint
							AnimationComponent["RifleSprinting"].speed = -2.0f;
							AnimationComponent.CrossFade("RifleSprinting", 0.35f,PlayMode.StopSameLayer);
						}else{
							if(AnimationComponent["PistolSprinting"].normalizedTime > 1){AnimationComponent["PistolSprinting"].normalizedTime = 1;}
							//reverse animation speed for smooth changing of direction/reversal
							//animation will need to finish before recoveryTime has elapsed to prevent twisting of view when recovering from sprint
							AnimationComponent["PistolSprinting"].speed = -4.0f;
							AnimationComponent.CrossFade("PistolSprinting", 0.35f,PlayMode.StopSameLayer);	
						}
						//set sprintAnimState to false to only perform these actions once per change of sprinting state checks
						sprintAnimState = false;
					}
				}else{
					if(!sprintAnimState){//animate weapon down
						//store time that sprint anim started to disable weapon switching during transition
						PlayerWeaponsComponent.sprintSwitchTime = Time.time;
						
						if(!PistolSprintAnim){
							//keep playback at first frame of animation to prevent it from being interrupted and to allow 
							//instant reversal of playback intstead of continuing past an animation playback time of 0 into negative values
							if(AnimationComponent["RifleSprinting"].normalizedTime < 0){AnimationComponent["RifleSprinting"].normalizedTime = 0;}
							//reverse animation speed for smooth changing of direction
							AnimationComponent["RifleSprinting"].speed = 2.0f;
							AnimationComponent.CrossFade("RifleSprinting", 0.35f,PlayMode.StopSameLayer);
						}else{
							//keep playback at first frame of animation to prevent it from being interrupted and to allow 
							//instant reversal of playback intstead of continuing past an animation playback time of 0 into negative values
							if(AnimationComponent["PistolSprinting"].normalizedTime < 0){AnimationComponent["PistolSprinting"].normalizedTime = 0;}
							//reverse animation speed for smooth changing of direction
							AnimationComponent["PistolSprinting"].speed = 4.0f;
							AnimationComponent.CrossFade("PistolSprinting", 0.35f,PlayMode.StopSameLayer);	
						}
						
						//set sprintAnimState to true to only perform these actions once per change of sprinting state checks
						sprintAnimState = true;
						//rewind reloading animation if reload is interrupted
						if(PlayerWeaponsComponent.switching && IronsightsComponent.reloading){
							WeaponAnimationComponent.CrossFade("Reload", 0.35f,PlayMode.StopSameLayer);
						}
					}
				}
			}
		}
		
		//update weapon sound effects pitches and keep them sync'ed with timescale
		otherfx.pitch = Time.timeScale;
		firefx.pitch = firePitch * Time.timeScale;
		autoFireAsource1.pitch = firefx.pitch;
		autoFireAsource2.pitch = firefx.pitch;
		
		if(Time.timeScale > 0){//allow pausing by setting timescale to 0
			
			//do not perform weapon actions if this is an unarmed/null weapon
			if(!unarmed && FPSPlayerComponent.hitPoints > 0){
				//Fade out muzzle flash alpha 
				if(muzzleFlash && !CameraControlComponent.thirdPersonActive){
					if (muzzleRendererComponent.enabled){
						if(muzzleFlashColor.a > 0.0f){
							muzzleFlashColor.a -= muzzleFlashReduction * Time.deltaTime;
							if(muzzleFlashColor.a < 0.0f){muzzleFlashColor.a = 0.0f;}//prevent alpha from going into negative values 
							muzzleRendererComponent.material.SetColor("_TintColor", muzzleFlashColor);
						}else{
							muzzleRendererComponent.enabled = false;//disable muzzle flash object after alpha has faded
						}	
					}
				}else{
					if(muzzleFlash){
						muzzleRendererComponent.enabled = false;
					}
				}
				
				//activate muzzle light for muzzle flash
				if(muzzleLightObj && !CameraControlComponent.thirdPersonActive){
					if(muzzleLightComponent.enabled){
						if(muzzleLightComponent.intensity > 0.0f){
							if(Time.time - shootStartTime > muzzleLightDelay){
								muzzleLightComponent.intensity -= muzzleLightReduction * Time.deltaTime;	
							}
						}else{
							muzzleLightComponent.enabled = false;
						}
					}
				}
				
				//start reload if reload button is pressed
				if (InputComponent.reloadPress
			    && !IronsightsComponent.reloading
			    && ammo > 0 
			    && doReload
			    && bulletsLeft < bulletsPerClip
			    && Time.time - shootStartTime > fireRate
			    && !InputComponent.fireHold){
					sprintReloadState = true;
					StartCoroutine("Reload");
				}
				
				
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				//Fire On Release
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////			
				
				//check if weapon should be fired after fire button is released (for grenades, bow & arrows)
				if(fireOnRelease && ammo > 0 && !meleeActive && !PlayerWeaponsComponent.displayingGrenade && !FPSWalkerComponent.holdingObject){
					if(((InputComponent.fireHold && !PlayerWeaponsComponent.offhandThrowActive) 
					|| (weaponNumber == PlayerWeaponsComponent.grenadeWeapon && PlayerWeaponsComponent.pullGrenadeState)) 
					&& Time.time - shootStartTime > fireRate 
					&& releaseTimer + releaseTime < Time.time){
						if(fireHoldTimer < maxHoldTime){
							fireHoldTimer += Time.deltaTime;
							fireHoldMult = fireHoldTimer / maxHoldTime;
						}
						if(!pullAnimState && !releaseAnimState){//play weapon pull animation
							if(WeaponAnimationComponent["Pull"]){
								WeaponAnimationComponent["Pull"].layer = 1;
								WeaponAnimationComponent.Rewind("Pull");
								if(!PlayerWeaponsComponent.offhandThrowActive){
									WeaponAnimationComponent["Pull"].speed = PullAnimSpeed;
								}else{
									WeaponAnimationComponent["Pull"].speed = PullAnimSpeed * 1.25f;
								}
								WeaponAnimationComponent.CrossFade("Pull", 0.35f,PlayMode.StopSameLayer);//play pulling animation
							}
							
							if(doCamPullAnim1){
								CameraAnimationComponent.Rewind("CameraMeleeSwingRight");//rewind camera swing animation 
								CameraAnimationComponent["CameraMeleeSwingRight"].speed = CamPullAnim1speed;//set camera animation speed
								CameraAnimationComponent.CrossFade("CameraMeleeSwingRight", 0.35f,PlayMode.StopSameLayer);//play camera view animation
							}
							
							if(doCamPullAnim2){
								CameraAnimationComponent.Rewind("CameraSwitch");//rewind camera swing animation 
								CameraAnimationComponent["CameraSwitch"].speed = CamPullAnim2speed;//set camera animation speed
								CameraAnimationComponent.CrossFade("CameraSwitch", 0.1f,PlayMode.StopSameLayer);//play camera view animation
							}
							
							if(pullSnd){
								otherfx.volume = 1.0f;
								otherfx.clip = pullSnd;
								otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
							}
							pullTimer = Time.time;
							pullAnimState = true;
							fireOnReleaseState = true;
							doReleaseFire = false;
							releaseAnimState = true;
						}

						//set up offhand throw
						if(PlayerWeaponsComponent.pullGrenadeState
						&& weaponNumber == PlayerWeaponsComponent.grenadeWeapon 
						&& !InputComponent.grenadeHold){
							PlayerWeaponsComponent.pullGrenadeState = false;
						}

					}else{
						if(fireOnReleaseState && pullTimer + pullTime < Time.time && startTime + readyTimeAmt < Time.time){
							
							pullAnimState = false;
							releaseTimer = Time.time;

							if(!doReload || !IronsightsComponent.reloading){//fire or throw weapon
								WeaponAnimationComponent["Fire"].time = 0.0f;
								WeaponAnimationComponent["Fire"].layer = 1;
								WeaponAnimationComponent.Rewind("Fire");
								WeaponAnimationComponent["Fire"].speed = fireAnimSpeed;
								WeaponAnimationComponent.CrossFade("Fire", 0.15f,PlayMode.StopAll);//play firing animation
								
								if(doCamReleaseAnim1){
									CameraAnimationComponent.Rewind("CameraMeleeSwingRight");//rewind camera swing animation 
									CameraAnimationComponent["CameraMeleeSwingRight"].speed = CamReleaseAnim1speed;//set camera animation speed
									CameraAnimationComponent.CrossFade("CameraMeleeSwingRight", 0.35f,PlayMode.StopSameLayer);//play camera view animation
								}
								
								if(doCamReleaseAnim2){
									CameraAnimationComponent.Rewind("CameraSwitch");//rewind camera swing animation 
									CameraAnimationComponent["CameraSwitch"].speed = CamReleaseAnim2speed;//set camera animation speed
									CameraAnimationComponent.CrossFade("CameraSwitch", 0.1f,PlayMode.StopSameLayer);//play camera view animation
								}
								
							}
							
							fireOnReleaseState = false;
						}
					}

					if(!fireOnReleaseState && releaseTimer + releaseTime < Time.time){
						if(releaseAnimState){
							if(!doReload || !IronsightsComponent.reloading){
								doReleaseFire = true;//actually fire on release now
								otherfx.Stop();
							}
							releaseAnimState = false;
						}
						if(PlayerWeaponsComponent.offhandThrowActive && weaponNumber == PlayerWeaponsComponent.grenadeWeapon){
							PlayerWeaponsComponent.grenadeThrownState = true;
						}
					}
				}
				
				
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
				//Offhand Melee Attack
				/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////			
				
				if(offhandMeleeAttack 
				&& !FPSWalkerComponent.holdingObject 
				&& ((MouseLookComponent.inputY > -70 && !allowDownwardMelee) || allowDownwardMelee)){
					if((InputComponent.meleePress 
					|| (meleeIfNoAmmo
						&& meleeSwingDelay == 0.0f 
						&& ammo == 0 
						&& bulletsLeft == 0 
						&& InputComponent.firePress))//also melee attack if out of ammo
					&& !meleeActive 
					&& Time.time - shootStartTime > fireRate 
					&& lastMeleeTime + meleeAttackTime < Time.time
				    && !PlayerWeaponsComponent.offhandThrowActive 
					&& fireHoldMult == 0.0f
					&& startTime + readyTimeAmt < Time.time
					&& !(FPSWalkerComponent.prone && FPSWalkerComponent.moving)
					&& !burstState){
						meleeActive = true;
						lastMeleeTime = Time.time;
						meleeBlendState = true;
						//allow offhand melee attack to interrupt auto fire
						lastFireTime = 0f;
						nextFireTime = 0f;
						//stop reload coroutine if interrupting reload
						StopCoroutine("Reload");
						IronsightsComponent.reloading = false;//update reloading var in Ironsights script if cancelling reload
						otherfx.clip = null;//stop playing reload sound effect if cancelling reload
						//rewind reloading animations
						CameraAnimationComponent.Rewind("Reload");
						WeaponAnimationComponent["Reload"].normalizedTime = 1.0f;
						WeaponAnimationComponent["Fire"].normalizedTime = 1.0f;
						CameraAnimationComponent.Rewind("CameraReloadAK47");
						CameraAnimationComponent.Rewind("CameraReloadPistol");
						CameraAnimationComponent.Rewind("CameraReloadMP5");
						
						Fire();
					}
					if(lastMeleeTime + meleeAttackTime < Time.time){
						meleeActive = false;
						if(meleeBlendState && bulletsToReload != bulletsPerClip){//play ready animation after offhand  melee attack, but fast forward to end to allow blending to reload anim if non-magazine reload
							WeaponAnimationComponent.CrossFade("Ready",0.35f,PlayMode.StopSameLayer);
							WeaponAnimationComponent["Ready"].normalizedTime = 1.0f;	
							meleeBlendState = false;
						}
					}
				}


				//Detect firemode (auto or semi auto) and call fire function
				if((InputComponent.fireHold && !fireOnRelease && !meleeActive)
				|| (fireOnRelease && doReleaseFire)
				&& !waterFireState){
					if(semiAuto){
						if(!semiState){
							Fire();
							semiState = true;
						}
					}else{
						if(!burstFire){//fire weapon in auto mode
							Fire();
						}else{//fire weapon in burst mode
							if(!burstState && !burstHold){
								burstState = true;
								if(burstShotsFired < burstShots){
									burstHold = true;//stop firing if in burst has finished, but fire button is still held
								}
							}
						}
					}
				}else{
					semiState = false;
					burstHold = false;
				}
				
				//fire shots in burst mode
				if(burstState && burstFire){
					if(!canShoot//cancel burst fire under these conditions
					|| IronsightsComponent.reloading
					|| PlayerWeaponsComponent.switching 
					|| AnimationComponent["RifleSprinting"].normalizedTime > 0.35f//weapon not centered after sprint
					|| ((startTime + readyTimeAmt) > Time.time)){
						burstState = false;
						burstShotsFired = 0;	
					}
					if(burstShotsFired < burstShots){//fire burst shots
						Fire();
					}else{
						if(burstShotsFired >= burstShots){//allow another burst to fire if fire button is released
							burstState = false;
							burstShotsFired = 0;
						}
						
					}
				}
				
				//set shooting var to false
				if(Time.time - shootStartTime > 0.2f){
					shooting = false;	
				}
				
			}else{
				//stop weapon animations if unarmed or dead
				if(!unarmed && FPSPlayerComponent.hitPoints < 1){
					if(doReleaseFire){
						WeaponAnimationComponent.Stop("Pull");
						PlayerWeaponsComponent.offhandThrowActive = false;
					}
					WeaponAnimationComponent.Stop("Fire");
				}
			}
			
			//smooth gun angle animation amounts 
			if(Time.smoothDeltaTime > 0.0f && !unarmed){
				gunAngles.x = Mathf.SmoothDampAngle(gunAngles.x, gunAnglesTarget.x, ref gunAngleVel.x, 0.14f, Mathf.Infinity, Time.smoothDeltaTime);
				gunAngles.y = Mathf.SmoothDampAngle(gunAngles.y, gunAnglesTarget.y, ref gunAngleVel.y, 0.14f, Mathf.Infinity, Time.smoothDeltaTime);
				gunAngles.z = Mathf.SmoothDampAngle(gunAngles.z, gunAnglesTarget.z, ref gunAngleVel.z, 0.14f, Mathf.Infinity, Time.smoothDeltaTime);
				myTransform.localEulerAngles = gunAngles;
			}
			
			
			//turn flashlight attachment on or off
			if(useLight){

				spot.transform.rotation = lightBaseObj.rotation;
				if(flipLightFacing){
					spot.transform.rotation *= Quaternion.Euler(0,180f,0);
				}
				
				if(InputComponent.flashlightPress || (useZoomSwitch && InputComponent.zoomPress)){
					otherfx.volume = 1.0f;
					otherfx.clip = noammoSnd;
					otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
					if(!lightOn){
						if(lightConeMesh){lightConeMesh.enabled = true;}
						if(spot){spot.enabled = true;}
						if(point){point.enabled = true;}
						lightOn = true;
					}else{
						if(lightConeMesh){lightConeMesh.enabled = false;}
						if(spot){spot.enabled = false;}
						if(point){point.enabled = false;}
						lightOn = false;
					}
				}
			}
				
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Set Up Fire Event
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void Fire(){

		//do not proceed to fire if out of ammo, have already fired in semi-auto mode, or chambering last round
		if(!meleeActive &&
		((bulletsLeft <= 0 && doReload) 
		|| (!doReload && ammo <= 0)
		|| (semiAuto && semiState))){
			return;
		}
		
		if(CameraControlComponent.rotating){
			return;//don't shoot if camera toggle button is being held
		}
			
		//don't fire this weapon underwater if fireableUnderwater var is false
		if(FPSWalkerComponent.holdingBreath && !meleeActive && (!fireableUnderwater || FPSWalkerComponent.lowerGunForSwim)){ 
			if(cantFireState){
				otherfx.volume = 1.0f;
				otherfx.clip = noammoSnd;
				otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
				cantFireState = false;//only play noammo sound once if this is an automatic weapon
				waterFireState = true;//make player have to press fire button again after surfacing to fire
			}
			return;
		}

		//fire weapon
		//don't allow fire button to interrupt a magazine reload
		if(((bulletsToReload == bulletsPerClip && !IronsightsComponent.reloading) || meleeActive)
		   || (!doReload && ammo > 0)
		   //allow normal firing when weapon does not reload by magazine
		   || (!IronsightsComponent.reloading && bulletsToReload != bulletsPerClip && bulletsLeft > 0)
		   //allow fire button to interrupt a non-magazine reload if there are at least 2 shells loaded
		   || (IronsightsComponent.reloading && bulletsToReload != bulletsPerClip && bulletsLeft >= bulletsToReload * 2.0f && reloadEndTime + reloadTime < Time.time))
		{
			if ((canShoot && !PlayerWeaponsComponent.switching) || doReleaseFire || meleeActive){//don't allow shooting when reloading, sprinting, or switching

				//Check sprint recovery timer so gun only shoots after returning to center.
				if(((recoveryTime + recoveryTimeAmt < Time.time || doReleaseFire || meleeActive) && (startTime + readyTimeAmt < Time.time))){

					double time = AudioSettings.dspTime;//use the audio engine's time for shot timing because it is more precise at smaller intervals

					if(lastFireTime + (fireRate * 0.8f) > time){
						return;
					}

					if (time > lastFireTime + firingPauseMinimumTime) {
						nextFireTime = time; //Set next sound to be played immediately.
					}

					if(Time.timeScale < 1.0f && Time.time < shootStartTime + fireRate){
						return;
					}

					if (time > nextFireTime - schedulingTime){
						firePitch = Random.Range(0.96f, 1.0f);//add slight random value to firing sound pitch for variety
						if(!meleeActive){
							if(Time.timeScale == 1.0f){
								PlayerAltAutoFireSources(nextFireTime);
							}else{
								firefx.clip = fireSnd;
								firefx.PlayOneShot(firefx.clip, fireVol);//play fire sound
							}
							silentShots = initialSilentShots;
						}else{
							firefx.clip = meleeSnd;
							firefx.PlayOneShot(firefx.clip, fireVol);//play fire sound
							silentShots = true;
						}
						
						lastFireTime = nextFireTime;
						nextFireTime += fireRate;
					

						//reset bullets reloaded for non magazine reloading weapons
						if(bulletsToReload != bulletsPerClip){
							bulletsReloaded = 0;
							reloadStartTime = -16f;
							reloadEndTime = -16f;
							reloadLastStartTime = -16f;
						}

						StopCoroutine("FireOneShot");
						StartCoroutine("FireOneShot");//fire bullet
						StopCoroutine("Reload");//stop reload coroutine if interrupting a non-magazine reload
						IronsightsComponent.reloading = false;//update reloading var in Ironsights script if cancelling reload to fire
						otherfx.clip = null;//stop playing reload sound effect if cancelling reload to fire
						
						if(meleeSwingDelay == 0 && !meleeActive){//eject shell and perform muzzle flash if not a melee weapon
							if(!CameraControlComponent.thirdPersonActive){
								MuzzFlash();
							}
							if(spawnShell){
								StartCoroutine("SpawnShell");
							}
							bulletsJustFired ++;//track number of shots fired recently to determine when to emit barrel smoke particles
						}
						
						if(!FPSWalkerComponent.holdingObject){
							//track time that we started firing
							shootStartTime = Time.time;
							shooting = true;
							doReleaseFire = false;
						}
					}
				}
			}
		}

	}

	//alternate audiosources to play scheduled auto fire sound effects to prevent playback cutoff
	public void PlayerAltAutoFireSources (double playTime){
		firefx.clip = fireSnd;
		autoFireAsource2.clip = fireSnd;
		if(curAutofireAsource){
			autoFireAsource1.volume = fireVol;
			autoFireAsource1.PlayScheduled(playTime);
			curAutofireAsource = false;
		}else{
			autoFireAsource2.volume = fireVol;
			autoFireAsource2.PlayScheduled(playTime);
			curAutofireAsource = true;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Fire Projectile
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	IEnumerator FireOneShot (){
	
		//wait for a time if sprinting to allow wepaon to return to center
		if(meleeActive && FPSWalkerComponent.sprintActive){
			yield return new WaitForSeconds(0.15f);	
		}
		
		hitCount = 0;//reset hitCount so flesh impacts are counted from zero again for this shot
		
		if(meleeSwingDelay == 0 && !meleeActive){//if this is not a melee weapon
			
			if(useWeaponKick){
				//rewind firing animation and set speed
				WeaponAnimationComponent.Rewind("Fire");
				WeaponAnimationComponent["Fire"].speed = fireAnimSpeed;
				WeaponAnimationComponent.CrossFade("Fire", fireAnimFadeSpeed,PlayMode.StopSameLayer);//play firing animation
				//make view recoil with shot
				WeaponKick();
			}
			
			if(doReload){
				bulletsLeft--;//subtract fired bullet from magazine amount	
			}else{
				ammo--;
			}
			
			if(burstFire){
				burstShotsFired++;
			}
			
		}else{
			
			if((swingSide || FPSPlayerComponent.canBackstab) && !meleeActive){//determine which side to swing melee weapon, swing from left if this is a backstab
				CameraAnimationComponent.Rewind("CameraMeleeSwingRight");//rewind camera swing animation 
				CameraAnimationComponent["CameraMeleeSwingRight"].speed = 1.7f;//set camera animation speed
				CameraAnimationComponent.CrossFade("CameraMeleeSwingRight", 0.35f, PlayMode.StopSameLayer);//play camera view animation
				
				WeaponAnimationComponent.Rewind("MeleeSwingRight");
				WeaponAnimationComponent["MeleeSwingRight"].speed = fireAnimSpeed;//set weapon swing animation speed
				WeaponAnimationComponent.CrossFade("MeleeSwingRight", 0.1f, PlayMode.StopSameLayer);//play weapon swing animation
				
				swingSide = false;//set swingSide to false to make next swing from other direction 
				
			}else{
				CameraAnimationComponent.Rewind("CameraMeleeSwingLeft");//rewind camera swing animation 
				CameraAnimationComponent["CameraMeleeSwingLeft"].speed = 1.6f;//set camera animation speed
				CameraAnimationComponent.CrossFade("CameraMeleeSwingLeft", 0.35f, PlayMode.StopSameLayer);//play camera view animation
				
				if(!meleeActive){
					WeaponAnimationComponent.Rewind("MeleeSwingLeft");
					WeaponAnimationComponent["MeleeSwingLeft"].speed = fireAnimSpeed;//set weapon swing animation speed
					WeaponAnimationComponent.CrossFade("MeleeSwingLeft",0.1f, PlayMode.StopSameLayer);//play weapon swing animation
					
					swingSide = true;//set swingSide to true to make next swing from other direction 
					
				}else{
					if(WeaponAnimationComponent["Melee"]){
						WeaponAnimationComponent.Rewind("Melee");
						WeaponAnimationComponent["Melee"].speed = meleeAnimSpeed;//set weapon swing animation speed
						WeaponAnimationComponent.CrossFade("Melee", 0.1f, PlayMode.StopSameLayer);//play weapon swing animation
					}
				}
				
			}
			
			if(!meleeActive){
				//wait for the meleeSwingDelay amount while swinging forward before hitting anything
				yield return new WaitForSeconds(meleeSwingDelay);	
			}else{
				//wait for the meleeSwingDelay amount while swinging forward before hitting anything
				yield return new WaitForSeconds(offhandMeleeDelay);	
			}	
		}
		
		//fire the number of projectiles defined by projectileCount 
		for(float i = 0; i < projectileCount; i++){
			direction = SprayDirection();
			RaycastHit hit;
			
			//set up weapon look direction and origin
			Vector3 shotOrigin = playerObj.transform.position + playerObj.transform.up * 0.8f;
			float playerDist = Vector3.Distance(mainCamTransform.position, shotOrigin);
			RaycastHit tpRayHit;
			Vector3 cameraForwardPoint;
			ArrowObject ArrowRef;
			
			if(Physics.Raycast(mainCamTransform.position + (mainCamTransform.forward * playerDist), mainCamTransform.forward + direction, out tpRayHit, range, bulletMask)){
				cameraForwardPoint = tpRayHit.point;
			}else{
				cameraForwardPoint = mainCamTransform.position + ((mainCamTransform.forward + direction) * 20f);
			}
			
			lookDirection = (cameraForwardPoint - shotOrigin).normalized;
			
			//spawn pooled projectile object
			if(projectilePoolIndex != 0 && !meleeActive){

				float projSpawnForwardAmt;
				if(MouseLookComponent.dzAiming){
					projSpawnForwardAmt = 0.5f;
				}else{
					projSpawnForwardAmt = projSpawnForward;//allow projectiles to spawn farther behind camera, so arrows can hit at point blank range
				}
			
				//set up pooled projectile object 
				if(!CameraControlComponent.thirdPersonActive){
					projectile = AzuObjectPool.instance.SpawnPooledObj(projectilePoolIndex, mainCamTransform.position + (mainCamTransform.forward * projSpawnForwardAmt), mainCamTransform.rotation) as GameObject;
				}else{
					projectile = AzuObjectPool.instance.SpawnPooledObj(projectilePoolIndex, playerObj.transform.position + (playerObj.transform.forward * projSpawnForwardAmt) + playerObj.transform.up * 0.8f, playerObj.transform.rotation) as GameObject;
				}
				
				Physics.IgnoreCollision(projectile.GetComponent<Collider>(), FPSWalkerComponent.capsule,true);
				
				if(projectile.transform.GetComponent<ArrowObject>()){
					ArrowRef = projectile.transform.GetComponent<ArrowObject>();
					ArrowRef.InitializeProjectile();
					ArrowRef.damageAddAmt = ArrowRef.damageAdd * fireHoldMult;//make arrow cause more damage if fire key was held longer
					//Calculate velFactor of ArrowObject.cs to scale forward raycast (hit detection) based on arrow release velocity
					//doing this, allows arrows to not miss collisions as much because raycast distance is lengthened
					//and there is more of a chance that the collision will hit between Fixed Updates.
					//Too large of a raycast distance at low velocities makes arrow "jump" forward to impact point.
					ArrowRef.velFactor = (projectileForce * fireHoldMult) / projectileForce;
					ArrowRef.objectPoolIndex = projectilePoolIndex;
					//delay arrow visibility if camera near clip plane is close 
					if((Camera.main.nearClipPlane < 0.1f || CameraControlComponent.thirdPersonActive) && !MouseLookComponent.dzAiming){
						ArrowRef.visibleDelay = 0.2f;
					}else{
						ArrowRef.visibleDelay = 0.0f;
					}
				}
				
				projBody = projectile.GetComponent<Rigidbody>();
				projBody.velocity = Vector3.zero;
				projBody.angularVelocity = Vector3.zero;

				//make projectile force based on how long fire button was held, if pullProjectileForce is true
				if(pullProjectileForce){
					projectileForceAmt = projectileForce * fireHoldMult + minimumProjForce;
				}else{
					projectileForceAmt = projectileForce;
				}
				
				//apply velocity and torque to projectile object's rigidbody
				if(!CameraControlComponent.thirdPersonActive){
					projBody.AddForce(direction * projectileForceAmt, ForceMode.Impulse);
				}else{
					projBody.AddForce(lookDirection * projectileForceAmt, ForceMode.Impulse);
				}
				
				if(projRotUp > 0.0f || projRotSide > 0.0f){
					projBody.maxAngularVelocity = 10;//spin faster than default
					projBody.AddRelativeTorque(Vector3.up * Random.Range(projRotSide, projRotSide * 1.5f));
					projBody.AddRelativeTorque(Vector3.right * Random.Range (projRotUp, projRotUp * 1.5f));
				}
				
				//set fuse for grenade, based on how long fire button was held
				if(projectile.GetComponent<GrenadeObject>()){
					projectile.GetComponent<GrenadeObject>().fuseTimeAmt = fuseTime * (1.0f - fireHoldMult) + 0.2f;
				}
				
				fireHoldTimer = 0.0f;
				fireHoldMult = 0.0f;
				
			}else{
				
				if(meleeSwingDelay == 0 && !meleeActive){
					if(!CameraControlComponent.thirdPersonActive){
						//check for ranged weapon hit
						if((MouseLookComponent.dzAiming && Physics.Raycast(mainCamTransform.position, weaponLookDirection + direction, out hit, range, bulletMask))
						   ||(!MouseLookComponent.dzAiming && Physics.Raycast(mainCamTransform.position, direction, out hit, range, bulletMask))){
							HitObject(hit, weaponLookDirection + direction);
							
							//Perform a second ray cast from impact point if bullet collided with water, so bullets will hit underwater targets. 
							if(hit.transform.tag == "Water" && Physics.Raycast(hit.point, direction, out hit, range, liquidMask)){
								HitObject(hit, weaponLookDirection + direction, true);	
							}
						}
					}else{
					
						if(Physics.Raycast(shotOrigin, lookDirection, out hit, range, bulletMask)){
							HitObject(hit, lookDirection);
							
							//Perform a second ray cast from impact point if bullet collided with water, so bullets will hit underwater targets. 
							if(hit.transform.tag == "Water" && Physics.Raycast(hit.point, direction, out hit, range, liquidMask)){
								HitObject(hit, lookDirection, true);	
							}
						}
					}
					
					if(useTracers){
						if(!CameraControlComponent.thirdPersonActive){
							//Emit tracers for fired bullet
							Vector3 tracerOrigin = mainCamTransform.position + (mainCamTransform.right * tracerOffset.x) + (mainCamTransform.up * tracerOffset.y) + (mainCamTransform.forward * smokeForward) + (weaponLookDirection * tracerOffset.z);
							WeaponEffectsComponent.BulletTracers(weaponLookDirection + direction, tracerOrigin, tracerDist, tracerSwimDist);
						}else{
							WeaponEffectsComponent.BulletTracers(lookDirection, VisibleBodyComponent.weaponMesh.transform.position + VisibleBodyComponent.weaponMesh.transform.up * 0.2f, tracerDistTp);
						}
					}
					
				}else{
					//check for melee weapon hit
					if(meleeActive){
						rangeAmt = offhandMeleeRange;
					}else{
						rangeAmt = range;
					}
					if(!CameraControlComponent.thirdPersonActive){
						//use SphereCast instead of Raycast to simulate swinging arc where melee weapon may contact objects
						if(Physics.SphereCast(mainCamTransform.position, capsule.radius * 0.3f, weaponLookDirection + direction, out hit, rangeAmt + (rangeAmt * FPSWalkerComponent.playerHeightMod * 0.5f), bulletMask)){
							HitObject(hit, direction);
							//if sphereCast hits nothing, try a rayCast to hit trigger colliders like the surface of water
						}else if(Physics.Raycast(mainCamTransform.position, weaponLookDirection + direction, out hit, rangeAmt, bulletMask)){
							HitObject(hit, direction);
						}	
					}else{
						//use SphereCast instead of Raycast to simulate swinging arc where melee weapon may contact objects
						if(Physics.SphereCast(mainCamTransform.position, capsule.radius * 0.3f, mainCamTransform.forward, out hit, rangeAmt + CameraControlComponent.zoomDistance + (rangeAmt * FPSWalkerComponent.playerHeightMod * 0.5f), bulletMask)){
							HitObject(hit, mainCamTransform.forward);
							//if sphereCast hits nothing, try a rayCast to hit trigger colliders like the surface of water
						}else if(Physics.Raycast(mainCamTransform.position, mainCamTransform.forward, out hit, rangeAmt + CameraControlComponent.zoomDistance, bulletMask)){
							HitObject(hit, mainCamTransform.forward);
						}
					}
					
					if(meleeActive){
						break;//don't fire multiple offhand melee shots if projectile count is greater than 1
					}
				}
			}
		}
		
	}
	
	//weapon or projectile damage and effects for collider that is hit
	void HitObject ( RaycastHit hit, Vector3 directionArg, bool isSecondCast = false ){
		// Apply a force to the rigidbody we hit
		if (hit.rigidbody && hit.rigidbody.useGravity){
			hit.rigidbody.AddForceAtPosition(force * directionArg / (Time.fixedDeltaTime * 100.0f), hit.point);//scale the force with the Fixed Timestep setting
		}
		
		if(meleeActive){
			damageAmt = offhandMeleeDamage;
		}else{
			damageAmt = damage;
		}
		
		//call the ApplyDamage() function in the script of the object hit
		switch(hit.collider.gameObject.layer){
		case 0://hit object
			if(hit.collider.gameObject.GetComponent<AppleFall>()){
				hit.collider.gameObject.GetComponent<AppleFall>().ApplyDamage(damageAmt);
				FPSPlayerComponent.UpdateHitTime();//used for hitmarker
			}
			if(hit.collider.gameObject.GetComponent<BreakableObject>()){
				hit.collider.gameObject.GetComponent<BreakableObject>().ApplyDamage(damageAmt);
				FPSPlayerComponent.UpdateHitTime();//used for hitmarker
			}else if(hit.collider.gameObject.GetComponent<ExplosiveObject>()){
				hit.collider.gameObject.GetComponent<ExplosiveObject>().ApplyDamage(damageAmt);
				FPSPlayerComponent.UpdateHitTime();//used for hitmarker
			}else if(hit.collider.gameObject.GetComponent<MineExplosion>()){
				hit.collider.gameObject.GetComponent<MineExplosion>().ApplyDamage(damageAmt);
				FPSPlayerComponent.UpdateHitTime();//used for hitmarker
			}
			break;
		case 1://hit object is an object with transparent effects like a window
			if(hit.collider.gameObject.GetComponent<BreakableObject>()){
				hit.collider.gameObject.GetComponent<BreakableObject>().ApplyDamage(damageAmt);
				FPSPlayerComponent.UpdateHitTime();//used for hitmarker
			}	
			break;
		case 13://hit object is an NPC
			if(hit.collider.gameObject.GetComponent<CharacterDamage>() && hit.collider.gameObject.GetComponent<AI>().enabled){
				hit.collider.gameObject.GetComponent<CharacterDamage>().ApplyDamage(damageAmt, directionArg, mainCamTransform.position, myTransform, true, false);
				FPSPlayerComponent.UpdateHitTime();//used for hitmarker
			}
			if(hit.collider.gameObject.GetComponent<LocationDamage>() && hit.collider.gameObject.GetComponent<LocationDamage>().AIComponent.enabled){
				hit.collider.gameObject.GetComponent<LocationDamage>().ApplyDamage(damageAmt, directionArg, mainCamTransform.position, myTransform, true, false);
				FPSPlayerComponent.UpdateHitTime();//used for hitmarker
			}
			break;
		default:
			break;	
		}

		if(!isSecondCast){
			//emit impact particle effects and leave bullet marks by calling the ImpactEfects() and BulletMarks() function of WeaponEffects.cs 
			if(hit.collider.gameObject.tag == "Flesh"){
				hitCount ++;
				if(hitCount < 2){//only draw one flesh impact effect if this is a shotgun for optimization
					if(meleeSwingDelay == 0 && !meleeActive){
						WeaponEffectsComponent.ImpactEffects(hit.collider, hit.point, false, false, hit.normal);//draw flesh impact effects where the weapon hit NPC
					}else{
						WeaponEffectsComponent.ImpactEffects(hit.collider, hit.point, false, true, hit.normal);//draw flesh impact effects where the weapon hit NPC
					}
				}
			}else{
				if(meleeSwingDelay == 0 && !meleeActive){
					WeaponEffectsComponent.ImpactEffects(hit.collider, hit.point, false, false, hit.normal);//draw impact effects where the weapon hit NPC
					if(!hit.collider.isTrigger){
						WeaponEffectsComponent.BulletMarks(hit, false);//draw a bullet mark where the weapon hit
					}
				}else{
					WeaponEffectsComponent.ImpactEffects(hit.collider, hit.point, false, true, hit.normal);//draw impact effects where the weapon hit NPC
					if(!hit.collider.isTrigger){
						WeaponEffectsComponent.BulletMarks(hit, true);//draw a melee mark where the weapon hit
					}
				}
				
			}
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Weapon Muzzle Flash
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void MuzzFlash (){
		
		//enable muzzle flash
		if (muzzleFlash){
			
			if(!FPSWalkerComponent.holdingBreath){
				if(muzzleLightObj){
					muzzleLightComponent.enabled = true;
					muzzleLightComponent.intensity = 8.0f;
				}
				
				//emit smoke particle effect from muzzle
				if(useMuzzleSmoke && muzzleSmokeParticles){
					muzzleSmokeColor.a = muzzleSmokeAlpha;
					muzzleSmokeComponent.material.SetColor("_TintColor", muzzleSmokeColor);
					if(!FPSPlayerComponent.zoomed){
						muzzleSmokeParticles.transform.position = mainCamTransform.position + (mainCamTransform.right * muzzleSmokeOffset.x) + (mainCamTransform.up * muzzleSmokeOffset.y) + (mainCamTransform.forward * smokeForward) + (weaponLookDirection * muzzleSmokeOffset.z);//muzzleFlash.position;
					}else{
						muzzleSmokeParticles.transform.position = mainCamTransform.position + (mainCamTransform.forward * smokeForward) + (weaponLookDirection * muzzleSmokeOffset.z);//muzzleFlash.position;
					}
					muzzleSmokeParticles.Emit();
				}
			}
			
			//set muzzle flash color
			if(!FPSWalkerComponent.holdingBreath){
				muzzleFlashColor.r = 1.0f;
				muzzleFlashColor.g = 1.0f;
				muzzleFlashColor.b = 1.0f;
			}else{
				//set muzzle flash to underwaterFogColor from WaterZone.cs
				muzzleFlashColor.r = PlayerWeaponsComponent.waterMuzzleFlashColor.r;
				muzzleFlashColor.g = PlayerWeaponsComponent.waterMuzzleFlashColor.g;
				muzzleFlashColor.b = PlayerWeaponsComponent.waterMuzzleFlashColor.b;
			}
			
			muzzleFlashColor.a = Random.Range(0.4f, 0.5f);
			muzzleRendererComponent.material.SetColor("_TintColor", muzzleFlashColor);
			//add random rotation to muzzle flash
			muzzleFlash.localRotation = Quaternion.AngleAxis(Random.value * 360, Vector3.forward);
			muzzleRendererComponent.enabled = true;
			
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Shell Ejection
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	IEnumerator SpawnShell (){
		
		if(shellEjectDelay > 0.0f){//delay shell ejection for shotguns and bolt action rifles by shellEjectDelay amount
			yield return new WaitForSeconds(shellEjectDelay);
		}
		
		if(!CameraControlComponent.thirdPersonActive){
			if(shellEjectPositionZoom && FPSPlayerComponent.zoomed && !WeaponPivotComponent.deadzoneZooming){
				shellEjectPos = shellEjectPositionZoom;
			}else{
				shellEjectPos = shellEjectPosition;
			}
		}else{
			shellEjectPos = VisibleBodyComponent.weaponMesh.transform;
		}
		
		//get rigidbody shell object from object pool and calculate position and rotation (invisible w/ no mesh)
		shell = AzuObjectPool.instance.SpawnPooledObj(shellRBPoolIndex,
													  shellEjectPos.position,
													  shellEjectPos.transform.rotation) as GameObject;
													  
		//Initialize object references for instantiated shell object
		ShellEjection ShellEjectionComponent = shell.GetComponent<ShellEjection>();
		ShellEjectionComponent.playerObj = playerObj;
		if(MouseLookComponent.dzAiming){
			ShellEjectionComponent.dzAiming = true;
		}else{
			ShellEjectionComponent.dzAiming = false;
		}
		ShellEjectionComponent.PlayerRigidbodyComponent = playerObj.GetComponent<Rigidbody>();
		ShellEjectionComponent.WeaponBehaviorComponent = this;
		ShellEjectionComponent.RigidbodyComponent = ShellEjectionComponent.gameObject.GetComponent<Rigidbody>();
		ShellEjectionComponent.FPSPlayerComponent = FPSPlayerComponent;
		ShellEjectionComponent.gunObj = transform.gameObject;
		
		shell.transform.parent = shellEjectPosition.transform.parent;
		
		ShellEjectionComponent.RBPoolIndex = shellRBPoolIndex;
		
		if(!CameraControlComponent.thirdPersonActive){
			shell.transform.localScale = shellScale;//scale size of RB shell object by shellScale amount
		}else{
			if(shellScale.x > 3){
				shell.transform.localScale = shellScale * 0.75f;//scale size of RB shell object by shellScale amount
			}else{
				shell.transform.localScale = shellScale * 2.0f;//scale size of RB shell object by shellScale amount
			}
		}
		
		//direction of ejected shell casing, adding random values to direction for realism
		shellEjectDirection = new Vector3((shellSide * 0.7f) + (shellSide * 0.4f * Random.value), 
		                                  (shellUp * 0.6f) + (shellUp * 0.5f * Random.value),
		                                  (shellForward * 0.4f) + (shellForward * 0.2f * Random.value));
		                                  
		if(!CameraControlComponent.thirdPersonActive || (CameraControlComponent.thirdPersonActive && shellScale.x > 3)){
			//Apply velocity to shell
			ShellEjectionComponent.RigidbodyComponent.AddForce((transform.TransformDirection(shellEjectDirection) * shellForce), ForceMode.Impulse);
		}else{
			//Apply velocity to shell
			ShellEjectionComponent.RigidbodyComponent.AddForce((transform.TransformDirection(shellEjectDirection) * shellForce * 1.5f), ForceMode.Impulse);
		}
		
		ShellEjectionComponent.InitializeShell();
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Reload Weapon
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public bool CheckForReload(){
		if(!IronsightsComponent.reloading
		   && ammo > 0 
		   && doReload
		   && bulletsLeft < bulletsPerClip
		   && Time.time - shootStartTime > fireRate
		   && !InputComponent.fireHold){
			sprintReloadState = true;
			StartCoroutine("Reload");
			return true;
		}else{
			return false;
		}
	}
	
	IEnumerator Reload(){
	
		if(meleeActive){
			yield break;
		}
		
		if(Time.timeSinceLevelLoad > 2){//prevent any reloading behavior at level start 
			
			if((!(FPSWalkerComponent.sprintActive && (Mathf.Abs(horizontal) > 0.75f || Mathf.Abs(vertical) > 0.75f))//allow reload while walking
			//allow auto reload when sprint button is held, even if stationary
			|| FPSWalkerComponent.cancelSprint
			|| FPSWalkerComponent.sprintReload)
			&& !FPSWalkerComponent.hideWeapon){//dont allow reloading if gun is lowered while climbing, swimming, or holding object
				
				if(ammo > 0){//if player has no ammo in their inventory for this weapon, do not proceed with reload
					
					//cancel zooming when reloading
					FPSPlayerComponent.zoomed = false;
					
					//if loading by magazine, start these reloading actions immediately and wait for reloadTime before adding ammo and completing reload
					if(bulletsToReload == bulletsPerClip){
						//play reload sound once at start of reload
						otherfx.volume = 1.0f;
						otherfx.clip = reloadSnd;
						otherfx.Play();//OneShot(otherfx.clip, 1.0f / otherfx.volume);//play magazine reload sound effect
						
						//determine which weapon is selected and play camera view reloading animation
						if(!PistolSprintAnim){
							if(weaponNumber == 5 || weaponNumber == 6 || weaponNumber == 7){
								//rewind animation if already playing to allow overlapping playback
								CameraAnimationComponent.Rewind("CameraReloadAK47");
								//set camera reload animation speed to positive value to play forward because
								//it might have been reversed if we canceled a reload by sprinting
								CameraAnimationComponent["CameraReloadAK47"].speed = 1.0f;
								CameraAnimationComponent.CrossFade("CameraReloadAK47", 0.35f,PlayMode.StopSameLayer);
							}else{
								CameraAnimationComponent.Rewind("CameraReloadMP5");
								CameraAnimationComponent["CameraReloadMP5"].speed = 1.0f;
								CameraAnimationComponent.CrossFade("CameraReloadMP5", 0.35f,PlayMode.StopSameLayer);
							}
						}else{
							CameraAnimationComponent.Rewind("CameraReloadPistol");
							CameraAnimationComponent["CameraReloadPistol"].speed = 1.0f;
							CameraAnimationComponent.CrossFade("CameraReloadPistol", 0.35f,PlayMode.StopSameLayer);
						}
						
						//Rewind reloading animation, set speed, and play animation. This can cause sudden/jerky start of reload anim
						//if sprinting very briefly, but is necessary to keep reload animation and sound synchronized.
						WeaponAnimationComponent.Rewind("Reload");
						WeaponAnimationComponent["Reload"].speed = reloadAnimSpeed;
						WeaponAnimationComponent.CrossFade("Reload", 0.35f,PlayMode.StopSameLayer);//play reloading animation
					}
					
					//set reloading var in ironsights script to true
					IronsightsComponent.reloading = true;
					reloadStartTime = Time.time;
					
					burstState = false;
					burstShotsFired = 0;
					
					//do not wait for reloadTime if this is not a magazine reload and this is the first bullet/shell to be loaded,
					//otherwise, adding of ammo and finishing reload will wait for reloadTime while animation and sound plays
					if((bulletsToReload != bulletsPerClip && bulletsReloaded > 0) || bulletsToReload == bulletsPerClip){
						// Wait for reload time first, then proceed
						yield return new WaitForSeconds(reloadTime);
					}
					
					//determine how many bullets need to be reloaded
					bulletsNeeded = bulletsPerClip - bulletsLeft;	
					
					//if loading a magazine, update bullet amount and set reloading var to false after reloadTime has elapsed
					if(bulletsToReload == bulletsPerClip){
						
						//set reloading var in ironsights script to false after reloadTime has elapsed
						IronsightsComponent.reloading = false;
						
						//we have ammo left to reload
						if(ammo >= bulletsNeeded){
							ammo -= bulletsNeeded;//subtract bullets needed from total ammo
							bulletsLeft = bulletsPerClip;//add bullets to magazine 
						}else{
							bulletsLeft += ammo;//if ammo left is less than needed to reload, so just load all remaining bullets
							ammo = 0;//out of ammo for this weapon now
						}
						
					}else{//If we are reloading weapon one bullet at a time (or bulletsToReload is less than the magazine amount) run code below
						//determine if bulletsToReload var needs to be changed based on how many bullets need to be loaded						
						if(bulletsNeeded >= bulletsToReload){//bullets needed are more or equal to bulletsToReload amount, so add bulletsToReload amount
							if(ammo >= bulletsToReload){
								bulletsLeft += bulletsToReload;//add bulletsToReload amount to magazine
								ammo -= bulletsToReload;//subtract bullets needed from total ammo
								bulletsReloaded += bulletsToReload;//increment bulletsReloaded so we can track our progress in this non-magazine reload 
							}else{
								bulletsLeft += ammo;//if ammo left is less than needed to reload, just load all remaining bullets
								ammo = 0;//out of ammo for this weapon now
							}
						}else{//if bullets needed are less than bulletsToReload amount, just add the ammo that is needed
							if(ammo >= bulletsNeeded){
								bulletsLeft += bulletsNeeded;	
								ammo -= bulletsNeeded;//subtract bullets needed from total ammo
								bulletsReloaded += bulletsToReload;//increment bulletsReloaded so we can track our progress in this non-magazine reload 
							}else{
								bulletsLeft += ammo;//if ammo left is less than needed to reload, just load all remaining bullets
								ammo = 0;//out of ammo for this weapon now	
							}
						}
						
						if(bulletsNeeded > 0){//if bullets still need to be reloaded and we are not loading a magazine
							StartCoroutine("Reload");//start reload coroutine again to load number of bullets defined by bulletsToReload amount			
						}else{
							IronsightsComponent.reloading = false;//if magazine is full, set reloading var in ironsights script to false
							bulletsReloaded = 0;
							yield break;//also stop coroutine here to prevent sound from playing below
						}
						
						if(bulletsNeeded <= bulletsToReload || ammo <= 0){//if reloading last round, play normal reloading sound and also chambering effect
							otherfx.clip = reloadLastSnd;//set otherfx audio clip to reloadLastSnd
							WeaponAnimationComponent["Reload"].speed = 1.0f;
							//track time we started reloading last bullet to allow for additional time to chamber round before allowing weapon firing		
							reloadLastStartTime = Time.time;
							IronsightsComponent.reloading = false;
						}else{
							otherfx.clip = reloadSnd;//set otherfx audio clip to reloadSnd
							WeaponAnimationComponent["Reload"].speed = shellRldAnimSpeed;
							
						}
						
						//play reloading sound effect	
						otherfx.volume = 1.0f;
						otherfx.pitch = Random.Range(0.95f * Time.timeScale, 1 * Time.timeScale);
						otherfx.PlayOneShot(otherfx.clip, 1.0f / otherfx.volume);
						//play reloading animation
						WeaponAnimationComponent.Rewind("Reload");
						WeaponAnimationComponent.CrossFade("Reload", 0.35f,PlayMode.StopSameLayer);
						
						//play camera reload animation 
						CameraAnimationComponent.Rewind("CameraReloadSingle");
						//set camera reload animation speed to positive value to play forward because
						//it might have been reversed if we canceled a reload by sprinting
						CameraAnimationComponent["CameraReloadSingle"].speed = 1.0f;
						CameraAnimationComponent.CrossFade("CameraReloadSingle", 0.35f,PlayMode.StopSameLayer);
						
						reloadEndTime = Time.time;//track time that we finished reload to determine if this reload can be interrupted by fire button
						
					}	
				}
			}
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Calculate angle of bullet fire from muzzle
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	private Vector3 SprayDirection (){
		//increase weapon accuracy if player is crouched
		float crouchAccuracy = 1.0f;
		float spreadPerShot = 1.2f;
		
		if(FPSWalkerComponent.crouched){
			crouchAccuracy = 0.75f;	
		}else{
			crouchAccuracy = 1.0f;	
		}
		//make firing more accurate when sights are raised and/or in semi auto
		if(FPSPlayerComponent.zoomed && meleeSwingDelay == 0){
			if(fireModeSelectable && semiAuto){
				shotSpreadAmt = shotSpread / 5 * crouchAccuracy;
			}else{
				shotSpreadAmt = shotSpread / 3 * crouchAccuracy;
			}
		}else{
			if(fireModeSelectable && semiAuto){
				shotSpreadAmt = shotSpread / 2 * crouchAccuracy;
			}else{
				shotSpreadAmt = shotSpread * crouchAccuracy;
			}
		}
		
		//if using sustained fire recoil, increase aim angles exponentially after shotsBeforeRecoil have been fired
		if(useRecoilIncrease){
			if(bulletsJustFired > shotsBeforeRecoil){
				spreadPerShot = Mathf.Pow(bulletsJustFired - (shotsBeforeRecoil - 1), spreadPerShot / aimDirRecoilIncrease); 
			}else{
				spreadPerShot = 1.2f;	
			}
		}else{
			spreadPerShot = 1.0f;	
		}
		
		//apply accuracy spread amount to weapon facing angle
		float vx = (1 - 2 * Random.value) * shotSpreadAmt * spreadPerShot;
		float vy = (1 - 2 * Random.value) * shotSpreadAmt * spreadPerShot;
		float vz = 1.0f;
		return myTransform.TransformDirection(new Vector3(vx,vy,vz));
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Camera Recoil Kick
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void WeaponKick (){
	
		if(CameraControlComponent.thirdPersonActive){
			return;
		}
		
		float randXkick;
		float randYkick;
		float spreadPerShot = 1.2f;
		//make recoil less when zoomed or prone and more when zoomed out
		if((FPSPlayerComponent.zoomed || FPSWalkerComponent.prone) && meleeSwingDelay == 0.0f){
			kickUpAmt = kickUp;//set kick amounts to those set in the editor
			kickSideAmt = kickSide;
			kickRollAmt = kickRoll;
		}else{
			kickRollAmt = kickRoll * 0.5f;
			if(!FPSWalkerComponent.crouched
			   //normal view kick when crouching and not moving
			   ||(FPSWalkerComponent.crouched && Mathf.Abs(horizontal) == 0.0f && Mathf.Abs(vertical) == 0.0f)){
				kickUpAmt = kickUp * 1.75f;
				kickSideAmt = kickSide * 1.75f;
			}else{
				//increase view kick to offset increased bobbing 
				//amounts when crouching and moving 
				kickUpAmt = kickUp * 2.75f;
				kickSideAmt = kickSide * 2.75f;
			}
		}
		
		randXkick = Random.Range(-kickSideAmt * 2.0f, kickSideAmt * 2.0f);
		randYkick = Random.Range(kickUpAmt * 1.5f, kickUpAmt * 2.0f);
		
		if(!MouseLookComponent.dzAiming){
			if(Random.value <= 0.5f){
				randZkick = Random.Range(-kickRollAmt, -kickRollAmt * 0.5f);
			}else{
				randZkick = Random.Range(kickRollAmt * 0.5f, kickRollAmt);
			}
			randZkick += randZkick * 0.5f;
			Mathf.Clamp(randZkick, -kickRollAmt, kickRollAmt);
		}
		
		//Set rotation quaternion to random kick values
		kickRotation = Quaternion.Euler(mainCamTransform.localRotation.eulerAngles - new Vector3(randYkick, randXkick, 0.0f));
		
		//smooth current camera angles to recoil kick up angles using Slerp
		mainCamTransform.localRotation = Quaternion.Slerp(mainCamTransform.localRotation, kickRotation, 0.1f);
		
		if(useRecoilIncrease){
			
			if(bulletsJustFired > shotsBeforeRecoil && !FPSWalkerComponent.prone){
				//increase spreadPerShot exponentially for more realistic feel
				spreadPerShot = Mathf.Pow(bulletsJustFired - (shotsBeforeRecoil - 1), spreadPerShot / viewKickIncrease); 
			}else{
				if(!FPSWalkerComponent.prone){
					spreadPerShot = 1.2f;	
				}else{//less recoil when prone
					spreadPerShot = 0.5f;
				}
			}
			
			if(useViewClimb && !MouseLookComponent.dzAiming){//apply non-recoverable view climb to mouse input with sustained fire recoil
				if(viewClimbUp > 0.0f){
					MouseLookComponent.recoilY += ((randYkick / 8.0f * viewClimbUp) * (spreadPerShot / 6.0f ));
				}
				if(viewClimbSide > 0.0f || viewClimbRight > 0.0f){
					MouseLookComponent.recoilX += ((randXkick / 4.0f * viewClimbSide) + viewClimbRight) * (spreadPerShot / 2.0f);
				}
			}	
		}else{	
			
			if(useViewClimb && !MouseLookComponent.dzAiming){//apply non-recoverable view climb to mouse input without sustained fire recoil
				if(viewClimbUp > 0.0f){
					MouseLookComponent.recoilY += randYkick / 8.0f * viewClimbUp;
				}
				if(viewClimbSide > 0.0f || viewClimbRight > 0.0f){
					MouseLookComponent.recoilX += (randXkick / 4.0f * viewClimbSide) + viewClimbRight;
				}
			}	
		}
	}
	
	//cancel weapon pull after weapon switching if fire button was held when switching
	private void CancelWeaponPull () {
		releaseAnimState = false;
		fireOnReleaseState = false;
		pullAnimState = false;
		fireHoldTimer = 0.0f;
		fireHoldMult = 0.0f;
	}
	
}