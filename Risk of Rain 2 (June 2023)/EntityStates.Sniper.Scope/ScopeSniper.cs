using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Sniper.Scope;

public class ScopeSniper : BaseState
{
	public static float baseChargeDuration = 4f;

	public static GameObject crosshairPrefab;

	public float charge;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	private GameObject laserPointerObject;

	private CameraTargetParams.AimRequest aimRequest;

	public override void OnEnter()
	{
		base.OnEnter();
		charge = 0f;
		if (NetworkServer.active && Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.Slow50);
		}
		if (Object.op_Implicit((Object)(object)base.cameraTargetParams))
		{
			aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.FirstPerson);
			base.cameraTargetParams.fovOverride = 20f;
		}
		if (Object.op_Implicit((Object)(object)crosshairPrefab))
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairPrefab, CrosshairUtils.OverridePriority.Skill);
		}
		laserPointerObject = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/LaserPointerBeamEnd"));
		laserPointerObject.GetComponent<LaserPointerController>().source = base.inputBank;
	}

	public override void OnExit()
	{
		EntityState.Destroy((Object)(object)laserPointerObject);
		if (NetworkServer.active && Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.Slow50);
		}
		aimRequest?.Dispose();
		if (Object.op_Implicit((Object)(object)base.cameraTargetParams))
		{
			base.cameraTargetParams.fovOverride = -1f;
		}
		crosshairOverrideRequest?.Dispose();
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		charge = Mathf.Min(charge + attackSpeedStat / baseChargeDuration * Time.fixedDeltaTime, 1f);
		if (base.isAuthority && (!Object.op_Implicit((Object)(object)base.inputBank) || !base.inputBank.skill2.down))
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
