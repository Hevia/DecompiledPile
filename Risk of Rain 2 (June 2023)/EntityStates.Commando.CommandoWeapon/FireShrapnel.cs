using RoR2;
using UnityEngine;

namespace EntityStates.Commando.CommandoWeapon;

public class FireShrapnel : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject hitEffectPrefab;

	public static GameObject tracerEffectPrefab;

	public static float damageCoefficient;

	public static float blastRadius;

	public static float force;

	public static float minSpread;

	public static float maxSpread;

	public static int bulletCount;

	public static float baseDuration = 2f;

	public static string attackSoundString;

	public static float maxDistance;

	private float duration;

	private Ray modifiedAimRay;

	public override void OnEnter()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modifiedAimRay = GetAimRay();
		Util.PlaySound(attackSoundString, base.gameObject);
		string muzzleName = "MuzzleLaser";
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(2f);
		}
		PlayAnimation("Gesture", "FireLaser", "FireLaser.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (base.isAuthority)
		{
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(modifiedAimRay, ref val, 1000f, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.defaultLayer.mask)))
			{
				BlastAttack blastAttack = new BlastAttack();
				blastAttack.attacker = base.gameObject;
				blastAttack.inflictor = base.gameObject;
				blastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
				blastAttack.baseDamage = damageStat * damageCoefficient;
				blastAttack.baseForce = force * 0.2f;
				blastAttack.position = ((RaycastHit)(ref val)).point;
				blastAttack.radius = blastRadius;
				blastAttack.Fire();
			}
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = ((Ray)(ref modifiedAimRay)).origin;
			bulletAttack.aimVector = ((Ray)(ref modifiedAimRay)).direction;
			bulletAttack.radius = 0.25f;
			bulletAttack.minSpread = minSpread;
			bulletAttack.maxSpread = maxSpread;
			bulletAttack.bulletCount = ((bulletCount > 0) ? ((uint)bulletCount) : 0u);
			bulletAttack.damage = 0f;
			bulletAttack.procCoefficient = 0f;
			bulletAttack.force = force;
			bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
			bulletAttack.muzzleName = muzzleName;
			bulletAttack.hitEffectPrefab = hitEffectPrefab;
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			bulletAttack.Fire();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
