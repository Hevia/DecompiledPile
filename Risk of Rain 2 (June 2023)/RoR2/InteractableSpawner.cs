using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class InteractableSpawner : NetworkBehaviour
{
	[Tooltip("The selection of director cards to use when spawning stuff")]
	[SerializeField]
	private DirectorCardCategorySelection interactableCards;

	[Tooltip("How much stuff should this thing spawn")]
	[SerializeField]
	private float creditsToSpawn;

	private List<GameObject> spawnedObjects = new List<GameObject>();

	[Server]
	public void Spawn(Xoroshiro128Plus rng)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.InteractableSpawner::Spawn(Xoroshiro128Plus)' called on client");
			return;
		}
		WeightedSelection<DirectorCard> weightedSelection = interactableCards.GenerateDirectorCardWeightedSelection();
		float num = creditsToSpawn;
		while (num > 0f)
		{
			DirectorCard directorCard = weightedSelection.Evaluate(rng.nextNormalizedFloat);
			if (directorCard == null)
			{
				break;
			}
			if (!directorCard.IsAvailable())
			{
				num -= 1f;
				continue;
			}
			num -= (float)directorCard.cost;
			for (int i = 0; i < 10; i++)
			{
				DirectorPlacementRule placementRule = new DirectorPlacementRule
				{
					placementMode = DirectorPlacementRule.PlacementMode.Random
				};
				GameObject val = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(directorCard.spawnCard, placementRule, rng));
				if (Object.op_Implicit((Object)(object)val))
				{
					PurchaseInteraction component = val.GetComponent<PurchaseInteraction>();
					if (Object.op_Implicit((Object)(object)component) && component.costType == CostTypeIndex.Money)
					{
						component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
					}
					spawnedObjects.Add(val);
					break;
				}
			}
		}
	}

	[Server]
	public void DestroySpawnedInteractables()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.InteractableSpawner::DestroySpawnedInteractables()' called on client");
			return;
		}
		foreach (GameObject spawnedObject in spawnedObjects)
		{
			Object.Destroy((Object)(object)spawnedObject);
		}
		spawnedObjects.Clear();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
