using EntityStates;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace RandomSurvivors;

public class RandomProjectilePrimary : RandomPrimary
{
	private float totalDuration;

	public override void OnEnter()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		totalDuration = ((EntityState)this).GetComponent<RandomManager>().rate0;
		((BaseState)this).AddRecoil(-0f, -4f, -2f, 2f);
		Ray aimRay = ((BaseState)this).GetAimRay();
		((BaseState)this).StartAimMode(aimRay, 2f, false);
		bool isAuthority = ((EntityState)this).isAuthority;
		if (((EntityState)this).GetComponent<RandomManager>().myAnim != null)
		{
			string myAnim = ((EntityState)this).GetComponent<RandomManager>().myAnim;
			((EntityState)this).PlayAnimation("Gesture, Additive", myAnim, myAnim + ".playbackRate", totalDuration / ((BaseState)this).attackSpeedStat);
			((EntityState)this).PlayAnimation("Gesture, Additive", myAnim, myAnim + ".playbackRate", totalDuration / ((BaseState)this).attackSpeedStat);
		}
		if (isAuthority)
		{
			GameObject val = PrefabAPI.InstantiateClone(Resources.Load<GameObject>(((EntityState)this).GetComponent<RandomManager>().prefab0), ((EntityState)this).characterBody.baseNameToken + "_Primary", true, "C:\\Visual Studio Projects\\RoR2\\RoguelikeSurvivor\\FirstMod\\Class1.cs", "OnEnter", 1425);
			if (Object.op_Implicit((Object)(object)val.GetComponent<ProjectileDamage>()))
			{
				val.GetComponent<ProjectileDamage>().damage = 1f;
			}
			if (Object.op_Implicit((Object)(object)val.GetComponent<ProjectileImpactExplosion>()))
			{
				((ProjectileExplosion)val.GetComponent<ProjectileImpactExplosion>()).blastDamageCoefficient = 1f;
			}
			FireProjectileInfo val2 = default(FireProjectileInfo);
			val2.projectilePrefab = val;
			val2.position = ((Ray)(ref aimRay)).origin;
			val2.rotation = Quaternion.LookRotation(((Ray)(ref aimRay)).direction);
			val2.owner = ((Component)((EntityState)this).characterBody).gameObject;
			val2.damage = ((EntityState)this).GetComponent<RandomManager>().damage0 * ((EntityState)this).characterBody.damage;
			val2.force = ((EntityState)this).GetComponent<RandomManager>().force0;
			val2.crit = ((EntityState)this).characterBody.RollCrit();
			val2.damageColorIndex = (DamageColorIndex)0;
			val2.target = null;
			((FireProjectileInfo)(ref val2)).speedOverride = ((EntityState)this).GetComponent<RandomManager>().speed0;
			((FireProjectileInfo)(ref val2)).fuseOverride = -1f;
			FireProjectileInfo val3 = val2;
			ProjectileManager.instance.FireProjectile(val3);
			((EntityState)this).characterBody.AddSpreadBloom(0.1f);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (((EntityState)this).fixedAge >= totalDuration / ((BaseState)this).attackSpeedStat && ((EntityState)this).isAuthority)
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
