using System;
using System.Collections.Generic;
using HG;
using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(PointViewer))]
public class SniperTargetViewer : MonoBehaviour
{
	public GameObject visualizerPrefab;

	private PointViewer pointViewer;

	private HUD hud;

	private Dictionary<UnityObjectWrapperKey<HurtBox>, GameObject> hurtBoxToVisualizer = new Dictionary<UnityObjectWrapperKey<HurtBox>, GameObject>();

	private List<HurtBox> displayedTargets = new List<HurtBox>();

	private List<HurtBox> previousDisplayedTargets = new List<HurtBox>();

	private void Awake()
	{
		pointViewer = ((Component)this).GetComponent<PointViewer>();
		OnTransformParentChanged();
	}

	private void OnTransformParentChanged()
	{
		hud = ((Component)this).GetComponentInParent<HUD>();
	}

	private void OnDisable()
	{
		SetDisplayedTargets(Array.Empty<HurtBox>());
		hurtBoxToVisualizer.Clear();
	}

	private void Update()
	{
		List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
		if (Object.op_Implicit((Object)(object)hud) && Object.op_Implicit((Object)(object)hud.targetMaster))
		{
			TeamIndex teamIndex = hud.targetMaster.teamIndex;
			IReadOnlyList<HurtBox> readOnlySniperTargetsList = HurtBox.readOnlySniperTargetsList;
			int i = 0;
			for (int count = readOnlySniperTargetsList.Count; i < count; i++)
			{
				HurtBox hurtBox = readOnlySniperTargetsList[i];
				if (Object.op_Implicit((Object)(object)hurtBox.healthComponent) && hurtBox.healthComponent.alive && FriendlyFireManager.ShouldDirectHitProceed(hurtBox.healthComponent, teamIndex) && hurtBox.healthComponent.body != hud.targetMaster.GetBody())
				{
					list.Add(hurtBox);
				}
			}
		}
		SetDisplayedTargets(list);
		list = CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
	}

	private void OnTargetDiscovered(HurtBox hurtBox)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (!hurtBoxToVisualizer.ContainsKey(UnityObjectWrapperKey<HurtBox>.op_Implicit(hurtBox)))
		{
			GameObject value = pointViewer.AddElement(new PointViewer.AddElementRequest
			{
				elementPrefab = visualizerPrefab,
				target = ((Component)hurtBox).transform,
				targetWorldVerticalOffset = 0f,
				targetWorldRadius = HurtBox.sniperTargetRadius,
				scaleWithDistance = true
			});
			hurtBoxToVisualizer.Add(UnityObjectWrapperKey<HurtBox>.op_Implicit(hurtBox), value);
		}
		else
		{
			Debug.LogWarning((object)$"Already discovered hurtbox: {hurtBox}");
		}
	}

	private void OnTargetLost(HurtBox hurtBox)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (hurtBoxToVisualizer.TryGetValue(UnityObjectWrapperKey<HurtBox>.op_Implicit(hurtBox), out var value))
		{
			pointViewer.RemoveElement(value);
		}
	}

	private void SetDisplayedTargets(IReadOnlyList<HurtBox> newDisplayedTargets)
	{
		Util.Swap(ref displayedTargets, ref previousDisplayedTargets);
		displayedTargets.Clear();
		ListUtils.AddRange<HurtBox, IReadOnlyList<HurtBox>>(displayedTargets, newDisplayedTargets);
		List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
		List<HurtBox> list2 = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
		ListUtils.FindExclusiveEntriesByReference<HurtBox>(displayedTargets, previousDisplayedTargets, list, list2);
		foreach (HurtBox item in list2)
		{
			OnTargetLost(item);
		}
		foreach (HurtBox item2 in list)
		{
			OnTargetDiscovered(item2);
		}
		list2 = CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list2);
		list = CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
	}
}
