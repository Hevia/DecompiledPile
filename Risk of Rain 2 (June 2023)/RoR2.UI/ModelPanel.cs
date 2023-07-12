using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HG;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(RectTransform))]
public class ModelPanel : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IScrollHandler, IEndDragHandler
{
	private class CameraFramingCalculator
	{
		private GameObject modelInstance;

		private Transform root;

		private readonly List<Transform> boneList = new List<Transform>();

		private HurtBoxGroup hurtBoxGroup;

		private HurtBox[] hurtBoxes = Array.Empty<HurtBox>();

		public Vector3 outputPivotPoint;

		public Vector3 outputCameraPosition;

		public float outputMinDistance;

		public float outputMaxDistance;

		public Quaternion outputCameraRotation;

		private static void GenerateBoneList(Transform rootBone, List<Transform> boneList)
		{
			boneList.AddRange(((Component)rootBone).gameObject.GetComponentsInChildren<Transform>());
		}

		public CameraFramingCalculator(GameObject modelInstance)
		{
			this.modelInstance = modelInstance;
			root = modelInstance.transform;
			GenerateBoneList(root, boneList);
			hurtBoxGroup = modelInstance.GetComponent<HurtBoxGroup>();
			if (Object.op_Implicit((Object)(object)hurtBoxGroup))
			{
				hurtBoxes = hurtBoxGroup.hurtBoxes;
			}
		}

