using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

public class CustomScrollbar : MPScrollbar
{
	private Vector3 originalPosition;

	public bool scaleButtonOnHover = true;

	public bool showImageOnHover;

	public Image imageOnHover;

	public Image imageOnInteractable;

	private bool hovering;

	private float imageOnHoverAlphaVelocity;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		((UIBehaviour)this).Start();
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected I4, but got Unknown
		((Selectable)this).DoStateTransition(state, instant);
		switch ((int)state)
		{
		case 0:
			hovering = false;
			break;
		case 1:
			Util.PlaySound("Play_UI_menuHover", ((Component)RoR2Application.instance).gameObject);
			hovering = true;
			break;
		case 2:
			hovering = true;
			break;
		case 4:
			hovering = false;
			break;
		case 3:
			break;
		}
	}

	public void OnClickCustom()
	{
		Util.PlaySound("Play_UI_menuClick", ((Component)RoR2Application.instance).gameObject);
	}

	private void LateUpdate()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isPlaying)
		{
			if (showImageOnHover)
			{
				float num = (hovering ? 1f : 0f);
				Color color = ((Graphic)imageOnHover).color;
				float num2 = Mathf.SmoothDamp(color.a, num, ref imageOnHoverAlphaVelocity, 0.03f, 100f, Time.unscaledDeltaTime);
				Color color2 = default(Color);
				((Color)(ref color2))._002Ector(color.r, color.g, color.g, num2);
				((Graphic)imageOnHover).color = color2;
			}
			if (Object.op_Implicit((Object)(object)imageOnInteractable))
			{
				((Behaviour)imageOnInteractable).enabled = ((Selectable)this).interactable;
			}
		}
	}
}
