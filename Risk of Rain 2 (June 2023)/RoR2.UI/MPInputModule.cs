using System.Collections.Generic;
using System.Reflection;
using Rewired.Integration.UnityUI;
using Rewired.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPInput))]
public class MPInputModule : RewiredStandaloneInputModule
{
	private delegate bool ShouldStartDragDelegate(Vector2 pressPos, Vector2 currentPos, float threshold, bool useDragThreshold);

	private MouseState m_MouseState;

	private static readonly FieldInfo m_MouseStateField = typeof(RewiredPointerInputModule).GetField("m_MouseState", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly ShouldStartDragDelegate ShouldStartDrag = (ShouldStartDragDelegate)typeof(RewiredPointerInputModule).GetMethod("ShouldStartDrag", BindingFlags.Static | BindingFlags.NonPublic).CreateDelegate(typeof(ShouldStartDragDelegate));

	public bool isHovering;

	private bool useCursor => ((MPEventSystem)(object)((BaseInputModule)this).eventSystem).isCursorVisible;

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		((BaseInputModule)this).m_InputOverride = (BaseInput)(object)((Component)this).GetComponent<MPInput>();
		m_MouseState = (MouseState)m_MouseStateField.GetValue(this);
		((RewiredStandaloneInputModule)this).Awake();
	}

	protected override void Start()
	{
		((UIBehaviour)this).Start();
		((RewiredPointerInputModule)this).ClearMouseInputSources();
		((RewiredPointerInputModule)this).AddMouseInputSource((IMouseInputSource)(object)(MPInput)(object)((BaseInputModule)this).m_InputOverride);
	}

