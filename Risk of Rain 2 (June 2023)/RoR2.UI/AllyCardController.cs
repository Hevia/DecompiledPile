using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(LayoutElement))]
[RequireComponent(typeof(RectTransform))]
public class AllyCardController : MonoBehaviour
{
	public HealthBar healthBar;

	public TextMeshProUGUI nameLabel;

	public RawImage portraitIconImage;

	public bool shouldIndent { get; set; }

	public CharacterMaster sourceMaster { get; set; }

	public RectTransform rectTransform { get; private set; }

	public LayoutElement layoutElement { get; private set; }

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		rectTransform = (RectTransform)((Component)this).transform;
		layoutElement = ((Component)this).GetComponent<LayoutElement>();
	}

	private void LateUpdate()
	{
		UpdateInfo();
	}

	private void UpdateInfo()
	{
		HealthComponent source = null;
		string text = "";
		Texture val = null;
		if (Object.op_Implicit((Object)(object)sourceMaster))
		{
			CharacterBody body = sourceMaster.GetBody();
			if (Object.op_Implicit((Object)(object)body))
			{
				val = body.portraitIcon;
				source = body.healthComponent;
				text = Util.GetBestBodyName(((Component)body).gameObject);
			}
			else
			{
				text = Util.GetBestMasterName(sourceMaster);
			}
		}
		healthBar.source = source;
		((TMP_Text)nameLabel).text = text;
		portraitIconImage.texture = val;
		((Behaviour)portraitIconImage).enabled = (Object)(object)val != (Object)null;
	}
}
