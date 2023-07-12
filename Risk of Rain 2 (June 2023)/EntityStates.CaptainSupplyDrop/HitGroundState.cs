using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.CaptainSupplyDrop;

public class HitGroundState : BaseCaptainSupplyDropState
{
	public static float baseDuration;

	public static GameObject effectPrefab;

	public static float impactBulletDistance;

	public static float impactBulletRadius;

	private float duration;

	public override void OnEnter()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration;
		if (NetworkServer.active)
		{
			GameObject ownerObject = GetComponent<GenericOwnership>().ownerObject;
			ProjectileDamage component = GetComponent<ProjectileDamage>();
			Vector3 position = base.transform.position;
			Vector3 val = -base.transform.up;
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.origin = position - val * impactBulletDistance;
			bulletAttack.aimVector = val;
			bulletAttack.maxDistance = impactBulletDistance + 1f;
			bulletAttack.stopperMask = default(LayerMask);
			bulletAttack.hitMask = LayerIndex.CommonMasks.bullet;
			bulletAttack.damage = component.damage;
			bulletAttack.damageColorIndex = component.damageColorIndex;
			bulletAttack.damageType = component.damageType;
			bulletAttack.bulletCount = 1u;
			bulletAttack.minSpread = 0f;
			bulletAttack.maxSpread = 0f;
			bulletAttack.owner = ownerObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.procCoefficient = 0f;
			bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
			bulletAttack.isCrit = RollCrit();
			bulletAttack.smartCollision = false;
			bulletAttack.sniper = false;
			bulletAttack.force = component.force;
			bulletAttack.radius = impactBulletRadius;
			bulletAttack.hitEffectPrefab = effectPrefab;
			bulletAttack.Fire();
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextState(new DeployState());
		}
	}
}
