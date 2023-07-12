using RoR2;
using UnityEngine;

namespace EntityStates.Missions.BrotherEncounter;

public class Phase1 : BrotherEncounterPhaseBaseState
{
	public static string prespawnSoundString;

	public static float prespawnSoundDelay;

	public static GameObject centerOrbDestroyEffect;

	private bool hasPlayedPrespawnSound;

	protected override string phaseControllerChildString => "Phase1";

	protected override EntityState nextState => new Phase2();

	public override void OnEnter()
	{
		KillAllMonsters();
		base.OnEnter();
	}

	protected override void PreEncounterBegin()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		base.PreEncounterBegin();
		Transform val = childLocator.FindChild("CenterOrbEffect");
		((Component)val).gameObject.SetActive(false);
		EffectManager.SpawnEffect(centerOrbDestroyEffect, new EffectData
		{
			origin = ((Component)val).transform.position
		}, transmit: false);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!hasPlayedPrespawnSound && base.fixedAge > prespawnSoundDelay)
		{
			Transform val = childLocator.FindChild("CenterOrbEffect");
			Util.PlaySound(prespawnSoundString, ((Component)val).gameObject);
			hasPlayedPrespawnSound = true;
		}
	}

	protected override void OnMemberAddedServer(CharacterMaster master)
	{
		base.OnMemberAddedServer(master);
	}
}
