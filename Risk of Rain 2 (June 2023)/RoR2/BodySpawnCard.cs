using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/SpawnCards/BodySpawnCard")]
public class BodySpawnCard : SpawnCard
{
	protected override void Spawn(Vector3 position, Quaternion rotation, DirectorSpawnRequest directorSpawnRequest, ref SpawnResult result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = position;
		val.y += Util.GetBodyPrefabFootOffset(prefab);
		GameObject val2 = Object.Instantiate<GameObject>(prefab, val, rotation);
		NetworkServer.Spawn(val2);
		result.spawnedInstance = val2;
		result.success = true;
	}
}
