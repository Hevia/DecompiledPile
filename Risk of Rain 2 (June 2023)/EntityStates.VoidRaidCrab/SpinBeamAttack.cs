using RoR2;
using RoR2.Audio;
using UnityEngine;

namespace EntityStates.VoidRaidCrab;

public class SpinBeamAttack : BaseSpinBeamAttackState
{
	public static AnimationCurve revolutionsCurve;

	public static GameObject beamVfxPrefab;

	public static float beamRadius = 16f;

	public static float beamMaxDistance = 400f;

	public static float beamDpsCoefficient = 1f;

	public static float beamTickFrequency = 4f;

	public static GameObject beamImpactEffectPrefab;

	public static LoopSoundDef loopSound;

	public static string enterSoundString;

	private float beamTickTimer;

	private LoopSoundManager.SoundLoopPtr loopPtr;

	public override void OnEnter()
	{
		base.OnEnter();
		CreateBeamVFXInstance(beamVfxPrefab);
		loopPtr = LoopSoundManager.PlaySoundLoopLocal(base.gameObject, loopSound);
		Util.PlaySound(enterSoundString, base.gameObject);
	}

	public override void OnExit()
	{
		LoopSoundManager.StopSoundLoopLocal(loopPtr);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= base.duration && base.isAuthority)
		{
			outer.SetNextState(new SpinBeamWindDown());
		}
		if (base.isAuthority)
		{
			if (beamTickTimer <= 0f)
			{
				beamTickTimer += 1f / beamTickFrequency;
				FireBeamBulletAuthority();
			}
			beamTickTimer -= Time.fixedDeltaTime;
		}
		SetHeadYawRevolutions(revolutionsCurve.Evaluate(base.normalizedFixedAge));
	}

	private void FireBeamBulletAuthority()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		Ray beamRay = GetBeamRay();
		BulletAttack bulletAttack = new BulletAttack();
		bulletAttack.muzzleName = BaseSpinBeamAttackState.muzzleTransformNameInChildLocator;
		bulletAttack.origin = ((Ray)(ref beamRay)).origin;
		bulletAttack.aimVector = ((Ray)(ref beamRay)).direction;
		bulletAttack.minSpread = 0f;
		bulletAttack.maxSpread = 0f;
		bulletAttack.maxDistance = 400f;
		bulletAttack.hitMask = LayerIndex.CommonMasks.bullet;
		bulletAttack.stopperMask = LayerMask.op_Implicit(0);
		bulletAttack.bulletCount = 1u;
		bulletAttack.radius = beamRadius;
		bulletAttack.smartCollision = false;
		bulletAttack.queryTriggerInteraction = (QueryTriggerInteraction)1;
		bulletAttack.procCoefficient = 1f;
		bulletAttack.procChainMask = default(ProcChainMask);
		bulletAttack.owner = base.gameObject;
		bulletAttack.weapon = base.gameObject;
		bulletAttack.damage = beamDpsCoefficient * damageStat / beamTickFrequency;
		bulletAttack.damageColorIndex = DamageColorIndex.Default;
		bulletAttack.damageType = DamageType.Generic;
		bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
		bulletAttack.force = 0f;
		bulletAttack.hitEffectPrefab = beamImpactEffectPrefab;
		bulletAttack.tracerEffectPrefab = null;
		bulletAttack.isCrit = false;
		bulletAttack.HitEffectNormal = false;
		bulletAttack.Fire();
	}
}