		private bool FindBestEyePoint(out Vector3 result, out float approximateEyeRadius)
		{
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			approximateEyeRadius = 1f;
			IEnumerable<Transform> source = boneList.Where(FirstChoice);
			if (!source.Any())
			{
				source = boneList.Where(SecondChoice);
			}
			Vector3[] array = source.Select((Transform bone) => bone.position).ToArray();
			result = Vector3Utils.AveragePrecise<Vector3[]>(array);
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 val = array[i] - result;
				float magnitude = ((Vector3)(ref val)).magnitude;
				if (magnitude > approximateEyeRadius)
				{
					approximateEyeRadius = magnitude;
				}
			}
			return array.Length != 0;
			static bool FirstChoice(Transform bone)
			{
				if (Object.op_Implicit((Object)(object)((Component)bone).GetComponent<SkinnedMeshRenderer>()))
				{
					return false;
				}
				string name = ((Object)bone).name;
				if (name.Equals("eye", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if (name.Equals("eyeball.1", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				return false;
			}
			static bool SecondChoice(Transform bone)
			{
				if (Object.op_Implicit((Object)(object)((Component)bone).GetComponent<SkinnedMeshRenderer>()))
				{
					return false;
				}
				return ((Object)bone).name.ToLower().Contains("eye");
			}
		}

		private bool FindBestHeadPoint(string searchName, out Vector3 result, out float approximateRadius)
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			Transform[] array = boneList.Where((Transform bone) => string.Equals(((Object)bone).name, searchName, StringComparison.OrdinalIgnoreCase)).ToArray();
			if (array.Length == 0)
			{
				array = boneList.Where((Transform bone) => ((Object)bone).name.ToLower(CultureInfo.InvariantCulture).Contains(searchName)).ToArray();
			}
			if (array.Length != 0)
			{
				foreach (Transform bone2 in array)
				{
					if (TryCalcBoneBounds(bone2, 0.2f, out var bounds, out approximateRadius))
					{
						result = ((Bounds)(ref bounds)).center;
						return true;
					}
				}
			}
			result = Vector3.zero;
			approximateRadius = 0f;
			return false;
		}

		private static float CalcMagnitudeToFrameSphere(float sphereRadius, float fieldOfView)
		{
			float num = fieldOfView * 0.5f;
			float num2 = 90f;
			return Mathf.Tan((180f - num2 - num) * (MathF.PI / 180f)) * sphereRadius;
		}

		private bool FindBestCenterOfMass(out Vector3 result, out float approximateRadius)
		{
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			_ = from bone in boneList
				select ((Component)bone).GetComponent<HurtBox>() into hurtBox
				where Object.op_Implicit((Object)(object)hurtBox)
				select hurtBox;
			if (Object.op_Implicit((Object)(object)hurtBoxGroup) && Object.op_Implicit((Object)(object)hurtBoxGroup.mainHurtBox))
			{
				result = ((Component)hurtBoxGroup.mainHurtBox).transform.position;
				approximateRadius = Util.SphereVolumeToRadius(hurtBoxGroup.mainHurtBox.volume);
				return true;
			}
			result = Vector3.zero;
			approximateRadius = 1f;
			return false;
		}

		private static float GetWeightForBone(ref BoneWeight boneWeight, int boneIndex)
		{
			if (((BoneWeight)(ref boneWeight)).boneIndex0 == boneIndex)
			{
				return ((BoneWeight)(ref boneWeight)).weight0;
			}
			if (((BoneWeight)(ref boneWeight)).boneIndex1 == boneIndex)
			{
				return ((BoneWeight)(ref boneWeight)).weight1;
			}
			if (((BoneWeight)(ref boneWeight)).boneIndex2 == boneIndex)
			{
				return ((BoneWeight)(ref boneWeight)).weight2;
			}
			if (((BoneWeight)(ref boneWeight)).boneIndex3 == boneIndex)
			{
				return ((BoneWeight)(ref boneWeight)).weight3;
			}
			return 0f;
		}

		private static int FindBoneIndex(SkinnedMeshRenderer _skinnedMeshRenderer, Transform _bone)
		{
			Transform[] bones = _skinnedMeshRenderer.bones;
			for (int i = 0; i < bones.Length; i++)
			{
				if ((Object)(object)bones[i] == (Object)(object)_bone)
				{
					return i;
				}
			}
			return -1;
		}

		private bool TryCalcBoneBounds(Transform bone, float weightThreshold, out Bounds bounds, out float approximateRadius)
		{
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Expected O, but got Unknown
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			SkinnedMeshRenderer[] componentsInChildren = modelInstance.GetComponentsInChildren<SkinnedMeshRenderer>();
			SkinnedMeshRenderer val = null;
			Mesh val2 = null;
			int num = -1;
			List<int> list = new List<int>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				val = componentsInChildren[i];
				val2 = val.sharedMesh;
				if (!Object.op_Implicit((Object)(object)val2))
				{
					continue;
				}
				num = FindBoneIndex(val, bone);
				if (num != -1)
				{
					BoneWeight[] boneWeights = val2.boneWeights;
					for (int j = 0; j < boneWeights.Length; j++)
					{
						if (GetWeightForBone(ref boneWeights[j], num) > weightThreshold)
						{
							list.Add(j);
						}
					}
					if (list.Count == 0)
					{
						num = -1;
					}
				}
				if (num != -1)
				{
					break;
				}
			}
			if (num == -1)
			{
				bounds = default(Bounds);
				approximateRadius = 0f;
				return false;
			}
			Mesh val3 = new Mesh();
			val.BakeMesh(val3);
			Vector3[] vertices = val3.vertices;
			Object.Destroy((Object)(object)val3);
			if (val3.vertexCount != val2.vertexCount)
			{
				Debug.LogWarningFormat("Baked mesh vertex count differs from the original mesh vertex count! baked={0} original={1}", new object[2] { val3.vertexCount, val2.vertexCount });
				vertices = val2.vertices;
			}
			Vector3[] array = (Vector3[])(object)new Vector3[list.Count];
			Transform transform = ((Component)val).transform;
			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;
			for (int k = 0; k < list.Count; k++)
			{
				int num2 = list[k];
				Vector3 val4 = vertices[num2];
				Vector3 val5 = position + rotation * val4;
				array[k] = val5;
			}
			bounds = new Bounds(Vector3Utils.AveragePrecise<Vector3[]>(array), Vector3.zero);
			float num3 = 0f;
			for (int l = 0; l < array.Length; l++)
			{
				((Bounds)(ref bounds)).Encapsulate(array[l]);
				float num4 = Vector3.Distance(((Bounds)(ref bounds)).center, array[l]);
				if (num4 > num3)
				{
					num3 = num4;
				}
			}
			approximateRadius = num3;
			return true;
		}

		public void GetCharacterThumbnailPosition(float fov)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_0250: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0267: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_028c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_0295: Unknown result type (might be due to invalid IL or missing references)
			//IL_0297: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02df: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0301: Unknown result type (might be due to invalid IL or missing references)
			//IL_0306: Unknown result type (might be due to invalid IL or missing references)
			//IL_0309: Unknown result type (might be due to invalid IL or missing references)
			//IL_030a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0310: Unknown result type (might be due to invalid IL or missing references)
			//IL_0312: Unknown result type (might be due to invalid IL or missing references)
			//IL_0318: Unknown result type (might be due to invalid IL or missing references)
			//IL_0319: Unknown result type (might be due to invalid IL or missing references)
			//IL_031b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0320: Unknown result type (might be due to invalid IL or missing references)
			//IL_0325: Unknown result type (might be due to invalid IL or missing references)
			ModelPanelParameters component = modelInstance.GetComponent<ModelPanelParameters>();
			if (Object.op_Implicit((Object)(object)component))
			{
				if (Object.op_Implicit((Object)(object)component.focusPointTransform))
				{
					outputPivotPoint = component.focusPointTransform.position;
				}
				if (Object.op_Implicit((Object)(object)component.cameraPositionTransform))
				{
					outputCameraPosition = component.cameraPositionTransform.position;
				}
				outputCameraRotation = Util.QuaternionSafeLookRotation(component.cameraDirection);
				outputMinDistance = component.minDistance;
				outputMaxDistance = component.maxDistance;
				return;
			}
			Vector3 result = Vector3.zero;
			float approximateRadius = 1f;
			bool flag = FindBestHeadPoint("head", out result, out approximateRadius);
			if (!flag)
			{
				flag = FindBestHeadPoint("chest", out result, out approximateRadius);
			}
			bool flag2 = false;
			bool flag3 = false;
			float num = 1f;
			float approximateEyeRadius = 1f;
			flag2 = FindBestEyePoint(out var result2, out approximateEyeRadius);
			if (!flag)
			{
				approximateRadius = approximateEyeRadius;
			}
			if (flag2)
			{
				result = result2;
			}
			if (!flag && !flag2)
			{
				flag3 = FindBestCenterOfMass(out result, out approximateRadius);
			}
			float num2 = 1f;
			if (Util.GuessRenderBoundsMeshOnly(modelInstance, out var bounds))
			{
				if (flag3)
				{
					approximateRadius = Util.SphereVolumeToRadius(((Bounds)(ref bounds)).size.x * ((Bounds)(ref bounds)).size.y * ((Bounds)(ref bounds)).size.z);
				}
				Mathf.Max((result.y - ((Bounds)(ref bounds)).min.y) / ((Bounds)(ref bounds)).size.y - 0.5f - 0.2f, 0f);
				_ = ((Bounds)(ref bounds)).center;
				num2 = ((Bounds)(ref bounds)).size.z / ((Bounds)(ref bounds)).size.x;
				outputMinDistance = Mathf.Min(new float[3]
				{
					((Bounds)(ref bounds)).size.x,
					((Bounds)(ref bounds)).size.y,
					((Bounds)(ref bounds)).size.z
				}) * 1f;
				outputMaxDistance = Mathf.Max(new float[3]
				{
					((Bounds)(ref bounds)).size.x,
					((Bounds)(ref bounds)).size.y,
					((Bounds)(ref bounds)).size.z
				}) * 2f;
			}
			Vector3 val = -root.forward;
			for (int i = 0; i < boneList.Count; i++)
			{
				if (((Object)boneList[i]).name.Equals("muzzle", StringComparison.OrdinalIgnoreCase))
				{
					Vector3 val2 = root.position - boneList[i].position;
					val2.y = 0f;
					float magnitude = ((Vector3)(ref val2)).magnitude;
					if (magnitude > 0.2f)
					{
						val2 /= magnitude;
						val = val2;
						break;
					}
				}
			}
			val = Quaternion.Euler(0f, 57.29578f * Mathf.Atan(num2 - 1f) * 1f, 0f) * val;
			Vector3 val3 = -val * (CalcMagnitudeToFrameSphere(approximateRadius, fov) + num);
			Vector3 val4 = result + val3;
			outputPivotPoint = result;
			outputCameraPosition = val4;
			outputCameraRotation = Util.QuaternionSafeLookRotation(result - val4);
		}
	}

