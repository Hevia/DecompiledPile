using EntityStates;
using R2API;
using RoR2;
using UnityEngine;

namespace RandomSurvivors;

public class RandomMeleePrimary : RandomPrimary
{
	private bool hasAttacked;

	private float totalDuration;

	public override void OnEnter()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		hasAttacked = false;
		totalDuration = ((EntityState)this).GetComponent<RandomManager>().rate3;
		((BaseState)this).AddRecoil(-4f, 4f, -4f, 4f);
		Ray aimRay = ((BaseState)this).GetAimRay();
		((BaseState)this).StartAimMode(aimRay, 2f, false);
		bool isAuthority = ((EntityState)this).isAuthority;
		if (((EntityState)this).GetComponent<RandomManager>().myAnim != null)
		{
			string myAnim = ((EntityState)this).GetComponent<RandomManager>().myAnim;
			((EntityState)this).PlayAnimation("Gesture, Additive", myAnim, myAnim + ".playbackRate", totalDuration / ((BaseState)this).attackSpeedStat);
			((EntityState)this).PlayAnimation("Gesture, Additive", myAnim, myAnim + ".playbackRate", totalDuration / ((BaseState)this).attackSpeedStat);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Expected O, but got Unknown
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (((EntityState)this).fixedAge >= totalDuration / ((BaseState)this).attackSpeedStat && ((EntityState)this).isAuthority)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
		else
		{
			if (!((EntityState)this).isAuthority || !(((EntityState)this).fixedAge >= totalDuration * 0.25f) || hasAttacked)
			{
				return;
			}
			Ray aimRay = ((BaseState)this).GetAimRay();
			Vector3 val = ((Ray)(ref aimRay)).direction;
			val.y = 0f;
			val = ((Vector3)(ref val)).normalized;
			GameObject val2 = PrefabAPI.InstantiateClone(Resources.Load<GameObject>(((EntityState)this).GetComponent<RandomManager>().swing3), ((EntityState)this).characterBody.baseNameToken + "_PrimarySwing", true, "C:\\Visual Studio Projects\\RoR2\\RoguelikeSurvivor\\FirstMod\\Class1.cs", "FixedUpdate", 1581);
			BlastAttack val3 = new BlastAttack
			{
				attacker = ((EntityState)this).gameObject,
				baseDamage = ((BaseState)this).damageStat * ((EntityState)this).GetComponent<RandomManager>().damage3,
				baseForce = ((EntityState)this).GetComponent<RandomManager>().force3,
				crit = Util.CheckRoll(((BaseState)this).critStat, 0f, (CharacterMaster)null),
				damageType = ((EntityState)this).GetComponent<RandomManager>().damageType3,
				inflictor = ((EntityState)this).gameObject,
				falloffModel = (FalloffModel)0,
				position = ((EntityState)this).characterBody.corePosition + val * 3f,
				procCoefficient = 1f,
				radius = 4.5f,
				teamIndex = ((BaseState)this).GetTeam()
			};
			int hitCount = val3.Fire().hitCount;
			if (hitCount > 0)
			{
				((EntityState)this).characterMotor.velocity = new Vector3(((EntityState)this).characterMotor.velocity.x, Mathf.Max(0f, ((EntityState)this).characterMotor.velocity.y), ((EntityState)this).characterMotor.velocity.z);
				if (!((BaseState)this).isGrounded)
				{
					((EntityState)this).characterMotor.velocity.y = ((EntityState)this).characterMotor.velocity.y + 2f;
				}
			}
			Util.PlaySound(((EntityState)this).GetComponent<RandomManager>().sfx3, ((EntityState)this).gameObject);
			EffectManager.SimpleEffect(val2, ((EntityState)this).characterBody.corePosition + val * 3f, Random.rotation, true);
			hasAttacked = true;
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)2;
	}
}
