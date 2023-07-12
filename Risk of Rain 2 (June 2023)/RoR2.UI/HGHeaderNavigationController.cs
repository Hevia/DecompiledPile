using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RoR2.UI;

public class HGHeaderNavigationController : MonoBehaviour
{
	[Serializable]
	public struct Header
	{
		public MPButton headerButton;

		public string headerName;

		public TextMeshProUGUI tmpHeaderText;

		public GameObject headerRoot;
	}

	[FormerlySerializedAs("buttonSelectionRoot")]
	[Header("Header Parameters")]
	public GameObject headerHighlightObject;

	public int currentHeaderIndex;

	public Color selectedTextColor = Color.white;

	public Color unselectedTextColor = Color.gray;

	public bool makeSelectedHeaderButtonNoninteractable;

	[Header("Header Infos")]
	public Header[] headers;

	private void Start()
	{
		RebuildHeaders();
	}

	private void LateUpdate()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < headers.Length; i++)
		{
			Header header = headers[i];
			if (i == currentHeaderIndex)
			{
				((Graphic)header.tmpHeaderText).color = selectedTextColor;
			}
			else
			{
				((Graphic)header.tmpHeaderText).color = unselectedTextColor;
			}
		}
	}

	public void ChooseHeader(string headerName)
	{
		for (int i = 0; i < headers.Length; i++)
		{
			if (headers[i].headerName == headerName)
			{
				currentHeaderIndex = i;
				RebuildHeaders();
				break;
			}
		}
	}

	public void ChooseHeaderByButton(MPButton mpButton)
	{
		for (int i = 0; i < headers.Length; i++)
		{
			if ((Object)(object)headers[i].headerButton == (Object)(object)mpButton)
			{
				currentHeaderIndex = i;
				RebuildHeaders();
				break;
			}
		}
	}

	public void MoveHeaderLeft()
	{
		currentHeaderIndex = Mathf.Max(0, currentHeaderIndex - 1);
		Util.PlaySound("Play_UI_menuClick", ((Component)RoR2Application.instance).gameObject);
		RebuildHeaders();
	}

	public void MoveHeaderRight()
	{
		currentHeaderIndex = Mathf.Min(headers.Length - 1, currentHeaderIndex + 1);
		Util.PlaySound("Play_UI_menuClick", ((Component)RoR2Application.instance).gameObject);
		RebuildHeaders();
	}

	private void RebuildHeaders()
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < headers.Length; i++)
		{
			Header header = headers[i];
			if (i == currentHeaderIndex)
			{
				if (Object.op_Implicit((Object)(object)header.headerRoot))
				{
					header.headerRoot.SetActive(true);
				}
				if (Object.op_Implicit((Object)(object)header.headerButton))
				{
					if (makeSelectedHeaderButtonNoninteractable)
					{
						((Selectable)header.headerButton).interactable = false;
					}
					if (Object.op_Implicit((Object)(object)headerHighlightObject))
					{
						headerHighlightObject.transform.SetParent(((Component)header.headerButton).transform, false);
						headerHighlightObject.SetActive(false);
						headerHighlightObject.SetActive(true);
						RectTransform component = headerHighlightObject.GetComponent<RectTransform>();
						component.offsetMin = Vector2.zero;
						component.offsetMax = Vector2.zero;
						((Transform)component).localScale = Vector3.one;
					}
				}
			}
			else
			{
				if (Object.op_Implicit((Object)(object)header.headerButton) && makeSelectedHeaderButtonNoninteractable)
				{
					((Selectable)header.headerButton).interactable = true;
				}
				if (Object.op_Implicit((Object)(object)header.headerRoot))
				{
					header.headerRoot.SetActive(false);
				}
			}
		}
	}

	private Header GetCurrentHeader()
	{
		return headers[currentHeaderIndex];
	}
}
