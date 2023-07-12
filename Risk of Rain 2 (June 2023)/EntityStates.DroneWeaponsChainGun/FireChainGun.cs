using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace EntityStates.DroneWeaponsChainGun;

public class FireChainGun : BaseDroneWeaponChainGunState
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public GameObject orbEffectObject;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float orbSpeed;

	[SerializeField]
	public int shotCount;

	[SerializeField]
	public float procCoefficient;

	[SerializeField]
	public int additionalBounces;

	[SerializeField]
	public float bounceRange;

	[SerializeField]
	public float damageCoefficientPerBounce;

	[SerializeField]
	public int targetsToFindPerBounce;

	[SerializeField]
	public bool canBounceOnSameTarget;

	[SerializeField]
	public string muzzleName;

	[SerializeField]
	public GameObject muzzleFlashPrefab;

	[SerializeField]
	public string fireSoundString;

	private HurtBox targetHurtBox;

	private float duration;

	private int stepIndex;

	public FireChainGun()
	{
	}

	public FireChainGun(HurtBox targetHurtBox)
	{
		this.targetHurtBox = targetHurtBox;
	}

	public override void OnEnter()
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Transform val = FindChild(muzzleName);
		if (!Object.op_Implicit((Object)(object)val))
		{
			val = body.coreTransform;
		}
		if (base.isAuthority)
		{
			duration = baseDuration / body.attackSpeed;
			ChainGunOrb chainGunOrb = new ChainGunOrb(orbEffectObject);
			chainGunOrb.damageValue = body.damage * damageCoefficient;
			chainGunOrb.isCrit = Util.CheckRoll(body.crit, body.master);
			chainGunOrb.teamIndex = TeamComponent.GetObjectTeam(((Component)body).gameObject);
			chainGunOrb.attacker = ((Component)body).gameObject;
			chainGunOrb.procCoefficient = procCoefficient;
			chainGunOrb.procChainMask = default(ProcChainMask);
			chainGunOrb.origin = val.position;
			chainGunOrb.target = targetHurtBox;
			chainGunOrb.speed = orbSpeed;
			chainGunOrb.bouncesRemaining = additionalBounces;
			chainGunOrb.bounceRange = bounceRange;
			chainGunOrb.damageCoefficientPerBounce = damageCoefficientPerBounce;
			chainGunOrb.targetsToFindPerBounce = targetsToFindPerBounce;
			chainGunOrb.canBounceOnSameTarget = canBounceOnSameTarget;
			chainGunOrb.damageColorIndex = DamageColorIndex.Item;
			OrbManager.instance.AddOrb(chainGunOrb);
		}
		if (Object.op_Implicit((Object)(object)val))
		{
			EffectData effectData = new EffectData
			{
				origin = val.position
			};
			EffectManager.SpawnEffect(muzzleFlashPrefab, effectData, transmit: true);
		}
		Util.PlaySound(fireSoundString, base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge > duration)
		{
			BaseDroneWeaponChainGunState baseDroneWeaponChainGunState;
			if (stepIndex < shotCount)
			{
				baseDroneWeaponChainGunState = new FireChainGun(targetHurtBox);
				(baseDroneWeaponChainGunState as FireChainGun).stepIndex = stepIndex + 1;
			}
			else
			{
				baseDroneWeaponChainGunState = new AimChainGun();
			}
			baseDroneWeaponChainGunState.PassDisplayLinks(gunChildLocators, gunAnimators);
			outer.SetNextState(baseDroneWeaponChainGunState);
		}
	}
}
