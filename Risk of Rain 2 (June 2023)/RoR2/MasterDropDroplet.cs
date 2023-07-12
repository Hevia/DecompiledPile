using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(CharacterMaster))]
public class MasterDropDroplet : MonoBehaviour
{
	private CharacterMaster characterMaster;

	public PickupDropTable[] dropTables;

	public ulong salt;

	[Header("Deprecated")]
	public SerializablePickupIndex[] pickupsToDrop;

	private Xoroshiro128Plus rng;

	private void Start()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		characterMaster = ((Component)this).GetComponent<CharacterMaster>();
		rng = new Xoroshiro128Plus(Run.instance.seed ^ salt);
	}

	public void DropItems()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		CharacterBody body = characterMaster.GetBody();
		if (!Object.op_Implicit((Object)(object)body))
		{
			return;
		}
		PickupDropTable[] array = dropTables;
		foreach (PickupDropTable pickupDropTable in array)
		{
			if (Object.op_Implicit((Object)(object)pickupDropTable))
			{
				PickupDropletController.CreatePickupDroplet(pickupDropTable.GenerateDrop(rng), body.coreTransform.position, new Vector3(Random.Range(-4f, 4f), 20f, Random.Range(-4f, 4f)));
			}
		}
		SerializablePickupIndex[] array2 = pickupsToDrop;
		for (int i = 0; i < array2.Length; i++)
		{
			PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(array2[i].pickupName), body.coreTransform.position, new Vector3(Random.Range(-4f, 4f), 20f, Random.Range(-4f, 4f)));
		}
	}
}
