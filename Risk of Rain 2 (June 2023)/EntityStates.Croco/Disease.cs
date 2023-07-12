using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace EntityStates.Croco;

public class Disease : BaseState
{
	public static GameObject muzzleflashEffectPrefab;

	public static string muzzleString;

	public static float orbRange;

	public static float baseDuration;

	public static float damageCoefficient;

	public static int maxBounces;

	public static float bounceRange;

	public static float procCoefficient;

	private float duration;

	public override void OnEnter()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Ray aimRay = GetAimRay();
		Transform val = FindModelChild(muzzleString);
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.searchOrigin = ((Ray)(ref aimRay)).origin;
		bullseyeSearch.searchDirection = ((Ray)(ref aimRay)).direction;
		bullseyeSearch.maxDistanceFilter = orbRange;
		bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
		bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(base.gameObject));
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
		bullseyeSearch.RefreshCandidates();
		EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleString, transmit: true);
		List<HurtBox> list = bullseyeSearch.GetResults().ToList();
		if (list.Count > 0)
		{
			Debug.LogFormat("Shooting at {0}", new object[1] { list[0] });
			HurtBox target = list.FirstOrDefault();
			LightningOrb lightningOrb = new LightningOrb();
			lightningOrb.attacker = base.gameObject;
			lightningOrb.bouncedObjects = new List<HealthComponent>();
			lightningOrb.lightningType = LightningOrb.LightningType.CrocoDisease;
			lightningOrb.damageType = DamageType.PoisonOnHit;
			lightningOrb.damageValue = damageStat * damageCoefficient;
			lightningOrb.isCrit = RollCrit();
			lightningOrb.procCoefficient = procCoefficient;
			lightningOrb.bouncesRemaining = maxBounces;
			lightningOrb.origin = val.position;
			lightningOrb.target = target;
			lightningOrb.teamIndex = GetTeam();
			lightningOrb.range = bounceRange;
			OrbManager.instance.AddOrb(lightningOrb);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}
}
