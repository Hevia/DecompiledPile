using System.Collections.Generic;
using RoR2.ConVar;
using RoR2.UI;
using UnityEngine;

namespace RoR2;

[DisallowMultipleComponent]
public class PositionIndicator : MonoBehaviour
{
	public Transform targetTransform;

	private Transform transform;

	private static readonly List<PositionIndicator> instancesList;

	[Tooltip("The child object to enable when the target is within the frame.")]
	public GameObject insideViewObject;

	[Tooltip("The child object to enable when the target is outside the frame.")]
	public GameObject outsideViewObject;

	[Tooltip("The child object to ALWAYS enable, IF its not my own position indicator.")]
	public GameObject alwaysVisibleObject;

	[Tooltip("Whether or not outsideViewObject should be rotated to point to the target.")]
	public bool shouldRotateOutsideViewObject;

	[Tooltip("The offset to apply to the rotation of the outside view object when shouldRotateOutsideViewObject is set.")]
	public float outsideViewRotationOffset;

	private float yOffset;

	private bool generateDefaultPosition;

	private static BoolConVar cvPositionIndicatorsEnable;

	public Vector3 defaultPosition { get; set; }

	private void Awake()
	{
		transform = ((Component)this).transform;
	}

	private void Start()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!generateDefaultPosition)
		{
			generateDefaultPosition = true;
			defaultPosition = ((Component)this).transform.position;
		}
		if (Object.op_Implicit((Object)(object)targetTransform))
		{
			yOffset = CalcHeadOffset(targetTransform) + 1f;
		}
	}

	private static float CalcHeadOffset(Transform transform)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Collider component = ((Component)transform).GetComponent<Collider>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Bounds bounds = component.bounds;
			return ((Bounds)(ref bounds)).extents.y;
		}
		return 0f;
	}

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	private void OnValidate()
	{
		if (Object.op_Implicit((Object)(object)insideViewObject) && Object.op_Implicit((Object)(object)insideViewObject.GetComponentInChildren<PositionIndicator>()))
		{
			Debug.LogError((object)"insideViewObject may not be assigned another object with another PositionIndicator in its heirarchy!");
			insideViewObject = null;
		}
		if (Object.op_Implicit((Object)(object)outsideViewObject) && Object.op_Implicit((Object)(object)outsideViewObject.GetComponentInChildren<PositionIndicator>()))
		{
			Debug.LogError((object)"outsideViewObject may not be assigned another object with another PositionIndicator in its heirarchy!");
			outsideViewObject = null;
		}
	}

	static PositionIndicator()
	{
		instancesList = new List<PositionIndicator>();
		cvPositionIndicatorsEnable = new BoolConVar("position_indicators_enable", ConVarFlags.None, "1", "Enables/Disables position indicators for allies, bosses, pings, etc.");
		UICamera.onUICameraPreCull += UpdatePositions;
	}

	private static void UpdatePositions(UICamera uiCamera)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		Camera sceneCam = uiCamera.cameraRigController.sceneCam;
		Camera camera = uiCamera.camera;
		Rect pixelRect = camera.pixelRect;
		Vector2 center = ((Rect)(ref pixelRect)).center;
		((Rect)(ref pixelRect)).size = ((Rect)(ref pixelRect)).size * 0.95f;
		((Rect)(ref pixelRect)).center = center;
		Vector2 center2 = ((Rect)(ref pixelRect)).center;
		float num = 1f / (((Rect)(ref pixelRect)).width * 0.5f);
		float num2 = 1f / (((Rect)(ref pixelRect)).height * 0.5f);
		Quaternion rotation = ((Component)uiCamera).transform.rotation;
		CameraRigController cameraRigController = uiCamera.cameraRigController;
		Transform val = null;
		if (Object.op_Implicit((Object)(object)cameraRigController) && Object.op_Implicit((Object)(object)cameraRigController.target))
		{
			CharacterBody component = cameraRigController.target.GetComponent<CharacterBody>();
			val = ((!Object.op_Implicit((Object)(object)component)) ? cameraRigController.target.transform : component.coreTransform);
		}
		for (int i = 0; i < instancesList.Count; i++)
		{
			PositionIndicator positionIndicator = instancesList[i];
			bool flag = false;
			bool flag2 = false;
			bool active = false;
			if (!HUD.cvHudEnable.value)
			{
				positionIndicator.insideViewObject.SetActive(false);
				positionIndicator.outsideViewObject.SetActive(false);
				positionIndicator.alwaysVisibleObject.SetActive(false);
				continue;
			}
			float num3 = 0f;
			Vector3 val3;
			int num4;
			if (cvPositionIndicatorsEnable.value)
			{
				Vector3 val2 = (Object.op_Implicit((Object)(object)positionIndicator.targetTransform) ? positionIndicator.targetTransform.position : positionIndicator.defaultPosition);
				if (!Object.op_Implicit((Object)(object)positionIndicator.targetTransform) || (Object.op_Implicit((Object)(object)positionIndicator.targetTransform) && (Object)(object)positionIndicator.targetTransform != (Object)(object)val))
				{
					active = true;
					val3 = sceneCam.WorldToScreenPoint(val2 + new Vector3(0f, positionIndicator.yOffset, 0f));
					bool flag3 = val3.z <= 0f;
					if (!flag3)
					{
						num4 = (((Rect)(ref pixelRect)).Contains(val3) ? 1 : 0);
						if (num4 != 0)
						{
							goto IL_0279;
						}
					}
					else
					{
						num4 = 0;
					}
					Vector2 val4 = Vector2.op_Implicit(val3) - center2;
					float num5 = Mathf.Abs(val4.x * num);
					float num6 = Mathf.Abs(val4.y * num2);
					float num7 = Mathf.Max(num5, num6);
					val4 /= num7;
					val4 *= (flag3 ? (-1f) : 1f);
					val3 = Vector2.op_Implicit(val4 + center2);
					if (positionIndicator.shouldRotateOutsideViewObject)
					{
						num3 = Mathf.Atan2(val4.y, val4.x) * 57.29578f;
					}
					goto IL_0279;
				}
			}
			goto IL_02a7;
			IL_0279:
			flag = (byte)num4 != 0;
			flag2 = num4 == 0;
			val3.z = 1f;
			Vector3 val5 = camera.ScreenToWorldPoint(val3);
			positionIndicator.transform.SetPositionAndRotation(val5, rotation);
			goto IL_02a7;
			IL_02a7:
			if (Object.op_Implicit((Object)(object)positionIndicator.alwaysVisibleObject))
			{
				positionIndicator.alwaysVisibleObject.SetActive(active);
			}
			if ((Object)(object)positionIndicator.insideViewObject == (Object)(object)positionIndicator.outsideViewObject)
			{
				if (Object.op_Implicit((Object)(object)positionIndicator.insideViewObject))
				{
					positionIndicator.insideViewObject.SetActive(flag || flag2);
				}
				continue;
			}
			if (Object.op_Implicit((Object)(object)positionIndicator.insideViewObject))
			{
				positionIndicator.insideViewObject.SetActive(flag);
			}
			if (Object.op_Implicit((Object)(object)positionIndicator.outsideViewObject))
			{
				positionIndicator.outsideViewObject.SetActive(flag2);
				if (flag2 && positionIndicator.shouldRotateOutsideViewObject)
				{
					positionIndicator.outsideViewObject.transform.localEulerAngles = new Vector3(0f, 0f, num3 + positionIndicator.outsideViewRotationOffset);
				}
			}
		}
	}
}
