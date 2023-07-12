using System;
using System.Linq;
using EntityStates.VoidRaidCrab.Joint;
using EntityStates.VoidRaidCrab.Leg;
using HG;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.VoidRaidCrab;

public class LegController : MonoBehaviour
{
	private const string jointDeathStateMachineName = "Body";

	public EntityStateMachine stateMachine;

	public Animator animator;

	public string primaryLayerName;

	[Tooltip("The transform that should be considered the origin of this leg, points outward from the base, and provides a transform for consistent local space conversions. This field must always be set, and is available for the case that the object this component is attached to is not a bone meeting the metioned criteria.")]
	public Transform originTransform;

	[Header("Regeneration")]
	public float jointRegenDuration = 15f;

	[Header("Footstep Concussive Blast")]
	public float footstepBlastRadius = 20f;

	public BlastAttack.FalloffModel footstepFalloffModel = BlastAttack.FalloffModel.SweetSpot;

	public float footstepBlastForce = 500f;

	public Vector3 footstepBonusForce;

	public GameObject footstepBlastEffectPrefab;

	[Header("Stomp")]
	public Transform stompRangeNearMarker;

	public Transform stompRangeFarMarker;

	public Transform stompRangeLeftMarker;

	public Transform stompRangeRightMarker;

	public float stompAimSpeed = 15f;

	public string stompXParameter;

	public string stompYParameter;

	public string stompPlaybackRateParam;

	[Header("Retraction")]
	public AnimationCurve retractionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	public string retractionLayerName;

	private float currentRetractionWeight;

	private float desiredRetractionWeight;

	public float retractionBlendTransitionRate = 1f;

	public bool shouldRetract;

	[NonSerialized]
	public bool isBreakSuppressed;

	private bool wasJointBodyDead;

	private bool isJointBodyCurrentlyDying;

	private float jointRegenStopwatchServer;

	private Vector3? stompTargetWorldPosition;

	private Vector2 currentLocalStompPosition = Vector2.zero;

	public CharacterBody mainBody { get; private set; }

	public GameObject mainBodyGameObject { get; private set; }

	public bool mainBodyHasEffectiveAuthority => mainBody?.hasEffectiveAuthority ?? false;

	public ChildLocator legChildLocator { get; private set; }

	public ChildLocator childLocator { get; private set; }

	public Transform toeTransform { get; private set; }

	public Transform toeTipTransform { get; private set; }

	public Transform footTranform { get; private set; }

	public CharacterMaster jointMaster { get; private set; }

