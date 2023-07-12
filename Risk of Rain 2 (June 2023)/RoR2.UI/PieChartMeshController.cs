using System;
using System.Collections.ObjectModel;
using HG;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace RoR2.UI;

[ExecuteAlways]
public class PieChartMeshController : MonoBehaviour
{
	[Serializable]
	public struct SliceInfo : IEquatable<SliceInfo>
	{
		[Min(0f)]
		public float weight;

		public Color color;

		public TooltipContent tooltipContent;

		public bool Equals(SliceInfo other)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (weight.Equals(other.weight) && ((Color)(ref color)).Equals(other.color))
			{
				return tooltipContent.Equals(other.tooltipContent);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is SliceInfo other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((weight.GetHashCode() * 397) ^ ((object)(Color)(ref color)).GetHashCode()) * 397) ^ tooltipContent.GetHashCode();
		}

		public static bool operator ==(SliceInfo left, SliceInfo right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(SliceInfo left, SliceInfo right)
		{
			return !left.Equals(right);
		}
	}

	public RectTransform container;

	public GameObject slicePrefab;

	[Range(0f, 1f)]
	public float normalizedInnerRadius;

	[Tooltip("The minimum value required to display percentage and show an entry in the legend.")]
	public float minimumRequiredWeightToDisplay = 0.03f;

	public Texture texture;

	public Material material;

	[ColorUsage(true, true)]
	public Color color;

	public float uTile = 1f;

	public bool individualSegmentUMapping;

	[SerializeField]
	private SliceInfo[] sliceInfos = Array.Empty<SliceInfo>();

	private int totalSliceCount;

	private UIElementAllocator<RadialSliceGraphic> sliceAllocator;

	private bool slicesDirty = true;

	public float totalSliceWeight { get; private set; }

	public int sliceCount => sliceInfos.Length;

	public event Action onRebuilt;

	private void Awake()
	{
		InitSliceAllocator();
	}

	private void Update()
	{
		if (slicesDirty)
		{
			slicesDirty = false;
			RebuildSlices();
		}
	}

	private void OnValidate()
	{
		InitSliceAllocator();
		slicesDirty = true;
	}

	private void InitSliceAllocator()
	{
		sliceAllocator = new UIElementAllocator<RadialSliceGraphic>(container, slicePrefab, markElementsUnsavable: true, acquireExistingChildren: true);
	}

	public void SetSlices([NotNull] SliceInfo[] newSliceInfos)
	{
		if (!ArrayUtils.SequenceEquals<SliceInfo>(sliceInfos, newSliceInfos))
		{
			Array.Resize(ref sliceInfos, newSliceInfos.Length);
			Array.Copy(newSliceInfos, sliceInfos, sliceInfos.Length);
			slicesDirty = true;
		}
	}

	private void RebuildSlices()
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		totalSliceWeight = 0f;
		totalSliceCount = 0;
		for (int i = 0; i < sliceInfos.Length; i++)
		{
			float weight = sliceInfos[i].weight;
			if (!(weight <= 0f))
			{
				totalSliceCount++;
				totalSliceWeight += weight;
			}
		}
		sliceAllocator.AllocateElements(totalSliceCount);
		Radians start = default(Radians);
		((Radians)(ref start))._002Ector(MathF.PI / 2f);
		float num = 0f;
		ReadOnlyCollection<RadialSliceGraphic> elements = sliceAllocator.elements;
		int num2 = 0;
		int num3 = 0;
		int count = elements.Count;
		Radians val = default(Radians);
		Radians val3 = default(Radians);
		while (num3 < count)
		{
			ref SliceInfo reference = ref sliceInfos[num2++];
			if (reference.weight <= 0f)
			{
				continue;
			}
			float num4 = reference.weight / totalSliceWeight;
			((Radians)(ref val))._002Ector(num4 * (MathF.PI * 2f));
			Radians val2 = (ref start) - (ref val);
			((Radians)(ref val3))._002Ector(MathF.PI * 2f);
			float num5 = num - num4 * uTile;
			RadialSliceGraphic radialSliceGraphic = elements[num3++];
			RadialSliceGraphic.DisplayData newDisplayData = new RadialSliceGraphic.DisplayData
			{
				start = start,
				end = val2,
				startU = (individualSegmentUMapping ? 0f : num),
				endU = (individualSegmentUMapping ? uTile : num5),
				normalizedInnerRadius = normalizedInnerRadius
			};
			float num6 = 720f;
			newDisplayData.maxQuadWidth = (ref val3) / (ref num6);
			newDisplayData.material = material;
			newDisplayData.texture = texture;
			newDisplayData.color = reference.color * color;
			radialSliceGraphic.SetDisplayData(in newDisplayData);
			TooltipProvider component = ((Component)radialSliceGraphic).GetComponent<TooltipProvider>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.SetContent(reference.tooltipContent);
			}
			ChildLocator component2 = ((Component)radialSliceGraphic).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				Transform val4 = component2.FindChild("SliceCenterStickerLabel");
				if (Object.op_Implicit((Object)(object)val4))
				{
					((Component)val4).GetComponent<TMP_Text>().SetText((num4 >= minimumRequiredWeightToDisplay) ? Language.GetStringFormatted("PERCENT_FORMAT", num4) : string.Empty, true);
				}
			}
			start = val2;
			num = num5;
		}
		this.onRebuilt?.Invoke();
	}

	public SliceInfo GetSliceInfo(int i)
	{
		return sliceInfos[i];
	}
}
