using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI.SkinControllers;

[RequireComponent(typeof(ScrollRect))]
public class ScrollRectSkinController : BaseSkinController
{
	private ScrollRect scrollRect;

	protected new void Awake()
	{
		scrollRect = ((Component)this).GetComponent<ScrollRect>();
		base.Awake();
	}

	protected override void OnSkinUI()
	{
		Image component = ((Component)this).GetComponent<Image>();
		if (Object.op_Implicit((Object)(object)component))
		{
			skinData.scrollRectStyle.backgroundPanelStyle.Apply(component);
		}
		if (Object.op_Implicit((Object)(object)scrollRect.verticalScrollbar))
		{
			SkinScrollbar(scrollRect.verticalScrollbar);
		}
		if (Object.op_Implicit((Object)(object)scrollRect.horizontalScrollbar))
		{
			SkinScrollbar(scrollRect.horizontalScrollbar);
		}
	}

	private void SkinScrollbar(Scrollbar scrollbar)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		skinData.scrollRectStyle.scrollbarBackgroundStyle.Apply(((Component)scrollbar).GetComponent<Image>());
		((Selectable)scrollbar).colors = skinData.scrollRectStyle.scrollbarHandleColors;
		((Component)scrollbar.handleRect).GetComponent<Image>().sprite = skinData.scrollRectStyle.scrollbarHandleImage;
	}
}
