using System;
using System.Linq;
using EntityStates;
using EntityStates.AI;
using JetBrains.Annotations;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.CharacterAI;

[RequireComponent(typeof(CharacterMaster))]
public class BaseAI : MonoBehaviour
{
	[Serializable]
	public class Target
	{
		private readonly BaseAI owner;

		private bool unset = true;

		private GameObject _gameObject;

		public HurtBox bestHurtBox;

		private HurtBoxGroup hurtBoxGroup;

		private HurtBox mainHurtBox;

		private int bullseyeCount;

		private Vector3? lastKnownBullseyePosition;

		private Run.FixedTimeStamp lastKnownBullseyePositionTime = Run.FixedTimeStamp.negativeInfinity;

		public bool hasLoS;

		public GameObject gameObject
		{
			get
			{
				return _gameObject;
			}
			set
			{
				if (value != _gameObject)
				{
					_gameObject = value;
					GameObject obj = gameObject;
					characterBody = ((obj != null) ? obj.GetComponent<CharacterBody>() : null);
					healthComponent = characterBody?.healthComponent;
					hurtBoxGroup = characterBody?.hurtBoxGroup;
					bullseyeCount = (Object.op_Implicit((Object)(object)hurtBoxGroup) ? hurtBoxGroup.bullseyeCount : 0);
					mainHurtBox = (Object.op_Implicit((Object)(object)hurtBoxGroup) ? hurtBoxGroup.mainHurtBox : null);
					bestHurtBox = mainHurtBox;
					hasLoS = false;
					unset = !Object.op_Implicit((Object)(object)_gameObject);
				}
			}
		}

		public CharacterBody characterBody { get; private set; }

		public HealthComponent healthComponent { get; private set; }

		public Target([NotNull] BaseAI owner)
		{
			this.owner = owner;
		}

