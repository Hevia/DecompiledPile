using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Gup;

public class BaseSplitDeath : GenericCharacterDeath
{
	[SerializeField]
	public CharacterSpawnCard characterSpawnCard;

	[SerializeField]
	public int spawnCount;

	[SerializeField]
	public float deathDelay;

	[SerializeField]
	public float moneyMultiplier;

	public static float spawnRadiusCoefficient = 0.5f;

	public static GameObject deathEffectPrefab;

	private bool hasDied;

	public override void OnEnter()
	{
		base.OnEnter();
	}

	public override void FixedUpdate()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!(base.fixedAge > deathDelay) || hasDied)
		{
			return;
		}
		hasDied = true;
		if (NetworkServer.active)
		{
			EffectManager.SpawnEffect(deathEffectPrefab, new EffectData
			{
				origin = base.characterBody.corePosition,
				scale = base.characterBody.radius
			}, transmit: true);
			if (Object.op_Implicit((Object)(object)characterSpawnCard) && spawnCount > 0 && (base.healthComponent.killingDamageType & (DamageType.VoidDeath | DamageType.OutOfBounds)) == 0)
			{
				BodySplitter bodySplitter = new BodySplitter();
				bodySplitter.body = base.characterBody;
				bodySplitter.masterSummon.masterPrefab = characterSpawnCard.prefab;
				bodySplitter.count = spawnCount;
				bodySplitter.splinterInitialVelocityLocal = new Vector3(0f, 20f, 10f);
				bodySplitter.minSpawnCircleRadius = base.characterBody.radius * spawnRadiusCoefficient;
				bodySplitter.moneyMultiplier = moneyMultiplier;
				bodySplitter.Perform();
			}
			DestroyBodyAsapServer();
		}
	}

	public override void OnExit()
	{
		DestroyModel();
		base.OnExit();
	}
}
