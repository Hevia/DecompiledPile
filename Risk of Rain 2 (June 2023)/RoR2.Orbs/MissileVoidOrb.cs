using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RoR2.Orbs;

public class MissileVoidOrb : GenericDamageOrb
{
	public override void Begin()
	{
		speed = 75f;
		base.Begin();
	}

	protected override GameObject GetOrbEffect()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return Addressables.LoadAssetAsync<GameObject>((object)"RoR2/DLC1/MissileVoid/MissileVoidOrbEffect.prefab").WaitForCompletion();
	}
}
