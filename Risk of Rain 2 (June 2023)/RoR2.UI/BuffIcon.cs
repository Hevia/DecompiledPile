using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class BuffIcon : MonoBehaviour
{
	private BuffDef lastBuffDef;

	public BuffDef buffDef;

	public Image iconImage;

	public TextMeshProUGUI stackCount;

	public int buffCount;

	private float stopwatch;

	private const float flashDuration = 0.25f;

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	public RectTransform rectTransform { get; private set; }

	private void Awake()
	{
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		UpdateIcon();
	}

	public void Flash()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)iconImage).color = Color.white;
		((Graphic)iconImage).CrossFadeColor(buffDef.buffColor, 0.25f, true, false);
	}

	public void UpdateIcon()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)buffDef))
		{
			iconImage.sprite = buffDef.iconSprite;
			((Graphic)iconImage).color = buffDef.buffColor;
			if (buffDef.canStack)
			{
				sharedStringBuilder.Clear();
				sharedStringBuilder.Append("x");
				sharedStringBuilder.AppendInt(buffCount);
				((Behaviour)stackCount).enabled = true;
				((TMP_Text)stackCount).SetText(sharedStringBuilder);
			}
			else
			{
				((Behaviour)stackCount).enabled = false;
			}
		}
		else
		{
			iconImage.sprite = null;
		}
	}

	private void Update()
	{
		if ((Object)(object)lastBuffDef != (Object)(object)buffDef)
		{
			lastBuffDef = buffDef;
		}
	}
}
