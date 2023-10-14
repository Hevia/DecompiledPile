using EntityStates;
using RoR2;
using UnityEngine;
using VileMod.Modules;

namespace VileMod.SkillStates.BaseStates;

public class DeathState : GenericCharacterDeath
{
	private float duration;

	public float baseDuration = 1f;

	private Animator animator;

	public override void OnEnter()
	{
		((GenericCharacterDeath)this).OnEnter();
		duration = baseDuration / ((BaseState)this).attackSpeedStat;
		((EntityState)this).PlayAnimation("FullBody, Override", "Death", "attackSpeed", duration);
		Util.PlaySound(Sounds.vileDie, ((EntityState)this).gameObject);
	}

	public override void OnExit()
	{
		((GenericCharacterDeath)this).OnExit();
	}

	public override void FixedUpdate()
	{
		((GenericCharacterDeath)this).FixedUpdate();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)6;
	}
}