	protected void UpdateHover(List<RaycastResult> raycastResults)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		isHovering = false;
		if (!useCursor)
		{
			return;
		}
		foreach (RaycastResult raycastResult in raycastResults)
		{
			RaycastResult current = raycastResult;
			if (Object.op_Implicit((Object)(object)((RaycastResult)(ref current)).gameObject))
			{
				Selectable componentInParent = ((RaycastResult)(ref current)).gameObject.GetComponentInParent<Selectable>();
				if ((Object)(object)componentInParent != (Object)null && IsHoverable(componentInParent))
				{
					isHovering = true;
					break;
				}
			}
		}
		bool IsHoverable(Selectable selectable)
		{
			MPButton mPButton = selectable as MPButton;
			if (!Object.op_Implicit((Object)(object)mPButton))
			{
				return true;
			}
			return mPButton.InputModuleIsAllowed((BaseInputModule)(object)this);
		}
	}

	protected override MouseState GetMousePointerEventData(int playerId, int mouseIndex)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		IMouseInputSource mouseInputSource = ((RewiredPointerInputModule)this).GetMouseInputSource(playerId, mouseIndex);
		if (mouseInputSource == null)
		{
			return null;
		}
		PlayerPointerEventData val = default(PlayerPointerEventData);
		bool pointerData = ((RewiredPointerInputModule)this).GetPointerData(playerId, mouseIndex, -1, ref val, true, (PointerEventType)0);
		((AbstractEventData)val).Reset();
		if (pointerData)
		{
			((PointerEventData)val).position = ((BaseInputModule)this).input.mousePosition;
		}
		Vector2 mousePosition = ((BaseInputModule)this).input.mousePosition;
		if (mouseInputSource.locked || !mouseInputSource.enabled)
		{
			((PointerEventData)val).position = new Vector2(-1f, -1f);
			((PointerEventData)val).delta = Vector2.zero;
		}
		else
		{
			((PointerEventData)val).delta = mousePosition - ((PointerEventData)val).position;
			((PointerEventData)val).position = mousePosition;
		}
		((PointerEventData)val).scrollDelta = mouseInputSource.wheelDelta;
		((PointerEventData)val).button = (InputButton)0;
		((BaseInputModule)this).eventSystem.RaycastAll((PointerEventData)(object)val, ((BaseInputModule)this).m_RaycastResultCache);
		RaycastResult pointerCurrentRaycast = BaseInputModule.FindFirstRaycast(((BaseInputModule)this).m_RaycastResultCache);
		((PointerEventData)val).pointerCurrentRaycast = pointerCurrentRaycast;
		UpdateHover(((BaseInputModule)this).m_RaycastResultCache);
		((BaseInputModule)this).m_RaycastResultCache.Clear();
		PlayerPointerEventData val2 = default(PlayerPointerEventData);
		((RewiredPointerInputModule)this).GetPointerData(playerId, mouseIndex, -2, ref val2, true, (PointerEventType)0);
		((RewiredPointerInputModule)this).CopyFromTo((PointerEventData)(object)val, (PointerEventData)(object)val2);
		((PointerEventData)val2).button = (InputButton)1;
		PlayerPointerEventData val3 = default(PlayerPointerEventData);
		((RewiredPointerInputModule)this).GetPointerData(playerId, mouseIndex, -3, ref val3, true, (PointerEventType)0);
		((RewiredPointerInputModule)this).CopyFromTo((PointerEventData)(object)val, (PointerEventData)(object)val3);
		((PointerEventData)val3).button = (InputButton)2;
		PlayerPointerEventData val4 = default(PlayerPointerEventData);
		for (int i = 3; i < mouseInputSource.buttonCount; i++)
		{
			((RewiredPointerInputModule)this).GetPointerData(playerId, mouseIndex, -2147483520 + i, ref val4, true, (PointerEventType)0);
			((RewiredPointerInputModule)this).CopyFromTo((PointerEventData)(object)val, (PointerEventData)(object)val4);
			((PointerEventData)val4).button = (InputButton)(-1);
		}
		m_MouseState.SetButtonState(0, ((RewiredPointerInputModule)this).StateForMouseButton(playerId, mouseIndex, 0), val);
		m_MouseState.SetButtonState(1, ((RewiredPointerInputModule)this).StateForMouseButton(playerId, mouseIndex, 1), val2);
		m_MouseState.SetButtonState(2, ((RewiredPointerInputModule)this).StateForMouseButton(playerId, mouseIndex, 2), val3);
		PlayerPointerEventData val5 = default(PlayerPointerEventData);
		for (int j = 3; j < mouseInputSource.buttonCount; j++)
		{
			((RewiredPointerInputModule)this).GetPointerData(playerId, mouseIndex, -2147483520 + j, ref val5, false, (PointerEventType)0);
			m_MouseState.SetButtonState(j, ((RewiredPointerInputModule)this).StateForMouseButton(playerId, mouseIndex, j), val5);
		}
		return m_MouseState;
	}

	protected override void ProcessMove(PlayerPointerEventData pointerEvent)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		object obj;
		if (useCursor)
		{
			RaycastResult pointerCurrentRaycast = ((PointerEventData)pointerEvent).pointerCurrentRaycast;
			obj = ((RaycastResult)(ref pointerCurrentRaycast)).gameObject;
		}
		else
		{
			obj = null;
		}
		GameObject val = (GameObject)obj;
		((BaseInputModule)this).HandlePointerExitAndEnter((PointerEventData)(object)pointerEvent, val);
	}

	protected override void ProcessDrag(PlayerPointerEventData pointerEvent)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!((PointerEventData)pointerEvent).IsPointerMoving() || !useCursor || (Object)(object)((PointerEventData)pointerEvent).pointerDrag == (Object)null)
		{
			return;
		}
		if (!((PointerEventData)pointerEvent).dragging && ShouldStartDrag(((PointerEventData)pointerEvent).pressPosition, ((PointerEventData)pointerEvent).position, ((BaseInputModule)this).eventSystem.pixelDragThreshold, ((PointerEventData)pointerEvent).useDragThreshold))
		{
			ExecuteEvents.Execute<IBeginDragHandler>(((PointerEventData)pointerEvent).pointerDrag, (BaseEventData)(object)pointerEvent, ExecuteEvents.beginDragHandler);
			((PointerEventData)pointerEvent).dragging = true;
		}
		if (((PointerEventData)pointerEvent).dragging)
		{
			if ((Object)(object)((PointerEventData)pointerEvent).pointerPress != (Object)(object)((PointerEventData)pointerEvent).pointerDrag)
			{
				ExecuteEvents.Execute<IPointerUpHandler>(((PointerEventData)pointerEvent).pointerPress, (BaseEventData)(object)pointerEvent, ExecuteEvents.pointerUpHandler);
				((PointerEventData)pointerEvent).eligibleForClick = false;
				((PointerEventData)pointerEvent).pointerPress = null;
				((PointerEventData)pointerEvent).rawPointerPress = null;
			}
			ExecuteEvents.Execute<IDragHandler>(((PointerEventData)pointerEvent).pointerDrag, (BaseEventData)(object)pointerEvent, ExecuteEvents.dragHandler);
		}
	}
}
