using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.MiniMushroom;

public class Plant : BaseState
{
	public static float healFraction;

	public static float baseMaxDuration;

	public static float baseMinDuration;

	public static float mushroomRadius;

	public static string healSoundLoop;

	public static string healSoundStop;

	private float maxDuration;

	private float minDuration;

	private GameObject mushroomWard;

	private uint soundID;

	public override void OnEnter()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayAnimation("Plant", "PlantLoop");
		maxDuration = baseMaxDuration / attackSpeedStat;
		minDuration = baseMinDuration / attackSpeedStat;
		soundID = Util.PlaySound(healSoundLoop, ((Component)base.characterBody.modelLocator.modelTransform).gameObject);
		if (NetworkServer.active && (Object)(object)mushroomWard == (Object)null)
		{
			mushroomWard = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/MiniMushroomWard"), base.characterBody.footPosition, Quaternion.identity);
			mushroomWard.GetComponent<TeamFilter>().teamIndex = base.teamComponent.teamIndex;
			if (Object.op_Implicit((Object)(object)mushroomWard))
			{
				HealingWard component = mushroomWard.GetComponent<HealingWard>();
				component.healFraction = healFraction;
				component.healPoints = 0f;
				component.Networkradius = mushroomRadius;
			}
			NetworkServer.Spawn(mushroomWard);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			bool flag = ((Vector3)(ref base.inputBank.moveVector)).sqrMagnitude > 0.1f;
			if (base.fixedAge > maxDuration || (base.fixedAge > minDuration && flag))
			{
				outer.SetNextState(new UnPlant());
			}
		}
	}

	public override void OnExit()
	{
		PlayAnimation("Plant", "Empty");
		AkSoundEngine.StopPlayingID(soundID);
		Util.PlaySound(healSoundStop, base.gameObject);
		if (Object.op_Implicit((Object)(object)mushroomWard))
		{
			EntityState.Destroy((Object)(object)mushroomWard);
		}
		base.OnExit();
	}
}
