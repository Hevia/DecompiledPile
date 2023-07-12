using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class ArtifactDisplayPanelController : MonoBehaviour
{
	[Tooltip("The panel object that this component controls. Should usually a child, as this component manages the enabled/disabled status of the designated gameobject.")]
	public GameObject panelObject;

	public RectTransform iconContainer;

	public GameObject iconPrefab;

	private UIElementAllocator<RawImage> iconAllocator;

	public void SetDisplayData<T>(ref T enabledArtifacts) where T : IEnumerator<ArtifactDef>
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)panelObject))
		{
			return;
		}
		enabledArtifacts.Reset();
		int num = 0;
		while (enabledArtifacts.MoveNext())
		{
			num++;
		}
		panelObject.SetActive(num > 0);
		if (iconAllocator == null)
		{
			iconAllocator = new UIElementAllocator<RawImage>(iconContainer, iconPrefab);
		}
		iconAllocator.AllocateElements(num);
		int num2 = 0;
		enabledArtifacts.Reset();
		while (enabledArtifacts.MoveNext())
		{
			RawImage obj = iconAllocator.elements[num2++];
			ArtifactDef current = enabledArtifacts.Current;
			obj.texture = (Texture)(object)current.smallIconSelectedSprite.texture;
			TooltipProvider component = ((Component)obj).GetComponent<TooltipProvider>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.titleToken = current.nameToken;
				component.titleColor = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.Artifact));
				component.bodyToken = current.descriptionToken;
				component.bodyColor = Color.black;
			}
		}
	}
}
