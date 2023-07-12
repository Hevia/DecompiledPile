using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HG;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class AllyCardManager : MonoBehaviour, ILayoutGroup, ILayoutController
{
	private struct SourceInfo
	{
		public readonly GameObject gameObject;

		public readonly TeamComponent teamComponent;

		public readonly CharacterMaster master;

		public SourceInfo(GameObject gameObject)
		{
			bool flag = Object.op_Implicit((Object)(object)gameObject);
			this.gameObject = gameObject;
			teamComponent = (flag ? gameObject.GetComponent<TeamComponent>() : null);
			CharacterBody characterBody = (flag ? gameObject.GetComponent<CharacterBody>() : null);
			master = (Object.op_Implicit((Object)(object)characterBody) ? characterBody.master : null);
		}
	}

	private struct DisplayElement
	{
		public CharacterMaster master;

		public bool shouldIndent;

		public int priority;
	}

	private class DisplayElementComparer : IComparer<DisplayElement>
	{
		public static DisplayElementComparer instance = new DisplayElementComparer();

		public int Compare(DisplayElement a, DisplayElement b)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			int num = a.priority - b.priority;
			if (num == 0)
			{
				NetworkInstanceId netId = ((NetworkBehaviour)a.master).netId;
				uint value = ((NetworkInstanceId)(ref netId)).Value;
				netId = ((NetworkBehaviour)b.master).netId;
				num = (int)(value - ((NetworkInstanceId)(ref netId)).Value);
			}
			return num;
		}

		private DisplayElementComparer()
		{
		}
	}

	private struct CharacterData
	{
		public readonly CharacterMaster master;

		public readonly CharacterMaster leaderMaster;

		private readonly int masterInstanceId;

		private readonly int leaderMasterInstanceId;

		public readonly bool isMinion;

		public readonly bool isPlayer;

		public CharacterData(CharacterMaster master)
		{
			this.master = master;
			leaderMaster = master.minionOwnership.ownerMaster;
			isMinion = Object.op_Implicit((Object)(object)leaderMaster);
			isPlayer = Object.op_Implicit((Object)(object)master.playerCharacterMasterController);
			masterInstanceId = ((Object)((Component)master).gameObject).GetInstanceID();
			leaderMasterInstanceId = (isMinion ? ((Object)((Component)leaderMaster).gameObject).GetInstanceID() : 0);
		}

		public bool Equals(in CharacterData other)
		{
			if (masterInstanceId == other.masterInstanceId && leaderMasterInstanceId == other.leaderMasterInstanceId && isMinion == other.isMinion)
			{
				return isPlayer == other.isPlayer;
			}
			return false;
		}
	}

	private class CharacterDataSet
	{
		private CharacterData[] buffer = new CharacterData[128];

		private int _count;

		public int count => _count;

		public ref CharacterData this[int i] => ref buffer[i];

		public bool Equals(CharacterDataSet other)
		{
			if (other == null)
			{
				return false;
			}
			if (_count != other._count)
			{
				return false;
			}
			for (int i = 0; i < _count; i++)
			{
				if (!buffer[i].Equals(in other.buffer[i]))
				{
					return false;
				}
			}
			return true;
		}

		public void Clear()
		{
			Array.Clear(buffer, 0, _count);
			_count = 0;
		}

		public void Add(ref CharacterData element)
		{
			ArrayUtils.ArrayAppend<CharacterData>(ref buffer, ref _count, ref element);
		}

		public void CopyFrom(CharacterDataSet src)
		{
			int num = _count - src._count;
			if (num > 0)
			{
				Array.Clear(buffer, src._count, num);
			}
			ArrayUtils.EnsureCapacity<CharacterData>(ref buffer, src.buffer.Length);
			_count = src.count;
			Array.Copy(src.buffer, buffer, _count);
		}
	}

	private static class CharacterDataSetPool
	{
		private static readonly Stack<CharacterDataSet> pool = new Stack<CharacterDataSet>();

		public static CharacterDataSet Request()
		{
			if (pool.Count == 0)
			{
				return new CharacterDataSet();
			}
			return pool.Pop();
		}

		public static void Return(ref CharacterDataSet characterDataSet)
		{
			characterDataSet.Clear();
			pool.Push(characterDataSet);
		}
	}

	public float indentWidth = 16f;

	private SourceInfo currentSource;

	private UIElementAllocator<AllyCardController> cardAllocator;

	private RectTransform rectTransform;

	private bool needsRefresh;

	private DisplayElement[] displayElements = Array.Empty<DisplayElement>();

	private int displayElementCount;

	private CharacterDataSet currentCharacterData;

	public GameObject sourceGameObject
	{
		get
		{
			return currentSource.gameObject;
		}
		set
		{
			if (currentSource.gameObject != value)
			{
				currentSource = new SourceInfo(value);
				OnSourceChanged();
			}
		}
	}

	private void OnCardCreated(int index, AllyCardController element)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 anchorMin = element.rectTransform.anchorMin;
		anchorMin.x = 0f;
		anchorMin.y = 1f;
		element.rectTransform.anchorMin = anchorMin;
		anchorMin = element.rectTransform.anchorMax;
		anchorMin.x = 1f;
		anchorMin.y = 1f;
		element.rectTransform.anchorMax = anchorMin;
	}

	private void OnSourceChanged()
	{
		needsRefresh = true;
	}

	private TeamIndex FindTargetTeam()
	{
		TeamIndex result = TeamIndex.None;
		TeamComponent teamComponent = currentSource.teamComponent;
		if (Object.op_Implicit((Object)(object)teamComponent))
		{
			result = teamComponent.teamIndex;
		}
		return result;
	}

	private void SetCharacterData(CharacterDataSet newCharacterData)
	{
		if (!newCharacterData.Equals(currentCharacterData))
		{
			currentCharacterData.CopyFrom(newCharacterData);
			BuildFromCharacterData(currentCharacterData);
		}
	}

	private void PopulateCharacterDataSet(CharacterDataSet characterDataSet)
	{
		TeamIndex teamIndex = FindTargetTeam();
		ReadOnlyCollection<CharacterMaster> readOnlyInstancesList = CharacterMaster.readOnlyInstancesList;
		for (int i = 0; i < readOnlyInstancesList.Count; i++)
		{
			CharacterMaster characterMaster = readOnlyInstancesList[i];
			if (characterMaster.teamIndex == teamIndex)
			{
				CharacterBody body = characterMaster.GetBody();
				if ((!Object.op_Implicit((Object)(object)body) || !Object.op_Implicit((Object)(object)body.teamComponent) || !body.teamComponent.hideAllyCardDisplay) && (!Object.op_Implicit((Object)(object)characterMaster.playerCharacterMasterController) || Object.op_Implicit((Object)(object)characterMaster.playerCharacterMasterController.networkUser)) && currentSource.master != characterMaster)
				{
					CharacterData element = new CharacterData(characterMaster);
					characterDataSet.Add(ref element);
				}
			}
		}
	}

	private void BuildFromCharacterData(CharacterDataSet characterDataSet)
	{
		if (characterDataSet.count < displayElementCount)
		{
			Array.Clear(displayElements, characterDataSet.count, displayElementCount - characterDataSet.count);
		}
		displayElementCount = characterDataSet.count;
		ArrayUtils.EnsureCapacity<DisplayElement>(ref displayElements, displayElementCount);
		int i = 0;
		for (int count = characterDataSet.count; i < count; i++)
		{
			ref CharacterData reference = ref characterDataSet[i];
			displayElements[i] = new DisplayElement
			{
				master = reference.master,
				priority = -1
			};
		}
		int num = 0;
		int j = 0;
		for (int count2 = characterDataSet.count; j < count2; j++)
		{
			if (characterDataSet[j].isPlayer)
			{
				displayElements[j].priority = num;
				num += 2;
			}
		}
		int k = 0;
		for (int count3 = characterDataSet.count; k < count3; k++)
		{
			if (!characterDataSet[k].isMinion)
			{
				ref DisplayElement reference2 = ref displayElements[k];
				if (reference2.priority == -1)
				{
					reference2.priority = num;
					num += 2;
				}
			}
		}
		int l = 0;
		for (int count4 = characterDataSet.count; l < count4; l++)
		{
			ref CharacterData reference3 = ref characterDataSet[l];
			if (!reference3.isMinion)
			{
				continue;
			}
			ref DisplayElement reference4 = ref displayElements[l];
			if (reference4.priority == -1)
			{
				int num2 = FindIndexForMaster(reference3.leaderMaster);
				if (num2 != -1)
				{
					reference4.priority = displayElements[num2].priority + 1;
					reference4.shouldIndent = true;
				}
			}
		}
		int m = 0;
		for (int count5 = characterDataSet.count; m < count5; m++)
		{
			ref DisplayElement reference5 = ref displayElements[m];
			if (reference5.priority == -1)
			{
				reference5.priority = num;
				num += 2;
			}
		}
		Array.Sort(displayElements, 0, comparer: DisplayElementComparer.instance, length: displayElementCount);
		cardAllocator.AllocateElements(displayElementCount);
		for (int n = 0; n < displayElementCount; n++)
		{
			ref DisplayElement reference6 = ref displayElements[n];
			AllyCardController allyCardController = cardAllocator.elements[n];
			allyCardController.sourceMaster = reference6.master;
			allyCardController.shouldIndent = reference6.shouldIndent;
		}
		ArrayUtils.Clear<DisplayElement>(displayElements, ref displayElementCount);
		int FindIndexForMaster(CharacterMaster master)
		{
			for (int num3 = 0; num3 < displayElementCount; num3++)
			{
				if (master == displayElements[num3].master)
				{
					return num3;
				}
			}
			return -1;
		}
	}

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		cardAllocator = new UIElementAllocator<AllyCardController>((RectTransform)((Component)this).transform, LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/AllyCard"));
		rectTransform = (RectTransform)((Component)this).transform;
		UIElementAllocator<AllyCardController> uIElementAllocator = cardAllocator;
		uIElementAllocator.onCreateElement = (UIElementAllocator<AllyCardController>.ElementOperationDelegate)Delegate.Combine(uIElementAllocator.onCreateElement, new UIElementAllocator<AllyCardController>.ElementOperationDelegate(OnCardCreated));
	}

	private void FixedUpdate()
	{
		CharacterDataSet characterDataSet = CharacterDataSetPool.Request();
		PopulateCharacterDataSet(characterDataSet);
		SetCharacterData(characterDataSet);
		CharacterDataSetPool.Return(ref characterDataSet);
	}

	private void OnEnable()
	{
		needsRefresh = true;
		currentCharacterData = CharacterDataSetPool.Request();
	}

	private void OnDisable()
	{
		CharacterDataSetPool.Return(ref currentCharacterData);
	}

	public void SetLayoutHorizontal()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (cardAllocator != null)
		{
			ReadOnlyCollection<AllyCardController> elements = cardAllocator.elements;
			int i = 0;
			for (int count = elements.Count; i < count; i++)
			{
				AllyCardController allyCardController = elements[i];
				RectTransform obj = allyCardController.rectTransform;
				Vector2 offsetMin = obj.offsetMin;
				offsetMin.x = (allyCardController.shouldIndent ? indentWidth : 0f);
				obj.offsetMin = offsetMin;
				offsetMin = obj.offsetMax;
				offsetMin.x = 0f;
				obj.offsetMax = offsetMin;
			}
		}
	}

	public void SetLayoutVertical()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (cardAllocator != null)
		{
			ReadOnlyCollection<AllyCardController> elements = cardAllocator.elements;
			Rect rect = rectTransform.rect;
			float num = ((Rect)(ref rect)).height / (float)elements.Count;
			float num2 = 0f;
			int i = 0;
			for (int count = elements.Count; i < count; i++)
			{
				AllyCardController allyCardController = elements[i];
				RectTransform val = allyCardController.rectTransform;
				float preferredHeight = allyCardController.layoutElement.preferredHeight;
				val.SetSizeWithCurrentAnchors((Axis)1, preferredHeight);
				Vector2 anchoredPosition = val.anchoredPosition;
				anchoredPosition.y = num2;
				val.anchoredPosition = anchoredPosition;
				num2 -= Mathf.Min(preferredHeight, num);
			}
		}
	}
}
