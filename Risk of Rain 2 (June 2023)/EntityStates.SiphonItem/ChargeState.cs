using EntityStates.VagrantMonster;
using RoR2;
using UnityEngine;

namespace EntityStates.SiphonItem;

public class ChargeState : BaseSiphonItemState
{
	public static float baseDuration = 3f;

	public static string chargeSound;

	private float duration;

	private GameObject chargeVfxInstance;

	private GameObject areaIndicatorVfxInstance;

	public override void OnEnter()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / (Object.op_Implicit((Object)(object)base.attachedBody) ? base.attachedBody.attackSpeed : 1f);
		TurnOffHealingFX();
		if (Object.op_Implicit((Object)(object)base.attachedBody))
		{
			Vector3 position = base.transform.position;
			Quaternion rotation = base.transform.rotation;
			chargeVfxInstance = Object.Instantiate<GameObject>(ChargeMegaNova.chargingEffectPrefab, position, rotation);
			chargeVfxInstance.transform.localScale = Vector3.one * 0.25f;
			Util.PlaySound(chargeSound, base.gameObject);
			areaIndicatorVfxInstance = Object.Instantiate<GameObject>(ChargeMegaNova.areaIndicatorPrefab, position, rotation);
			ObjectScaleCurve component = areaIndicatorVfxInstance.GetComponent<ObjectScaleCurve>();
			component.timeMax = duration;
			component.baseScale = Vector3.one * DetonateState.baseSiphonRange * 2f;
			areaIndicatorVfxInstance.GetComponent<AnimateShaderAlpha>().timeMax = duration;
		}
		RoR2Application.onLateUpdate += OnLateUpdate;
	}

	public override void OnExit()
	{
		RoR2Application.onLateUpdate -= OnLateUpdate;
		if (chargeVfxInstance != null)
		{
			EntityState.Destroy((Object)(object)chargeVfxInstance);
			chargeVfxInstance = null;
		}
		if (areaIndicatorVfxInstance != null)
		{
			EntityState.Destroy((Object)(object)areaIndicatorVfxInstance);
			areaIndicatorVfxInstance = null;
		}
		base.OnExit();
	}

	private void OnLateUpdate()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)chargeVfxInstance))
		{
			chargeVfxInstance.transform.position = base.transform.position;
		}
		if (Object.op_Implicit((Object)(object)areaIndicatorVfxInstance))
		{
			areaIndicatorVfxInstance.transform.position = base.transform.position;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextState(new DetonateState());
		}
	}
}
