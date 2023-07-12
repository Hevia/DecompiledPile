using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.BrotherMonster;

public class TrueDeathState : GenericCharacterDeath
{
	public static float durationBeforeDissolving;

	public static float dissolveDuration;

	public static GameObject deathEffectPrefab;

	private bool dissolving;

	protected override bool shouldAutoDestroy => base.fixedAge > durationBeforeDissolving + dissolveDuration + 1f;

	public override void OnEnter()
	{
		base.OnEnter();
		if (NetworkServer.active)
		{
			Util.CleanseBody(base.characterBody, removeDebuffs: true, removeBuffs: true, removeCooldownBuffs: true, removeDots: true, removeStun: false, removeNearbyProjectiles: false);
			ReturnStolenItemsOnGettingHit component = GetComponent<ReturnStolenItemsOnGettingHit>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.itemStealController))
			{
				EntityState.Destroy((Object)(object)((Component)component.itemStealController).gameObject);
			}
		}
	}

	protected override void PlayDeathAnimation(float crossfadeDuration = 0.1f)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		PlayAnimation("FullBody Override", "TrueDeath");
		base.characterDirection.moveVector = base.characterDirection.forward;
		EffectManager.SimpleMuzzleFlash(deathEffectPrefab, base.gameObject, "MuzzleCenter", transmit: false);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= durationBeforeDissolving)
		{
			Dissolve();
		}
	}

	private void Dissolve()
	{
		if (!dissolving)
		{
			dissolving = true;
			Transform modelTransform = GetModelTransform();
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				((Component)modelTransform).gameObject.GetComponent<CharacterModel>();
				PrintController component = ((Component)base.modelLocator.modelTransform).gameObject.GetComponent<PrintController>();
				((Behaviour)component).enabled = false;
				component.printTime = dissolveDuration;
				((Behaviour)component).enabled = true;
			}
			Transform val = FindModelChild("TrueDeathEffect");
			if (Object.op_Implicit((Object)(object)val))
			{
				((Component)val).gameObject.SetActive(true);
				((Component)val).GetComponent<ScaleParticleSystemDuration>().newDuration = dissolveDuration;
			}
		}
	}
}
