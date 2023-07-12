using System;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(EffectComponent))]
public class CoinBehavior : MonoBehaviour
{
	[Serializable]
	public struct CoinTier
	{
		public ParticleSystem particleSystem;

		public int valuePerCoin;
	}

	public int originalCoinCount;

	public CoinTier[] coinTiers;

	private void Start()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		originalCoinCount = (int)((Component)this).GetComponent<EffectComponent>().effectData.genericFloat;
		int num = originalCoinCount;
		for (int i = 0; i < coinTiers.Length; i++)
		{
			CoinTier coinTier = coinTiers[i];
			int num2 = 0;
			while (num >= coinTier.valuePerCoin)
			{
				num -= coinTier.valuePerCoin;
				num2++;
			}
			if (num2 > 0)
			{
				EmissionModule emission = coinTier.particleSystem.emission;
				((EmissionModule)(ref emission)).enabled = true;
				((EmissionModule)(ref emission)).SetBursts((Burst[])(object)new Burst[1]
				{
					new Burst(0f, MinMaxCurve.op_Implicit((float)num2))
				});
				((Component)coinTier.particleSystem).gameObject.SetActive(true);
			}
		}
	}
}
