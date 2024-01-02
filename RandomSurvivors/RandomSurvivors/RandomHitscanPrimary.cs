using EntityStates;
using RoR2;
using UnityEngine;

namespace RandomSurvivors;

public class RandomHitscanPrimary : RandomPrimary
{
	private float totalDuration;

	public override void OnEnter()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		totalDuration = ((EntityState)this).GetComponent<RandomManager>().rate2;
		float num = 1f + totalDuration * (float)((EntityState)this).GetComponent<RandomManager>().multi2 * 4f;
		((BaseState)this).AddRecoil(-0f, -0.5f * num, -0.5f * num, 0.5f * num);
		Ray aimRay = ((BaseState)this).GetAimRay();
		((BaseState)this).StartAimMode(aimRay, 2f, false);
		Util.PlaySound(((EntityState)this).GetComponent<RandomManager>().sfx2, ((EntityState)this).gameObject);
		bool isAuthority = ((EntityState)this).isAuthority;
		if (((EntityState)this).GetComponent<RandomManager>().myAnim != null)
		{
			string myAnim = ((EntityState)this).GetComponent<RandomManager>().myAnim;
			((EntityState)this).PlayAnimation("Gesture, Additive", myAnim, myAnim + ".playbackRate", totalDuration / ((BaseState)this).attackSpeedStat);
			((EntityState)this).PlayAnimation("Gesture, Additive", myAnim, myAnim + ".playbackRate", totalDuration / ((BaseState)this).attackSpeedStat);
		}
		if (isAuthority)
		{
			((EntityState)this).characterBody.AddSpreadBloom(0.1f * num);
			BulletAttack val = new BulletAttack
			{
				owner = ((EntityState)this).gameObject,
				weapon = ((EntityState)this).gameObject,
				origin = ((Ray)(ref aimRay)).origin,
				aimVector = ((Ray)(ref aimRay)).direction,
				minSpread = 0f,
				maxSpread = ((EntityState)this).characterBody.spreadBloomAngle,
				bulletCount = (uint)((EntityState)this).GetComponent<RandomManager>().multi2,
				procCoefficient = 1f,
				damage = ((BaseState)this).damageStat * ((EntityState)this).GetComponent<RandomManager>().damage2,
				force = ((EntityState)this).GetComponent<RandomManager>().force2,
				falloffModel = (FalloffModel)1,
				tracerEffectPrefab = Resources.Load<GameObject>(((EntityState)this).GetComponent<RandomManager>().tracer2),
				hitEffectPrefab = Resources.Load<GameObject>(((EntityState)this).GetComponent<RandomManager>().impact2),
				isCrit = Util.CheckRoll(((BaseState)this).critStat, 0f, (CharacterMaster)null),
				HitEffectNormal = true
			};
			LayerIndex val2 = LayerIndex.world;
			int num2 = LayerMask.op_Implicit(((LayerIndex)(ref val2)).mask);
			val2 = LayerIndex.entityPrecise;
			val.stopperMask = LayerMask.op_Implicit(num2 | LayerMask.op_Implicit(((LayerIndex)(ref val2)).mask));
			val.smartCollision = true;
			val.radius = 0.25f;
			val.maxDistance = 5000f;
			val.damageType = ((EntityState)this).GetComponent<RandomManager>().damageType2;
			val.Fire();
		}
	}

	public override void OnExit()
	{
		if (((EntityState)this).skillLocator.primary.stock == 0)
		{
			Util.PlaySound(((EntityState)this).GetComponent<RandomManager>().reload2, ((EntityState)this).gameObject);
		}
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
