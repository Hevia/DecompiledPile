using System.Linq;
using RoR2;
using UnityEngine;

namespace EntityStates.NullifierMonster;

public class AimPortalBomb : BaseState
{
	private HurtBox target;

	public static float baseDuration;

	public static float arcMultiplier;

	private float duration;

	private Vector3? pointA;

	private Vector3? pointB;

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	public override void OnEnter()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (base.isAuthority)
		{
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.viewer = base.characterBody;
			bullseyeSearch.searchOrigin = base.characterBody.corePosition;
			bullseyeSearch.searchDirection = base.characterBody.corePosition;
			bullseyeSearch.maxDistanceFilter = FirePortalBomb.maxDistance;
			bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
			bullseyeSearch.RefreshCandidates();
			target = bullseyeSearch.GetResults().FirstOrDefault();
			if (Object.op_Implicit((Object)(object)target))
			{
				pointA = RaycastToFloor(((Component)target).transform.position);
			}
		}
		duration = baseDuration;
	}

	public override void FixedUpdate()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!base.isAuthority || !(base.fixedAge >= duration))
		{
			return;
		}
		EntityState entityState = null;
		if (Object.op_Implicit((Object)(object)target))
		{
			pointB = RaycastToFloor(((Component)target).transform.position);
			if (pointA.HasValue && pointB.HasValue)
			{
				Ray aimRay = GetAimRay();
				Vector3 val = pointA.Value - ((Ray)(ref aimRay)).origin;
				Vector3 val2 = pointB.Value - ((Ray)(ref aimRay)).origin;
				Quaternion val3 = Quaternion.LookRotation(val);
				Quaternion val4 = Quaternion.LookRotation(val2);
				Quaternion value = val4;
				Quaternion value2 = Quaternion.SlerpUnclamped(val3, val4, 1f + arcMultiplier);
				entityState = new FirePortalBomb
				{
					startRotation = value,
					endRotation = value2
				};
			}
		}
		if (entityState != null)
		{
			outer.SetNextState(entityState);
		}
		else
		{
			outer.SetNextStateToMain();
		}
	}

	private Vector3? RaycastToFloor(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(new Ray(position, Vector3.down), ref val, 10f, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
		{
			return ((RaycastHit)(ref val)).point;
		}
		return null;
	}
}
