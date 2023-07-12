using UnityEngine;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/SpawnCards/MultiCharacterSpawnCard")]
public class MultiCharacterSpawnCard : CharacterSpawnCard
{
	public GameObject[] masterPrefabs;

	protected override void Spawn(Vector3 position, Quaternion rotation, DirectorSpawnRequest directorSpawnRequest, ref SpawnResult result)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		prefab = masterPrefabs[(int)(directorSpawnRequest.rng.nextNormalizedFloat * (float)masterPrefabs.Length)];
		base.Spawn(position, rotation, directorSpawnRequest, ref result);
	}
}
