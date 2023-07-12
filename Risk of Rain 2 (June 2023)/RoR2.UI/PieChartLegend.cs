using System;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class PieChartLegend : MonoBehaviour
{
	[SerializeField]
	private PieChartMeshController _source;

	public RectTransform container;

	public GameObject stripPrefab;

	private UIElementAllocator<ChildLocator> stripAllocator;

	private PieChartMeshController _subscribedSource;

	public PieChartMeshController source
	{
		get
		{
			return _source;
		}
		set
		{
			if (_source != value)
			{
				_source = value;
				subscribedSource = source;
			}
		}
	}

	private PieChartMeshController subscribedSource
	{
		get
		{
			return _subscribedSource;
		}
		set
		{
			if (_subscribedSource != value)
			{
				if (Object.op_Implicit((Object)(object)_subscribedSource))
				{
					_subscribedSource.onRebuilt -= OnSourceUpdated;
				}
				_subscribedSource = value;
				if (Object.op_Implicit((Object)(object)_subscribedSource))
				{
					_subscribedSource.onRebuilt += OnSourceUpdated;
				}
			}
		}
	}

	private void Awake()
	{
		InitStripAllocator();
		subscribedSource = source;
	}

	private void OnDestroy()
	{
		subscribedSource = null;
	}

	private void OnValidate()
	{
		InitStripAllocator();
		subscribedSource = source;
		((MonoBehaviour)this).Invoke("Rebuild", 0f);
	}

	private void InitStripAllocator()
	{
		if (stripAllocator != null && ((Object)(object)stripAllocator.containerTransform != (Object)(object)container || (Object)(object)stripAllocator.elementPrefab != (Object)(object)stripPrefab))
		{
			stripAllocator.AllocateElements(0);
			stripAllocator = null;
		}
		if (stripAllocator == null)
		{
			stripAllocator = new UIElementAllocator<ChildLocator>(container, stripPrefab, markElementsUnsavable: true, acquireExistingChildren: true);
		}
	}

	private void Rebuild()
	{
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)source) || !Object.op_Implicit((Object)(object)stripAllocator.containerTransform) || !Object.op_Implicit((Object)(object)stripAllocator.elementPrefab))
		{
			stripAllocator.AllocateElements(0);
			return;
		}
		int num = 0;
		for (int i = 0; i < source.sliceCount; i++)
		{
			if (!(source.GetSliceInfo(i).weight / source.totalSliceWeight <= source.minimumRequiredWeightToDisplay) || num < 10)
			{
				num++;
			}
		}
		stripAllocator.AllocateElements(num);
		ReadOnlyCollection<ChildLocator> elements = stripAllocator.elements;
		int j = 0;
		int num2 = Math.Min(num, elements.Count);
		int num3 = 0;
		for (; j < num2; j++)
		{
			PieChartMeshController.SliceInfo sliceInfo = source.GetSliceInfo(j);
			if (!(source.GetSliceInfo(j).weight / source.totalSliceWeight <= source.minimumRequiredWeightToDisplay) || num3 < 10)
			{
				num3++;
				ChildLocator childLocator = elements[j];
				Transform obj = childLocator.FindChild("ColorBox");
				Graphic val = ((obj != null) ? ((Component)obj).GetComponent<Graphic>() : null);
				Transform obj2 = childLocator.FindChild("Label");
				TMP_Text val2 = ((obj2 != null) ? ((Component)obj2).GetComponent<TMP_Text>() : null);
				if (Object.op_Implicit((Object)(object)val))
				{
					val.color = sliceInfo.color;
				}
				if (Object.op_Implicit((Object)(object)val2))
				{
					val2.SetText(sliceInfo.tooltipContent.GetTitleText(), true);
				}
			}
		}
	}

	private void OnSourceUpdated()
	{
		Rebuild();
	}
}
