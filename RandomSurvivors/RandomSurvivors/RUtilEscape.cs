using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using UnityEngine;

namespace RandomSurvivors;

public class RUtilEscape : BaseState
{
	private float newDuration;

	public override void OnEnter()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		((BaseState)this).OnEnter();
		if (((EntityState)this).isAuthority)
		{
			Random.InitState(((EntityState)this).GetComponent<RandomManager>().mySeed + 1);
			newDuration = Random.Range(((EntityState)this).skillLocator.utility.baseRechargeInterval * 0.25f, ((EntityState)this).skillLocator.utility.baseRechargeInterval * 0.75f);
			newDuration = Mathf.Clamp(newDuration, 1f, 4f);
			Util.PlaySound(CastSmokescreenNoDelay.startCloakSoundString, ((EntityState)this).gameObject);
			EffectManager.SpawnEffect(CastSmokescreenNoDelay.smokescreenEffectPrefab, new EffectData
			{
				origin = ((EntityState)this).transform.position
			}, false);
			GenericCharacterMain.ApplyJumpVelocity(((EntityState)this).characterMotor, ((EntityState)this).characterBody, 1f, 1f, false);
			((EntityState)this).characterMotor.velocity = Vector3.up * 25f;
			ICharacterGravityParameterProvider component = ((EntityState)this).gameObject.GetComponent<ICharacterGravityParameterProvider>();
			ICharacterFlightParameterProvider component2 = ((EntityState)this).gameObject.GetComponent<ICharacterFlightParameterProvider>();
			if (component != null)
			{
				CharacterGravityParameters gravityParameters = component.gravityParameters;
				gravityParameters.channeledAntiGravityGranterCount++;
				component.gravityParameters = gravityParameters;
			}
			if (component2 != null)
			{
				CharacterFlightParameters flightParameters = component2.flightParameters;
				flightParameters.channeledFlightGranterCount++;
				component2.flightParameters = flightParameters;
			}
			Util.PlaySound(((EntityState)this).sfxLocator.barkSound, ((EntityState)this).gameObject);
			((EntityState)this).characterBody.AddTimedBuff(Buffs.Cloak, newDuration);
			((EntityState)this).characterBody.AddTimedBuff(Buffs.CloakSpeed, newDuration);
		}
	}

	public override void OnExit()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (((EntityState)this).isAuthority)
		{
			Util.PlaySound(CastSmokescreenNoDelay.stopCloakSoundString, ((EntityState)this).gameObject);
			ICharacterGravityParameterProvider component = ((EntityState)this).gameObject.GetComponent<ICharacterGravityParameterProvider>();
			ICharacterFlightParameterProvider component2 = ((EntityState)this).gameObject.GetComponent<ICharacterFlightParameterProvider>();
			if (component != null)
			{
				CharacterGravityParameters gravityParameters = component.gravityParameters;
				gravityParameters.channeledAntiGravityGranterCount--;
				component.gravityParameters = gravityParameters;
			}
			if (component2 != null)
			{
				CharacterFlightParameters flightParameters = component2.flightParameters;
				flightParameters.channeledFlightGranterCount--;
				component2.flightParameters = flightParameters;
			}
			((EntityState)this).OnExit();
		}
	}

	public override void FixedUpdate()
	{
		((EntityState)this).FixedUpdate();
		if (((EntityState)this).isAuthority && ((EntityState)this).fixedAge >= newDuration)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)4;
	}
}
