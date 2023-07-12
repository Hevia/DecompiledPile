using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.VoidRaidCrab.Weapon;

public class FireMultiBeamFinale : BaseFireMultiBeam
{
	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public float projectileVerticalSpawnOffset;

	[SerializeField]
	public float projectileDamageCoefficient;

	[SerializeField]
	public SkillDef skillDefToReplaceAtStocksEmpty;

	[SerializeField]
	public SkillDef nextSkillDef;

	protected override void OnFireBeam(Vector3 beamStart, Vector3 beamEnd)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)nextSkillDef))
		{
			GenericSkill genericSkill = base.skillLocator.FindSkillByDef(skillDefToReplaceAtStocksEmpty);
			if (Object.op_Implicit((Object)(object)genericSkill) && genericSkill.stock == 0)
			{
				genericSkill.SetBaseSkill(nextSkillDef);
			}
		}
		FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
		fireProjectileInfo.projectilePrefab = projectilePrefab;
		fireProjectileInfo.position = beamEnd + Vector3.up * projectileVerticalSpawnOffset;
		fireProjectileInfo.owner = base.gameObject;
		fireProjectileInfo.damage = damageStat * projectileDamageCoefficient;
		fireProjectileInfo.crit = Util.CheckRoll(critStat, base.characterBody.master);
		ProjectileManager.instance.FireProjectile(fireProjectileInfo);
	}

	protected override EntityState InstantiateNextState()
	{
		return EntityStateCatalog.InstantiateState(outer.mainStateType);
	}
}
