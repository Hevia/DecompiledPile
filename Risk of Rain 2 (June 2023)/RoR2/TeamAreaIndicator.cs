using System;
using UnityEngine;

namespace RoR2;

public class TeamAreaIndicator : MonoBehaviour
{
	[Serializable]
	public struct TeamMaterialPair
	{
		public TeamIndex teamIndex;

		public Material sharedMaterial;
	}

	public TeamComponent teamComponent;

	public TeamFilter teamFilter;

	public TeamMaterialPair[] teamMaterialPairs;

	public Renderer[] areaIndicatorRenderers;

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)teamFilter) || Object.op_Implicit((Object)(object)teamComponent))
		{
			TeamIndex teamIndex = (Object.op_Implicit((Object)(object)teamFilter) ? teamFilter.teamIndex : (Object.op_Implicit((Object)(object)teamComponent) ? teamComponent.teamIndex : TeamIndex.None));
			for (int i = 0; i < teamMaterialPairs.Length; i++)
			{
				if (teamMaterialPairs[i].teamIndex == teamIndex)
				{
					Renderer[] array = areaIndicatorRenderers;
					for (int j = 0; j < array.Length; j++)
					{
						array[j].sharedMaterial = teamMaterialPairs[i].sharedMaterial;
					}
				}
			}
		}
		else
		{
			Debug.LogWarning((object)"No TeamFilter or TeamComponent assigned to TeamAreaIndicator.");
			((Component)this).gameObject.SetActive(false);
		}
	}
}
