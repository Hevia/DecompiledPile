using EntityStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace RandomSurvivors;

public class RandomSecondary : BaseState
{
	private float totalDuration;

	public override void OnEnter()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		((BaseState)this).OnEnter();
		totalDuration = ((EntityState)this).GetComponent<RandomManager>().rate1;
		((BaseState)this).AddRecoil(-0f, -8f, -4f, 4f);
		Ray aimRay = ((BaseState)this).GetAimRay();
		((BaseState)this).StartAimMode(aimRay, 2f, false);
		bool isAuthority = ((EntityState)this).isAuthority;
		if (((EntityState)this).GetComponent<RandomManager>().myAnim2 != null)
		{
			string myAnim = ((EntityState)this).GetComponent<RandomManager>().myAnim2;
			((EntityState)this).PlayAnimation("Gesture, Additive", myAnim, myAnim + ".playbackRate", totalDuration / base.attackSpeedStat);
			((EntityState)this).PlayAnimation("Gesture, Additive", myAnim, myAnim + ".playbackRate", totalDuration / base.attackSpeedStat);
		}
		for (int i = 0; i < ((EntityState)this).GetComponent<RandomManager>().multi1 + 1; i++)
		{
			if (isAuthority)
			{
				int multi1Type = ((EntityState)this).GetComponent<RandomManager>().multi1Type;
				float num = ((EntityState)this).GetComponent<RandomManager>().multi1;
				float num2 = 0f;
				float num3 = 0f;
				switch (multi1Type)
				{
				case 0:
					num2 = 8f * (num / 2f - num + (float)i);
					break;
				case 1:
					num3 = 8f * (num / 2f - num + (float)i);
					break;
				default:
					num2 = Random.Range(-8f, 8f);
					num3 = Random.Range(-8f, 8f);
					break;
				}
				Vector3 val = Util.ApplySpread(((Ray)(ref aimRay)).direction, 0f, 0f, 1f, 1f, num2, num3);
				GameObject val2 = PrefabAPI.InstantiateClone(Resources.Load<GameObject>(((EntityState)this).GetComponent<RandomManager>().prefab1), ((EntityState)this).characterBody.baseNameToken + "_Secondary", true, "C:\\Visual Studio Projects\\RoR2\\RoguelikeSurvivor\\FirstMod\\Class1.cs", "OnEnter", 1657);
				if (Object.op_Implicit((Object)(object)val2.GetComponent<ProjectileDamage>()))
				{
					val2.GetComponent<ProjectileDamage>().damage = 1f;
				}
				if (Object.op_Implicit((Object)(object)val2.GetComponent<ProjectileImpactExplosion>()))
				{
					((ProjectileExplosion)val2.GetComponent<ProjectileImpactExplosion>()).blastDamageCoefficient = 1f;
				}
				FireProjectileInfo val3 = default(FireProjectileInfo);
				val3.projectilePrefab = val2;
				val3.position = ((Ray)(ref aimRay)).origin;
				val3.rotation = Quaternion.LookRotation(val);
				val3.owner = ((Component)((EntityState)this).characterBody).gameObject;
				val3.damage = ((EntityState)this).GetComponent<RandomManager>().damage1 * ((EntityState)this).characterBody.damage;
				val3.force = ((EntityState)this).GetComponent<RandomManager>().force1;
				val3.crit = ((EntityState)this).characterBody.RollCrit();
				val3.damageColorIndex = (DamageColorIndex)0;
				val3.target = null;
				((FireProjectileInfo)(ref val3)).speedOverride = ((EntityState)this).GetComponent<RandomManager>().speed1;
				((FireProjectileInfo)(ref val3)).fuseOverride = -1f;
				FireProjectileInfo val4 = val3;
				ProjectileManager.instance.FireProjectile(val4);
				((EntityState)this).characterBody.AddSpreadBloom(0.4f);
			}
		}
	}

	public override void OnExit()
	{
		if (((EntityState)this).isAuthority)
		{
			((EntityState)this).OnExit();
		}
	}

	public override void FixedUpdate()
	{
		((EntityState)this).FixedUpdate();
		if (((EntityState)this).fixedAge >= totalDuration && ((EntityState)this).isAuthority)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)2;
	}
}
