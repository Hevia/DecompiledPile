using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/SpawnCards/InteractableSpawnCard")]
public class InteractableSpawnCard : SpawnCard
{
	[Tooltip("Whether or not to orient the object to the normal of the ground it spawns on.")]
	public bool orientToFloor;

	[Tooltip("Slightly tweaks the rotation for things like chests and barrels so it looks more natural.")]
	public bool slightlyRandomizeOrientation;

	public bool skipSpawnWhenSacrificeArtifactEnabled;

	[Tooltip("When Sacrifice is enabled, this is multiplied by the card's weight")]
	public float weightScalarWhenSacrificeArtifactEnabled = 1f;

	[Tooltip("Won't spawn more than this many per stage.  If it's negative, there's no cap")]
	public int maxSpawnsPerStage = -1;

	private static readonly float floorOffset = 3f;

	private static readonly float raycastLength = 6f;

	private static readonly Xoroshiro128Plus rng = new Xoroshiro128Plus(0uL);

	protected override void Spawn(Vector3 position, Quaternion rotation, DirectorSpawnRequest directorSpawnRequest, ref SpawnResult result)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		ulong nextUlong = directorSpawnRequest.rng.nextUlong;
		rng.ResetSeed(nextUlong);
		if (skipSpawnWhenSacrificeArtifactEnabled && RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.sacrificeArtifactDef))
		{
			return;
		}
		GameObject val = Object.Instantiate<GameObject>(prefab, position, rotation);
		Transform transform = val.transform;
		if (orientToFloor)
		{
			Vector3 up = val.transform.up;
			RaycastHit val2 = default(RaycastHit);
			if (Physics.Raycast(new Ray(position + up * floorOffset, -up), ref val2, raycastLength + floorOffset, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				transform.up = ((RaycastHit)(ref val2)).normal;
			}
		}
		transform.Rotate(Vector3.up, rng.RangeFloat(0f, 360f), (Space)1);
		if (slightlyRandomizeOrientation)
		{
			transform.Translate(Vector3.down * 0.3f, (Space)1);
			transform.rotation *= Quaternion.Euler(rng.RangeFloat(-30f, 30f), rng.RangeFloat(-30f, 30f), rng.RangeFloat(-30f, 30f));
		}
		NetworkServer.Spawn(val);
		result.spawnedInstance = val;
		result.success = true;
	}
}
