using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Drone.DroneWeapon;

public class StartHealBeam : BaseState
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public float targetSelectionRange;

	[SerializeField]
	public float healRateCoefficient;

	[SerializeField]
	public GameObject healBeamPrefab;

	[SerializeField]
	public string muzzleName;

	[SerializeField]
	public int maxSimultaneousBeams;

	private HurtBox targetHurtBox;

	private float duration;

	public override void OnEnter()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		if (base.isAuthority)
		{
			Ray aimRay = GetAimRay();
			targetHurtBox = FindTarget(aimRay);
		}
		if (NetworkServer.active && HealBeamController.GetHealBeamCountForOwner(base.gameObject) < maxSimultaneousBeams && Object.op_Implicit((Object)(object)targetHurtBox))
		{
			Transform val = FindModelChild(muzzleName);
			if (Object.op_Implicit((Object)(object)val))
			{
				GameObject obj = Object.Instantiate<GameObject>(healBeamPrefab, val);
				HealBeamController component = obj.GetComponent<HealBeamController>();
				component.healRate = healRateCoefficient * damageStat * attackSpeedStat;
				component.target = targetHurtBox;
				component.ownership.ownerObject = base.gameObject;
				obj.AddComponent<DestroyOnTimer>().duration = duration;
				NetworkServer.Spawn(obj);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private HurtBox FindTarget(Ray aimRay)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.teamMaskFilter = TeamMask.none;
		if (Object.op_Implicit((Object)(object)base.teamComponent))
		{
			bullseyeSearch.teamMaskFilter.AddTeam(base.teamComponent.teamIndex);
		}
		bullseyeSearch.filterByLoS = false;
		bullseyeSearch.maxDistanceFilter = targetSelectionRange;
		bullseyeSearch.maxAngleFilter = 180f;
		bullseyeSearch.searchOrigin = ((Ray)(ref aimRay)).origin;
		bullseyeSearch.searchDirection = ((Ray)(ref aimRay)).direction;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
		bullseyeSearch.RefreshCandidates();
		bullseyeSearch.FilterOutGameObject(base.gameObject);
		return bullseyeSearch.GetResults().Where(NotAlreadyHealingTarget).Where(IsHurt)
			.FirstOrDefault();
	}

	private bool NotAlreadyHealingTarget(HurtBox hurtBox)
	{
		return !HealBeamController.HealBeamAlreadyExists(base.gameObject, hurtBox);
	}

	private static bool IsHurt(HurtBox hurtBox)
	{
		if (hurtBox.healthComponent.alive)
		{
			return hurtBox.healthComponent.health < hurtBox.healthComponent.fullHealth;
		}
		return false;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write(HurtBoxReference.FromHurtBox(targetHurtBox));
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		targetHurtBox = reader.ReadHurtBoxReference().ResolveHurtBox();
	}
}
