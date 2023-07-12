using RoR2;
using UnityEngine;

namespace EntityStates.BrotherMonster;

public class UltExitState : BaseState
{
	public static float lendInterval;

	public static float duration;

	public static string soundString;

	public static GameObject channelFinishEffectPrefab;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "UltExit", "Ult.playbackRate", duration);
		Util.PlaySound(soundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)channelFinishEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(channelFinishEffectPrefab, base.gameObject, "MuzzleUlt", transmit: false);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		GenericSkill genericSkill = (Object.op_Implicit((Object)(object)base.skillLocator) ? base.skillLocator.special : null);
		if (Object.op_Implicit((Object)(object)genericSkill))
		{
			genericSkill.UnsetSkillOverride(outer, UltChannelState.replacementSkillDef, GenericSkill.SkillOverridePriority.Contextual);
		}
		base.OnExit();
	}
}
