using System.Linq;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Huntress.HuntressWeapon;

public class IdleTracking : BaseState
{
	public static float maxTrackingDistance = 20f;

	public static float maxTrackingAngle = 20f;

	public static float orbDamageCoefficient;

	public static float orbProcCoefficient;

	public static string muzzleString;

	public static GameObject muzzleflashEffectPrefab;

	public static string attackSoundString;

	public static float fireFrequency;

	private float fireTimer;

	private Transform trackingIndicatorTransform;

	private HurtBox trackingTarget;

	private ChildLocator childLocator;

	public override void OnEnter()
	{
		base.OnEnter();
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)trackingIndicatorTransform))
		{
			EntityState.Destroy((Object)(object)((Component)trackingIndicatorTransform).gameObject);
		}
		base.OnExit();
	}

	private void FireOrbArrow()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			HuntressArrowOrb huntressArrowOrb = new HuntressArrowOrb();
			huntressArrowOrb.damageValue = base.characterBody.damage * orbDamageCoefficient;
			huntressArrowOrb.isCrit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master);
			huntressArrowOrb.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
			huntressArrowOrb.attacker = base.gameObject;
			huntressArrowOrb.damageColorIndex = DamageColorIndex.Poison;
			huntressArrowOrb.procChainMask.AddProc(ProcType.HealOnHit);
			huntressArrowOrb.procCoefficient = orbProcCoefficient;
			HurtBox hurtBox = trackingTarget;
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				Transform val = ((Component)childLocator.FindChild(muzzleString)).transform;
				EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleString, transmit: true);
				huntressArrowOrb.origin = val.position;
				huntressArrowOrb.target = hurtBox;
				PlayAnimation("Gesture, Override", "FireSeekingArrow");
				PlayAnimation("Gesture, Additive", "FireSeekingArrow");
				Util.PlaySound(attackSoundString, base.gameObject);
				OrbManager.instance.AddOrb(huntressArrowOrb);
			}
		}
	}

	public override void FixedUpdate()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!base.isAuthority)
		{
			return;
		}
		fireTimer -= Time.fixedDeltaTime;
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			float extraRaycastDistance = 0f;
			Ray val = CameraRigController.ModifyAimRayIfApplicable(GetAimRay(), base.gameObject, out extraRaycastDistance);
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.searchOrigin = ((Ray)(ref val)).origin;
			bullseyeSearch.searchDirection = ((Ray)(ref val)).direction;
			bullseyeSearch.maxDistanceFilter = maxTrackingDistance + extraRaycastDistance;
			bullseyeSearch.maxAngleFilter = maxTrackingAngle;
			bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
			bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(base.gameObject));
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
			bullseyeSearch.RefreshCandidates();
			trackingTarget = bullseyeSearch.GetResults().FirstOrDefault();
		}
		if (Object.op_Implicit((Object)(object)trackingTarget))
		{
			if (!Object.op_Implicit((Object)(object)trackingIndicatorTransform))
			{
				trackingIndicatorTransform = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/ShieldTransferIndicator"), ((Component)trackingTarget).transform.position, Quaternion.identity).transform;
			}
			trackingIndicatorTransform.position = ((Component)trackingTarget).transform.position;
			if (Object.op_Implicit((Object)(object)base.inputBank) && base.inputBank.skill1.down && fireTimer <= 0f)
			{
				fireTimer = 1f / fireFrequency / attackSpeedStat;
				FireOrbArrow();
			}
		}
		else if (Object.op_Implicit((Object)(object)trackingIndicatorTransform))
		{
			EntityState.Destroy((Object)(object)((Component)trackingIndicatorTransform).gameObject);
			trackingIndicatorTransform = null;
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Any;
	}
}
