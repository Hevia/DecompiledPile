using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Huntress.HuntressWeapon;

public class ChargeArrow : BaseState
{
	public static float baseTotalDuration;

	public static float baseMaxChargeTime;

	public static int maxCharges;

	public static GameObject chargeEffectPrefab;

	public static GameObject muzzleflashEffectPrefab;

	public static string chargeStockSoundString;

	public static string chargeLoopStartSoundString;

	public static string chargeLoopStopSoundString;

	public static float minBonusBloom;

	public static float maxBonusBloom;

	public static float minArrowDamageCoefficient;

	public static float maxArrowDamageCoefficient;

	public static float orbDamageCoefficient;

	public static float orbRange;

	public static float orbFrequency;

	public static float orbProcCoefficient;

	private float stopwatch;

	private GameObject chargeLeftInstance;

	private GameObject chargeRightInstance;

	private Animator animator;

	private int charge;

	private int lastCharge;

	private ChildLocator childLocator;

	private float totalDuration;

	private float maxChargeTime;

	private bool cachedSprinting;

	private float originalMinYaw;

	private float originalMaxYaw;

	private string muzzleString;

	public override void OnEnter()
	{
		base.OnEnter();
		totalDuration = baseTotalDuration / attackSpeedStat;
		maxChargeTime = baseMaxChargeTime / attackSpeedStat;
		muzzleString = "Muzzle";
		Transform modelTransform = GetModelTransform();
		childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		animator = GetModelAnimator();
		cachedSprinting = base.characterBody.isSprinting;
		if (!cachedSprinting)
		{
			animator.SetBool("chargingArrow", true);
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(maxChargeTime + 1f);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		animator.SetBool("chargingArrow", false);
		if (!cachedSprinting)
		{
			PlayAnimation("Gesture, Override", "BufferEmpty");
			PlayAnimation("Gesture, Additive", "BufferEmpty");
		}
	}

	private void FireOrbArrow()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
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
			Ray aimRay = GetAimRay();
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.searchOrigin = ((Ray)(ref aimRay)).origin;
			bullseyeSearch.searchDirection = ((Ray)(ref aimRay)).direction;
			bullseyeSearch.maxDistanceFilter = orbRange;
			bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
			bullseyeSearch.teamMaskFilter.RemoveTeam(huntressArrowOrb.teamIndex);
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
			bullseyeSearch.RefreshCandidates();
			List<HurtBox> list = bullseyeSearch.GetResults().ToList();
			HurtBox hurtBox = ((list.Count > 0) ? list[Random.Range(0, list.Count)] : null);
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				Transform val = ((Component)childLocator.FindChild(muzzleString)).transform;
				EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleString, transmit: true);
				huntressArrowOrb.origin = val.position;
				huntressArrowOrb.target = hurtBox;
				PlayAnimation("Gesture, Override", "FireSeekingArrow");
				PlayAnimation("Gesture, Additive", "FireSeekingArrow");
				OrbManager.instance.AddOrb(huntressArrowOrb);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (cachedSprinting != base.characterBody.isSprinting && base.isAuthority)
		{
			Debug.Log((object)"switched states");
			outer.SetNextStateToMain();
		}
		else if (!cachedSprinting)
		{
			lastCharge = charge;
			stopwatch += Time.fixedDeltaTime;
			charge = Mathf.Min((int)(stopwatch / maxChargeTime * (float)maxCharges), maxCharges);
			float damageCoefficient = Mathf.Lerp(minArrowDamageCoefficient, maxArrowDamageCoefficient, (float)charge);
			if (lastCharge < charge && charge == maxCharges)
			{
				EffectManager.SimpleMuzzleFlash(chargeEffectPrefab, base.gameObject, muzzleString, transmit: false);
			}
			if ((stopwatch >= totalDuration || !Object.op_Implicit((Object)(object)base.inputBank) || !base.inputBank.skill1.down) && base.isAuthority)
			{
				FireArrow fireArrow = new FireArrow();
				fireArrow.damageCoefficient = damageCoefficient;
				outer.SetNextState(fireArrow);
			}
		}
		else
		{
			stopwatch += Time.fixedDeltaTime;
			if (stopwatch >= 1f / orbFrequency / attackSpeedStat)
			{
				stopwatch -= 1f / orbFrequency / attackSpeedStat;
				FireOrbArrow();
			}
			if ((!Object.op_Implicit((Object)(object)base.inputBank) || !base.inputBank.skill1.down) && base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
