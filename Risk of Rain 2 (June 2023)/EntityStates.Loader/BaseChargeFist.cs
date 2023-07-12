using System;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Loader;

public class BaseChargeFist : BaseSkillState
{
	private class ArcVisualizer : IDisposable
	{
		private readonly Vector3[] points;

		private readonly float duration;

		private readonly GameObject arcVisualizerInstance;

		private readonly LineRenderer lineRenderer;

		public ArcVisualizer(GameObject arcVisualizerPrefab, float duration, int vertexCount)
		{
			arcVisualizerInstance = Object.Instantiate<GameObject>(arcVisualizerPrefab);
			lineRenderer = arcVisualizerInstance.GetComponent<LineRenderer>();
			lineRenderer.positionCount = vertexCount;
			points = (Vector3[])(object)new Vector3[vertexCount];
			this.duration = duration;
		}

		public void Dispose()
		{
			EntityState.Destroy((Object)(object)arcVisualizerInstance);
		}

		public void SetParameters(Vector3 origin, Vector3 initialVelocity, float characterMaxSpeed, float characterAcceleration)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			arcVisualizerInstance.transform.position = origin;
			if (!lineRenderer.useWorldSpace)
			{
				Quaternion val = Quaternion.LookRotation(initialVelocity);
				Vector3 eulerAngles = ((Quaternion)(ref val)).eulerAngles;
				eulerAngles.x = 0f;
				eulerAngles.z = 0f;
				Quaternion val2 = Quaternion.Euler(eulerAngles);
				arcVisualizerInstance.transform.rotation = val2;
				origin = Vector3.zero;
				initialVelocity = Quaternion.Inverse(val2) * initialVelocity;
			}
			else
			{
				arcVisualizerInstance.transform.rotation = Quaternion.LookRotation(Vector3.Cross(initialVelocity, Vector3.up));
			}
			float y = Physics.gravity.y;
			float num = duration / (float)points.Length;
			Vector3 val3 = origin;
			Vector3 val4 = initialVelocity;
			float num2 = num;
			float num3 = y * num2;
			float num4 = characterAcceleration * num2;
			for (int i = 0; i < points.Length; i++)
			{
				points[i] = val3;
				Vector2 val5 = Util.Vector3XZToVector2XY(val4);
				val5 = Vector2.MoveTowards(val5, Vector2.op_Implicit(Vector3.zero), num4);
				val4.x = val5.x;
				val4.z = val5.y;
				val4.y += num3;
				val3 += val4 * num2;
			}
			lineRenderer.SetPositions(points);
		}
	}

	public static GameObject arcVisualizerPrefab;

	public static float arcVisualizerSimulationLength;

	public static int arcVisualizerVertexCount;

	[SerializeField]
	public float baseChargeDuration = 1f;

	public static float minChargeForChargedAttack;

	public static GameObject chargeVfxPrefab;

	public static string chargeVfxChildLocatorName;

	public static GameObject crosshairOverridePrefab;

	public static float walkSpeedCoefficient;

	public static string startChargeLoopSFXString;

	public static string endChargeLoopSFXString;

	public static string enterSFXString;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	private Transform chargeVfxInstanceTransform;

	private int gauntlet;

	private uint soundID;

	protected float chargeDuration { get; private set; }

	protected float charge { get; private set; }

	public override void OnEnter()
	{
		base.OnEnter();
		chargeDuration = baseChargeDuration / attackSpeedStat;
		Util.PlaySound(enterSFXString, base.gameObject);
		soundID = Util.PlaySound(startChargeLoopSFXString, base.gameObject);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)chargeVfxInstanceTransform))
		{
			EntityState.Destroy((Object)(object)((Component)chargeVfxInstanceTransform).gameObject);
			PlayAnimation("Gesture, Additive", "Empty");
			PlayAnimation("Gesture, Override", "Empty");
			crosshairOverrideRequest?.Dispose();
			chargeVfxInstanceTransform = null;
		}
		base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
		Util.PlaySound(endChargeLoopSFXString, base.gameObject);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		charge = Mathf.Clamp01(base.fixedAge / chargeDuration);
		AkSoundEngine.SetRTPCValueByPlayingID("loaderShift_chargeAmount", charge * 100f, soundID);
		base.characterBody.SetSpreadBloom(charge);
		base.characterBody.SetAimTimer(3f);
		if (charge >= minChargeForChargedAttack && !Object.op_Implicit((Object)(object)chargeVfxInstanceTransform) && Object.op_Implicit((Object)(object)chargeVfxPrefab))
		{
			if (Object.op_Implicit((Object)(object)crosshairOverridePrefab) && crosshairOverrideRequest == null)
			{
				crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
			}
			Transform val = FindModelChild(chargeVfxChildLocatorName);
			if (Object.op_Implicit((Object)(object)val))
			{
				chargeVfxInstanceTransform = Object.Instantiate<GameObject>(chargeVfxPrefab, val).transform;
				ScaleParticleSystemDuration component = ((Component)chargeVfxInstanceTransform).GetComponent<ScaleParticleSystemDuration>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.newDuration = (1f - minChargeForChargedAttack) * chargeDuration;
				}
			}
			PlayCrossfade("Gesture, Additive", "ChargePunchIntro", "ChargePunchIntro.playbackRate", chargeDuration, 0.1f);
			PlayCrossfade("Gesture, Override", "ChargePunchIntro", "ChargePunchIntro.playbackRate", chargeDuration, 0.1f);
		}
		if (Object.op_Implicit((Object)(object)chargeVfxInstanceTransform))
		{
			base.characterMotor.walkSpeedPenaltyCoefficient = walkSpeedCoefficient;
		}
		if (base.isAuthority)
		{
			AuthorityFixedUpdate();
		}
	}

	public override void Update()
	{
		base.Update();
		Mathf.Clamp01(base.age / chargeDuration);
	}

	private void AuthorityFixedUpdate()
	{
		if (!ShouldKeepChargingAuthority())
		{
			outer.SetNextState(GetNextStateAuthority());
		}
	}

	protected virtual bool ShouldKeepChargingAuthority()
	{
		return IsKeyDownAuthority();
	}

	protected virtual EntityState GetNextStateAuthority()
	{
		return new SwingChargedFist
		{
			charge = charge
		};
	}
}
