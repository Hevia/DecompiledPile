using System;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class CursorIndicatorController : MonoBehaviour
{
	public enum CursorImage
	{
		None,
		Pointer,
		Hover
	}

	[Serializable]
	public struct CursorSet
	{
		public GameObject pointerObject;

		public GameObject hoverObject;

		public GameObject GetGameObject(CursorImage cursorImage)
		{
			return (GameObject)(cursorImage switch
			{
				CursorImage.None => null, 
				CursorImage.Pointer => pointerObject, 
				CursorImage.Hover => hoverObject, 
				_ => null, 
			});
		}
	}

	[NonSerialized]
	public CursorSet noneCursorSet;

	public CursorSet mouseCursorSet;

	public CursorSet gamepadCursorSet;

	private GameObject currentChildIndicator;

	public RectTransform containerTransform;

	private Color cachedIndicatorColor = Color.clear;

	public void SetCursor(CursorSet cursorSet, CursorImage cursorImage, Color color)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		GameObject gameObject = cursorSet.GetGameObject(cursorImage);
		bool flag = color != cachedIndicatorColor;
		if ((Object)(object)gameObject != (Object)(object)currentChildIndicator)
		{
			if (Object.op_Implicit((Object)(object)currentChildIndicator))
			{
				currentChildIndicator.SetActive(false);
			}
			currentChildIndicator = gameObject;
			if (Object.op_Implicit((Object)(object)currentChildIndicator))
			{
				currentChildIndicator.SetActive(true);
			}
			flag = true;
		}
		if (flag)
		{
			cachedIndicatorColor = color;
			ApplyIndicatorColor();
		}
	}

	private void ApplyIndicatorColor()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)currentChildIndicator))
		{
			Image[] componentsInChildren = currentChildIndicator.GetComponentsInChildren<Image>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Graphic)componentsInChildren[i]).color = cachedIndicatorColor;
			}
		}
	}

	public void SetPosition(Vector2 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((Transform)containerTransform).position = Vector2.op_Implicit(position);
	}
}
