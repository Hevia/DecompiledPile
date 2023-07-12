using TMPro;
using UnityEngine;

namespace RoR2.UI;

public class HGTextMeshProUGUI : TextMeshProUGUI
{
	public bool useLanguageDefaultFont = true;

	public static TMP_FontAsset defaultLanguageFont;

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		Language.onCurrentLanguageChanged += OnCurrentLanguageChanged;
		OnCurrentLanguageChanged();
	}

	private static void OnCurrentLanguageChanged()
	{
		defaultLanguageFont = LegacyResourcesAPI.Load<TMP_FontAsset>(Language.GetString("DEFAULT_FONT"));
	}

	protected override void Awake()
	{
		((TextMeshProUGUI)this).Awake();
		if (useLanguageDefaultFont)
		{
			((TMP_Text)this).font = defaultLanguageFont;
			((TextMeshProUGUI)this).UpdateFontAsset();
		}
	}
}
