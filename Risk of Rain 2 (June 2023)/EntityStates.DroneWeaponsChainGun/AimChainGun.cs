using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace EntityStates.DroneWeaponsChainGun;

public class AimChainGun : BaseDroneWeaponChainGunState
{
	[SerializeField]
	public float minDuration;

	[SerializeField]
	public float maxEnemyDistanceToStartFiring;

	[SerializeField]
	public float searchRefreshSeconds;

	private BullseyeSearch enemyFinder;

	private float searchRefreshTimer;

	public override void OnEnter()
	{
		base.OnEnter();
		if (base.isAuthority)
		{
			enemyFinder = new BullseyeSearch();
			enemyFinder.teamMaskFilter = TeamMask.allButNeutral;
			enemyFinder.maxDistanceFilter = maxEnemyDistanceToStartFiring;
			enemyFinder.maxAngleFilter = float.MaxValue;
			enemyFinder.filterByLoS = true;
			enemyFinder.sortMode = BullseyeSearch.SortMode.Angle;
			if (Object.op_Implicit((Object)(object)bodyTeamComponent))
			{
				enemyFinder.teamMaskFilter.RemoveTeam(bodyTeamComponent.teamIndex);
			}
		}
	}

	public override void FixedUpdate()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!base.isAuthority || !(base.fixedAge > minDuration))
		{
			return;
		}
		searchRefreshTimer -= Time.fixedDeltaTime;
		if (!(searchRefreshTimer < 0f))
		{
			return;
		}
		searchRefreshTimer = searchRefreshSeconds;
		Ray aimRay = GetAimRay();
		enemyFinder.searchOrigin = ((Ray)(ref aimRay)).origin;
		enemyFinder.searchDirection = ((Ray)(ref aimRay)).direction;
		enemyFinder.RefreshCandidates();
		using IEnumerator<HurtBox> enumerator = enemyFinder.GetResults().GetEnumerator();
		if (enumerator.MoveNext())
		{
			HurtBox current = enumerator.Current;
			outer.SetNextState(new FireChainGun(current));
		}
	}
}
