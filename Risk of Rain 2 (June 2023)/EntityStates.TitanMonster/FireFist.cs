using System.Linq;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.TitanMonster;

public class FireFist : BaseState
{
	private enum SubState
	{
		Prep,
		FireFist,
		Exit
	}

	private class Predictor
	{
		private enum ExtrapolationType
		{
			None,
			Linear,
			Polar
		}

		private Transform bodyTransform;

		private Transform targetTransform;

		private Vector3 targetPosition0;

		private Vector3 targetPosition1;

		private Vector3 targetPosition2;

		private int collectedPositions;

		public bool hasTargetTransform => Object.op_Implicit((Object)(object)targetTransform);

		public bool isPredictionReady => collectedPositions > 2;

		public Predictor(Transform bodyTransform)
		{
			this.bodyTransform = bodyTransform;
		}

		private void PushTargetPosition(Vector3 newTargetPosition)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			targetPosition2 = targetPosition1;
			targetPosition1 = targetPosition0;
			targetPosition0 = newTargetPosition;
			collectedPositions++;
		}

		public void SetTargetTransform(Transform newTargetTransform)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			targetTransform = newTargetTransform;
			targetPosition2 = (targetPosition1 = (targetPosition0 = newTargetTransform.position));
			collectedPositions = 1;
		}

		public void Update()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)targetTransform))
			{
				PushTargetPosition(targetTransform.position);
			}
		}

		public bool GetPredictedTargetPosition(float time, out Vector3 predictedPosition)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0241: Unknown result type (might be due to invalid IL or missing references)
			//IL_0246: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = targetPosition1 - targetPosition2;
			Vector3 val2 = targetPosition0 - targetPosition1;
			val.y = 0f;
			val2.y = 0f;
			ExtrapolationType extrapolationType;
			if (val == Vector3.zero || val2 == Vector3.zero)
			{
				extrapolationType = ExtrapolationType.None;
			}
			else
			{
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				Vector3 normalized2 = ((Vector3)(ref val2)).normalized;
				extrapolationType = ((Vector3.Dot(normalized, normalized2) > 0.98f) ? ExtrapolationType.Linear : ExtrapolationType.Polar);
			}
			float num = 1f / Time.fixedDeltaTime;
			predictedPosition = targetPosition0;
			switch (extrapolationType)
			{
			case ExtrapolationType.Linear:
				predictedPosition = targetPosition0 + val2 * (time * num);
				break;
			case ExtrapolationType.Polar:
			{
				Vector3 position = bodyTransform.position;
				Vector3 val3 = Vector2.op_Implicit(Util.Vector3XZToVector2XY(targetPosition2 - position));
				Vector3 val4 = Vector2.op_Implicit(Util.Vector3XZToVector2XY(targetPosition1 - position));
				Vector3 val5 = Vector2.op_Implicit(Util.Vector3XZToVector2XY(targetPosition0 - position));
				float magnitude = ((Vector3)(ref val3)).magnitude;
				float magnitude2 = ((Vector3)(ref val4)).magnitude;
				float magnitude3 = ((Vector3)(ref val5)).magnitude;
				float num2 = Vector2.SignedAngle(Vector2.op_Implicit(val3), Vector2.op_Implicit(val4)) * num;
				float num3 = Vector2.SignedAngle(Vector2.op_Implicit(val4), Vector2.op_Implicit(val5)) * num;
				float num4 = (magnitude2 - magnitude) * num;
				float num5 = (magnitude3 - magnitude2) * num;
				float num6 = (num2 + num3) * 0.5f;
				float num7 = (num4 + num5) * 0.5f;
				float num8 = magnitude3 + num7 * time;
				if (num8 < 0f)
				{
					num8 = 0f;
				}
				Vector2 val6 = Util.RotateVector2(Vector2.op_Implicit(val5), num6 * time);
				val6 *= num8 * magnitude3;
				predictedPosition = position;
				predictedPosition.x += val6.x;
				predictedPosition.z += val6.y;
				break;
			}
			}
			RaycastHit val7 = default(RaycastHit);
			if (Physics.Raycast(new Ray(predictedPosition + Vector3.up * 1f, Vector3.down), ref val7, 200f, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
			{
				predictedPosition = ((RaycastHit)(ref val7)).point;
				return true;
			}
			return false;
		}
	}

	public static float entryDuration = 1f;

	public static float fireDuration = 2f;

	public static float exitDuration = 1f;

	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public GameObject fistEffectPrefab;

	[SerializeField]
	public GameObject fireEffectPrefab;

	[SerializeField]
	public GameObject fistProjectilePrefab;

	public static float maxDistance = 40f;

	public static float trackingDuration = 0.5f;

	public static float fistDamageCoefficient = 2f;

	public static float fistForce = 2000f;

	public static string chargeFistAttackSoundString;

	private bool hasShownPrediction;

	private bool predictionOk;

	protected Vector3 predictedTargetPosition;

	private AimAnimator aimAnimator;

	private GameObject chargeEffect;

	private Transform fistTransform;

	private float stopwatch;

	private SubState subState;

	private Predictor predictor;

	private GameObject predictorDebug;

	public override void OnEnter()
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		stopwatch = 0f;
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			ChildLocator component = ((Component)base.modelLocator.modelTransform).GetComponent<ChildLocator>();
			aimAnimator = ((Component)base.modelLocator.modelTransform).GetComponent<AimAnimator>();
			if (Object.op_Implicit((Object)(object)aimAnimator))
			{
				((Behaviour)aimAnimator).enabled = true;
			}
			if (Object.op_Implicit((Object)(object)component))
			{
				fistTransform = component.FindChild("RightFist");
				if (Object.op_Implicit((Object)(object)fistTransform))
				{
					chargeEffect = Object.Instantiate<GameObject>(chargeEffectPrefab, fistTransform);
				}
			}
		}
		subState = SubState.Prep;
		PlayCrossfade("Body", "PrepFist", "PrepFist.playbackRate", entryDuration, 0.1f);
		Util.PlayAttackSpeedSound(chargeFistAttackSoundString, base.gameObject, attackSpeedStat);
		if (NetworkServer.active)
		{
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
			if (Object.op_Implicit((Object)(object)base.teamComponent))
			{
				bullseyeSearch.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
			}
			bullseyeSearch.maxDistanceFilter = maxDistance;
			bullseyeSearch.maxAngleFilter = 90f;
			Ray aimRay = GetAimRay();
			bullseyeSearch.searchOrigin = ((Ray)(ref aimRay)).origin;
			bullseyeSearch.searchDirection = ((Ray)(ref aimRay)).direction;
			bullseyeSearch.filterByLoS = false;
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
			bullseyeSearch.RefreshCandidates();
			HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				predictor = new Predictor(base.transform);
				predictor.SetTargetTransform(((Component)hurtBox).transform);
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)chargeEffect))
		{
			EntityState.Destroy((Object)(object)chargeEffect);
		}
		EntityState.Destroy((Object)(object)predictorDebug);
		predictorDebug = null;
	}

	public override void FixedUpdate()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		switch (subState)
		{
		case SubState.Prep:
			if (predictor != null)
			{
				predictor.Update();
			}
			if (stopwatch <= trackingDuration)
			{
				if (predictor != null)
				{
					predictionOk = predictor.GetPredictedTargetPosition(entryDuration - trackingDuration, out predictedTargetPosition);
					if (predictionOk && Object.op_Implicit((Object)(object)predictorDebug))
					{
						predictorDebug.transform.position = predictedTargetPosition;
					}
				}
			}
			else if (!hasShownPrediction)
			{
				hasShownPrediction = true;
				PlacePredictedAttack();
			}
			if (stopwatch >= entryDuration)
			{
				predictor = null;
				subState = SubState.FireFist;
				stopwatch = 0f;
				PlayAnimation("Body", "FireFist");
				if (Object.op_Implicit((Object)(object)chargeEffect))
				{
					EntityState.Destroy((Object)(object)chargeEffect);
				}
				Object.Instantiate<GameObject>(fireEffectPrefab, fistTransform.position, Quaternion.identity, fistTransform);
			}
			break;
		case SubState.FireFist:
			if (stopwatch >= fireDuration)
			{
				subState = SubState.Exit;
				stopwatch = 0f;
				PlayCrossfade("Body", "ExitFist", "ExitFist.playbackRate", exitDuration, 0.3f);
			}
			break;
		case SubState.Exit:
			if (stopwatch >= exitDuration && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
			break;
		}
	}

	protected virtual void PlacePredictedAttack()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		PlaceSingleDelayBlast(predictedTargetPosition, 0f);
	}

	protected void PlaceSingleDelayBlast(Vector3 position, float delay)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (base.isAuthority)
		{
			FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
			fireProjectileInfo.projectilePrefab = fistProjectilePrefab;
			fireProjectileInfo.position = position;
			fireProjectileInfo.rotation = Quaternion.identity;
			fireProjectileInfo.owner = base.gameObject;
			fireProjectileInfo.damage = damageStat * fistDamageCoefficient;
			fireProjectileInfo.force = fistForce;
			fireProjectileInfo.crit = base.characterBody.RollCrit();
			fireProjectileInfo.fuseOverride = entryDuration - trackingDuration + delay;
			ProjectileManager.instance.FireProjectile(fireProjectileInfo);
		}
	}
}
