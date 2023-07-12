using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RoR2;

public class VoidMegaCrabItemBehavior : CharacterBody.ItemBehavior
{
	private const float baseSecondsPerSpawn = 60f;

	private const int baseMaxAllies = 1;

	private const int maxAlliesPerStack = 1;

	private const float minSpawnDist = 3f;

	private const float maxSpawnDist = 40f;

	private float spawnTimer;

	private Xoroshiro128Plus rng;

	private WeightedSelection<CharacterSpawnCard> spawnSelection;

	private DirectorPlacementRule placementRule;

	public static int GetMaxProjectiles(Inventory inventory)
	{
		return 1 + (inventory.GetItemCount(DLC1Content.Items.VoidMegaCrabItem) - 1);
	}

	private void Awake()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)this).enabled = false;
		ulong num = Run.instance.seed ^ (ulong)Run.instance.stageClearCount;
		rng = new Xoroshiro128Plus(num);
		spawnSelection = new WeightedSelection<CharacterSpawnCard>();
		spawnSelection.AddChoice(Addressables.LoadAssetAsync<CharacterSpawnCard>((object)"RoR2/DLC1/VoidJailer/cscVoidJailerAlly.asset").WaitForCompletion(), 15f);
		spawnSelection.AddChoice(Addressables.LoadAssetAsync<CharacterSpawnCard>((object)"RoR2/Base/Nullifier/cscNullifierAlly.asset").WaitForCompletion(), 15f);
		spawnSelection.AddChoice(Addressables.LoadAssetAsync<CharacterSpawnCard>((object)"RoR2/DLC1/VoidMegaCrab/cscVoidMegaCrabAlly.asset").WaitForCompletion(), 1f);
		placementRule = new DirectorPlacementRule
		{
			placementMode = DirectorPlacementRule.PlacementMode.Approximate,
			minDistance = 3f,
			maxDistance = 40f,
			spawnOnTarget = ((Component)this).transform
		};
	}

	private void FixedUpdate()
	{
		spawnTimer += Time.fixedDeltaTime;
		if (!body.master.IsDeployableLimited(DeployableSlot.VoidMegaCrabItem) && spawnTimer > 60f / (float)stack)
		{
			spawnTimer = 0f;
			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnSelection.Evaluate(rng.nextNormalizedFloat), placementRule, rng);
			directorSpawnRequest.summonerBodyObject = ((Component)this).gameObject;
			directorSpawnRequest.onSpawnedServer = OnMasterSpawned;
			directorSpawnRequest.summonerBodyObject = ((Component)this).gameObject;
			DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
		}
	}

	private void OnMasterSpawned(SpawnCard.SpawnResult spawnResult)
	{
		GameObject spawnedInstance = spawnResult.spawnedInstance;
		if (!Object.op_Implicit((Object)(object)spawnedInstance))
		{
			return;
		}
		CharacterMaster component = spawnedInstance.GetComponent<CharacterMaster>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Deployable component2 = ((Component)component).GetComponent<Deployable>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				body.master.AddDeployable(component2, DeployableSlot.VoidMegaCrabItem);
			}
		}
	}
}
