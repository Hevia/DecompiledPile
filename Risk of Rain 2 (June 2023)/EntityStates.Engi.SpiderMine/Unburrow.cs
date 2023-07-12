using System;
using System.Linq;
using RoR2;
using UnityEngine;

namespace EntityStates.Engi.SpiderMine;

public class Unburrow : BaseSpiderMineState
{
	public static float baseDuration;

	public static int betterTargetSearchLimit;

	private float duration;

	protected override bool shouldStick => true;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration;
		Util.PlaySound("Play_beetle_worker_idle", base.gameObject);
		PlayAnimation("Base", "ArmedToChase", "ArmedToChase.playbackRate", duration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!base.isAuthority)
		{
			return;
		}
		EntityState entityState = null;
		if (!base.projectileStickOnImpact.stuck)
		{
			entityState = new WaitForStick();
		}
		else if (Object.op_Implicit((Object)(object)base.projectileTargetComponent.target))
		{
			if (duration <= base.fixedAge)
			{
				FindBetterTarget(base.projectileTargetComponent.target);
				entityState = new ChaseTarget();
			}
		}
		else
		{
			entityState = new Burrow();
		}
		if (entityState != null)
		{
			outer.SetNextState(entityState);
		}
	}

	private BullseyeSearch CreateBullseyeSearch(Vector3 origin)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new BullseyeSearch
		{
			searchOrigin = origin,
			filterByDistinctEntity = true,
			maxDistanceFilter = Detonate.blastRadius,
			sortMode = BullseyeSearch.SortMode.Distance,
			maxAngleFilter = 360f,
			teamMaskFilter = TeamMask.GetEnemyTeams(base.projectileController.teamFilter.teamIndex)
		};
	}

	private void FindBetterTarget(Transform initialTarget)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		BullseyeSearch bullseyeSearch = CreateBullseyeSearch(initialTarget.position);
		bullseyeSearch.RefreshCandidates();
		HurtBox[] array = bullseyeSearch.GetResults().ToArray();
		int num = array.Length;
		int num2 = -1;
		int i = 0;
		for (int num3 = Math.Min(array.Length, betterTargetSearchLimit); i < num3; i++)
		{
			HurtBox hurtBox = array[i];
			int num4 = CountTargets(((Component)hurtBox).transform.position);
			if (num < num4)
			{
				num = num4;
				num2 = i;
			}
		}
		if (num2 != -1)
		{
			base.projectileTargetComponent.target = ((Component)array[num2]).transform;
		}
	}

	private int CountTargets(Vector3 origin)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		BullseyeSearch bullseyeSearch = CreateBullseyeSearch(origin);
		bullseyeSearch.RefreshCandidates();
		return bullseyeSearch.GetResults().Count();
	}
}
