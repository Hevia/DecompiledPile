using System;
using System.Collections.Generic;
using HG;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(RectTransform))]
public class PointViewer : MonoBehaviour, ILayoutGroup, ILayoutController
{
	private struct ElementInfo
	{
		public Transform targetTransform;

		public Vector3 targetLastKnownPosition;

		public float targetWorldDiameter;

		public float targetWorldVerticalOffset;

		public bool scaleWithDistance;

		public GameObject elementInstance;

		public RectTransform elementRectTransform;

		public CanvasGroup elementCanvasGroup;
	}

	public struct AddElementRequest
	{
		public GameObject elementPrefab;

		public Transform target;

		public float targetWorldVerticalOffset;

		public float targetWorldDiameter;

		public bool scaleWithDistance;

		public float targetWorldRadius
		{
			get
			{
				return targetWorldDiameter * 0.5f;
			}
			set
			{
				targetWorldDiameter = value * 2f;
			}
		}
	}

	private UICamera uiCamController;

	private Camera sceneCam;

	private StructAllocator<ElementInfo> elementInfoAllocator = new StructAllocator<ElementInfo>(16u);

	private Dictionary<UnityObjectWrapperKey<GameObject>, Ptr<ElementInfo>> elementToElementInfo = new Dictionary<UnityObjectWrapperKey<GameObject>, Ptr<ElementInfo>>();

	protected RectTransform rectTransform { get; private set; }

	protected Canvas canvas { get; private set; }

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		rectTransform = (RectTransform)((Component)this).transform;
		canvas = ((Component)this).GetComponent<Canvas>();
	}

	private void Start()
	{
		FindCamera();
	}

	private void Update()
	{
		SetDirty();
	}

	public GameObject AddElement(AddElementRequest request)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)request.target) || !Object.op_Implicit((Object)(object)request.elementPrefab))
		{
			return null;
		}
		GameObject val = Object.Instantiate<GameObject>(request.elementPrefab, (Transform)(object)rectTransform);
		Ptr<ElementInfo> val2 = elementInfoAllocator.Alloc();
		ref ElementInfo @ref = ref elementInfoAllocator.GetRef(val2);
		@ref.targetTransform = request.target;
		@ref.targetWorldVerticalOffset = request.targetWorldVerticalOffset;
		@ref.targetWorldDiameter = request.targetWorldDiameter;
		@ref.targetLastKnownPosition = request.target.position;
		@ref.scaleWithDistance = request.scaleWithDistance;
		@ref.elementInstance = val;
		@ref.elementRectTransform = (RectTransform)val.transform;
		@ref.elementCanvasGroup = val.GetComponent<CanvasGroup>();
		elementToElementInfo.Add(UnityObjectWrapperKey<GameObject>.op_Implicit(val), val2);
		return val;
	}

	public void RemoveElement(GameObject elementInstance)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (elementToElementInfo.TryGetValue(UnityObjectWrapperKey<GameObject>.op_Implicit(elementInstance), out var value))
		{
			elementToElementInfo.Remove(UnityObjectWrapperKey<GameObject>.op_Implicit(elementInstance));
			elementInfoAllocator.Free(ref value);
			Object.Destroy((Object)(object)elementInstance);
		}
	}

	public void RemoveAll()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<UnityObjectWrapperKey<GameObject>, Ptr<ElementInfo>> item in elementToElementInfo)
		{
			GameObject obj = UnityObjectWrapperKey<GameObject>.op_Implicit(item.Key);
			Ptr<ElementInfo> value = item.Value;
			elementInfoAllocator.Free(ref value);
			Object.Destroy((Object)(object)obj);
		}
		elementToElementInfo.Clear();
	}

	private void FindCamera()
	{
		uiCamController = ((Component)canvas.rootCanvas.worldCamera).GetComponent<UICamera>();
		sceneCam = (Object.op_Implicit((Object)(object)uiCamController) ? uiCamController.cameraRigController.sceneCam : null);
	}

	private void SetDirty()
	{
		if (((Behaviour)this).isActiveAndEnabled && !CanvasUpdateRegistry.IsRebuildingLayout())
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}

	protected void UpdateAllElementPositions()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)uiCamController) || !Object.op_Implicit((Object)(object)sceneCam))
		{
			return;
		}
		Camera camera = uiCamController.camera;
		Rect rect = rectTransform.rect;
		Vector2 size = ((Rect)(ref rect)).size;
		float num = sceneCam.fieldOfView * (MathF.PI / 180f);
		float num2 = 1f / num;
		Vector2 val3 = default(Vector2);
		foreach (KeyValuePair<UnityObjectWrapperKey<GameObject>, Ptr<ElementInfo>> item in elementToElementInfo)
		{
			UnityObjectWrapperKey<GameObject>.op_Implicit(item.Key);
			Ptr<ElementInfo> value = item.Value;
			ref ElementInfo @ref = ref elementInfoAllocator.GetRef(value);
			if (Object.op_Implicit((Object)(object)@ref.targetTransform))
			{
				@ref.targetLastKnownPosition = @ref.targetTransform.position;
			}
			Vector3 targetLastKnownPosition = @ref.targetLastKnownPosition;
			targetLastKnownPosition.y += @ref.targetWorldVerticalOffset;
			Vector3 val = sceneCam.WorldToViewportPoint(targetLastKnownPosition);
			float z = val.z;
			Vector3 val2 = camera.ViewportToScreenPoint(val);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Vector2.op_Implicit(val2), camera, ref val3);
			Vector3 localPosition = Vector2.op_Implicit(val3);
			localPosition.z = ((z >= 0f) ? 0f : (-1f));
			((Transform)@ref.elementRectTransform).localPosition = localPosition;
			if (@ref.scaleWithDistance)
			{
				float num3 = @ref.targetWorldDiameter * num2 / z;
				@ref.elementRectTransform.sizeDelta = num3 * size;
			}
		}
	}

	public void SetLayoutHorizontal()
	{
		UpdateAllElementPositions();
	}

	public void SetLayoutVertical()
	{
	}
}
