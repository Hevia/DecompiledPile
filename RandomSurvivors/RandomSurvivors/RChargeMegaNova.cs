using EntityStates;
using EntityStates.VagrantMonster;
using RoR2;
using UnityEngine;

namespace RandomSurvivors;

public class RChargeMegaNova : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject chargingEffectPrefab = ChargeMegaNova.chargingEffectPrefab;

	public static GameObject areaIndicatorPrefab = ChargeMegaNova.areaIndicatorPrefab;

	public static string chargingSoundString = ChargeMegaNova.chargingSoundString;

	public static float novaRadius = ChargeMegaNova.novaRadius;

	private float duration;

	private float stopwatch;

	private GameObject chargeEffectInstance;

	private GameObject areaIndicatorInstance;

	private uint soundID;

	public override void OnEnter()
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		((BaseState)this).OnEnter();
		stopwatch = 0f;
		duration = baseDuration / base.attackSpeedStat;
		GenericCharacterMain.ApplyJumpVelocity(((EntityState)this).characterMotor, ((EntityState)this).characterBody, 1f, 1f, false);
		Transform modelTransform = ((EntityState)this).GetModelTransform();
		Util.PlaySound(((EntityState)this).sfxLocator.deathSound, ((EntityState)this).gameObject);
		soundID = Util.PlayAttackSpeedSound(chargingSoundString, ((EntityState)this).gameObject, base.attackSpeedStat);
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			if (Object.op_Implicit((Object)(object)chargingEffectPrefab))
			{
				chargeEffectInstance = Object.Instantiate<GameObject>(chargingEffectPrefab, ((EntityState)this).transform.position, ((EntityState)this).transform.rotation);
				chargeEffectInstance.transform.localScale = new Vector3(novaRadius, novaRadius, novaRadius);
				chargeEffectInstance.transform.parent = ((EntityState)this).transform;
				chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
			}
			if (Object.op_Implicit((Object)(object)areaIndicatorPrefab))
			{
				areaIndicatorInstance = Object.Instantiate<GameObject>(areaIndicatorPrefab, ((EntityState)this).transform.position, ((EntityState)this).transform.rotation);
				areaIndicatorInstance.transform.localScale = new Vector3(novaRadius * 2f, novaRadius * 2f, novaRadius * 2f);
				areaIndicatorInstance.transform.parent = ((EntityState)this).transform;
			}
		}
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
		if (Object.op_Implicit((Object)(object)chargeEffectInstance))
		{
			EntityState.Destroy((Object)(object)chargeEffectInstance);
		}
		if (Object.op_Implicit((Object)(object)areaIndicatorInstance))
		{
			EntityState.Destroy((Object)(object)areaIndicatorInstance);
		}
	}

	public override void FixedUpdate()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		((EntityState)this).FixedUpdate();
		((EntityState)this).characterMotor.velocity = Vector3.up * 4f;
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && ((EntityState)this).isAuthority)
		{
			RFireMegaNova rFireMegaNova = new RFireMegaNova();
			rFireMegaNova.novaRadius = novaRadius;
			((EntityState)this).characterMotor.velocity = Vector3.up * 10f;
			((EntityState)this).outer.SetNextState((EntityState)(object)rFireMegaNova);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)4;
	}
}
