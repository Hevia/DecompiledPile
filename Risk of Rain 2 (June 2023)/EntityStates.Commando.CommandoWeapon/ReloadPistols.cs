using UnityEngine;

namespace EntityStates.Commando.CommandoWeapon;

public class ReloadPistols : GenericReload
{
	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Gesture, Override", "ReloadPistols", "ReloadPistols.playbackRate", duration);
		PlayAnimation("Gesture, Additive", "ReloadPistols", "ReloadPistols.playbackRate", duration);
		Transform obj = FindModelChild("GunMeshL");
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		Transform obj2 = FindModelChild("GunMeshR");
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(false);
		}
	}

	public override void OnExit()
	{
		Transform obj = FindModelChild("ReloadFXL");
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		Transform obj2 = FindModelChild("ReloadFXR");
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(false);
		}
		Transform obj3 = FindModelChild("GunMeshL");
		if (obj3 != null)
		{
			((Component)obj3).gameObject.SetActive(true);
		}
		Transform obj4 = FindModelChild("GunMeshR");
		if (obj4 != null)
		{
			((Component)obj4).gameObject.SetActive(true);
		}
		PlayAnimation("Gesture, Override", "ReloadPistolsExit");
		PlayAnimation("Gesture, Additive", "ReloadPistolsExit");
		base.OnExit();
	}
}
