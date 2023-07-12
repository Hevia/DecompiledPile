using System.Collections.ObjectModel;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Engi.EngiWeapon;

public class EngiTeamShield : BaseState
{
	public static float duration = 3f;

	public static float radius;

	public override void OnEnter()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (!Object.op_Implicit((Object)(object)base.teamComponent) || !NetworkServer.active)
		{
			return;
		}
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(base.teamComponent.teamIndex);
		float num = radius * radius;
		Vector3 position = base.transform.position;
		for (int i = 0; i < teamMembers.Count; i++)
		{
			Vector3 val = ((Component)teamMembers[i]).transform.position - position;
			if (!(((Vector3)(ref val)).sqrMagnitude <= num))
			{
				continue;
			}
			CharacterBody component = ((Component)teamMembers[i]).GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.AddTimedBuff(JunkContent.Buffs.EngiTeamShield, duration);
				HealthComponent component2 = ((Component)component).GetComponent<HealthComponent>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					component2.RechargeShieldFull();
				}
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
