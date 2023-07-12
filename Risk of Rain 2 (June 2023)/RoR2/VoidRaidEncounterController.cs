using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class VoidRaidEncounterController : NetworkBehaviour
{
	public GameObject VoidRaidBossPrefab;

	public Transform VoidRaidBossSpawnPosition;

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = Object.Instantiate<GameObject>(VoidRaidBossPrefab, VoidRaidBossSpawnPosition.position, Quaternion.identity);
		obj.GetComponent<CharacterMaster>().teamIndex = TeamIndex.Monster;
		NetworkServer.Spawn(obj);
		obj.GetComponent<CharacterMaster>().SpawnBodyHere();
	}

	private void Update()
	{
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
