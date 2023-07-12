using System.Collections.ObjectModel;
using RoR2;
using UnityEngine;

namespace EntityStates.AncientWispMonster;

public class ChannelRain : BaseState
{
	private float castTimer;

	public static float baseDuration = 4f;

	public static float explosionDelay = 2f;

	public static int explosionCount = 10;

	public static float damageCoefficient;

	public static float randomRadius;

	public static float radius;

	public static GameObject delayPrefab;

	private float duration;

	private float durationBetweenCast;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration;
		durationBetweenCast = baseDuration / (float)explosionCount / attackSpeedStat;
		PlayCrossfade("Body", "ChannelRain", 0.3f);
	}

	private void PlaceRain()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		Ray aimRay = GetAimRay();
		((Ray)(ref aimRay)).origin = ((Ray)(ref aimRay)).origin + Random.insideUnitSphere * randomRadius;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(aimRay, ref val2, (float)LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			val = ((RaycastHit)(ref val2)).point;
		}
		if (!(val != Vector3.zero))
		{
			return;
		}
		Transform val3 = FindTargetClosest(val, ((Component)base.characterBody).GetComponent<TeamComponent>().teamIndex switch
		{
			TeamIndex.Monster => TeamIndex.Player, 
			TeamIndex.Player => TeamIndex.Monster, 
			_ => TeamIndex.Neutral, 
		});
		Vector3 val4 = val;
		if (Object.op_Implicit((Object)(object)val3))
		{
			val4 = ((Component)val3).transform.position;
		}
		val4 += Random.insideUnitSphere * randomRadius;
		Ray val5 = default(Ray);
		((Ray)(ref val5)).origin = val4 + Vector3.up * randomRadius;
		((Ray)(ref val5)).direction = Vector3.down;
		if (Physics.Raycast(val5, ref val2, 500f, LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			Vector3 point = ((RaycastHit)(ref val2)).point;
			Quaternion val6 = Util.QuaternionSafeLookRotation(((RaycastHit)(ref val2)).normal);
			GameObject obj = Object.Instantiate<GameObject>(delayPrefab, point, val6);
			DelayBlast component = obj.GetComponent<DelayBlast>();
			component.position = point;
			component.baseDamage = base.characterBody.damage * damageCoefficient;
			component.baseForce = 2000f;
			component.bonusForce = Vector3.up * 1000f;
			component.radius = radius;
			component.attacker = base.gameObject;
			component.inflictor = null;
			component.crit = Util.CheckRoll(critStat, base.characterBody.master);
			component.maxTimer = explosionDelay;
			obj.GetComponent<TeamFilter>().teamIndex = TeamComponent.GetObjectTeam(component.attacker);
			obj.transform.localScale = new Vector3(radius, radius, 1f);
			ScaleParticleSystemDuration component2 = obj.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.newDuration = explosionDelay;
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		castTimer += Time.fixedDeltaTime;
		if (castTimer >= durationBetweenCast)
		{
			PlaceRain();
			castTimer -= durationBetweenCast;
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextState(new EndRain());
		}
	}

	private Transform FindTargetClosest(Vector3 point, TeamIndex enemyTeam)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(enemyTeam);
		float num = 99999f;
		Transform result = null;
		for (int i = 0; i < teamMembers.Count; i++)
		{
			float num2 = Vector3.SqrMagnitude(((Component)teamMembers[i]).transform.position - point);
			if (num2 < num)
			{
				num = num2;
				result = ((Component)teamMembers[i]).transform;
			}
		}
		return result;
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Frozen;
	}
}
