using RoR2.Orbs;
using UnityEngine;

namespace RoR2;

public class PaladinBarrierController : MonoBehaviour, IBarrier
{
	public float blockLaserDamageCoefficient;

	public float blockLaserProcCoefficient;

	public float blockLaserDistance;

	private float totalDamageBlocked;

	private CharacterBody characterBody;

	private InputBankTest inputBank;

	private TeamComponent teamComponent;

	private bool barrierIsOn;

	public Transform barrierPivotTransform;

	public void BlockedDamage(DamageInfo damageInfo, float actualDamageBlocked)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		totalDamageBlocked += actualDamageBlocked;
		LightningOrb lightningOrb = new LightningOrb();
		lightningOrb.teamIndex = teamComponent.teamIndex;
		lightningOrb.origin = damageInfo.position;
		lightningOrb.damageValue = actualDamageBlocked * blockLaserDamageCoefficient;
		lightningOrb.bouncesRemaining = 0;
		lightningOrb.attacker = damageInfo.attacker;
		lightningOrb.procCoefficient = blockLaserProcCoefficient;
		lightningOrb.lightningType = LightningOrb.LightningType.TreePoisonDart;
		HurtBox hurtBox = lightningOrb.PickNextTarget(lightningOrb.origin);
		if (Object.op_Implicit((Object)(object)hurtBox))
		{
			lightningOrb.target = hurtBox;
			lightningOrb.isCrit = Util.CheckRoll(characterBody.crit, characterBody.master);
			OrbManager.instance.AddOrb(lightningOrb);
		}
	}

	public void EnableBarrier()
	{
		((Component)barrierPivotTransform).gameObject.SetActive(true);
		barrierIsOn = true;
	}

	public void DisableBarrier()
	{
		((Component)barrierPivotTransform).gameObject.SetActive(false);
		barrierIsOn = false;
	}

	private void Start()
	{
		inputBank = ((Component)this).GetComponent<InputBankTest>();
		characterBody = ((Component)this).GetComponent<CharacterBody>();
		teamComponent = ((Component)this).GetComponent<TeamComponent>();
		DisableBarrier();
	}

	private void Update()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (barrierIsOn)
		{
			barrierPivotTransform.rotation = Util.QuaternionSafeLookRotation(inputBank.aimDirection);
		}
	}
}
