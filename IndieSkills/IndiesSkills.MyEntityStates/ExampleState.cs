using EntityStates;
using RoR2;
using UnityEngine;

namespace IndiesSkills.MyEntityStates;

public class ExampleState : BaseSkillState
{
	public float baseDuration = 0.5f;

	private float duration;

	public GameObject effectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/Hitspark");

	public GameObject hitEffectPrefab = Resources.Load<GameObject>("prefabs/effects/impacteffects/critspark");

	public GameObject tracerEffectPrefab = Resources.Load<GameObject>("prefabs/effects/tracers/tracerbanditshotgun");

	public override void OnEnter()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		((BaseState)this).OnEnter();
		duration = baseDuration / ((BaseState)this).attackSpeedStat;
		Ray aimRay = ((BaseState)this).GetAimRay();
		((BaseState)this).StartAimMode(aimRay, 2f, false);
		((EntityState)this).PlayAnimation("Gesture, Override", "FireShotgun", "FireShotgun.playbackRate", duration * 1.1f);
		if (((EntityState)this).isAuthority)
		{
			new BulletAttack
			{
				owner = ((EntityState)this).gameObject,
				weapon = ((EntityState)this).gameObject,
				origin = ((Ray)(ref aimRay)).origin,
				aimVector = ((Ray)(ref aimRay)).direction,
				minSpread = 0f,
				maxSpread = ((EntityState)this).characterBody.spreadBloomAngle,
				bulletCount = 1u,
				procCoefficient = 1f,
				damage = ((EntityState)this).characterBody.damage,
				force = 3f,
				falloffModel = (FalloffModel)1,
				tracerEffectPrefab = tracerEffectPrefab,
				hitEffectPrefab = hitEffectPrefab,
				isCrit = ((BaseState)this).RollCrit(),
				HitEffectNormal = false,
				stopperMask = ((LayerIndex)(ref LayerIndex.world)).mask,
				smartCollision = true,
				maxDistance = 300f
			}.Fire();
		}
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
	}

	public override void FixedUpdate()
	{
		((EntityState)this).FixedUpdate();
		if (((EntityState)this).fixedAge >= duration && ((EntityState)this).isAuthority)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)1;
	}
}
