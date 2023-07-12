using UnityEngine;

namespace RoR2;

public class LevelUpEffectManager
{
	private static float levelUpEffectInterval = 0.25f;

	private static float levelUpEffectJitter = 0.25f;

	private static int pendingLevelUpEffects = 0;

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		GlobalEventManager.onTeamLevelUp += OnTeamLevelUp;
		GlobalEventManager.onCharacterLevelUp += OnCharacterLevelUp;
		Run.onRunAmbientLevelUp += OnRunAmbientLevelUp;
	}

	private static void OnTeamLevelUp(TeamIndex teamIndex)
	{
		if (TeamComponent.GetTeamMembers(teamIndex).Count > 0)
		{
			Util.PlaySound(TeamCatalog.GetTeamDef(teamIndex)?.levelUpSound, ((Component)RoR2Application.instance).gameObject);
		}
	}

	private static void OnCharacterLevelUp(CharacterBody characterBody)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		GameObject levelUpEffect = TeamCatalog.GetTeamDef(characterBody.teamComponent.teamIndex)?.levelUpEffect;
		if (!Object.op_Implicit((Object)(object)characterBody))
		{
			return;
		}
		Transform val = (Object.op_Implicit((Object)(object)characterBody.mainHurtBox) ? ((Component)characterBody.mainHurtBox).transform : ((Component)characterBody).transform);
		EffectData effectData = new EffectData
		{
			origin = val.position
		};
		if (Object.op_Implicit((Object)(object)characterBody.mainHurtBox))
		{
			effectData.SetHurtBoxReference(((Component)characterBody).gameObject);
			effectData.scale = characterBody.radius;
		}
		RoR2Application.fixedTimeTimers.CreateTimer((float)pendingLevelUpEffects * levelUpEffectInterval + Random.value * levelUpEffectJitter, delegate
		{
			if (Object.op_Implicit((Object)(object)characterBody))
			{
				EffectManager.SpawnEffect(levelUpEffect, effectData, transmit: false);
			}
			levelUpEffectInterval -= 1f;
		});
		levelUpEffectInterval += 1f;
	}

	private static void OnRunAmbientLevelUp(Run run)
	{
		Util.PlaySound("Play_UI_levelUp_enemy", ((Component)RoR2Application.instance).gameObject);
	}
}
