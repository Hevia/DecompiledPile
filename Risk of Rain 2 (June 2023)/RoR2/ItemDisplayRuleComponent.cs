using UnityEngine;

namespace RoR2;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[SelectionBase]
public class ItemDisplayRuleComponent : MonoBehaviour
{
	public Object keyAsset;

	public LimbFlags limbMask;

	[SerializeField]
	[HideInInspector]
	private ItemDisplayRuleType _ruleType;

	public string nameInLocator;

	[HideInInspector]
	[SerializeField]
	private GameObject _prefab;

	private GameObject prefabInstance;

	private ItemDisplayRule itemDisplayRule;

	public ItemDisplayRuleType ruleType
	{
		get
		{
			return _ruleType;
		}
		set
		{
			_ruleType = value;
			if (_ruleType != 0)
			{
				prefab = null;
			}
		}
	}

	public GameObject prefab
	{
		get
		{
			return _prefab;
		}
		set
		{
			if (!Object.op_Implicit((Object)(object)prefabInstance) || (Object)(object)_prefab != (Object)(object)value)
			{
				_prefab = value;
				BuildPreview();
			}
		}
	}

	private void Start()
	{
		BuildPreview();
	}

	public bool SetItemDisplayRule(ItemDisplayRule newItemDisplayRule, ChildLocator childLocator, Object newKeyAsset)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		itemDisplayRule = newItemDisplayRule;
		string childName = itemDisplayRule.childName;
		Transform val = childLocator.FindChild(childName);
		if (!Object.op_Implicit((Object)(object)val))
		{
			Debug.LogWarningFormat((Object)(object)((Component)childLocator).gameObject, "Could not fully load item display rules for {0} because child {1} could not be found in the child locator.", new object[2]
			{
				((Object)((Component)childLocator).gameObject).name,
				childName
			});
			return false;
		}
		((Component)this).transform.parent = val;
		keyAsset = newKeyAsset;
		nameInLocator = itemDisplayRule.childName;
		ruleType = itemDisplayRule.ruleType;
		switch (ruleType)
		{
		case ItemDisplayRuleType.ParentedPrefab:
			((Component)this).transform.localPosition = itemDisplayRule.localPos;
			((Component)this).transform.localEulerAngles = itemDisplayRule.localAngles;
			((Component)this).transform.localScale = itemDisplayRule.localScale;
			prefab = itemDisplayRule.followerPrefab;
			limbMask = LimbFlags.None;
			break;
		case ItemDisplayRuleType.LimbMask:
			prefab = null;
			limbMask = itemDisplayRule.limbMask;
			break;
		}
		return true;
	}

	private void DestroyPreview()
	{
		if (Object.op_Implicit((Object)(object)prefabInstance))
		{
			Object.DestroyImmediate((Object)(object)prefabInstance);
		}
		prefabInstance = null;
	}

	private void BuildPreview()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		DestroyPreview();
		if (Object.op_Implicit((Object)(object)prefab))
		{
			prefabInstance = Object.Instantiate<GameObject>(prefab);
			((Object)prefabInstance).name = "Preview";
			prefabInstance.transform.parent = ((Component)this).transform;
			prefabInstance.transform.localPosition = Vector3.zero;
			prefabInstance.transform.localRotation = Quaternion.identity;
			prefabInstance.transform.localScale = Vector3.one;
			SetPreviewFlags(prefabInstance.transform);
		}
	}

	private static void SetPreviewFlags(Transform transform)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		((Object)((Component)transform).gameObject).hideFlags = (HideFlags)60;
		foreach (Transform item in transform)
		{
			SetPreviewFlags(item);
		}
	}

	private void OnDestroy()
	{
		DestroyPreview();
	}
}
