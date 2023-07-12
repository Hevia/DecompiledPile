using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Engi.SpiderMine;

public class Detonate : BaseSpiderMineState
{
	public static float blastRadius;

	public static GameObject blastEffectPrefab;

	protected override bool shouldStick => false;

	public override void OnEnter()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (NetworkServer.active)
		{
			ProjectileDamage component = GetComponent<ProjectileDamage>();
			Vector3 position = base.transform.position;
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.position = position;
			blastAttack.attacker = base.projectileController.owner;
			blastAttack.baseDamage = component.damage;
			blastAttack.baseForce = component.force;
			blastAttack.bonusForce = Vector3.zero;
			blastAttack.crit = component.crit;
			blastAttack.damageColorIndex = component.damageColorIndex;
			blastAttack.damageType = component.damageType;
			blastAttack.falloffModel = BlastAttack.FalloffModel.None;
			blastAttack.inflictor = base.gameObject;
			blastAttack.procChainMask = base.projectileController.procChainMask;
			blastAttack.radius = blastRadius;
			blastAttack.teamIndex = base.projectileController.teamFilter.teamIndex;
			blastAttack.procCoefficient = base.projectileController.procCoefficient;
			blastAttack.Fire();
			EffectManager.SpawnEffect(blastEffectPrefab, new EffectData
			{
				origin = position,
				scale = blastRadius
			}, transmit: true);
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}
}
