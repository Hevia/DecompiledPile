using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.BeetleQueenMonster;

public class SpawnWards : BaseState
{
	public static string muzzleString;

	public static string attackSoundString;

	public static GameObject muzzleflashEffectPrefab;

	public static float baseDuration = 0.9f;

	public static float orbRange;

	public static float orbTravelSpeed;

	public static int orbCountMax;

	private float stopwatch;

	private int orbCount;

	private float duration;

	private bool hasFiredOrbs;

	private Animator animator;

	private ChildLocator childLocator;

	public override void OnEnter()
	{
		base.OnEnter();
		animator = GetModelAnimator();
		childLocator = ((Component)animator).GetComponent<ChildLocator>();
		duration = baseDuration / attackSpeedStat;
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
		PlayCrossfade("Gesture", "SpawnWards", "SpawnWards.playbackRate", duration, 0.5f);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (!hasFiredOrbs && animator.GetFloat("SpawnWards.active") > 0.5f)
		{
			hasFiredOrbs = true;
			FireOrbs();
		}
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private void FireOrbs()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			Transform val = ((Component)childLocator.FindChild(muzzleString)).transform;
			BullseyeSearch bullseyeSearch = new BullseyeSearch();
			bullseyeSearch.searchOrigin = val.position;
			bullseyeSearch.searchDirection = val.forward;
			bullseyeSearch.maxDistanceFilter = orbRange;
			bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
			bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(base.gameObject));
			bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
			bullseyeSearch.RefreshCandidates();
			EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleString, transmit: true);
			List<HurtBox> list = bullseyeSearch.GetResults().ToList();
			for (int i = 0; i < list.Count; i++)
			{
				HurtBox target = list[i];
				BeetleWardOrb beetleWardOrb = new BeetleWardOrb();
				beetleWardOrb.origin = val.position;
				beetleWardOrb.target = target;
				beetleWardOrb.speed = orbTravelSpeed;
				OrbManager.instance.AddOrb(beetleWardOrb);
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)base.cameraTargetParams))
		{
			base.cameraTargetParams.fovOverride = -1f;
		}
		int layerIndex = animator.GetLayerIndex("Impact");
		if (layerIndex >= 0)
		{
			animator.SetLayerWeight(layerIndex, 1.5f);
			animator.PlayInFixedTime("LightImpact", layerIndex, 0f);
		}
	}
}