	private GameObject _modelPrefab;

	public RenderSettingsState renderSettings;

	public Color camBackgroundColor = Color.clear;

	public bool disablePostProcessLayer = true;

	private RectTransform rectTransform;

	private RawImage rawImage;

	private GameObject modelInstance;

	private CameraRigController cameraRigController;

	private ModelCamera modelCamera;

	public GameObject headlightPrefab;

	public GameObject[] lightPrefabs;

	private Light headlight;

	public float fov = 60f;

	public bool enableGamepadControls;

	public float gamepadZoomSensitivity;

	public float gamepadRotateSensitivity;

	public UILayerKey requiredTopLayer;

	private MPEventSystemLocator mpEventSystemLocator;

	private float zoom = 0.5f;

	private float desiredZoom = 0.5f;

	private float zoomVelocity;

	private float minDistance = 0.5f;

	private float maxDistance = 10f;

	private float orbitPitch;

	private float orbitYaw = 180f;

	private Vector3 orbitalVelocity = Vector3.zero;

	private Vector3 orbitalVelocitySmoothDampVelocity = Vector3.zero;

	private Vector2 pan;

	private Vector2 panVelocity;

	private Vector2 panVelocitySmoothDampVelocity;

	private Vector3 pivotPoint = Vector3.zero;

