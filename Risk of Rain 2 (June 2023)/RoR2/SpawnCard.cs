using System;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/SpawnCards/SpawnCard")]
public class SpawnCard : ScriptableObject
{
	public struct SpawnResult
	{
		public GameObject spawnedInstance;

		public DirectorSpawnRequest spawnRequest;

		public Vector3 position;

		public Quaternion rotation;

		public bool success;
	}

	public enum EliteRules
	{
		Default,
		ArtifactOnly,
		Lunar
	}

	public GameObject prefab;

	public bool sendOverNetwork;

	public HullClassification hullSize;

	public MapNodeGroup.GraphType nodeGraphType;

	[EnumMask(typeof(NodeFlags))]
	public NodeFlags requiredFlags;

	[EnumMask(typeof(NodeFlags))]
	public NodeFlags forbiddenFlags;

	public int directorCreditCost;

	public bool occupyPosition;

	[Tooltip("Default = default rules, ArtifactOnly = only elite when forced by the elite-only artifact, Lunar = special lunar elites only (+ regular w/ elite-only artifact)")]
	public EliteRules eliteRules;

	public static event Action<SpawnResult> onSpawnedServerGlobal;

	protected virtual void Spawn(Vector3 position, Quaternion rotation, DirectorSpawnRequest spawnRequest, ref SpawnResult spawnResult)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(prefab, position, rotation);
		if (sendOverNetwork)
		{
			NetworkServer.Spawn(val);
		}
		spawnResult.spawnedInstance = val;
		spawnResult.success = true;
	}

	public SpawnResult DoSpawn(Vector3 position, Quaternion rotation, DirectorSpawnRequest spawnRequest)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		SpawnResult spawnResult = default(SpawnResult);
		spawnResult.spawnRequest = spawnRequest;
		spawnResult.position = position;
		spawnResult.rotation = rotation;
		SpawnResult spawnResult2 = spawnResult;
		Spawn(position, rotation, spawnRequest, ref spawnResult2);
		spawnRequest.onSpawnedServer?.Invoke(spawnResult2);
		SpawnCard.onSpawnedServerGlobal?.Invoke(spawnResult2);
		return spawnResult2;
	}
}
