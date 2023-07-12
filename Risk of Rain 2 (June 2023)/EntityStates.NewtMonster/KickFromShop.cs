using RoR2;
using UnityEngine;

namespace EntityStates.NewtMonster;

public class KickFromShop : BaseState
{
	public static float duration = 3.5f;

	public static string attackSoundString;

	public static string stompSoundString;

	public static GameObject chargeEffectPrefab;

	public static GameObject stompEffectPrefab;

	private Animator modelAnimator;

	private Transform modelTransform;

	private bool hasAttacked;

	private GameObject chargeEffectInstance;

	public override void OnEnter()
	{
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		modelTransform = GetModelTransform();
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
		PlayCrossfade("Body", "Stomp", "Stomp.playbackRate", duration, 0.1f);
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild("StompMuzzle");
			if (Object.op_Implicit((Object)(object)val))
			{
				chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val);
			}
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)chargeEffectInstance))
		{
			EntityState.Destroy((Object)(object)chargeEffectInstance);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("Stomp.hitBoxActive") > 0.5f && !hasAttacked)
		{
			Util.PlayAttackSpeedSound(stompSoundString, base.gameObject, attackSpeedStat);
			EffectManager.SimpleMuzzleFlash(stompEffectPrefab, base.gameObject, "HealthBarOrigin", transmit: false);
			if (Object.op_Implicit((Object)(object)SceneInfo.instance))
			{
				GameObject val = ((Component)((Component)SceneInfo.instance).transform.Find("KickOutOfShop")).gameObject;
				if (Object.op_Implicit((Object)(object)val))
				{
					val.gameObject.SetActive(true);
				}
			}
			if (base.isAuthority)
			{
				HurtBoxGroup component = ((Component)modelTransform).GetComponent<HurtBoxGroup>();
				if (Object.op_Implicit((Object)(object)component))
				{
					int hurtBoxesDeactivatorCounter = component.hurtBoxesDeactivatorCounter + 1;
					component.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
				}
			}
			hasAttacked = true;
			EntityState.Destroy((Object)(object)chargeEffectInstance);
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
