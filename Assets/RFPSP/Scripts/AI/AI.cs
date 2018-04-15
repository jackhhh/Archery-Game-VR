//AI.cs by Azuline Studios© All Rights Reserved
//Allows NPC to track and attack targets and patrol waypoints.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI : MonoBehaviour {
	[HideInInspector]
	public bool spawned = false;
	[HideInInspector]
	public GameObject playerObj;
	[HideInInspector]
	public Transform playerTransform;
	[HideInInspector]
	public GameObject NPCMgrObj;
	[HideInInspector]
	public GameObject weaponObj;//currently equipped weapon object of player
	[HideInInspector]
	public FPSRigidBodyWalker FPSWalker;
	[HideInInspector]
	public NPCAttack NPCAttackComponent;
	[HideInInspector]
	public PlayerWeapons PlayerWeaponsComponent;
	private WeaponBehavior WeaponBehaviorComponent;
	[HideInInspector]
	public CharacterDamage TargetCharacterDamageComponent;
	[HideInInspector]
	public CharacterDamage CharacterDamageComponent;
	[HideInInspector]
	public NPCSpawner NPCSpawnerComponent;
	[HideInInspector]
	public NPCRegistry NPCRegistryComponent;
	[HideInInspector]
	public AI TargetAIComponent;
	[HideInInspector]
	public UnityEngine.AI.NavMeshAgent agent;
	[HideInInspector]
	public Collider[] colliders;
	private bool collisionState;
	[HideInInspector]
	public Animation AnimationComponent;
	[HideInInspector]
	public Animator AnimatorComponent;
	private float animSpeed;
	[Tooltip("If true, AI.cs will use Mecanim animations for this NPC.")]
	public bool useMecanim;
	[HideInInspector]
	public bool recycleNpcObjOnDeath;
	[Tooltip("The object with the Animation/Animator component which will be accessed by AI.cs to play NPC animations. If none, this root object will be checked for the Animator/Animations component.")]
	public Transform objectWithAnims;
	[Tooltip("Chance between 0 and 1 that NPC will spawn. Used to randomize NPC locations and surprise the player.")]
	[Range(0.0f, 1.0f)]
	public float randomSpawnChance = 1.0f;
	
	//NPC movement speeds
	[Tooltip("Running speed of the NPC.")]
	public float runSpeed = 6.0f;
	[Tooltip("Walking speed of the NPC.")]
	public float walkSpeed = 1.0f;
	[Tooltip("Speed of running animation.")]
	public float walkAnimSpeed = 1.0f;
	[Tooltip("Speed of walking animation.")]
	public float runAnimSpeed = 1.0f;
	private float speedAmt = 1.0f;
	private float lastRunTime;//to prevent rapid walking, then running when following player
	
	[Tooltip("NPC yaw angle offset when standing.")]
	public float idleYaw = 0f;
	[Tooltip("NPC yaw angle offset when moving.")]
	public float movingYaw = 0f;
	private float yawAmt;

	[Tooltip("Sets the alignment of this NPC. 1 = friendly to player and hostile to factions 2 and 3, 2 = hostile to player and factions 1 and 3, 3 = hostile to player and factions 1 and 2.")]
	public int factionNum = 1;//1 = human(friendly to player), 2 = alien(hostile to player), 3 = zombie(hostile to all other factions)
	[Tooltip("If false, NPC will attack any character that attacks it, regardless of faction.")]
	public bool ignoreFriendlyFire;
	[HideInInspector]
	public bool playerAttacked;//to make friendly NPCs go hostile if attacked by player
	[HideInInspector]
	public float attackedTime;

	//waypoints and patrolling
	[HideInInspector]
	public Transform myTransform;
	private Vector3 upVec;//cache this for optimization;
	[Tooltip("True if NPC will hunt player accross map without needing to detect player first.")]
	public bool huntPlayer;
	[Tooltip("True if NPC should only follow patrol waypoints once.")]
	public bool patrolOnce;
	[Tooltip("True if NPC should walk on patrol, will run on patrol if false.")]
	public bool walkOnPatrol = true;
	private  Transform curWayPoint;
	[Tooltip("Drag the parent waypoint object with the WaypointGroup.cs script attached here. If none, NPC will stand watch instead of patrolling.")]
	public WaypointGroup waypointGroup;
	[Tooltip("The number of the waypoint in the waypoint group which should be followed first.")]
	public int firstWaypoint = 1;
	[Tooltip("True if player should stand in one place and not patrol waypoint path.")]
	public bool standWatch;
	[Tooltip("True if NPC is following player.")]
	public bool followPlayer;
	[Tooltip("True if NPC can be activated with the use button and start following the player.")]
	public bool followOnUse;
	[Tooltip("True if this NPC wants player to follow them (wont take move orders from player, only from MoveTrigger.cs).")]
	public bool leadPlayer;
	[HideInInspector]
	public bool orderedMove;
	[HideInInspector]
	public bool playerFollow;//true if this NPC wants player to follow them (wont take move orders from player, only from MoveTrigger.cs)
	private float commandedTime;
	private float talkedTime;
	private bool  countBackwards = false;
	[Tooltip("Minimum distance to destination waypoint that NPC will consider their destination as reached.")]
	public float pickNextDestDist = 2.5f;
	private Vector3 startPosition;
	private float spawnTime;

	[Tooltip("Volume of NPC's vocal sound effects.")]
	public float vocalVol = 0.7f;
	[Tooltip("Sound to play when player commands NPC to stop following.")]
	public AudioClip stayFx1;
	[Tooltip("Sound to play when player commands NPC to stop following.")]
	public AudioClip stayFx2;
	[Tooltip("Sound to play when player commands NPC to start following.")]
	public AudioClip followFx1;
	[Tooltip("Sound to play when player commands NPC to start following.")]
	public AudioClip followFx2;
	[Tooltip("Sound to play when player commands NPC to move to position.")]
	public AudioClip moveToFx1;
	[Tooltip("Sound to play when player commands NPC to move to position.")]
	public AudioClip moveToFx2;
	[Tooltip("Sound to play when NPC has been activated more than joke activate times.")]
	public AudioClip jokeFx;
	[Tooltip("Sound to play when NPC has been activated more than joke activate times.")]
	public AudioClip jokeFx2;
	[Tooltip("Number of consecutive use button presses that activates joke fx.")]
	public int jokeActivate = 33;
	private float jokePlaying = 0f;
	private int jokeCount;
	
	[Tooltip("Sound effects to play when pursuing player.")]
	public AudioClip[] tauntSnds;
	[Tooltip("True if taunt sound shouldn't be played when attacking.")]
	public bool cancelAttackTaunt;
	private float lastTauntTime;
	[Tooltip("Delay between times to check if taunt sound should be played.")]
	public float tauntDelay = 2f;
	[Tooltip("Chance that a taunt sound will play after taunt delay.")]
	[Range(0.0f, 1.0f)]
	public float tauntChance = 0.5f;
	[Tooltip("Volume of taunt sound effects.")]
	public float tauntVol = 0.7f;
	[Tooltip("Sound effects to play when NPC discovers player.")]
	public AudioClip[] alertSnds;
	[Tooltip("Volume of alert sound effects.")]
	public float alertVol = 0.7f;
	private bool alertTaunt;
	
	
	[HideInInspector]
	public AudioSource vocalFx;
	private AudioSource footstepsFx;
	[Tooltip("Sound effects to play for NPC footsteps.")]
	public AudioClip[] footSteps;
	[Tooltip("Volume of footstep sound effects.")]
	public float footStepVol = 0.5f;
	[Tooltip("Time between footstep sound effects when walking (sync with anim).")]
	public float walkStepTime = 1.0f;
	[Tooltip("Time between footstep sound effects when running (sync with anim).")]
	public float runStepTime = 1.0f;
	private float stepInterval;
	private float stepTime;

	//targeting and attacking
	[Tooltip("Minimum range to target to start attack.")]
	public float shootRange = 15.0f;
	[Tooltip("Range that NPC will start chasing target until they are within shoot range.")]
	public float attackRange = 30.0f;
	[Tooltip("Range that NPC will hear player attacks.")]
	public float listenRange = 30.0f;
	[Tooltip("Time between shots (longer for burst weapons).")]
	public float shotDuration = 0.0f;
	[Tooltip("Speed of attack animation.")]
	public float shootAnimSpeed = 1.0f;
	[HideInInspector]
	public float attackRangeAmt = 30.0f;//increased by character damage script if NPC is damaged by player
	[Tooltip("Percentage to reduce enemy search range if player is crouching.")]
	public float sneakRangeMod = 0.4f;
	private float shootAngle = 3.0f;
	[Tooltip("Time before atack starts, to allow weapon to be raised before firing.")]
	public float delayShootTime = 0.35f;
	[Tooltip("Random delay between NPC attacks.")]
	public float randShotDelay = 0.75f;
	[Tooltip("Height of rayCast origin which detects targets (can be raised if NPC origin is at their feet).")]
	public float eyeHeight = 0.4f;
	[Tooltip("Draws spheres in editor for position and eye height.")]
	public bool drawDebugGizmos;
