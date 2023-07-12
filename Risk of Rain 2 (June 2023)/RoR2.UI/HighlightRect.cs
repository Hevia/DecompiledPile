using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(Canvas))]
public class HighlightRect : MonoBehaviour
{
	public enum HighlightState
	{
		Expanding,
		Holding,
		Contracting
	}

	public AnimationCurve curve;

	public Color highlightColor;

	public Sprite cornerImage;

	public string nametagString;

	private Image bottomLeftImage;

	private Image bottomRightImage;

	private Image topLeftImage;

	private Image topRightImage;

	private TextMeshProUGUI nametagText;

	public Renderer targetRenderer;

	public GameObject cameraTarget;

	public RectTransform nametagRectTransform;

	public RectTransform bottomLeftRectTransform;

	public RectTransform bottomRightRectTransform;

	public RectTransform topLeftRectTransform;

	public RectTransform topRightRectTransform;

	public float expandTime = 1f;

	public float maxLifeTime;

	public bool destroyOnLifeEnd;

	private float time;

	public HighlightState highlightState;

	private static List<HighlightRect> instancesList;

	private Canvas canvas;

	private Camera uiCam;

	private Camera sceneCam;

	private static readonly Vector2[] extentPoints;

	static HighlightRect()
	{
		instancesList = new List<HighlightRect>();
		extentPoints = (Vector2[])(object)new Vector2[8];
		RoR2Application.onLateUpdate += UpdateAll;
	}

	private void Awake()
	{
		canvas = ((Component)this).GetComponent<Canvas>();
	}

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	private void Start()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		highlightState = HighlightState.Expanding;
		bottomLeftImage = ((Component)bottomLeftRectTransform).GetComponent<Image>();
		bottomRightImage = ((Component)bottomRightRectTransform).GetComponent<Image>();
		topLeftImage = ((Component)topLeftRectTransform).GetComponent<Image>();
		topRightImage = ((Component)topRightRectTransform).GetComponent<Image>();
		bottomLeftImage.sprite = cornerImage;
		bottomRightImage.sprite = cornerImage;
		topLeftImage.sprite = cornerImage;
		topRightImage.sprite = cornerImage;
		((Graphic)bottomLeftImage).color = highlightColor;
		((Graphic)bottomRightImage).color = highlightColor;
		((Graphic)topLeftImage).color = highlightColor;
		((Graphic)topRightImage).color = highlightColor;
		if (Object.op_Implicit((Object)(object)nametagRectTransform))
		{
			nametagText = ((Component)nametagRectTransform).GetComponent<TextMeshProUGUI>();
			((Graphic)nametagText).color = highlightColor;
			((TMP_Text)nametagText).text = nametagString;
		}
	}

	private static void UpdateAll()
	{
		for (int num = instancesList.Count - 1; num >= 0; num--)
		{
			instancesList[num].DoUpdate();
		}
	}

	private void DoUpdate()
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)targetRenderer))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		switch (highlightState)
		{
		case HighlightState.Expanding:
			time += Time.deltaTime;
			if (time >= expandTime)
			{
				time = expandTime;
				highlightState = HighlightState.Holding;
			}
			break;
		case HighlightState.Holding:
			if (destroyOnLifeEnd)
			{
				time += Time.deltaTime;
				if (time > maxLifeTime)
				{
					highlightState = HighlightState.Contracting;
					time = expandTime;
				}
			}
			break;
		case HighlightState.Contracting:
			time -= Time.deltaTime;
			if (time <= 0f)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
				return;
			}
			break;
		}
		Rect val = GUIRectWithObject(sceneCam, targetRenderer);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(Mathf.Lerp(((Rect)(ref val)).xMin, ((Rect)(ref val)).xMax, 0.5f), Mathf.Lerp(((Rect)(ref val)).yMin, ((Rect)(ref val)).yMax, 0.5f));
		float num = curve.Evaluate(time / expandTime);
		bottomLeftRectTransform.anchoredPosition = Vector2.LerpUnclamped(val2, new Vector2(((Rect)(ref val)).xMin, ((Rect)(ref val)).yMin), num);
		bottomRightRectTransform.anchoredPosition = Vector2.LerpUnclamped(val2, new Vector2(((Rect)(ref val)).xMax, ((Rect)(ref val)).yMin), num);
		topLeftRectTransform.anchoredPosition = Vector2.LerpUnclamped(val2, new Vector2(((Rect)(ref val)).xMin, ((Rect)(ref val)).yMax), num);
		topRightRectTransform.anchoredPosition = Vector2.LerpUnclamped(val2, new Vector2(((Rect)(ref val)).xMax, ((Rect)(ref val)).yMax), num);
		if (Object.op_Implicit((Object)(object)nametagRectTransform))
		{
			nametagRectTransform.anchoredPosition = Vector2.LerpUnclamped(val2, new Vector2(((Rect)(ref val)).xMin, ((Rect)(ref val)).yMax), num);
		}
	}

	public static Rect GUIRectWithObject(Camera cam, Renderer rend)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = rend.bounds;
		Vector3 center = ((Bounds)(ref bounds)).center;
		bounds = rend.bounds;
		Vector3 extents = ((Bounds)(ref bounds)).extents;
		extentPoints[0] = WorldToGUIPoint(cam, new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z));
		extentPoints[1] = WorldToGUIPoint(cam, new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z));
		extentPoints[2] = WorldToGUIPoint(cam, new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z));
		extentPoints[3] = WorldToGUIPoint(cam, new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z));
		extentPoints[4] = WorldToGUIPoint(cam, new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z));
		extentPoints[5] = WorldToGUIPoint(cam, new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z));
		extentPoints[6] = WorldToGUIPoint(cam, new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z));
		extentPoints[7] = WorldToGUIPoint(cam, new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z));
		Vector2 val = extentPoints[0];
		Vector2 val2 = extentPoints[0];
		Vector2[] array = extentPoints;
		foreach (Vector2 val3 in array)
		{
			val = Vector2.Min(val, val3);
			val2 = Vector2.Max(val2, val3);
		}
		return new Rect(val.x, val.y, val2.x - val.x, val2.y - val.y);
	}

	public static Vector2 WorldToGUIPoint(Camera cam, Vector3 world)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.op_Implicit(cam.WorldToScreenPoint(world));
	}

	public static void CreateHighlight(GameObject viewerBodyObject, Renderer targetRenderer, GameObject highlightPrefab, float overrideDuration = -1f, bool visibleToAll = false)
	{
		ReadOnlyCollection<CameraRigController> readOnlyInstancesList = CameraRigController.readOnlyInstancesList;
		int i = 0;
		for (int count = readOnlyInstancesList.Count; i < count; i++)
		{
			CameraRigController cameraRigController = readOnlyInstancesList[i];
			if (!((Object)(object)cameraRigController.target != (Object)(object)viewerBodyObject) || visibleToAll)
			{
				HighlightRect component = Object.Instantiate<GameObject>(highlightPrefab).GetComponent<HighlightRect>();
				component.targetRenderer = targetRenderer;
				component.canvas.worldCamera = cameraRigController.uiCam;
				component.uiCam = cameraRigController.uiCam;
				component.sceneCam = cameraRigController.sceneCam;
				if (overrideDuration > 0f)
				{
					component.maxLifeTime = overrideDuration;
				}
			}
		}
	}
}
