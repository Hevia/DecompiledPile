using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.TitanMonster;

public class RechargeRocks : BaseState
{
	public static float baseDuration = 3f;

	public static float baseRechargeDuration = 2f;

	public static GameObject effectPrefab;

	public static string attackSoundString;

	public static GameObject rockControllerPrefab;

	private int rocksFired;

	private float duration;

	private float stopwatch;

	private float rechargeStopwatch;

	private GameObject chargeEffect;

	public override void OnEnter()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		stopwatch = 0f;
		duration = baseDuration / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		Util.PlaySound(attackSoundString, base.gameObject);
		PlayCrossfade("Body", "RechargeRocks", "RechargeRocks.playbackRate", duration, 0.2f);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild("LeftFist");
				if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)effectPrefab))
				{
					chargeEffect = Object.Instantiate<GameObject>(effectPrefab, val.position, val.rotation);
					chargeEffect.transform.parent = val;
					ScaleParticleSystemDuration component2 = chargeEffect.GetComponent<ScaleParticleSystemDuration>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						component2.newDuration = duration;
					}
				}
			}
		}
		if (NetworkServer.active)
		{
			GameObject obj = Object.Instantiate<GameObject>(rockControllerPrefab);
			obj.GetComponent<TitanRockController>().SetOwner(base.gameObject);
			NetworkServer.Spawn(obj);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (Object.op_Implicit((Object)(object)chargeEffect))
		{
			EntityState.Destroy((Object)(object)chargeEffect);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