//	[HideInInspector]
	public Transform target = null;
//	[HideInInspector]
	public bool targetVisible;
	private float lastVisibleTime;
	private Vector3 targetPos;
	[HideInInspector]
	public float targetRadius;
	[HideInInspector]
	public float attackTime = -16.0f;//time last attacked
	private bool attackFinished = true;
	private bool turning;//true if turning to face target
	[HideInInspector]
	public bool cancelRotate;
	[HideInInspector]
	public bool followed;//true when npc is moving to destination defined by MoveTrigger.cs

	private float targetDistance;
	private Vector3 targetDirection;
	private RaycastHit[] hits;
	private bool sightBlocked;//true if sight to target is blocked
	[HideInInspector]
	public bool playerIsBehind;//true if player is behind NPC
	[HideInInspector]
	public float targetEyeHeight;
	private bool pursueTarget;
	[HideInInspector]
	public Vector3 lastVisibleTargetPosition;
	[HideInInspector]
	public float timeout = 0.0f;//to allow NPC to resume initial behavior after searching for target
	[HideInInspector]
	public bool heardPlayer = false;
	[HideInInspector]
	public bool heardTarget = false;
	[HideInInspector]
	public bool damaged;//true if attacked
	private bool damagedState;
	[HideInInspector]
	public float lastDamageTime;

	[HideInInspector]
	public LayerMask searchMask = 0;//only layers to include in target search (for efficiency)
	
	[HideInInspector]
	public RaycastHit attackHit;
	
	//public int navTest;

	void Start(){
	
		NPCMgrObj = GameObject.Find("NPC Manager");
		NPCRegistryComponent = NPCMgrObj.GetComponent<NPCRegistry>();
		NPCRegistryComponent.Npcs.Add(myTransform.gameObject.GetComponent<AI>());//register this active NPC with the NPCRegistry
		
		Mathf.Clamp01(randomSpawnChance);

		if(Random.value > randomSpawnChance){
			Destroy(myTransform.gameObject);
		}
	}
	
	void OnEnable(){
		
		myTransform = transform;
		upVec = Vector3.up;;
		startPosition = myTransform.position;
		timeout = 0.0f;
		attackedTime = -16f;//set to negative value to prevent NPCs from having larger search radius briefly after spawning
		if(jokeFx){
			jokePlaying = jokeFx.length * -2f;
		}
		
		collisionState = false;

		//initialize audiosource for footsteps
		footstepsFx = myTransform.gameObject.AddComponent<AudioSource>();
		footstepsFx.spatialBlend = 1.0f;
		footstepsFx.volume = footStepVol;
		footstepsFx.pitch = 1.0f;
		footstepsFx.dopplerLevel = 0.0f;
		footstepsFx.bypassEffects = true;
		footstepsFx.bypassListenerEffects = true;
		footstepsFx.bypassReverbZones = true;
		footstepsFx.maxDistance = 10.0f;
		footstepsFx.rolloffMode = AudioRolloffMode.Linear;
		footstepsFx.playOnAwake = false;
		
		vocalFx = myTransform.gameObject.AddComponent<AudioSource>();
		vocalFx.spatialBlend = 1.0f;
		vocalFx.volume = vocalVol;
		vocalFx.pitch = 1.0f;
		vocalFx.dopplerLevel = 0.0f;
		vocalFx.bypassEffects = true;
		vocalFx.bypassListenerEffects = true;
		vocalFx.bypassReverbZones = true;
		vocalFx.maxDistance = 10.0f;
		vocalFx.rolloffMode = AudioRolloffMode.Linear;
		vocalFx.playOnAwake = false;

		//set layermask to layers such as layer 10 (world collision) and 19 (interactive objects) for target detection 
		searchMask = ~(~(1 << 10) & ~(1 << 0) & ~(1 << 13) & ~(1 << 11) & ~(1 << 20));
		
		//if there is no objectWithAnims defined, use the Animation Component attached to this game object
		if(objectWithAnims == null){objectWithAnims = transform;}

		if(!useMecanim){//initialize legacy animations
			AnimationComponent = objectWithAnims.GetComponent<Animation>();
			// Set all animations to loop
			AnimationComponent.wrapMode = WrapMode.Loop;
			// Except our action animations, Dont loop those
			AnimationComponent["shoot"].wrapMode = WrapMode.Once;
			// Put idle and run in a lower layer. They will only animate if our action animations are not playing
			AnimationComponent["idle"].layer = -1;
			AnimationComponent["walk"].layer = -1;
			AnimationComponent["run"].layer = -1;
			
			AnimationComponent["walk"].speed = walkAnimSpeed;
			AnimationComponent["shoot"].speed = shootAnimSpeed;
			AnimationComponent["run"].speed = runAnimSpeed;
			
			AnimationComponent.Stop();
		}else{
			AnimatorComponent = objectWithAnims.GetComponent<Animator>();//set reference to Mecanim animator component
		}
		
		//initialize AI vars
		playerObj = Camera.main.transform.GetComponent<CameraControl>().playerObj;
		playerTransform = playerObj.transform;
		PlayerWeaponsComponent = Camera.main.transform.GetComponent<CameraControl>().weaponObj.GetComponentInChildren<PlayerWeapons>();
		FPSWalker = playerObj.GetComponent<FPSRigidBodyWalker>();
		NPCAttackComponent = GetComponent<NPCAttack>();
		CharacterDamageComponent = GetComponent<CharacterDamage>();

		//initialize navmesh agent
		agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		agent.speed = runSpeed;
		agent.acceleration = 60.0f;

		//Get all colliders for this NPC's body parts 
		colliders = GetComponentsInChildren<Collider>();
		
		attackRangeAmt = attackRange;

		if(!useMecanim){
			AnimationComponent.CrossFade("idle", 0.3f);
		}else{
			AnimatorComponent.SetBool("Idle", true);
			AnimatorComponent.SetBool("Walk", false);
			AnimatorComponent.SetBool("Run", false);
		}

		if(!spawned){//if spawned, SpawnNPC function will be called from NPCSpawner.cs. Otherwise, spawn now.		
			StopAllCoroutines();
			SpawnNPC();
		}
		
	}

	//initialize NPC behavior
	public void SpawnNPC(){


		if(agent.isOnNavMesh){
			spawnTime = Time.time;
			StartCoroutine(PlayFootSteps());
			if(objectWithAnims != myTransform){
				StartCoroutine(UpdateModelYaw());
			}
			if(!huntPlayer){
				//determine if NPC should patrol or stand watch
				if(!standWatch && waypointGroup && waypointGroup.wayPoints[firstWaypoint - 1]){
					curWayPoint = waypointGroup.wayPoints[firstWaypoint - 1];
					speedAmt = runSpeed;
					startPosition = curWayPoint.position;
					TravelToPoint(curWayPoint.position);
					StartCoroutine(Patrol());
				}else{
					TravelToPoint(startPosition);
					StartCoroutine(StandWatch());
				}
			}else{
				//hunt the player accross the map
				playerObj = Camera.main.transform.GetComponent<CameraControl>().playerObj;
				playerTransform = playerObj.transform;
				PlayerWeaponsComponent = Camera.main.transform.GetComponent<CameraControl>().weaponObj.GetComponentInChildren<PlayerWeapons>();
				FPSWalker = playerObj.GetComponent<FPSRigidBodyWalker>();
				factionNum = 2;
				target = playerTransform;
				targetEyeHeight = FPSWalker.capsule.height * 0.25f;
				lastVisibleTargetPosition = target.position + (target.up * targetEyeHeight);
				attackRange = 2048.0f;
				StartCoroutine(AttackTarget());

				speedAmt = runSpeed;
				TravelToPoint(playerObj.transform.position);
			}
		}else{
			Debug.Log("<color=red>NPC can't find Navmesh:</color> Please bake Navmesh for this scene or reposition NPC closer to navmesh.");
		}
	}

	//draw debug spheres in editor
	void OnDrawGizmos() {
		if(drawDebugGizmos){
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(transform.position, 0.2f);
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(transform.position + new Vector3(0.0f, eyeHeight, 0.0f), 0.2f);
		
			Vector3 myPos = transform.position + (transform.up * eyeHeight);
			Vector3 targetPos = lastVisibleTargetPosition;
			Vector3 testdir1 = (targetPos - myPos).normalized;
			float distance = Vector3.Distance(myPos, targetPos);
			Vector3 testpos3 = myPos + (testdir1 * distance);
			
			if (Physics.Linecast(myPos, targetPos)) {
				Gizmos.color = Color.red;
			}else{
				Gizmos.color = Color.green;
			}

			Gizmos.DrawLine (myPos, testpos3);
			Gizmos.DrawSphere(testpos3, 0.2f);

			
			if(target){
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine (myPos, target.position + (transform.up * targetEyeHeight));
			}

			Gizmos.color = Color.green;
			Gizmos.DrawSphere(transform.position + (transform.forward * 0.6f) + (transform.up * eyeHeight), 0.2f);

		}
		
	}
	
	IEnumerator StandWatch(){
		while (true) {

			if(huntPlayer){
				SpawnNPC();
				yield break;
			}
			
			//expand search radius if attacked
			if(attackedTime + 6.0f > Time.time){
				attackRangeAmt = attackRange * 6.0f;//expand enemy search radius if attacked to defend against sniping
			}else{
				attackRangeAmt = attackRange;
			}

			//allow player to push friendly NPCs out of their way
			if(playerObj.activeInHierarchy && !collisionState && FPSWalker.capsule){
				foreach(Collider col in colliders){
					Physics.IgnoreCollision(col, FPSWalker.capsule, true);
				}
				collisionState = true;
			}

			CanSeeTarget();
			if ((target && targetVisible) || heardPlayer || heardTarget){
				yield return StartCoroutine(AttackTarget());
			}else{
				if(NPCRegistryComponent){
					NPCRegistryComponent.FindClosestTarget(myTransform.gameObject, this, myTransform.position, attackRangeAmt, factionNum);
				}
			}
			if(attackTime < Time.time){
				if((!followPlayer || orderedMove) && Vector3.Distance(startPosition, myTransform.position) > pickNextDestDist){
					if(!orderedMove){
						speedAmt = walkSpeed;
					}else{
						speedAmt = runSpeed;
					}
					TravelToPoint(startPosition);//
				}else if(followPlayer && !orderedMove && Vector3.Distance(playerObj.transform.position, myTransform.position) > pickNextDestDist){
					if(followPlayer && Vector3.Distance(playerObj.transform.position, myTransform.position) > pickNextDestDist * 2f){
							speedAmt = runSpeed;
							lastRunTime = Time.time;
					}else{
						if(lastRunTime + 2.0f < Time.time){
							speedAmt = walkSpeed;
						}
					}
					TravelToPoint(playerObj.transform.position);
				}else{
					//play idle animation
					speedAmt = 0.0f;
					agent.Stop();
					SetSpeed(speedAmt);
					if(!useMecanim){
						AnimationComponent.CrossFade("idle", 0.3f);
					}else{
						if(attackFinished && attackTime < Time.time){
							AnimatorComponent.SetBool("Idle", true);
						}
						AnimatorComponent.SetBool("Walk", false);
						AnimatorComponent.SetBool("Run", false);
					}
				}
			}
			
			yield return new WaitForSeconds(0.3f);
		}
		
	}
	
	IEnumerator Patrol(){
		while (true) {
		
			if(huntPlayer){
				SpawnNPC();
				yield break;
			}
			
			if(curWayPoint && waypointGroup){//patrol if NPC has a current waypoint, otherwise stand watch
				Vector3 waypointPosition = curWayPoint.position;
				float waypointDist = Vector3.Distance(waypointPosition, myTransform.position);
				int waypointNumber = waypointGroup.wayPoints.IndexOf(curWayPoint);

				//if NPC is close to a waypoint, pick the next one
				if((patrolOnce && waypointNumber == waypointGroup.wayPoints.Count - 1)){
					if(waypointDist < pickNextDestDist){
						speedAmt = 0.0f;
						startPosition = waypointPosition;
						StartCoroutine(StandWatch());
						yield break;//cancel patrol if patrolOnce var is true
					}
				}else{	
					if(waypointDist < pickNextDestDist){
						if(waypointGroup.wayPoints.Count == 1){
							speedAmt = 0.0f;
							startPosition = waypointPosition;
							StartCoroutine(StandWatch());
							yield break;//cancel patrol if NPC has reached their only waypoint
						}
						curWayPoint = PickNextWaypoint (curWayPoint, waypointNumber);
						if(spawned && Vector3.Distance(waypointPosition, myTransform.position) < pickNextDestDist){
							walkOnPatrol = true;//make spawned NPCs run to their first waypoint, but walk on the patrol
						}
					}
				}

				//expand search radius if attacked
				if(attackedTime + 6.0f > Time.time){
					attackRangeAmt = attackRange * 6.0f;//expand enemy search radius if attacked to defend against sniping
				}else{
					attackRangeAmt = attackRange;
				}
				
				//allow player to push friendly NPCs out of their way
				if(playerObj.activeInHierarchy && !collisionState && FPSWalker.capsule){
					foreach(Collider col in colliders){
						Physics.IgnoreCollision(col, FPSWalker.capsule, true);
					}
					collisionState = true;
				}

				//determine if player is within sight of NPC
				CanSeeTarget();
				if((target && targetVisible) || heardPlayer || heardTarget){
					yield return StartCoroutine(AttackTarget());
				}else{
					if(NPCRegistryComponent){
						NPCRegistryComponent.FindClosestTarget(myTransform.gameObject, this, myTransform.position, attackRangeAmt, factionNum);
					}
					// Move towards our target
					if(attackTime < Time.time){
						if(orderedMove && !followPlayer){
							if(Vector3.Distance(startPosition, myTransform.position) > pickNextDestDist){
								speedAmt = runSpeed;
								TravelToPoint(startPosition);
							}else{
								//play idle animation
								speedAmt = 0.0f;
								agent.Stop();
								SetSpeed(speedAmt);
								if(!useMecanim){
									AnimationComponent.CrossFade("idle", 0.3f);
								}else{
									if(attackFinished && attackTime < Time.time){
										AnimatorComponent.SetBool("Idle", true);
									}
									AnimatorComponent.SetBool("Walk", false);
									AnimatorComponent.SetBool("Run", false);
								}
								StartCoroutine(StandWatch());//npc reached player-designated position, stop patrolling and wait here
								yield break;
							}
						}else if(!orderedMove && followPlayer){
							if(Vector3.Distance(playerObj.transform.position, myTransform.position) > pickNextDestDist){
								if(Vector3.Distance(playerObj.transform.position, myTransform.position) > pickNextDestDist * 2f){
									speedAmt = runSpeed;
									lastRunTime = Time.time;
								}else{
									if(lastRunTime + 2.0f < Time.time){
										speedAmt = walkSpeed;
									}
								}
								TravelToPoint(playerObj.transform.position);
							}else{
								//play idle animation
								speedAmt = 0.0f;
								agent.Stop();
								SetSpeed(speedAmt);
								if(!useMecanim){
									AnimationComponent.CrossFade("idle", 0.3f);
								}else{
									if(attackFinished && attackTime < Time.time){
										AnimatorComponent.SetBool("Idle", true);
									}
									AnimatorComponent.SetBool("Walk", false);
									AnimatorComponent.SetBool("Run", false);
								}
							}
						}else{
							//determine if NPC should walk or run on patrol
							if(walkOnPatrol){speedAmt = walkSpeed;}else{speedAmt = runSpeed;}
							TravelToPoint(waypointPosition);
						}
					}
				}
			}else{
				StartCoroutine(StandWatch());//don't patrol if we have no waypoints
				return false;
			}
			yield return new WaitForSeconds(0.3f);
		}

	}


	void CanSeeTarget(){
	
		if(spawnTime + 1f > Time.time){//add small delay before checking target visibility
			return;
		}
		
		//stop tracking target if it is deactivated
		if((TargetAIComponent && !TargetAIComponent.enabled) || (target && !target.gameObject.activeInHierarchy)){
			target = null;
			TargetAIComponent = null;
			targetVisible = false;
			heardTarget = false;
			return;
		}

		//target player
		if((factionNum != 1 || playerAttacked) && FPSWalker.capsule){

			float playerDistance = Vector3.Distance(myTransform.position + (upVec * eyeHeight), playerTransform.position + (upVec * FPSWalker.capsule.height * 0.25f));

			//listen for player attacks
			if(!heardPlayer && !huntPlayer && FPSWalker.dropTime + 2.5f < Time.time){
				if(playerDistance < listenRange && (target == playerTransform || target == FPSWalker.leanObj.transform || target == null)){
					if(PlayerWeaponsComponent && PlayerWeaponsComponent.CurrentWeaponBehaviorComponent){
						WeaponBehaviorComponent = PlayerWeaponsComponent.CurrentWeaponBehaviorComponent;
						if(WeaponBehaviorComponent.shootStartTime + 2.0f > Time.time && !WeaponBehaviorComponent.silentShots){
							if(target == FPSWalker.leanObj.transform){
								targetEyeHeight = 0.0f;
								target = FPSWalker.leanObj.transform;
								pursueTarget = true;
							}else{
								targetEyeHeight = FPSWalker.capsule.height * 0.25f;
								target = playerTransform;
							}
							timeout = Time.time + 6.0f;
							heardPlayer = true;
							return;
						}
					}
				}
			}

			if(huntPlayer){
				targetEyeHeight = FPSWalker.capsule.height * 0.25f;
				target = playerTransform;
			}

			if(playerDistance < attackRangeAmt){

				//target lean collider if player is leaning around a corner
				if(Mathf.Abs(FPSWalker.leanAmt) > 0.1f 
				&& playerDistance > 2.0f && target == playerTransform 
				&& (attackedTime + 6.0f > Time.time || heardPlayer)){//allow player to peek around corners undetected if they haven't attacked this npc
					targetEyeHeight = 0.0f;
					target = FPSWalker.leanObj.transform;
				}
				//target main player object if they are not leaning
				if((Mathf.Abs(FPSWalker.leanAmt) < 0.1f || playerDistance < 2.0f) && target == FPSWalker.leanObj.transform){
					targetEyeHeight = FPSWalker.capsule.height * 0.25f;
					target = playerTransform;
				}

			}

		}

		//calculate range and LOS to target
		if(target == playerTransform || target == FPSWalker.leanObj.transform || (TargetAIComponent && TargetAIComponent.enabled && target != null)){
			Vector3 myPos = myTransform.position + (upVec * eyeHeight);
			targetPos = target.position + (target.up * targetEyeHeight);

			targetDistance = Vector3.Distance(myPos, targetPos);
			targetDirection = (targetPos - myPos).normalized;

			if(targetDistance > attackRangeAmt){
				sightBlocked = true;
				targetVisible = false;
				return;//don't continue to check LOS if target is not within attack range
			}

			//check LOS with sphere casts and raycasts
			if(target == playerTransform){
				hits = Physics.SphereCastAll(myPos, agent.radius * 0.35f, targetDirection, targetDistance, searchMask);
			}else if(target == FPSWalker.leanObj.transform){
				hits = Physics.SphereCastAll(myPos, agent.radius * 0.1f, targetDirection, targetDistance, searchMask);
			}else{
				hits = Physics.RaycastAll (myPos, targetDirection, targetDistance, searchMask);
			}
			sightBlocked = false;

			//detect if target is behind NPC
			if(!huntPlayer 
			&& timeout < Time.time
			&& attackedTime + 6.0f < Time.time
			&& ((target == playerTransform || target == FPSWalker.leanCol) && !heardPlayer)
			&& !FPSWalker.sprintActive){
				Vector3 toTarget = (targetPos - myPos).normalized;
				if(Vector3.Dot(toTarget, transform.forward) < -0.45f){
					sightBlocked = true;
					playerIsBehind = true;
					targetVisible = false;
					return;
				}
			}
			
			playerIsBehind = false;

			//check if NPC can see their target
			for(int i = 0; i < hits.Length; i++){
				if((!hits[i].transform.IsChildOf(target)//hit is not target
				&& !hits[i].transform.IsChildOf(myTransform))//hit is not NPC's own colliders
			    || (!playerAttacked//attack player if they attacked us (friendly fire)
			    	&& (factionNum == 1 && target != playerObj && (hits[i].collider == FPSWalker.capsule//try not to shoot the player if we are a friendly NPC
                   		|| hits[i].collider == FPSWalker.leanCol)))
				){
					sightBlocked = true;
					break;
				}
				if(hits[i].transform.IsChildOf(target)){
					attackHit = hits[i];
					break;
				}
			}
			
			if(!sightBlocked){
				if(target != FPSWalker.leanObj.transform){
					pursueTarget = false;
					targetVisible = true;
					return;
				}else{
					pursueTarget = true;//true when NPC has seen only the player leaning around a corner
					targetVisible = true;
					return;
				}
			}else{
				if(TargetAIComponent && !huntPlayer){
					if(TargetAIComponent.attackTime > Time.time && Vector3.Distance(myTransform.position, target.position) < listenRange ){
						timeout = Time.time + 6.0f;
						heardTarget = true;
					}
				}
				targetVisible = false;
				return;
			}
			
		}else{
			targetVisible = false;
			return;
		}
	
	}
	
	IEnumerator Shoot(){

		attackFinished = false;

		// Start shoot animation
		if(!useMecanim){
			AnimationComponent.CrossFade("shoot", 0.3f);
		}else{
			AnimatorComponent.SetBool("Idle", false);
			AnimatorComponent.SetBool("Walk", false);
			AnimatorComponent.SetBool("Run", false);
			AnimatorComponent.SetBool("Attacking", true);
		}
		//don't move during attack
		speedAmt = 0.0f;
		SetSpeed(speedAmt);
		agent.Stop();

		// Wait until delayShootTime to allow part of the animation to play
		yield return new WaitForSeconds(delayShootTime);
		//attack
		NPCAttackComponent.Fire();
		if(cancelAttackTaunt){
			vocalFx.Stop();
		}
		attackTime = Time.time + 2.0f;
		// Wait for the rest of the animation to finish
		if(!useMecanim){
			yield return new WaitForSeconds(AnimationComponent["shoot"].length + delayShootTime + Random.Range(shotDuration, shotDuration + randShotDelay));
		}else{
			yield return new WaitForSeconds(delayShootTime + Random.Range(shotDuration, shotDuration + 0.75f));
		}

		attackFinished = true;
		if(useMecanim){
			AnimatorComponent.SetBool("Attacking", false);
		}

	}
	
	IEnumerator AttackTarget(){
		while (true) {

//			navTest = agent.hasPath;

//			if(agent.pathStatus == NavMeshPathStatus.PathComplete){
//				navTest = 1;
//			}else if(agent.pathStatus == NavMeshPathStatus.PathPartial){
//				navTest = 2;
//			}else if(agent.pathStatus == NavMeshPathStatus.PathInvalid){
//				navTest = 3;
//			}else{
//				navTest = 0;
//			}
		
			if(Time.timeSinceLevelLoad < 1f){//add small delay before checking target visibility
				yield return new WaitForSeconds(1.0f);
			}
			
			// no target - stop hunting
			if(target == null || (TargetAIComponent && !TargetAIComponent.enabled) && !huntPlayer){
				timeout = 0.0f;
				heardPlayer = false;
				heardTarget = false;
				damaged = false;
				TargetAIComponent = null;
				return false;
			}
			
			//play a taunt if hunting target
			if(lastTauntTime + tauntDelay < Time.time 
			&& Random.value < tauntChance 
			&& (alertTaunt || alertSnds.Length <= 0)){
				if(tauntSnds.Length > 0){
					vocalFx.volume = tauntVol;
					vocalFx.pitch = Random.Range(0.94f, 1f);
					vocalFx.spatialBlend = 1.0f;
					vocalFx.clip = tauntSnds[Random.Range(0, tauntSnds.Length)];
					vocalFx.PlayOneShot(vocalFx.clip);
					lastTauntTime = Time.time;
				}
			}
			
			//play alert sound if target detected
			if(!alertTaunt){
				if(alertSnds.Length > 0){
					vocalFx.volume = alertVol;
					vocalFx.pitch = Random.Range(0.94f, 1f);
					vocalFx.spatialBlend = 1.0f;
					vocalFx.clip = alertSnds[Random.Range(0, alertSnds.Length)];
					vocalFx.PlayOneShot(vocalFx.clip);
					lastTauntTime = Time.time;
					alertTaunt = true;
				}
			}
			
			float distance = Vector3.Distance(myTransform.position, target.position);
			
			if(!huntPlayer){
				
				//search for player if their attacks have been heard
				if(heardPlayer && (target == playerTransform || target == FPSWalker.leanObj.transform)){
					speedAmt = runSpeed;
					SearchTarget(lastVisibleTargetPosition);
				}
				//search for target if their attacks have been heard
				if(heardTarget){
					speedAmt = runSpeed;
					SearchTarget(lastVisibleTargetPosition);
				}
		
				// Target is too far away - give up	
				if(distance > attackRangeAmt){
					speedAmt = walkSpeed;
					target = null;
					return false;
				}
				
			}else{
				target = playerTransform;
				speedAmt = runSpeed;
				TravelToPoint(target.position);
			}

			if(pursueTarget){//should NPC attack player collider or leaning collider?
				lastVisibleTargetPosition = FPSWalker.leanObj.transform.position;
			}else{
				lastVisibleTargetPosition = target.position + (target.up * targetEyeHeight);
			}

			CanSeeTarget();
			if(targetVisible){
				timeout = Time.time + 6.0f;
			
				if(distance > shootRange){
					if(!huntPlayer){
						SearchTarget(lastVisibleTargetPosition);
					}
				}else{//close to target, rotate NPC to face it
					if(!turning){
						StopCoroutine("RotateTowards");
						StartCoroutine(RotateTowards(lastVisibleTargetPosition, 20.0f, 2.0f));
					}
					speedAmt = 0.0f;
					SetSpeed(speedAmt);
					agent.speed = speedAmt;
				}
				
				speedAmt = runSpeed;

				Vector3 forward = myTransform.TransformDirection(Vector3.forward);
				Vector3 targetDirection = lastVisibleTargetPosition - (myTransform.position + (myTransform.up * eyeHeight));
				targetDirection.y = 0;
				
				float angle = Vector3.Angle(targetDirection, forward);
				
				// Start shooting if close and player is in sight
				if(distance < shootRange && angle < shootAngle){
					if(attackFinished){
						yield return StartCoroutine(Shoot());
					}else{
						speedAmt = 0.0f;
						SetSpeed(speedAmt);
						agent.Stop();
					}
				}
				
			}else{
				if(!huntPlayer){
					if(attackFinished || huntPlayer){
						if(timeout > Time.time){
							speedAmt = runSpeed;
							SetSpeed(speedAmt);
							SearchTarget(lastVisibleTargetPosition);
						}else{//if timeout has elapsed and target is not visible, resume initial behavior
							heardPlayer = false;
							heardTarget = false;
							alertTaunt = false;
							speedAmt = 0.0f;
							SetSpeed(speedAmt);
							agent.Stop();
							target = null;
							return false;
						}
					}
				}
			}
			
			yield return new WaitForSeconds(0.3f);
		}
	}

	//look for target at a location
	void SearchTarget( Vector3 position  ){
		if(attackFinished){
			if(target == playerTransform || target == FPSWalker.leanObj.transform || (TargetAIComponent && TargetAIComponent.enabled)){
				if(!huntPlayer){
					speedAmt = runSpeed;
					TravelToPoint(target.position);
				}
			}else{
				timeout = 0.0f;
				damaged = false;
			}
		}
	}

	//rotate to face target
	public IEnumerator RotateTowards( Vector3 position, float rotationSpeed, float turnTimer, bool attacking = true ){
		float turnTime;
		turnTime = Time.time;

		SetSpeed(0.0f);
		agent.Stop();

		while(turnTime + turnTimer > Time.time && !cancelRotate){
			turning = true;

			if(pursueTarget){
				position = FPSWalker.leanObj.transform.position;
			}else{
				if((target && attacking && (target == playerTransform || (TargetAIComponent && TargetAIComponent.enabled)))){
					lastVisibleTargetPosition = target.position + (target.up * targetEyeHeight);
				}else{
					lastVisibleTargetPosition = position;
				}
			}
			
			Vector3 direction = lastVisibleTargetPosition - myTransform.position;
			direction.y = 0;
			if(direction.x != 0 && direction.z != 0){
				// Rotate towards the target
				myTransform.rotation = Quaternion.Slerp (myTransform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
				myTransform.eulerAngles = new Vector3(0, myTransform.eulerAngles.y, 0);
				yield return null;
			}else{
				break;//stop rotating 
			}
		}
		cancelRotate = false;
		turning = false;
	}
	
	//Allow tweaking of model yaw/facing direction from the inspector for NPC alignment with attack direction 
	private IEnumerator UpdateModelYaw(){
		while(true){
			
			if(stepInterval > 0f){
				yawAmt = Mathf.MoveTowards(yawAmt, movingYaw, Time.deltaTime * 180f);
			}else{
				yawAmt = Mathf.MoveTowards(yawAmt, idleYaw, Time.deltaTime * 180f);
			}

			objectWithAnims.transform.localRotation = Quaternion.Euler(0.0f, yawAmt, 0.0f);

			yield return null;
		}
	}

	//set navmesh destination and set NPC speed
	void TravelToPoint( Vector3 position  ){
		if(attackFinished){
			agent.SetDestination(position);
			agent.Resume();
			agent.speed = speedAmt;
			SetSpeed(speedAmt);
			
		}
	}
	
	//pick the next waypoint and determine if patrol should continue forward or backward through waypoint group
	Transform PickNextWaypoint( Transform currentWaypoint, int curWaypointNumber  ){
		
		Transform waypoint = currentWaypoint;

		if(!countBackwards){
			if(curWaypointNumber < waypointGroup.wayPoints.Count -1){
				waypoint = waypointGroup.wayPoints[curWaypointNumber + 1];
			}else{
				waypoint = waypointGroup.wayPoints[curWaypointNumber - 1];
				countBackwards = true;
			}
		}else{
			if(curWaypointNumber != 0){
				waypoint = waypointGroup.wayPoints[curWaypointNumber - 1];
			}else{
				waypoint = waypointGroup.wayPoints[curWaypointNumber + 1];
				countBackwards = false;
			}
		}
		return waypoint;
	}

	//play animations for NPC moving state/speed and set footstep sound intervals
	void SetSpeed( float speed  ){
		if(!useMecanim){
			if (speed > walkSpeed && agent.hasPath){
				AnimationComponent.CrossFade("run");
				stepInterval = runStepTime;
			}else{
				if(speed > 0.0f && agent.hasPath){
					AnimationComponent.CrossFade("walk");
					stepInterval = walkStepTime;
				}else{
					AnimationComponent.CrossFade("idle");
					stepInterval = -1.0f;
				}
			}
		}else{
			if (speed > walkSpeed && agent.hasPath){
				AnimatorComponent.SetBool("Attacking", false);
				AnimatorComponent.SetBool("Run", true);
				AnimatorComponent.SetBool("Walk", false);
				AnimatorComponent.SetBool("Idle", false);
				AnimatorComponent.SetBool("Attacking", false);
				stepInterval = runStepTime;
			}else{
				if(speed > 0.0f && agent.hasPath){
					AnimatorComponent.SetBool("Attacking", false);
					AnimatorComponent.SetBool("Walk", true);
					AnimatorComponent.SetBool("Idle", false);
					AnimatorComponent.SetBool("Run", false);
					AnimatorComponent.SetBool("Attacking", false);
					stepInterval = walkStepTime;
				}else{
					if(attackFinished && attackTime < Time.time){
						AnimatorComponent.SetBool("Idle", true);
						AnimatorComponent.SetBool("Attacking", false);
					}else{
						AnimatorComponent.SetBool("Idle", false);
					}
					AnimatorComponent.SetBool("Walk", false);
					AnimatorComponent.SetBool("Run", false);
					stepInterval = -1.0f;
				}
			}
		}
	}

	IEnumerator PlayFootSteps(){
		while(true){
			if(footSteps.Length > 0 && stepInterval > 0.0f ){
				footstepsFx.pitch = 1.0f;
				footstepsFx.volume = footStepVol;
				footstepsFx.clip = footSteps[Random.Range(0, footSteps.Length)];
				footstepsFx.PlayOneShot(footstepsFx.clip);
			}
			yield return new WaitForSeconds(stepInterval);
		}
	}
	
	//Interact with NPC when pressing use key over them 
	public void CommandNPC () {
		if(factionNum == 1 && followOnUse && commandedTime + 0.5f < Time.time){
			orderedMove = false;
			cancelRotate = false;
			commandedTime = Time.time;
			if(attackFinished && !turning){
				StopCoroutine("RotateTowards");
				StartCoroutine(RotateTowards(playerTransform.position, 10.0f, 2.0f, false));
			}
			if(!followPlayer){
				if((followFx1 || followFx2) && ((jokeFx && jokePlaying + jokeFx.length < Time.time) || !jokeFx)){
					if(Random.value > 0.5f){
						vocalFx.clip = followFx1;
					}else{
						vocalFx.clip = followFx2;
					}
					vocalFx.pitch = Random.Range(0.94f, 1f);
					vocalFx.spatialBlend = 1.0f;
					vocalFx.PlayOneShot(vocalFx.clip);
				}
				followPlayer = true;
			}else{
				if((stayFx1 || stayFx2) && ((jokeFx && jokePlaying + jokeFx.length < Time.time) || !jokeFx)){
					if(Random.value > 0.5f){
						vocalFx.clip = stayFx1;
					}else{
						vocalFx.clip = stayFx2;
					}
					vocalFx.pitch = Random.Range(0.94f, 1f);
					vocalFx.spatialBlend = 1.0f;
					vocalFx.PlayOneShot(vocalFx.clip);
				}
				startPosition = myTransform.position;
				followPlayer = false;
			}
		}
		if(jokeFx && factionNum == 1 && followOnUse){
			if(jokeCount == 0){
				talkedTime = Time.time;
			}
			if(talkedTime + 0.5f > Time.time){
				talkedTime = Time.time;
				jokeCount++;
				if(jokeCount > jokeActivate){
					if(!jokeFx2){
						vocalFx.clip = jokeFx;
					}else{
						if(Random.value > 0.5f){
							vocalFx.clip = jokeFx;
						}else{
							vocalFx.clip = jokeFx2;
						}
					}
					vocalFx.pitch = Random.Range(0.94f, 1f);
					vocalFx.spatialBlend = 1.0f;
					vocalFx.PlayOneShot(vocalFx.clip);
					jokePlaying = Time.time;
					jokeCount = 0;
				}
			}else{
				jokeCount = 0;
			}
		}
	}
	
	//Move an NPC to a specific position 
	public void GoToPosition (Vector3 position, bool runToPos) {
		if(runToPos){
			orderedMove = true;
		}else{
			orderedMove = false;
		}
		cancelRotate = true;
		startPosition = position;
	}

//	//used to change the faction of the NPC
	public void ChangeFaction(int factionChange){
		target = null;
		factionNum = factionChange;
	}

}