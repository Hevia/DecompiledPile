using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoR2.UI;

public class UIElementAllocator<T> where T : Component
{
	public delegate void ElementOperationDelegate(int index, T element);

	public readonly RectTransform containerTransform;

	public readonly GameObject elementPrefab;

	public readonly bool markElementsUnsavable;

	[NotNull]
	private List<T> elementControllerComponentsList;

	[NotNull]
	public readonly ReadOnlyCollection<T> elements;

	[CanBeNull]
	public ElementOperationDelegate onCreateElement;

	[CanBeNull]
	public ElementOperationDelegate onDestroyElement;

	public UIElementAllocator([NotNull] RectTransform containerTransform, [NotNull] GameObject elementPrefab, bool markElementsUnsavable = true, bool acquireExistingChildren = false)
	{
		this.containerTransform = containerTransform;
		this.elementPrefab = elementPrefab;
		this.markElementsUnsavable = markElementsUnsavable;
		elementControllerComponentsList = new List<T>();
		elements = new ReadOnlyCollection<T>(elementControllerComponentsList);
		if (!acquireExistingChildren)
		{
			return;
		}
		for (int i = 0; i < ((Transform)containerTransform).childCount; i++)
		{
			T component = ((Component)((Transform)containerTransform).GetChild(i)).GetComponent<T>();
			if (Object.op_Implicit((Object)(object)component) && ((Component)component).gameObject.activeInHierarchy)
			{
				elementControllerComponentsList.Add(component);
			}
		}
	}

	private void DestroyElementAt(int i)
	{
		T val = elementControllerComponentsList[i];
		onDestroyElement?.Invoke(i, val);
		GameObject gameObject = ((Component)val).gameObject;
		if (Application.isPlaying)
		{
			Object.Destroy((Object)(object)gameObject);
		}
		else
		{
			Object.DestroyImmediate((Object)(object)gameObject);
		}
		elementControllerComponentsList.RemoveAt(i);
	}

	public void AllocateElements(int desiredCount)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (desiredCount < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		Scene scene = ((Component)containerTransform).gameObject.scene;
		if (!((Scene)(ref scene)).IsValid())
		{
			return;
		}
		for (int num = elementControllerComponentsList.Count - 1; num >= desiredCount; num--)
		{
			DestroyElementAt(num);
		}
		for (int i = elementControllerComponentsList.Count; i < desiredCount; i++)
		{
			T component = Object.Instantiate<GameObject>(elementPrefab, (Transform)(object)containerTransform).GetComponent<T>();
			elementControllerComponentsList.Add(component);
			GameObject gameObject = ((Component)component).gameObject;
			if (markElementsUnsavable)
			{
				((Object)gameObject).hideFlags = (HideFlags)(((Object)gameObject).hideFlags | 0x14);
			}
			gameObject.SetActive(true);
			onCreateElement?.Invoke(i, component);
		}
	}

	public void MoveElementsToContainerEnd()
	{
		int num = ((Transform)containerTransform).childCount - elementControllerComponentsList.Count;
		for (int num2 = elementControllerComponentsList.Count - 1; num2 >= 0; num2--)
		{
			((Component)elementControllerComponentsList[num2]).transform.SetSiblingIndex(num + num2);
		}
	}
}
