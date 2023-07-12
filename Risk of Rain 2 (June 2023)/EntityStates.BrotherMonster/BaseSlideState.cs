using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.BrotherMonster;

public class BaseSlideState : BaseState
{
	public static float duration;

	public static AnimationCurve speedCoefficientCurve;

	public static AnimationCurve jumpforwardSpeedCoefficientCurve;

	public static string soundString;

	public static GameObject slideEffectPrefab;

	public static string slideEffectMuzzlestring;

	protected Vector3 slideVector;

	protected Quaternion slideRotation;

	public override void OnEnter()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(soundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			Object.op_Implicit((Object)(object)base.characterDirection);
		}
		if (NetworkServer.active)
		{
			Util.CleanseBody(base.characterBody, removeDebuffs: true, removeBuffs: false, removeCooldownBuffs: false, removeDots: false, removeStun: false, removeNearbyProjectiles: false);
		}
		if (Object.op_Implicit((Object)(object)slideEffectPrefab) && Object.op_Implicit((Object)(object)base.characterBody))
		{
			Vector3 position = base.characterBody.corePosition;
			Quaternion rotation = Quaternion.identity;
			Transform val = FindModelChild(slideEffectMuzzlestring);
			if (Object.op_Implicit((Object)(object)val))
			{
				position = val.position;
			}
			if (Object.op_Implicit((Object)(object)base.characterDirection))
			{
				rotation = Util.QuaternionSafeLookRotation(slideRotation * base.characterDirection.forward, Vector3.up);
			}
			EffectManager.SimpleEffect(slideEffectPrefab, position, rotation, transmit: false);
		}
	}

	public override void FixedUpdate()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.isAuthority)
		{
			Vector3 val = Vector3.zero;
			if (Object.op_Implicit((Object)(object)base.inputBank) && Object.op_Implicit((Object)(object)base.characterDirection))
			{
				val = base.characterDirection.forward;
			}
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				float num = speedCoefficientCurve.Evaluate(base.fixedAge / duration);
				CharacterMotor obj = base.characterMotor;
				obj.rootMotion += slideRotation * (num * moveSpeedStat * val * Time.fixedDeltaTime);
			}
			if (base.fixedAge >= duration)
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override void OnExit()
	{
		if (!outer.destroying)
		{
			PlayImpactAnimation();
		}
		base.OnExit();
	}

	private void PlayImpactAnimation()
	{
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
