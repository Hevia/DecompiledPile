using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.TeleporterHealNovaController;

public class TeleporterHealNovaPulse : BaseState
{
	private class HealPulse
	{
		private readonly List<HealthComponent> healedTargets = new List<HealthComponent>();

		private readonly SphereSearch sphereSearch;

		private float rate;

		private float t;

		private float finalRadius;

		private float healFractionValue;

		private TeamMask teamMask;

		private readonly List<HurtBox> hurtBoxesList = new List<HurtBox>();

		public bool isFinished => t >= 1f;

		public HealPulse(Vector3 origin, float finalRadius, float healFractionValue, float duration, TeamIndex teamIndex)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			sphereSearch = new SphereSearch
			{
				mask = LayerIndex.entityPrecise.mask,
				origin = origin,
				queryTriggerInteraction = (QueryTriggerInteraction)2,
				radius = 0f
			};
			this.finalRadius = finalRadius;
			this.healFractionValue = healFractionValue;
			rate = 1f / duration;
			teamMask = default(TeamMask);
			teamMask.AddTeam(teamIndex);
		}

		public void Update(float deltaTime)
		{
			t += rate * deltaTime;
			t = ((t > 1f) ? 1f : t);
			sphereSearch.radius = finalRadius * novaRadiusCurve.Evaluate(t);
			sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(teamMask).FilterCandidatesByDistinctHurtBoxEntities()
				.GetHurtBoxes(hurtBoxesList);
			int i = 0;
			for (int count = hurtBoxesList.Count; i < count; i++)
			{
				HealthComponent healthComponent = hurtBoxesList[i].healthComponent;
				if (!healedTargets.Contains(healthComponent))
				{
					healedTargets.Add(healthComponent);
					HealTarget(healthComponent);
				}
			}
			hurtBoxesList.Clear();
		}

		private void HealTarget(HealthComponent target)
		{
			target.HealFraction(healFractionValue, default(ProcChainMask));
			Util.PlaySound("Play_item_proc_TPhealingNova_hitPlayer", ((Component)target).gameObject);
		}
	}

	public static AnimationCurve novaRadiusCurve;

	public static float duration;

	private Transform effectTransform;

	private HealPulse healPulse;

	private float radius;

	public override void OnEnter()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.transform.parent))
		{
			HoldoutZoneController componentInParent = base.gameObject.GetComponentInParent<HoldoutZoneController>();
			if (Object.op_Implicit((Object)(object)componentInParent))
			{
				radius = componentInParent.currentRadius;
			}
		}
		TeamFilter component = GetComponent<TeamFilter>();
		TeamIndex teamIndex = (Object.op_Implicit((Object)(object)component) ? component.teamIndex : TeamIndex.None);
		if (NetworkServer.active)
		{
			healPulse = new HealPulse(base.transform.position, radius, 0.5f, duration, teamIndex);
		}
		effectTransform = base.transform.Find("PulseEffect");
		if (Object.op_Implicit((Object)(object)effectTransform))
		{
			((Component)effectTransform).gameObject.SetActive(true);
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)effectTransform))
		{
			((Component)effectTransform).gameObject.SetActive(false);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active)
		{
			healPulse.Update(Time.fixedDeltaTime);
			if (duration < base.fixedAge)
			{
				EntityState.Destroy((Object)(object)((Component)outer).gameObject);
			}
		}
	}

	public override void Update()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)effectTransform))
		{
			float num = radius * novaRadiusCurve.Evaluate(base.fixedAge / duration);
			effectTransform.localScale = new Vector3(num, num, num);
		}
	}
}
