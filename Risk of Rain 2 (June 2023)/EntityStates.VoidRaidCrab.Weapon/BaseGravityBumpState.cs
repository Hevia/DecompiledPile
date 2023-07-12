using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidRaidCrab.Weapon;

public abstract class BaseGravityBumpState : BaseState
{
	[SerializeField]
	public float maxDistance;

	protected Vector3 airborneForce;

	protected Vector3 groundedForce;

	protected bool isLeft;

	public override void OnSerialize(NetworkWriter writer)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		base.OnSerialize(writer);
		writer.Write(isLeft);
		writer.Write(groundedForce);
		writer.Write(airborneForce);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeserialize(reader);
		isLeft = reader.ReadBoolean();
		groundedForce = reader.ReadVector3();
		airborneForce = reader.ReadVector3();
	}

	public override void ModifyNextState(EntityState nextState)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		base.ModifyNextState(nextState);
		if (nextState is BaseGravityBumpState baseGravityBumpState)
		{
			baseGravityBumpState.groundedForce = groundedForce;
			baseGravityBumpState.airborneForce = airborneForce;
			baseGravityBumpState.isLeft = isLeft;
		}
	}

	protected IEnumerable<HurtBox> GetTargets()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.viewer = base.characterBody;
		bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(base.characterBody.teamComponent.teamIndex);
		bullseyeSearch.minDistanceFilter = 0f;
		bullseyeSearch.maxDistanceFilter = maxDistance;
		bullseyeSearch.searchOrigin = base.inputBank.aimOrigin;
		bullseyeSearch.searchDirection = base.inputBank.aimDirection;
		bullseyeSearch.maxAngleFilter = 360f;
		bullseyeSearch.filterByLoS = false;
		bullseyeSearch.filterByDistinctEntity = true;
		bullseyeSearch.RefreshCandidates();
		return bullseyeSearch.GetResults();
	}
}
