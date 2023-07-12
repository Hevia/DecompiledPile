using RoR2;
using UnityEngine;

namespace EntityStates.Duplicator;

public class Duplicating : BaseState
{
	public static float initialDelayDuration = 1f;

	public static float timeBetweenStartAndDropDroplet = 3f;

	public static string muzzleString;

	public static GameObject bakeEffectPrefab;

	public static GameObject releaseEffectPrefab;

	private GameObject bakeEffectInstance;

	private bool hasStartedCooking;

	private bool hasDroppedDroplet;

	private Transform muzzleTransform;

	public override void OnEnter()
	{
		base.OnEnter();
		ChildLocator component = ((Component)GetModelTransform()).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			muzzleTransform = component.FindChild(muzzleString);
		}
	}

	private void BeginCooking()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (!hasStartedCooking)
		{
			hasStartedCooking = true;
			PlayAnimation("Body", "Cook");
			if (Object.op_Implicit((Object)(object)base.sfxLocator))
			{
				Util.PlaySound(base.sfxLocator.openSound, base.gameObject);
			}
			if (Object.op_Implicit((Object)(object)muzzleTransform))
			{
				bakeEffectInstance = Object.Instantiate<GameObject>(bakeEffectPrefab, muzzleTransform.position, muzzleTransform.rotation);
			}
		}
	}

	private void DropDroplet()
	{
		if (hasDroppedDroplet)
		{
			return;
		}
		hasDroppedDroplet = true;
		GetComponent<ShopTerminalBehavior>().DropPickup();
		if (Object.op_Implicit((Object)(object)muzzleTransform))
		{
			if (Object.op_Implicit((Object)(object)bakeEffectInstance))
			{
				EntityState.Destroy((Object)(object)bakeEffectInstance);
			}
			if (Object.op_Implicit((Object)(object)releaseEffectPrefab))
			{
				EffectManager.SimpleMuzzleFlash(releaseEffectPrefab, base.gameObject, muzzleString, transmit: false);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= initialDelayDuration)
		{
			BeginCooking();
		}
		if (base.fixedAge >= initialDelayDuration + timeBetweenStartAndDropDroplet)
		{
			DropDroplet();
		}
	}
}