	public CharacterBody jointBody
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)jointMaster))
			{
				return null;
			}
			return jointMaster.GetBody();
		}
	}

	public bool IsBusy()
	{
		return !(stateMachine.state is Idle);
	}

	public bool IsStomping()
	{
		return stateMachine.state is BaseStompState;
	}

	private void OnEnable()
	{
		CharacterModel componentInParent = ((Component)this).GetComponentInParent<CharacterModel>();
		mainBody = (Object.op_Implicit((Object)(object)componentInParent) ? componentInParent.body : null);
		mainBodyGameObject = (Object.op_Implicit((Object)(object)mainBody) ? ((Component)mainBody).gameObject : null);
		childLocator = ((Component)this).GetComponent<ChildLocator>();
		footTranform = childLocator.FindChild("Foot");
		toeTransform = childLocator.FindChild("Toe");
		toeTipTransform = childLocator.FindChild("ToeTip");
	}

	private void FixedUpdate()
	{
		desiredRetractionWeight = (shouldRetract ? 1f : 0f);
		currentRetractionWeight = Mathf.Clamp01(Mathf.MoveTowards(currentRetractionWeight, desiredRetractionWeight, Time.fixedDeltaTime * retractionBlendTransitionRate));
		int layerIndex = animator.GetLayerIndex(retractionLayerName);
		float num = retractionCurve.Evaluate(currentRetractionWeight);
		animator.SetLayerWeight(layerIndex, num);
		if (!wasJointBodyDead && IsBreakPending())
		{
			wasJointBodyDead = true;
			isJointBodyCurrentlyDying = true;
		}
		if (mainBodyHasEffectiveAuthority)
		{
			UpdateStompTargetPositionAuthority(Time.fixedDeltaTime);
		}
		if (NetworkServer.active && !Object.op_Implicit((Object)(object)jointBody))
		{
			jointRegenStopwatchServer += Time.fixedDeltaTime;
			if (jointRegenStopwatchServer >= jointRegenDuration)
			{
				RegenerateServer();
			}
		}
	}

	public bool RequestStomp(GameObject target)
	{
		if (!IsBusy())
		{
			stateMachine.SetNextState(new PreStompLegRaise
			{
				target = target
			});
			return true;
		}
		return false;
	}

	public void SetStompTargetWorldPosition(Vector3? newStompTargetWorldPosition)
	{
		stompTargetWorldPosition = newStompTargetWorldPosition;
	}

	public bool SetJointMaster(CharacterMaster master, ChildLocator legChildLocator)
	{
		this.legChildLocator = legChildLocator;
		if (!Object.op_Implicit((Object)(object)jointMaster))
		{
			jointMaster = master;
			if (Object.op_Implicit((Object)(object)jointMaster))
			{
				jointMaster.onBodyDestroyed += OnJointBodyDestroyed;
				jointMaster.onBodyStart += OnJointBodyStart;
				MirrorLegJoints();
			}
			return true;
		}
		Debug.LogError((object)("LegController on " + ((Object)((Component)this).gameObject).name + " already has a jointMaster set!"));
		return false;
	}

	private void OnJointBodyStart(CharacterBody body)
	{
		wasJointBodyDead = false;
		isJointBodyCurrentlyDying = false;
		MirrorLegJoints();
	}

	private void OnJointBodyDestroyed(CharacterBody body)
	{
		isJointBodyCurrentlyDying = false;
	}

	public bool IsSupportingWeight()
	{
		if (!IsBroken() && !IsBreakPending())
		{
			return !IsBusy();
		}
		return false;
	}

	public bool CanBreak()
	{
		return Object.op_Implicit((Object)(object)jointBody);
	}

	public bool IsBreakPending()
	{
		if (Object.op_Implicit((Object)(object)jointBody))
		{
			HealthComponent healthComponent = jointBody.healthComponent;
			if (Object.op_Implicit((Object)(object)healthComponent))
			{
				return !healthComponent.alive;
			}
			return false;
		}
		return false;
	}

	public bool IsBroken()
	{
		if (!isJointBodyCurrentlyDying)
		{
			return !Object.op_Implicit((Object)(object)jointBody);
		}
		return true;
	}

	public bool DoesJointExist()
	{
		return Object.op_Implicit((Object)(object)jointBody);
	}

	public void CompleteBreakAuthority()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)jointMaster))
		{
			return;
		}
		CharacterBody characterBody = jointBody;
		if (!Object.op_Implicit((Object)(object)characterBody))
		{
			return;
		}
		HealthComponent healthComponent = characterBody.healthComponent;
		if (Object.op_Implicit((Object)(object)healthComponent))
		{
			if (NetworkServer.active)
			{
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.crit = false;
				damageInfo.damage = healthComponent.fullCombinedHealth;
				damageInfo.procCoefficient = 0f;
				mainBody.healthComponent.TakeDamage(damageInfo);
				GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)healthComponent).gameObject);
				GlobalEventManager.instance.OnHitAll(damageInfo, ((Component)healthComponent).gameObject);
			}
			mainBody.healthComponent.UpdateLastHitTime(0f, Vector3.zero, damageIsSilent: true, healthComponent.lastHitAttacker);
		}
		if (EntityStateMachine.FindByCustomName(((Component)characterBody).gameObject, "Body").state is PreDeathState preDeathState)
		{
			preDeathState.canProceed = true;
		}
	}

	public void RegenerateServer()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)jointMaster))
		{
			jointRegenStopwatchServer = 0f;
			if (!Object.op_Implicit((Object)(object)jointMaster.GetBody()))
			{
				jointMaster.Respawn(((Component)mainBody).transform.position, ((Component)mainBody).transform.rotation);
			}
		}
	}

	private void MirrorLegJoints()
	{
		GameObject bodyObject = jointMaster.GetBodyObject();
		if (Object.op_Implicit((Object)(object)bodyObject) && Object.op_Implicit((Object)(object)legChildLocator))
		{
			ChildLocatorMirrorController component = bodyObject.GetComponent<ChildLocatorMirrorController>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.referenceLocator = legChildLocator;
			}
		}
	}

	private Vector2 WorldPointToLocalStompPoint(Vector3 worldPoint)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = originTransform.InverseTransformVector(worldPoint);
		return new Vector2(val.x, val.z);
	}

	private Vector2 LocalStompPointToStompParams(Vector2 stompPoint)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		float x = originTransform.InverseTransformVector(stompRangeLeftMarker.position).x;
		float x2 = originTransform.InverseTransformVector(stompRangeRightMarker.position).x;
		float z = originTransform.InverseTransformVector(stompRangeNearMarker.position).z;
		float z2 = originTransform.InverseTransformVector(stompRangeFarMarker.position).z;
		float num = Util.Remap(currentLocalStompPosition.x, x, x2, -1f, 1f);
		float num2 = Util.Remap(currentLocalStompPosition.y, z, z2, -1f, 1f);
		return new Vector2(num, num2);
	}

	private void UpdateStompTargetPositionAuthority(float deltaTime)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPoint = toeTipTransform.position;
		if (stompTargetWorldPosition.HasValue)
		{
			worldPoint = stompTargetWorldPosition.Value;
		}
		Vector2 val = WorldPointToLocalStompPoint(worldPoint);
		currentLocalStompPosition = Vector2.MoveTowards(currentLocalStompPosition, val, stompAimSpeed * deltaTime);
		Vector2 val2 = LocalStompPointToStompParams(currentLocalStompPosition);
		animator.SetFloat(stompXParameter, val2.x);
		animator.SetFloat(stompYParameter, val2.y);
	}

	private bool GetKneeToToeTipRaycast(out Vector3 hitPosition, out Vector3 hitNormal, out Vector3 rayNormal)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = footTranform.position;
		Vector3 position2 = toeTipTransform.position;
		Vector3 val = position2 - position;
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (magnitude <= Mathf.Epsilon)
		{
			hitPosition = position2;
			hitNormal = Vector3.up;
			rayNormal = Vector3.down;
			return false;
		}
		Vector3 val2 = val / magnitude;
		RaycastHit[] array = Physics.RaycastAll(new Ray(position, val2), magnitude, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1);
		float num = float.PositiveInfinity;
		int num2 = -1;
		for (int i = 0; i < array.Length; i++)
		{
			ref RaycastHit reference = ref array[i];
			if (((RaycastHit)(ref reference)).distance < num && !((Object)(object)((Component)((RaycastHit)(ref reference)).collider).transform.root == (Object)(object)((Component)this).transform.root))
			{
				num2 = i;
				num = ((RaycastHit)(ref reference)).distance;
			}
		}
		rayNormal = val2;
		if (num2 != -1)
		{
			ref RaycastHit reference2 = ref array[num2];
			hitPosition = ((RaycastHit)(ref reference2)).point;
			hitNormal = ((RaycastHit)(ref reference2)).normal;
			return true;
		}
		hitPosition = toeTipTransform.position;
		hitNormal = -rayNormal;
		return false;
	}

	public void DoToeConcussionBlastAuthority(Vector3? positionOverride = null, bool useEffect = true)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		if (!mainBodyHasEffectiveAuthority)
		{
			throw new Exception("Caller does not have authority.");
		}
		Vector3 hitPosition;
		if (positionOverride.HasValue)
		{
			hitPosition = positionOverride.Value;
		}
		else
		{
			GetKneeToToeTipRaycast(out hitPosition, out var _, out var _);
		}
		if (useEffect)
		{
			EffectData effectData = new EffectData();
			effectData.origin = hitPosition;
			effectData.scale = footstepBlastRadius;
			effectData.rotation = Quaternion.identity;
			EffectManager.SpawnEffect(footstepBlastEffectPrefab, effectData, transmit: true);
		}
		BlastAttack blastAttack = new BlastAttack();
		blastAttack.attacker = mainBodyGameObject;
		blastAttack.teamIndex = (Object.op_Implicit((Object)(object)mainBody) ? mainBody.teamComponent.teamIndex : TeamIndex.None);
		blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
		blastAttack.inflictor = mainBodyGameObject;
		blastAttack.radius = footstepBlastRadius;
		blastAttack.position = hitPosition;
		blastAttack.losType = BlastAttack.LoSType.None;
		blastAttack.procCoefficient = 0f;
		blastAttack.procChainMask = default(ProcChainMask);
		blastAttack.baseDamage = 0f;
		blastAttack.baseForce = footstepBlastForce;
		blastAttack.bonusForce = footstepBonusForce;
		blastAttack.canRejectForce = false;
		blastAttack.crit = false;
		blastAttack.damageColorIndex = DamageColorIndex.Default;
		blastAttack.damageType = DamageType.Silent;
		blastAttack.impactEffect = EffectIndex.Invalid;
		blastAttack.falloffModel = footstepFalloffModel;
		blastAttack.Fire();
	}

	public GameObject CheckForStompTarget()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		Vector3 a2 = stompRangeLeftMarker.position;
		Vector3 b2 = stompRangeRightMarker.position;
		Vector3 c2 = stompRangeNearMarker.position;
		Vector3 d2 = stompRangeFarMarker.position;
		Vector3 val = Average(in a2, in b2, in c2, in d2);
		float a3 = Vector3.Distance(val, a2);
		float b3 = Vector3.Distance(val, b2);
		float c3 = Vector3.Distance(val, c2);
		float d3 = Vector3.Distance(val, d2);
		float num = Min(a3, b3, c3, d3);
		float num2 = Mathf.Sqrt(2f);
		float num3 = 1f / num2;
		float maxDistanceFilter = num * num3;
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.searchOrigin = val;
		bullseyeSearch.minDistanceFilter = 0f;
		bullseyeSearch.maxDistanceFilter = maxDistanceFilter;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
		bullseyeSearch.viewer = mainBody;
		bullseyeSearch.teamMaskFilter = TeamMask.AllExcept(mainBody.teamComponent.teamIndex);
		bullseyeSearch.filterByDistinctEntity = true;
		bullseyeSearch.filterByLoS = false;
		bullseyeSearch.RefreshCandidates();
		HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
		if (hurtBox == null)
		{
			return null;
		}
		HealthComponent healthComponent = hurtBox.healthComponent;
		if (healthComponent == null)
		{
			return null;
		}
		return ((Component)healthComponent).gameObject;
		static Vector3 Average(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val2 = Vector3Utils.Average(ref a, ref b);
			Vector3 val3 = Vector3Utils.Average(ref c, ref d);
			return Vector3Utils.Average(ref val2, ref val3);
		}
		static float Min(float a, float b, float c, float d)
		{
			return Mathf.Min(Mathf.Min(a, b), Mathf.Min(c, d));
		}
	}
}
