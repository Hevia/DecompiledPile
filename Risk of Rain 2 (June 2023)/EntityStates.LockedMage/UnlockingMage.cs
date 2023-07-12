using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.LockedMage;

public class UnlockingMage : BaseState
{
	public static GameObject unlockingMageChargeEffectPrefab;

	public static GameObject unlockingMageExplosionEffectPrefab;

	public static float unlockingDuration;

	public static string unlockingChargeSFXString;

	public static float unlockingChargeSFXStringPitch;

	public static string unlockingExplosionSFXString;

	public static float unlockingExplosionSFXStringPitch;

	private bool unlocked;

	public static event Action<Interactor> onOpened;

	public override void OnEnter()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		EffectManager.SimpleEffect(unlockingMageChargeEffectPrefab, base.transform.position, Util.QuaternionSafeLookRotation(Vector3.up), transmit: false);
		Util.PlayAttackSpeedSound(unlockingChargeSFXString, base.gameObject, unlockingChargeSFXStringPitch);
		((Component)((Component)GetModelTransform()).GetComponent<ChildLocator>().FindChild("Suspension")).gameObject.SetActive(false);
	}

	public override void FixedUpdate()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.fixedAge >= unlockingDuration && !unlocked)
		{
			base.gameObject.SetActive(false);
			EffectManager.SimpleEffect(unlockingMageExplosionEffectPrefab, base.transform.position, Util.QuaternionSafeLookRotation(Vector3.up), transmit: false);
			Util.PlayAttackSpeedSound(unlockingExplosionSFXString, base.gameObject, unlockingExplosionSFXStringPitch);
			unlocked = true;
			if (NetworkServer.active)
			{
				UnlockingMage.onOpened?.Invoke(GetComponent<PurchaseInteraction>().lastActivator);
			}
		}
	}
}