		public void Update()
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)gameObject))
			{
				hasLoS = Object.op_Implicit((Object)(object)bestHurtBox) && owner.HasLOS(((Component)bestHurtBox).transform.position);
				if (bullseyeCount > 1 && !hasLoS)
				{
					bestHurtBox = GetBestHurtBox(out hasLoS);
				}
			}
		}

		public bool TestLOSNow()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)bestHurtBox))
			{
				return owner.HasLOS(((Component)bestHurtBox).transform.position);
			}
			return false;
		}

		public bool GetBullseyePosition(out Vector3 position)
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)characterBody) && Object.op_Implicit((Object)(object)owner.body) && (characterBody.GetVisibilityLevel(owner.body) >= VisibilityLevel.Revealed || lastKnownBullseyePositionTime.timeSince >= 2f))
			{
				if (Object.op_Implicit((Object)(object)bestHurtBox))
				{
					lastKnownBullseyePosition = ((Component)bestHurtBox).transform.position;
				}
				else
				{
					lastKnownBullseyePosition = null;
				}
				lastKnownBullseyePositionTime = Run.FixedTimeStamp.now;
			}
			if (lastKnownBullseyePosition.HasValue)
			{
				position = lastKnownBullseyePosition.Value;
				return true;
			}
			position = (Object.op_Implicit((Object)(object)gameObject) ? gameObject.transform.position : Vector3.zero);
			return false;
		}

		private HurtBox GetBestHurtBox(out bool hadLoS)
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)owner) && Object.op_Implicit((Object)(object)owner.bodyInputBank) && Object.op_Implicit((Object)(object)hurtBoxGroup))
			{
				Vector3 aimOrigin = owner.bodyInputBank.aimOrigin;
				HurtBox hurtBox = null;
				float num = float.PositiveInfinity;
				HurtBox[] hurtBoxes = hurtBoxGroup.hurtBoxes;
				foreach (HurtBox hurtBox2 in hurtBoxes)
				{
					if (!Object.op_Implicit((Object)(object)hurtBox2) || !Object.op_Implicit((Object)(object)((Component)hurtBox2).transform) || !hurtBox2.isBullseye)
					{
						continue;
					}
					Vector3 position = ((Component)hurtBox2).transform.position;
					if (CheckLoS(aimOrigin, ((Component)hurtBox2).transform.position))
					{
						Vector3 val = position - aimOrigin;
						float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							num = sqrMagnitude;
							hurtBox = hurtBox2;
						}
					}
				}
				if (Object.op_Implicit((Object)(object)hurtBox))
				{
					hadLoS = true;
					return hurtBox;
				}
			}
			hadLoS = false;
			return mainHurtBox;
		}

		public void Reset()
		{
			if (!unset)
			{
				_gameObject = null;
				characterBody = null;
				healthComponent = null;
				hurtBoxGroup = null;
				bullseyeCount = 0;
				mainHurtBox = null;
				bestHurtBox = mainHurtBox;
				hasLoS = false;
				unset = true;
			}
		}
	}

	public struct SkillDriverEvaluation
	{
		public AISkillDriver dominantSkillDriver;

		public Target target;

		public Target aimTarget;

		public float separationSqrMagnitude;
	}

	public struct BodyInputs
	{
		public Vector3 desiredAimDirection;

		public Vector3 moveVector;

		public bool pressSprint;

		public bool pressJump;

		public bool pressSkill1;

		public bool pressSkill2;

		public bool pressSkill3;

		public bool pressSkill4;

		public bool pressActivateEquipment;
	}

	[Tooltip("If true, this character can spot enemies behind itself.")]
	public bool fullVision;

	[Tooltip("If true, this AI will not be allowed to retaliate against damage received from a source on its own team.")]
	public bool neverRetaliateFriendlies;

	public float enemyAttentionDuration = 5f;

	public MapNodeGroup.GraphType desiredSpawnNodeGraphType;

	[Tooltip("The state machine to run while the body exists.")]
	public EntityStateMachine stateMachine;

	public SerializableEntityStateType scanState;

	public bool isHealer;

	public float enemyAttention;

	public float aimVectorDampTime = 0.2f;

	public float aimVectorMaxSpeed = 6f;

	private float aimVelocity;

	private float targetRefreshTimer;

	private const float targetRefreshInterval = 0.5f;

	public LocalNavigator localNavigator = new LocalNavigator();

	public string selectedSkilldriverName;

	private const float maxVisionDistance = float.PositiveInfinity;

	public static readonly NodeGraphNavigationSystem nodeGraphNavigationSystem = new NodeGraphNavigationSystem();

	private BroadNavigationSystem _broadNavigationSystem;

	private BroadNavigationSystem.Agent _broadNavigationAgent = BroadNavigationSystem.Agent.invalid;

	public HurtBox debugEnemyHurtBox;

	private BullseyeSearch enemySearch = new BullseyeSearch();

	private BullseyeSearch buddySearch = new BullseyeSearch();

	private float skillDriverUpdateTimer;

	private const float skillDriverMinUpdateInterval = 1f / 6f;

	private const float skillDriverMaxUpdateInterval = 0.2f;

	public SkillDriverEvaluation skillDriverEvaluation;

	protected BodyInputs bodyInputs;

	public CharacterMaster master { get; protected set; }

	public CharacterBody body { get; protected set; }

	public Transform bodyTransform { get; protected set; }

	public CharacterDirection bodyCharacterDirection { get; protected set; }

	public CharacterMotor bodyCharacterMotor { get; protected set; }

	public InputBankTest bodyInputBank { get; protected set; }

	public HealthComponent bodyHealthComponent { get; protected set; }

	public SkillLocator bodySkillLocator { get; protected set; }

	public NetworkIdentity networkIdentity { get; protected set; }

	public AISkillDriver[] skillDrivers { get; protected set; }

	public bool hasAimConfirmation { get; private set; }

	public bool hasAimTarget
	{
		get
		{
			if (skillDriverEvaluation.aimTarget != null)
			{
				return Object.op_Implicit((Object)(object)skillDriverEvaluation.aimTarget.gameObject);
			}
			return false;
		}
	}

	private BroadNavigationSystem broadNavigationSystem
	{
		get
		{
			return _broadNavigationSystem;
		}
		set
		{
			if (_broadNavigationSystem != value)
			{
				_broadNavigationSystem?.ReturnAgent(ref _broadNavigationAgent);
				_broadNavigationSystem = value;
				_broadNavigationSystem?.RequestAgent(ref _broadNavigationAgent);
			}
		}
	}

	public BroadNavigationSystem.Agent broadNavigationAgent => _broadNavigationAgent;

	public Target currentEnemy { get; private set; }

	public Target leader { get; private set; }

	private Target buddy { get; set; }

	public Target customTarget { get; private set; }

	public event Action<CharacterBody> onBodyDiscovered;

	public event Action<CharacterBody> onBodyLost;

	[ContextMenu("Toggle broad navigation debug drawing")]
	private void ToggleBroadNavigationDebugDraw()
	{
		if (broadNavigationSystem is NodeGraphNavigationSystem)
		{
			NodeGraphNavigationSystem.Agent agent = (NodeGraphNavigationSystem.Agent)broadNavigationAgent;
			agent.drawPath = !agent.drawPath;
		}
	}

	public void SetBroadNavigationDebugDrawEnabled(bool newBroadNavigationDebugDrawEnabled)
	{
		if (broadNavigationSystem is NodeGraphNavigationSystem)
		{
			NodeGraphNavigationSystem.Agent agent = (NodeGraphNavigationSystem.Agent)broadNavigationAgent;
			agent.drawPath = newBroadNavigationDebugDrawEnabled;
		}
	}

	protected void Awake()
	{
		targetRefreshTimer = 0.5f;
		master = ((Component)this).GetComponent<CharacterMaster>();
		stateMachine = ((Component)this).GetComponent<EntityStateMachine>();
		((Behaviour)stateMachine).enabled = false;
		networkIdentity = ((Component)this).GetComponent<NetworkIdentity>();
		skillDrivers = ((Component)this).GetComponents<AISkillDriver>();
		currentEnemy = new Target(this);
		leader = new Target(this);
		buddy = new Target(this);
		customTarget = new Target(this);
		broadNavigationSystem = nodeGraphNavigationSystem;
	}

	protected void OnDestroy()
	{
		broadNavigationSystem = null;
	}

	protected void Start()
	{
		if (!Util.HasEffectiveAuthority(networkIdentity))
		{
			((Behaviour)this).enabled = false;
			if (Object.op_Implicit((Object)(object)stateMachine))
			{
				((Behaviour)stateMachine).enabled = false;
			}
		}
		if (NetworkServer.active)
		{
			skillDriverUpdateTimer = Random.value;
		}
	}

	protected void FixedUpdate()
	{
		enemyAttention -= Time.fixedDeltaTime;
		targetRefreshTimer -= Time.fixedDeltaTime;
		skillDriverUpdateTimer -= Time.fixedDeltaTime;
		if (Object.op_Implicit((Object)(object)body))
		{
			broadNavigationAgent.ConfigureFromBody(body);
			if (skillDriverUpdateTimer <= 0f)
			{
				if (Object.op_Implicit((Object)(object)skillDriverEvaluation.dominantSkillDriver) && skillDriverEvaluation.dominantSkillDriver.resetCurrentEnemyOnNextDriverSelection)
				{
					currentEnemy.Reset();
					targetRefreshTimer = 0f;
				}
				if (!Object.op_Implicit((Object)(object)currentEnemy.gameObject) && targetRefreshTimer <= 0f)
				{
					targetRefreshTimer = 0.5f;
					HurtBox hurtBox = FindEnemyHurtBox(float.PositiveInfinity, fullVision, filterByLoS: true);
					if (Object.op_Implicit((Object)(object)hurtBox) && Object.op_Implicit((Object)(object)hurtBox.healthComponent))
					{
						currentEnemy.gameObject = ((Component)hurtBox.healthComponent).gameObject;
						currentEnemy.bestHurtBox = hurtBox;
					}
					if (Object.op_Implicit((Object)(object)currentEnemy.gameObject))
					{
						enemyAttention = enemyAttentionDuration;
					}
				}
				BeginSkillDriver(EvaluateSkillDrivers());
			}
		}
		_broadNavigationAgent.currentPosition = GetNavigationStartPos();
		UpdateBodyInputs();
		UpdateBodyAim(Time.fixedDeltaTime);
		debugEnemyHurtBox = currentEnemy.bestHurtBox;
	}

	private void BeginSkillDriver(SkillDriverEvaluation newSkillDriverEvaluation)
	{
		skillDriverEvaluation = newSkillDriverEvaluation;
		skillDriverUpdateTimer = Random.Range(1f / 6f, 0.2f);
		if (Object.op_Implicit((Object)(object)skillDriverEvaluation.dominantSkillDriver))
		{
			selectedSkilldriverName = skillDriverEvaluation.dominantSkillDriver.customName;
			if (skillDriverEvaluation.dominantSkillDriver.driverUpdateTimerOverride >= 0f)
			{
				skillDriverUpdateTimer = skillDriverEvaluation.dominantSkillDriver.driverUpdateTimerOverride;
			}
			skillDriverEvaluation.dominantSkillDriver.OnSelected();
		}
		else
		{
			selectedSkilldriverName = "";
		}
	}

	public virtual void OnBodyStart(CharacterBody newBody)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		body = newBody;
		bodyTransform = ((Component)newBody).transform;
		bodyCharacterDirection = ((Component)newBody).GetComponent<CharacterDirection>();
		bodyCharacterMotor = ((Component)newBody).GetComponent<CharacterMotor>();
		bodyInputBank = ((Component)newBody).GetComponent<InputBankTest>();
		bodyHealthComponent = ((Component)newBody).GetComponent<HealthComponent>();
		bodySkillLocator = ((Component)newBody).GetComponent<SkillLocator>();
		localNavigator.SetBody(newBody);
		_broadNavigationAgent.enabled = true;
		if (Object.op_Implicit((Object)(object)stateMachine) && Util.HasEffectiveAuthority(networkIdentity))
		{
			((Behaviour)stateMachine).enabled = true;
			stateMachine.SetNextState(EntityStateCatalog.InstantiateState(scanState));
		}
		((Behaviour)this).enabled = true;
		if (Object.op_Implicit((Object)(object)bodyCharacterDirection))
		{
			bodyInputs.desiredAimDirection = bodyCharacterDirection.forward;
		}
		else
		{
			bodyInputs.desiredAimDirection = bodyTransform.forward;
		}
		if (Object.op_Implicit((Object)(object)bodyInputBank))
		{
			bodyInputBank.aimDirection = bodyInputs.desiredAimDirection;
		}
		this.onBodyDiscovered?.Invoke(newBody);
	}

	public virtual void OnBodyDeath(CharacterBody characterBody)
	{
		OnBodyLost(characterBody);
	}

	public virtual void OnBodyDestroyed(CharacterBody characterBody)
	{
		OnBodyLost(characterBody);
	}

	public virtual void OnBodyLost(CharacterBody characterBody)
	{
		if (body != null)
		{
			((Behaviour)this).enabled = false;
			body = null;
			bodyTransform = null;
			bodyCharacterDirection = null;
			bodyCharacterMotor = null;
			bodyInputBank = null;
			bodyHealthComponent = null;
			bodySkillLocator = null;
			localNavigator.SetBody(null);
			_broadNavigationAgent.enabled = false;
			if (Object.op_Implicit((Object)(object)stateMachine))
			{
				((Behaviour)stateMachine).enabled = false;
				stateMachine.SetNextState(new Idle());
			}
			this.onBodyLost?.Invoke(characterBody);
		}
	}

	public virtual void OnBodyDamaged(DamageReport damageReport)
	{
		DamageInfo damageInfo = damageReport.damageInfo;
		if (Object.op_Implicit((Object)(object)damageInfo.attacker) && Object.op_Implicit((Object)(object)body) && (!Object.op_Implicit((Object)(object)currentEnemy.gameObject) || enemyAttention <= 0f) && (Object)(object)damageInfo.attacker != (Object)(object)((Component)body).gameObject && (!neverRetaliateFriendlies || !damageReport.isFriendlyFire))
		{
			currentEnemy.gameObject = damageInfo.attacker;
			enemyAttention = enemyAttentionDuration;
		}
	}

	[Obsolete]
	public virtual bool HasLOS(Vector3 start, Vector3 end)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)body))
		{
			return false;
		}
		Vector3 direction = end - start;
		float magnitude = ((Vector3)(ref direction)).magnitude;
		if (body.visionDistance < magnitude)
		{
			return false;
		}
		Ray val = default(Ray);
		((Ray)(ref val)).origin = start;
		((Ray)(ref val)).direction = direction;
		RaycastHit val2 = default(RaycastHit);
		return !Physics.Raycast(val, ref val2, magnitude, LayerMask.op_Implicit(LayerIndex.world.mask));
	}

	public virtual bool HasLOS(Vector3 end)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)body) || !Object.op_Implicit((Object)(object)bodyInputBank))
		{
			return false;
		}
		Vector3 aimOrigin = bodyInputBank.aimOrigin;
		Vector3 direction = end - aimOrigin;
		float magnitude = ((Vector3)(ref direction)).magnitude;
		if (body.visionDistance < magnitude)
		{
			return false;
		}
		Ray val = default(Ray);
		((Ray)(ref val)).origin = aimOrigin;
		((Ray)(ref val)).direction = direction;
		RaycastHit val2 = default(RaycastHit);
		return !Physics.Raycast(val, ref val2, magnitude, LayerMask.op_Implicit(LayerIndex.world.mask));
	}

	private Vector3? GetNavigationStartPos()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)body))
		{
			return null;
		}
		return body.footPosition;
	}

	public NodeGraph GetDesiredSpawnNodeGraph()
	{
		return SceneInfo.instance.GetNodeGraph(desiredSpawnNodeGraphType);
	}

	public void SetGoalPosition(Vector3? goalPos)
	{
		BroadNavigationSystem.Agent agent = broadNavigationAgent;
		agent.goalPosition = goalPos;
	}

	public void SetGoalPosition(Target goalTarget)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3? goalPosition = null;
		if (goalTarget.GetBullseyePosition(out var position))
		{
			Vector3 value = position;
			goalPosition = value;
		}
		SetGoalPosition(goalPosition);
	}

	public void DebugDrawPath(Color color, float duration)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (broadNavigationSystem is NodeGraphNavigationSystem)
		{
			((NodeGraphNavigationSystem.Agent)broadNavigationAgent).DebugDrawPath(color, duration);
		}
	}

	private static bool CheckLoS(Vector3 start, Vector3 end)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = end - start;
		RaycastHit val2 = default(RaycastHit);
		return !Physics.Raycast(start, val, ref val2, ((Vector3)(ref val)).magnitude, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1);
	}

	public HurtBox GetBestHurtBox(GameObject target)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		HurtBoxGroup hurtBoxGroup = target.GetComponent<CharacterBody>()?.hurtBoxGroup;
		if (Object.op_Implicit((Object)(object)hurtBoxGroup) && hurtBoxGroup.bullseyeCount > 1 && Object.op_Implicit((Object)(object)bodyInputBank))
		{
			Vector3 aimOrigin = bodyInputBank.aimOrigin;
			HurtBox hurtBox = null;
			float num = float.PositiveInfinity;
			HurtBox[] hurtBoxes = hurtBoxGroup.hurtBoxes;
			foreach (HurtBox hurtBox2 in hurtBoxes)
			{
				if (!hurtBox2.isBullseye)
				{
					continue;
				}
				Vector3 position = ((Component)hurtBox2).transform.position;
				if (CheckLoS(aimOrigin, ((Component)hurtBox2).transform.position))
				{
					Vector3 val = position - aimOrigin;
					float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						hurtBox = hurtBox2;
					}
				}
			}
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				return hurtBox;
			}
		}
		return Util.FindBodyMainHurtBox(target);
	}

	public void ForceAcquireNearestEnemyIfNoCurrentEnemy()
	{
		if (Object.op_Implicit((Object)(object)currentEnemy.gameObject))
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)body))
		{
			Debug.LogErrorFormat("BaseAI.ForceAcquireNearestEnemyIfNoCurrentEnemy for CharacterMaster '{0}' failed: AI has no body to search from.", new object[1] { ((Object)((Component)this).gameObject).name });
			return;
		}
		HurtBox hurtBox = FindEnemyHurtBox(float.PositiveInfinity, full360Vision: true, filterByLoS: false);
		if (Object.op_Implicit((Object)(object)hurtBox) && Object.op_Implicit((Object)(object)hurtBox.healthComponent))
		{
			currentEnemy.gameObject = ((Component)hurtBox.healthComponent).gameObject;
			currentEnemy.bestHurtBox = hurtBox;
		}
	}

	private HurtBox FindEnemyHurtBox(float maxDistance, bool full360Vision, bool filterByLoS)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)body))
		{
			return null;
		}
		enemySearch.viewer = body;
		enemySearch.teamMaskFilter = TeamMask.allButNeutral;
		enemySearch.teamMaskFilter.RemoveTeam(master.teamIndex);
		enemySearch.sortMode = BullseyeSearch.SortMode.Distance;
		enemySearch.minDistanceFilter = 0f;
		enemySearch.maxDistanceFilter = maxDistance;
		enemySearch.searchOrigin = bodyInputBank.aimOrigin;
		enemySearch.searchDirection = bodyInputBank.aimDirection;
		enemySearch.maxAngleFilter = (full360Vision ? 180f : 90f);
		enemySearch.filterByLoS = filterByLoS;
		enemySearch.RefreshCandidates();
		return enemySearch.GetResults().FirstOrDefault();
	}

	public bool GameObjectPassesSkillDriverFilters(Target target, AISkillDriver skillDriver, out float separationSqrMagnitude)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		separationSqrMagnitude = 0f;
		if (!Object.op_Implicit((Object)(object)target.gameObject))
		{
			return false;
		}
		float num = 1f;
		if (Object.op_Implicit((Object)(object)target.healthComponent))
		{
			num = target.healthComponent.combinedHealthFraction;
		}
		if (num < skillDriver.minTargetHealthFraction || num > skillDriver.maxTargetHealthFraction)
		{
			return false;
		}
		float num2 = 0f;
		if (Object.op_Implicit((Object)(object)body))
		{
			num2 = body.radius;
		}
		float num3 = 0f;
		if (Object.op_Implicit((Object)(object)target.characterBody))
		{
			num3 = target.characterBody.radius;
		}
		Vector3 val = (Object.op_Implicit((Object)(object)bodyInputBank) ? bodyInputBank.aimOrigin : bodyTransform.position);
		target.GetBullseyePosition(out var position);
		Vector3 val2 = position - val;
		float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
		separationSqrMagnitude = sqrMagnitude - num3 * num3 - num2 * num2;
		if (separationSqrMagnitude < skillDriver.minDistanceSqr || separationSqrMagnitude > skillDriver.maxDistanceSqr)
		{
			return false;
		}
		if (skillDriver.selectionRequiresTargetLoS && !target.hasLoS)
		{
			return false;
		}
		return true;
	}

	private void UpdateTargets()
	{
		currentEnemy.Update();
		leader.Update();
	}

	protected SkillDriverEvaluation? EvaluateSingleSkillDriver(in SkillDriverEvaluation currentSkillDriverEvaluation, AISkillDriver aiSkillDriver, float myHealthFraction)
	{
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)body) || !Object.op_Implicit((Object)(object)bodySkillLocator))
		{
			return null;
		}
		float separationSqrMagnitude = float.PositiveInfinity;
		if (aiSkillDriver.noRepeat && (Object)(object)currentSkillDriverEvaluation.dominantSkillDriver == (Object)(object)aiSkillDriver)
		{
			return null;
		}
		if (aiSkillDriver.maxTimesSelected >= 0 && aiSkillDriver.timesSelected >= aiSkillDriver.maxTimesSelected)
		{
			return null;
		}
		Target target = null;
		if (aiSkillDriver.requireEquipmentReady && body.equipmentSlot.stock <= 0)
		{
			return null;
		}
		if (aiSkillDriver.skillSlot != SkillSlot.None)
		{
			GenericSkill skill = bodySkillLocator.GetSkill(aiSkillDriver.skillSlot);
			if (aiSkillDriver.requireSkillReady && (!Object.op_Implicit((Object)(object)skill) || !skill.IsReady()))
			{
				return null;
			}
			if (Object.op_Implicit((Object)(object)aiSkillDriver.requiredSkill) && (!Object.op_Implicit((Object)(object)skill) || !((Object)(object)skill.skillDef == (Object)(object)aiSkillDriver.requiredSkill)))
			{
				return null;
			}
		}
		if (aiSkillDriver.minUserHealthFraction > myHealthFraction || aiSkillDriver.maxUserHealthFraction < myHealthFraction)
		{
			return null;
		}
		if (Object.op_Implicit((Object)(object)bodyCharacterMotor) && !bodyCharacterMotor.isGrounded && aiSkillDriver.selectionRequiresOnGround)
		{
			return null;
		}
		switch (aiSkillDriver.moveTargetType)
		{
		case AISkillDriver.TargetType.CurrentEnemy:
			if (GameObjectPassesSkillDriverFilters(currentEnemy, aiSkillDriver, out separationSqrMagnitude))
			{
				target = currentEnemy;
			}
			break;
		case AISkillDriver.TargetType.NearestFriendlyInSkillRange:
			if (Object.op_Implicit((Object)(object)bodyInputBank))
			{
				buddySearch.teamMaskFilter = TeamMask.none;
				buddySearch.teamMaskFilter.AddTeam(master.teamIndex);
				buddySearch.sortMode = BullseyeSearch.SortMode.Distance;
				buddySearch.minDistanceFilter = aiSkillDriver.minDistanceSqr;
				buddySearch.maxDistanceFilter = aiSkillDriver.maxDistance;
				buddySearch.searchOrigin = bodyInputBank.aimOrigin;
				buddySearch.searchDirection = bodyInputBank.aimDirection;
				buddySearch.maxAngleFilter = 180f;
				buddySearch.filterByLoS = aiSkillDriver.activationRequiresTargetLoS;
				buddySearch.RefreshCandidates();
				if (Object.op_Implicit((Object)(object)body))
				{
					buddySearch.FilterOutGameObject(((Component)body).gameObject);
				}
				buddySearch.FilterCandidatesByHealthFraction(aiSkillDriver.minTargetHealthFraction, aiSkillDriver.maxTargetHealthFraction);
				HurtBox hurtBox = buddySearch.GetResults().FirstOrDefault();
				if (Object.op_Implicit((Object)(object)hurtBox) && Object.op_Implicit((Object)(object)hurtBox.healthComponent))
				{
					buddy.gameObject = ((Component)hurtBox.healthComponent).gameObject;
					buddy.bestHurtBox = hurtBox;
				}
				if (GameObjectPassesSkillDriverFilters(buddy, aiSkillDriver, out separationSqrMagnitude))
				{
					target = buddy;
				}
			}
			break;
		case AISkillDriver.TargetType.CurrentLeader:
			if (GameObjectPassesSkillDriverFilters(leader, aiSkillDriver, out separationSqrMagnitude))
			{
				target = leader;
			}
			break;
		case AISkillDriver.TargetType.Custom:
			if (GameObjectPassesSkillDriverFilters(customTarget, aiSkillDriver, out separationSqrMagnitude))
			{
				target = customTarget;
			}
			break;
		}
		if (target == null)
		{
			return null;
		}
		Target target2 = null;
		if (aiSkillDriver.aimType != 0)
		{
			bool flag = aiSkillDriver.selectionRequiresAimTarget;
			switch (aiSkillDriver.aimType)
			{
			case AISkillDriver.AimType.AtMoveTarget:
				target2 = target;
				break;
			case AISkillDriver.AimType.AtCurrentEnemy:
				target2 = currentEnemy;
				break;
			case AISkillDriver.AimType.AtCurrentLeader:
				target2 = leader;
				break;
			default:
				flag = false;
				break;
			}
			if (flag && (target2 == null || !Object.op_Implicit((Object)(object)target2.gameObject)))
			{
				return null;
			}
		}
		SkillDriverEvaluation value = default(SkillDriverEvaluation);
		value.dominantSkillDriver = aiSkillDriver;
		value.target = target;
		value.aimTarget = target2;
		value.separationSqrMagnitude = separationSqrMagnitude;
		return value;
	}

	public SkillDriverEvaluation EvaluateSkillDrivers()
	{
		UpdateTargets();
		float myHealthFraction = 1f;
		if (Object.op_Implicit((Object)(object)bodyHealthComponent))
		{
			myHealthFraction = bodyHealthComponent.combinedHealthFraction;
		}
		if (Object.op_Implicit((Object)(object)bodySkillLocator))
		{
			if (Object.op_Implicit((Object)(object)this.skillDriverEvaluation.dominantSkillDriver) && Object.op_Implicit((Object)(object)this.skillDriverEvaluation.dominantSkillDriver.nextHighPriorityOverride))
			{
				SkillDriverEvaluation? skillDriverEvaluation = EvaluateSingleSkillDriver(in this.skillDriverEvaluation, this.skillDriverEvaluation.dominantSkillDriver.nextHighPriorityOverride, myHealthFraction);
				if (skillDriverEvaluation.HasValue)
				{
					return skillDriverEvaluation.Value;
				}
			}
			for (int i = 0; i < skillDrivers.Length; i++)
			{
				AISkillDriver aISkillDriver = skillDrivers[i];
				if (((Behaviour)aISkillDriver).enabled)
				{
					SkillDriverEvaluation? skillDriverEvaluation2 = EvaluateSingleSkillDriver(in this.skillDriverEvaluation, aISkillDriver, myHealthFraction);
					if (skillDriverEvaluation2.HasValue)
					{
						return skillDriverEvaluation2.Value;
					}
				}
			}
		}
		return default(SkillDriverEvaluation);
	}

	protected void UpdateBodyInputs()
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (stateMachine.state is BaseAIState baseAIState)
		{
			bodyInputs = baseAIState.GenerateBodyInputs(in bodyInputs);
		}
		if (Object.op_Implicit((Object)(object)bodyInputBank))
		{
			bodyInputBank.skill1.PushState(bodyInputs.pressSkill1);
			bodyInputBank.skill2.PushState(bodyInputs.pressSkill2);
			bodyInputBank.skill3.PushState(bodyInputs.pressSkill3);
			bodyInputBank.skill4.PushState(bodyInputs.pressSkill4);
			bodyInputBank.jump.PushState(bodyInputs.pressJump);
			bodyInputBank.sprint.PushState(bodyInputs.pressSprint);
			bodyInputBank.activateEquipment.PushState(bodyInputs.pressActivateEquipment);
			bodyInputBank.moveVector = bodyInputs.moveVector;
		}
	}

	protected void UpdateBodyAim(float deltaTime)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		hasAimConfirmation = false;
		if (Object.op_Implicit((Object)(object)bodyInputBank))
		{
			Vector3 aimDirection = bodyInputBank.aimDirection;
			Vector3 desiredAimDirection = bodyInputs.desiredAimDirection;
			if (desiredAimDirection != Vector3.zero)
			{
				Quaternion target = Util.QuaternionSafeLookRotation(desiredAimDirection);
				Vector3 val = Util.SmoothDampQuaternion(Util.QuaternionSafeLookRotation(aimDirection), target, ref aimVelocity, aimVectorDampTime, aimVectorMaxSpeed, deltaTime) * Vector3.forward;
				bodyInputBank.aimDirection = val;
				hasAimConfirmation = Vector3.Dot(val, desiredAimDirection) >= 0.95f;
			}
		}
	}

	[ConCommand(commandName = "ai_draw_path", flags = ConVarFlags.Cheat, helpText = "Enables or disables the drawing of the specified AI's broad navigation path. Format: ai_draw_path <CharacterMaster selector> <0/1>")]
	private static void CCAiDrawPath(ConCommandArgs args)
	{
		CharacterMaster argCharacterMasterInstance = args.GetArgCharacterMasterInstance(0);
		args.GetArgBool(1);
		if (!Object.op_Implicit((Object)(object)argCharacterMasterInstance))
		{
			throw new ConCommandException("Could not find target.");
		}
		BaseAI component = ((Component)argCharacterMasterInstance).GetComponent<BaseAI>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			throw new ConCommandException("Target has no AI.");
		}
		component.ToggleBroadNavigationDebugDraw();
	}
}
