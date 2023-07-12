using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Engi.EngiWeapon;

public class EngiSelfShield : BaseState
{
	public static float transferDelay = 0.1f;

	private HurtBox transferTarget;

	private BullseyeSearch friendLocator;

	private Indicator indicator;

	public override void OnEnter()
	{
		base.OnEnter();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.EngiShield);
			base.characterBody.RecalculateStats();
			if (Object.op_Implicit((Object)(object)base.healthComponent))
			{
				base.healthComponent.RechargeShieldFull();
			}
		}
		friendLocator = new BullseyeSearch();
		friendLocator.teamMaskFilter = TeamMask.none;
		if (Object.op_Implicit((Object)(object)base.teamComponent))
		{
			friendLocator.teamMaskFilter.AddTeam(base.teamComponent.teamIndex);
		}
		friendLocator.maxDistanceFilter = 80f;
		friendLocator.maxAngleFilter = 20f;
		friendLocator.sortMode = BullseyeSearch.SortMode.Angle;
		friendLocator.filterByLoS = false;
		indicator = new Indicator(base.gameObject, LegacyResourcesAPI.Load<GameObject>("Prefabs/ShieldTransferIndicator"));
	}

	public override void OnExit()
	{
		base.skillLocator.utility = base.skillLocator.FindSkill("RetractShield");
		if (NetworkServer.active)
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.EngiShield);
		}
		if (base.isAuthority)
		{
			base.skillLocator.utility.RemoveAllStocks();
		}
		indicator.active = false;
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!base.isAuthority || !(base.fixedAge >= transferDelay) || !base.skillLocator.utility.IsReady())
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			float extraRaycastDistance = 0f;
			Ray val = CameraRigController.ModifyAimRayIfApplicable(GetAimRay(), base.gameObject, out extraRaycastDistance);
			friendLocator.searchOrigin = ((Ray)(ref val)).origin;
			friendLocator.searchDirection = ((Ray)(ref val)).direction;
			friendLocator.maxDistanceFilter += extraRaycastDistance;
			friendLocator.RefreshCandidates();
			friendLocator.FilterOutGameObject(base.gameObject);
			transferTarget = friendLocator.GetResults().FirstOrDefault();
		}
		HealthComponent healthComponent = (Object.op_Implicit((Object)(object)transferTarget) ? transferTarget.healthComponent : null);
		if (Object.op_Implicit((Object)(object)healthComponent))
		{
			indicator.targetTransform = Util.GetCoreTransform(((Component)healthComponent).gameObject);
			if (Object.op_Implicit((Object)(object)base.inputBank) && base.inputBank.skill3.justPressed)
			{
				EngiOtherShield engiOtherShield = new EngiOtherShield();
				engiOtherShield.target = ((Component)healthComponent).gameObject.GetComponent<CharacterBody>();
				outer.SetNextState(engiOtherShield);
				return;
			}
		}
		else
		{
			indicator.targetTransform = null;
		}
		indicator.active = Object.op_Implicit((Object)(object)indicator.targetTransform);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
