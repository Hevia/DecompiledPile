using EntityStates;
using RoR2;
using UnityEngine;

namespace RandomSurvivors;

public class RSpecialAddBuff : BaseState
{
	private readonly float totalDuration = 0.5f;

	public override void OnEnter()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		((BaseState)this).OnEnter();
		EffectManager.SimpleEffect(Resources.Load<GameObject>("prefabs/effects/impacteffects/infusionorbflash"), ((EntityState)this).transform.position, ((EntityState)this).transform.rotation, true);
		if (((EntityState)this).GetComponent<RandomManager>().myAnim2 != null)
		{
			string myAnim = ((EntityState)this).GetComponent<RandomManager>().myAnim2;
			((EntityState)this).PlayAnimation("Gesture, Additive", myAnim, myAnim + ".playbackRate", totalDuration / base.attackSpeedStat);
			((EntityState)this).PlayAnimation("Gesture, Additive", myAnim, myAnim + ".playbackRate", totalDuration / base.attackSpeedStat);
		}
		if (((EntityState)this).isAuthority)
		{
			string[] array = new string[14]
			{
				"AffixBlue", "AffixPoison", "AffixRed", "AffixWhite", "CloakSpeed", "Energized", "FullCrit", "LoaderPylonPowered", "TeslaField", "TonicBuff",
				"WarCryBuff", "BugWings", "EngiShield", "EngiTeamShield"
			};
			Random.InitState(((EntityState)this).GetComponent<RandomManager>().mySeed + 1);
			float num = Random.Range(((EntityState)this).skillLocator.special.baseRechargeInterval * 0.25f, ((EntityState)this).skillLocator.special.baseRechargeInterval * 0.75f);
			string text = array[Random.Range(0, array.Length)];
			Util.PlaySound(((EntityState)this).sfxLocator.deathSound, ((EntityState)this).gameObject);
			((EntityState)this).characterBody.AddTimedBuff(BuffCatalog.FindBuffIndex(text), num);
			MonoBehaviour.print((object)("Applied " + text));
		}
	}

	public override void OnExit()
	{
		if (((EntityState)this).isAuthority)
		{
			((EntityState)this).OnExit();
		}
	}

	public override void FixedUpdate()
	{
		((EntityState)this).FixedUpdate();
		if (((EntityState)this).fixedAge >= totalDuration && ((EntityState)this).isAuthority)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)2;
	}
}