	private List<Light> lights = new List<Light>();

	private Vector2 orbitDragPoint;

	private Vector2 panDragPoint;

	private int orbitDragCount;

	private int panDragCount;

	public GameObject modelPrefab
	{
		get
		{
			return _modelPrefab;
		}
		set
		{
			if (!((Object)(object)_modelPrefab == (Object)(object)value))
			{
				DestroyModelInstance();
				_modelPrefab = value;
				BuildModelInstance();
			}
		}
	}

	public RenderTexture renderTexture { get; private set; }

	private void DestroyModelInstance()
	{
		Object.Destroy((Object)(object)modelInstance);
		modelInstance = null;
	}

	private void BuildModelInstance()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)_modelPrefab) || !((Behaviour)this).enabled || Object.op_Implicit((Object)(object)modelInstance))
		{
			return;
		}
		modelInstance = Object.Instantiate<GameObject>(_modelPrefab, Vector3.zero, Quaternion.identity);
		ModelPanelParameters component = _modelPrefab.GetComponent<ModelPanelParameters>();
		if (Object.op_Implicit((Object)(object)component))
		{
			modelInstance.transform.rotation = component.modelRotation;
		}
		Util.GuessRenderBoundsMeshOnly(modelInstance, out var bounds);
		pivotPoint = ((Bounds)(ref bounds)).center;
		minDistance = Mathf.Min(new float[3]
		{
			((Bounds)(ref bounds)).size.x,
			((Bounds)(ref bounds)).size.y,
			((Bounds)(ref bounds)).size.z
		}) * 1f;
		maxDistance = Mathf.Max(new float[3]
		{
			((Bounds)(ref bounds)).size.x,
			((Bounds)(ref bounds)).size.y,
			((Bounds)(ref bounds)).size.z
		}) * 2f;
		Renderer[] componentsInChildren = modelInstance.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Component)componentsInChildren[i]).gameObject.layer = LayerIndex.noDraw.intVal;
		}
		AimAnimator[] componentsInChildren2 = modelInstance.GetComponentsInChildren<AimAnimator>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].inputBank = null;
			componentsInChildren2[j].directionComponent = null;
			((Behaviour)componentsInChildren2[j]).enabled = false;
		}
		Animator[] componentsInChildren3 = modelInstance.GetComponentsInChildren<Animator>();
		foreach (Animator obj in componentsInChildren3)
		{
			obj.SetBool("isGrounded", true);
			obj.SetFloat("aimPitchCycle", 0.5f);
			obj.SetFloat("aimYawCycle", 0.5f);
			obj.Play("Idle");
			obj.Update(0f);
		}
		IKSimpleChain[] componentsInChildren4 = modelInstance.GetComponentsInChildren<IKSimpleChain>();
		for (int l = 0; l < componentsInChildren4.Length; l++)
		{
			((Behaviour)componentsInChildren4[l]).enabled = false;
		}
		DitherModel[] componentsInChildren5 = modelInstance.GetComponentsInChildren<DitherModel>();
		for (int m = 0; m < componentsInChildren5.Length; m++)
		{
			((Behaviour)componentsInChildren5[m]).enabled = false;
		}
		PrintController[] componentsInChildren6 = modelInstance.GetComponentsInChildren<PrintController>();
		for (int m = 0; m < componentsInChildren6.Length; m++)
		{
			((Behaviour)componentsInChildren6[m]).enabled = false;
		}
		LightIntensityCurve[] componentsInChildren7 = modelInstance.GetComponentsInChildren<LightIntensityCurve>();
		foreach (LightIntensityCurve lightIntensityCurve in componentsInChildren7)
		{
			if (!lightIntensityCurve.loop)
			{
				((Behaviour)lightIntensityCurve).enabled = false;
			}
		}
		AkEvent[] componentsInChildren8 = modelInstance.GetComponentsInChildren<AkEvent>();
		for (int m = 0; m < componentsInChildren8.Length; m++)
		{
			((Behaviour)componentsInChildren8[m]).enabled = false;
		}
		desiredZoom = 0.5f;
		zoom = desiredZoom;
		zoomVelocity = 0f;
		ResetOrbitAndPan();
	}

	private void ResetOrbitAndPan()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		orbitPitch = 0f;
		orbitYaw = 0f;
		orbitalVelocity = Vector3.zero;
		orbitalVelocitySmoothDampVelocity = Vector3.zero;
		pan = Vector2.zero;
		panVelocity = Vector2.zero;
		panVelocitySmoothDampVelocity = Vector2.zero;
	}

	private void Awake()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		rawImage = ((Component)this).GetComponent<RawImage>();
		mpEventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
		cameraRigController = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/Main Camera")).GetComponent<CameraRigController>();
		((Object)((Component)cameraRigController).gameObject).name = "ModelCamera";
		((Component)cameraRigController.uiCam).gameObject.SetActive(false);
		cameraRigController.createHud = false;
		cameraRigController.enableFading = false;
		GameObject gameObject = ((Component)cameraRigController.sceneCam).gameObject;
		modelCamera = gameObject.AddComponent<ModelCamera>();
		((Component)cameraRigController).transform.position = -Vector3.forward * 10f;
		((Component)cameraRigController).transform.forward = Vector3.forward;
		CameraResolutionScaler component = gameObject.GetComponent<CameraResolutionScaler>();
		if (Object.op_Implicit((Object)(object)component))
		{
			((Behaviour)component).enabled = false;
		}
		Camera sceneCam = cameraRigController.sceneCam;
		sceneCam.backgroundColor = Color.clear;
		sceneCam.clearFlags = (CameraClearFlags)2;
		if (disablePostProcessLayer)
		{
			PostProcessLayer component2 = ((Component)sceneCam).GetComponent<PostProcessLayer>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				((Behaviour)component2).enabled = false;
			}
		}
		Vector3 eulerAngles = ((Component)cameraRigController).transform.eulerAngles;
		orbitPitch = eulerAngles.x;
		orbitYaw = eulerAngles.y;
		modelCamera.attachedCamera.backgroundColor = camBackgroundColor;
		modelCamera.attachedCamera.clearFlags = (CameraClearFlags)2;
		modelCamera.attachedCamera.cullingMask = LayerMask.op_Implicit(LayerIndex.manualRender.mask);
		if (Object.op_Implicit((Object)(object)headlightPrefab))
		{
			headlight = Object.Instantiate<GameObject>(headlightPrefab, ((Component)modelCamera).transform).GetComponent<Light>();
			if (Object.op_Implicit((Object)(object)headlight))
			{
				((Component)headlight).gameObject.SetActive(true);
				modelCamera.AddLight(headlight);
			}
		}
		for (int i = 0; i < lightPrefabs.Length; i++)
		{
			GameObject obj = Object.Instantiate<GameObject>(lightPrefabs[i]);
			Light component3 = obj.GetComponent<Light>();
			obj.SetActive(true);
			lights.Add(component3);
			modelCamera.AddLight(component3);
		}
	}

	public void Start()
	{
		BuildRenderTexture();
		desiredZoom = 0.5f;
		zoom = desiredZoom;
		zoomVelocity = 0f;
	}

	private void OnDestroy()
	{
		Object.Destroy((Object)(object)renderTexture);
		if (Object.op_Implicit((Object)(object)cameraRigController))
		{
			Object.Destroy((Object)(object)((Component)cameraRigController).gameObject);
		}
		foreach (Light light in lights)
		{
			Object.Destroy((Object)(object)((Component)light).gameObject);
		}
	}

	private void OnDisable()
	{
		DestroyModelInstance();
	}

	private void OnEnable()
	{
		BuildModelInstance();
	}

	public void Update()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		UpdateForModelViewer(Time.unscaledDeltaTime);
		if (enableGamepadControls && (!Object.op_Implicit((Object)(object)requiredTopLayer) || requiredTopLayer.representsTopLayer) && mpEventSystemLocator.eventSystem.currentInputSource == MPEventSystem.InputSource.Gamepad && Object.op_Implicit((Object)(object)mpEventSystemLocator.eventSystem))
		{
			float axis = mpEventSystemLocator.eventSystem.player.GetAxis(16);
			float axis2 = mpEventSystemLocator.eventSystem.player.GetAxis(17);
			bool button = mpEventSystemLocator.eventSystem.player.GetButton(29);
			bool button2 = mpEventSystemLocator.eventSystem.player.GetButton(30);
			Vector3 zero = Vector3.zero;
			zero.y = (0f - axis) * gamepadRotateSensitivity;
			zero.x = axis2 * gamepadRotateSensitivity;
			orbitalVelocity = zero;
			if (button != button2)
			{
				desiredZoom = Mathf.Clamp01(desiredZoom + (button ? 0.1f : (-0.1f)) * Time.deltaTime * gamepadZoomSensitivity);
			}
		}
	}

	public void LateUpdate()
	{
		modelCamera.attachedCamera.aspect = (float)((Texture)renderTexture).width / (float)((Texture)renderTexture).height;
		cameraRigController.baseFov = fov;
		modelCamera.renderSettings = renderSettings;
		modelCamera.RenderItem(modelInstance, renderTexture);
	}

	private void OnRectTransformDimensionsChange()
	{
		BuildRenderTexture();
	}

	private void BuildRenderTexture()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		if (!Object.op_Implicit((Object)(object)rectTransform))
		{
			return;
		}
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		rectTransform.GetLocalCorners(array);
		Rect rect = rectTransform.rect;
		Vector2 size = ((Rect)(ref rect)).size;
		int num = Mathf.FloorToInt(size.x);
		int num2 = Mathf.FloorToInt(size.y);
		if (!Object.op_Implicit((Object)(object)renderTexture) || ((Texture)renderTexture).width != num || ((Texture)renderTexture).height != num2)
		{
			Object.Destroy((Object)(object)renderTexture);
			renderTexture = null;
			if (num > 0 && num2 > 0)
			{
				RenderTextureDescriptor val = default(RenderTextureDescriptor);
				((RenderTextureDescriptor)(ref val))._002Ector(num, num2, (RenderTextureFormat)0);
				((RenderTextureDescriptor)(ref val)).sRGB = true;
				renderTexture = new RenderTexture(val);
				renderTexture.useMipMap = false;
				((Texture)renderTexture).filterMode = (FilterMode)1;
			}
			rawImage.texture = (Texture)(object)renderTexture;
		}
	}

	private void UpdateForModelViewer(float deltaTime)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		zoom = Mathf.SmoothDamp(zoom, desiredZoom, ref zoomVelocity, 0.1f);
		orbitPitch %= 360f;
		if (orbitPitch < -180f)
		{
			orbitPitch += 360f;
		}
		else if (orbitPitch > 180f)
		{
			orbitPitch -= 360f;
		}
		orbitPitch = Mathf.Clamp(orbitPitch + orbitalVelocity.x * deltaTime, -89f, 89f);
		orbitYaw += orbitalVelocity.y * deltaTime;
		orbitalVelocity = Vector3.SmoothDamp(orbitalVelocity, Vector3.zero, ref orbitalVelocitySmoothDampVelocity, 0.25f, 2880f, deltaTime);
		if (orbitDragCount > 0)
		{
			orbitalVelocity = Vector3.zero;
			orbitalVelocitySmoothDampVelocity = Vector3.zero;
		}
		pan += panVelocity * deltaTime;
		panVelocity = Vector2.SmoothDamp(panVelocity, Vector2.zero, ref panVelocitySmoothDampVelocity, 0.25f, 100f, deltaTime);
		if (panDragCount > 0)
		{
			panVelocity = Vector2.zero;
			panVelocitySmoothDampVelocity = Vector2.zero;
		}
		Quaternion val = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
		((Component)cameraRigController).transform.forward = val * Vector3.forward;
		Vector3 forward = ((Component)cameraRigController).transform.forward;
		Vector3 position = pivotPoint + forward * (0f - Mathf.LerpUnclamped(minDistance, maxDistance, zoom)) + ((Component)cameraRigController).transform.up * pan.y + ((Component)cameraRigController).transform.right * pan.x;
		((Component)cameraRigController).transform.position = position;
	}

	public void SetAnglesForCharacterThumbnailForSeconds(float time, bool setZoom = false)
	{
		SetAnglesForCharacterThumbnail(setZoom);
		float t = time;
		Action func = null;
		func = delegate
		{
			t -= Time.deltaTime;
			if (Object.op_Implicit((Object)(object)this))
			{
				SetAnglesForCharacterThumbnail(setZoom);
			}
			if (t <= 0f)
			{
				RoR2Application.onUpdate -= func;
			}
		};
		RoR2Application.onUpdate += func;
	}

	public void SetAnglesForCharacterThumbnail(bool setZoom = false)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)modelInstance))
		{
			CameraFramingCalculator cameraFramingCalculator = new CameraFramingCalculator(modelInstance);
			cameraFramingCalculator.GetCharacterThumbnailPosition(fov);
			pivotPoint = cameraFramingCalculator.outputPivotPoint;
			minDistance = cameraFramingCalculator.outputMinDistance;
			maxDistance = cameraFramingCalculator.outputMaxDistance;
			ResetOrbitAndPan();
			Vector3 eulerAngles = ((Quaternion)(ref cameraFramingCalculator.outputCameraRotation)).eulerAngles;
			orbitPitch = eulerAngles.x;
			orbitYaw = eulerAngles.y;
			if (setZoom)
			{
				zoom = Util.Remap(Vector3.Distance(cameraFramingCalculator.outputCameraPosition, cameraFramingCalculator.outputPivotPoint), minDistance, maxDistance, 0f, 1f);
				desiredZoom = zoom;
			}
			zoomVelocity = 0f;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 1)
		{
			orbitDragCount++;
			if (orbitDragCount == 1)
			{
				orbitDragPoint = eventData.pressPosition;
			}
		}
		else if ((int)eventData.button == 0)
		{
			panDragCount++;
			if (panDragCount == 1)
			{
				panDragPoint = eventData.pressPosition;
			}
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((int)eventData.button == 1)
		{
			orbitDragCount--;
		}
		else if ((int)eventData.button == 0)
		{
			panDragCount--;
		}
		OnDrag(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		if ((int)eventData.button == 1)
		{
			Vector2 val = eventData.position - orbitDragPoint;
			orbitDragPoint = eventData.position;
			float num = 0.5f / unscaledDeltaTime;
			orbitalVelocity = new Vector3((0f - val.y) * num * 0.5f, val.x * num, 0f);
		}
		else
		{
			Vector2 val2 = eventData.position - panDragPoint;
			panDragPoint = eventData.position;
			float num2 = -0.01f;
			panVelocity = val2 * num2 / unscaledDeltaTime;
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		desiredZoom = Mathf.Clamp01(desiredZoom + eventData.scrollDelta.y * -0.05f);
	}
}
