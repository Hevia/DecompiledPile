using RoR2;
using UnityEngine;

namespace EntityStates.AncientWispMonster;

public class ChargeBomb : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject effectPrefab;

	public static GameObject delayPrefab;

	public static float radius = 10f;

	public static float damageCoefficient = 1f;

	private float duration;

	private GameObject chargeEffectLeft;

	private GameObject chargeEffectRight;

	private Vector3 startLine = Vector3.zero;

	private Vector3 endLine = Vector3.zero;

	private bool hasFired;

	public override void OnEnter()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture", "ChargeBomb", "ChargeBomb.playbackRate", duration);
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)effectPrefab))
			{
				Transform val = component.FindChild("MuzzleLeft");
				Transform val2 = component.FindChild("MuzzleRight");
				if (Object.op_Implicit((Object)(object)val))
				{
					chargeEffectLeft = Object.Instantiate<GameObject>(effectPrefab, val.position, val.rotation);
					chargeEffectLeft.transform.parent = val;
				}
				if (Object.op_Implicit((Object)(object)val2))
				{
					chargeEffectRight = Object.Instantiate<GameObject>(effectPrefab, val2.position, val2.rotation);
					chargeEffectRight.transform.parent = val2;
				}
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
		RaycastHit val3 = default(RaycastHit);
		if (Physics.Raycast(GetAimRay(), ref val3, (float)LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			startLine = ((RaycastHit)(ref val3)).point;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		EntityState.Destroy((Object)(object)chargeEffectLeft);
		EntityState.Destroy((Object)(object)chargeEffectRight);
	}

	public override void Update()
	{
		base.Update();
	}

	public override void FixedUpdate()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		float num = 0f;
		if (base.fixedAge >= num && !hasFired)
		{
			hasFired = true;
			Ray aimRay = GetAimRay();
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(aimRay, ref val, (float)LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				endLine = ((RaycastHit)(ref val)).point;
			}
			Vector3 val2 = endLine - startLine;
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			normalized.y = 0f;
			((Vector3)(ref normalized)).Normalize();
			for (int i = 0; i < 1; i++)
			{
				Vector3 val3 = endLine;
				Ray val4 = default(Ray);
				((Ray)(ref val4)).origin = ((Ray)(ref aimRay)).origin;
				((Ray)(ref val4)).direction = val3 - ((Ray)(ref aimRay)).origin;
				Debug.DrawLine(((Ray)(ref val4)).origin, val3, Color.red, 5f);
				if (Physics.Raycast(val4, ref val, 500f, LayerMask.op_Implicit(LayerIndex.world.mask)))
				{
					Vector3 point = ((RaycastHit)(ref val)).point;
					Quaternion val5 = Util.QuaternionSafeLookRotation(((RaycastHit)(ref val)).normal);
					GameObject obj = Object.Instantiate<GameObject>(delayPrefab, point, val5);
					DelayBlast component = obj.GetComponent<DelayBlast>();
					component.position = point;
					component.baseDamage = base.characterBody.damage * damageCoefficient;
					component.baseForce = 2000f;
					component.bonusForce = Vector3.up * 1000f;
					component.radius = radius;
					component.attacker = base.gameObject;
					component.inflictor = null;
					component.crit = Util.CheckRoll(critStat, base.characterBody.master);
					component.maxTimer = duration;
					obj.GetComponent<TeamFilter>().teamIndex = TeamComponent.GetObjectTeam(component.attacker);
					obj.transform.localScale = new Vector3(radius, radius, 1f);
					ScaleParticleSystemDuration component2 = obj.GetComponent<ScaleParticleSystemDuration>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						component2.newDuration = duration;
					}
				}
			}
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextState(new FireBomb());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
