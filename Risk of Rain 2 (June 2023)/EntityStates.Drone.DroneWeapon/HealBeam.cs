using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Drone.DroneWeapon;

public class HealBeam : BaseState
{
	public static float baseDuration;

	public static float healCoefficient = 5f;

	public static GameObject healBeamPrefab;

	public HurtBox target;

	private HealBeamController healBeamController;

	private float duration;

	private float lineWidthRefVelocity;

	public override void OnEnter()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayCrossfade("Gesture", "Heal", 0.2f);
		duration = baseDuration / attackSpeedStat;
		float healRate = healCoefficient * damageStat / duration;
		Ray aimRay = GetAimRay();
		Transform val = FindModelChild("Muzzle");
		if (NetworkServer.active)
		{
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.teamMaskFilter = TeamMask.none;
			if (Object.op_Implicit((Object)(object)base.teamComponent))
			{
				bullseyeSearch.teamMaskFilter.AddTeam(base.teamComponent.teamIndex);
			}
			bullseyeSearch.filterByLoS = false;
			bullseyeSearch.maxDistanceFilter = 50f;
			bullseyeSearch.maxAngleFilter = 180f;
			bullseyeSearch.searchOrigin = aimRay.origin;
			bullseyeSearch.searchDirection = aimRay.direction;
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
			bullseyeSearch.RefreshCandidates();
			bullseyeSearch.FilterOutGameObject(base.gameObject);
			target = bullseyeSearch.GetResults().FirstOrDefault();
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)target))
			{
				GameObject val2 = Object.Instantiate<GameObject>(healBeamPrefab, val);
				healBeamController = val2.GetComponent<HealBeamController>();
				healBeamController.healRate = healRate;
				healBeamController.target = target;
				healBeamController.ownership.ownerObject = base.gameObject;
				NetworkServer.Spawn(val2);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if ((base.fixedAge >= duration || !Object.op_Implicit((Object)(object)target)) && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		PlayCrossfade("Gesture", "Empty", 0.2f);
		if (Object.op_Implicit((Object)(object)healBeamController))
		{
			healBeamController.BreakServer();
		}
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Any;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		HurtBoxReference.FromHurtBox(target).Write(writer);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		HurtBoxReference hurtBoxReference = default(HurtBoxReference);
		hurtBoxReference.Read(reader);
		GameObject obj = hurtBoxReference.ResolveGameObject();
		target = ((obj != null) ? obj.GetComponent<HurtBox>() : null);
	}
}
