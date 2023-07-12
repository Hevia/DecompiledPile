using UnityEngine;

namespace RoR2;

public class DamageInfo
{
	public float damage;

	public bool crit;

	public GameObject inflictor;

	public GameObject attacker;

	public Vector3 position;

	public Vector3 force;

	public bool rejected;

	public ProcChainMask procChainMask;

	public float procCoefficient = 1f;

	public DamageType damageType;

	public DamageColorIndex damageColorIndex;

	public DotController.DotIndex dotIndex = DotController.DotIndex.None;

	public bool canRejectForce = true;

	public void ModifyDamageInfo(HurtBox.DamageModifier damageModifier)
	{
		switch (damageModifier)
		{
		case HurtBox.DamageModifier.Weak:
			damageType |= DamageType.WeakPointHit;
			break;
		case HurtBox.DamageModifier.Normal:
		case HurtBox.DamageModifier.SniperTarget:
			break;
		}
	}
}
