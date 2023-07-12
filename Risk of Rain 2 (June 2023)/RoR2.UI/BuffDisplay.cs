using System.Collections.Generic;
using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class BuffDisplay : MonoBehaviour
{
	private RectTransform rectTranform;

	public CharacterBody source;

	public GameObject buffIconPrefab;

	public float iconWidth = 24f;

	[SerializeField]
	[HideInInspector]
	private List<BuffIcon> buffIcons;

	private void Awake()
	{
		rectTranform = ((Component)this).GetComponent<RectTransform>();
	}

	private void AllocateIcons()
	{
		int num = 0;
		if (Object.op_Implicit((Object)(object)source))
		{
			BuffIndex[] nonHiddenBuffIndices = BuffCatalog.nonHiddenBuffIndices;
			foreach (BuffIndex buffType in nonHiddenBuffIndices)
			{
				if (source.HasBuff(buffType))
				{
					num++;
				}
			}
		}
		if (num != buffIcons.Count)
		{
			while (buffIcons.Count > num)
			{
				Object.Destroy((Object)(object)((Component)buffIcons[buffIcons.Count - 1]).gameObject);
				buffIcons.RemoveAt(buffIcons.Count - 1);
			}
			while (buffIcons.Count < num)
			{
				BuffIcon component = Object.Instantiate<GameObject>(buffIconPrefab, (Transform)(object)rectTranform).GetComponent<BuffIcon>();
				buffIcons.Add(component);
			}
			UpdateLayout();
		}
	}

	private void UpdateLayout()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		AllocateIcons();
		Rect rect = rectTranform.rect;
		_ = ((Rect)(ref rect)).width;
		if (!Object.op_Implicit((Object)(object)source))
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		int num = 0;
		BuffIndex[] nonHiddenBuffIndices = BuffCatalog.nonHiddenBuffIndices;
		foreach (BuffIndex buffIndex in nonHiddenBuffIndices)
		{
			if (source.HasBuff(buffIndex))
			{
				BuffIcon buffIcon = buffIcons[num];
				buffIcon.buffDef = BuffCatalog.GetBuffDef(buffIndex);
				buffIcon.rectTransform.anchoredPosition = zero;
				buffIcon.buffCount = source.GetBuffCount(buffIndex);
				zero.x += iconWidth;
				buffIcon.UpdateIcon();
				num++;
			}
		}
	}

	private void Update()
	{
		UpdateLayout();
	}
}
