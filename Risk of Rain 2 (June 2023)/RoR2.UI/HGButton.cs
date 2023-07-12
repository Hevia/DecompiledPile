using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

public class HGButton : MPButton
{
	private TextMeshProUGUI textMeshProUGui;

	private float stopwatch;

	private Color originalColor;

	public bool showImageOnHover;

	public Image imageOnHover;

	public Image imageOnInteractable;

	public bool updateTextOnHover;

	public LanguageTextMeshController hoverLanguageTextMeshController;

	public string hoverToken;

	private bool hovering;

	private SelectionState previousState = (SelectionState)4;

	public string uiClickSoundOverride;

	private float buttonScaleVelocity;

	private float imageOnHoverAlphaVelocity;

	private float imageOnHoverScaleVelocity;

	protected override void Awake()
	{
		base.Awake();
		textMeshProUGui = ((Component)this).GetComponent<TextMeshProUGUI>();
	}

	protected override void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		((UIBehaviour)this).Start();
		((UnityEvent)((Button)this).onClick).AddListener(new UnityAction(OnClickCustom));
		if (updateTextOnHover && !Object.op_Implicit((Object)(object)hoverLanguageTextMeshController))
		{
			Debug.LogErrorFormat("HGButton \"{0}\" is missing an object assigned to its .hoverLangaugeTextMeshController field.", new object[1] { Util.GetGameObjectHierarchyName(((Component)this).gameObject) });
		}
	}

	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected I4, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		((Selectable)this).DoStateTransition(state, instant);
		if (previousState != state)
		{
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
			}
			previousState = state;
		}
		originalColor = ((Selectable)this).targetGraphic.color;
	}

	public void OnClickCustom()
	{
		Util.PlaySound((!string.IsNullOrEmpty(uiClickSoundOverride)) ? uiClickSoundOverride : "Play_UI_menuClick", ((Component)RoR2Application.instance).gameObject);
	}

	private void LateUpdate()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		stopwatch += Time.deltaTime;
		if (Application.isPlaying)
		{
			if (showImageOnHover)
			{
				float num = (hovering ? 1f : 0f);
				float num2 = (hovering ? 1f : 1.1f);
				Color color = ((Graphic)imageOnHover).color;
				float x = ((Component)imageOnHover).transform.localScale.x;
				float a = color.a;
				ref float reference = ref imageOnHoverAlphaVelocity;
				ColorBlock colors = ((Selectable)this).colors;
				float num3 = Mathf.SmoothDamp(a, num, ref reference, 0.03f * ((ColorBlock)(ref colors)).fadeDuration, float.PositiveInfinity, Time.unscaledDeltaTime);
				float num4 = Mathf.SmoothDamp(x, num2, ref imageOnHoverScaleVelocity, 0.03f, float.PositiveInfinity, Time.unscaledDeltaTime);
				Color color2 = default(Color);
				((Color)(ref color2))._002Ector(color.r, color.g, color.g, num3);
				Vector3 localScale = default(Vector3);
				((Vector3)(ref localScale))._002Ector(num4, num4, num4);
				((Graphic)imageOnHover).color = color2;
				((Component)imageOnHover).transform.localScale = localScale;
			}
			if (Object.op_Implicit((Object)(object)imageOnInteractable))
			{
				((Behaviour)imageOnInteractable).enabled = ((Selectable)this).interactable;
			}
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (updateTextOnHover && Object.op_Implicit((Object)(object)hoverLanguageTextMeshController))
		{
			hoverLanguageTextMeshController.token = hoverToken;
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (updateTextOnHover && Object.op_Implicit((Object)(object)hoverLanguageTextMeshController))
		{
			hoverLanguageTextMeshController.token = "";
		}
		base.OnPointerExit(eventData);
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (updateTextOnHover && Object.op_Implicit((Object)(object)hoverLanguageTextMeshController))
		{
			hoverLanguageTextMeshController.token = hoverToken;
		}
	}
}
