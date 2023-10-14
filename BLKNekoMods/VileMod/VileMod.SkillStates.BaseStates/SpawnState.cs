using EntityStates;
using RoR2;
using UnityEngine;
using VileMod.Modules;

namespace VileMod.SkillStates.BaseStates;

public class SpawnState : GenericCharacterSpawnState
{
	private float duration;

	public float baseDuration = 3f;

	private Animator animator;

	public override void OnEnter()
	{
		((GenericCharacterSpawnState)this).OnEnter();
		duration = baseDuration;
		Util.PlaySound(Sounds.VReady, ((EntityState)this).gameObject);
		((EntityState)this).PlayAnimation("Body", "Spawn", "attackSpeed", duration);
	}

	public override void OnExit()
	{
		Util.PlaySound(Sounds.vileSpawn, ((EntityState)this).gameObject);
		((EntityState)this).OnExit();
	}

	public override void FixedUpdate()
	{
		((GenericCharacterSpawnState)this).FixedUpdate();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)6;
	}
}
