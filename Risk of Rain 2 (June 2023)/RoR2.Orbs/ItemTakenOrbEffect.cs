using UnityEngine;

namespace RoR2.Orbs;

[RequireComponent(typeof(EffectComponent))]
public class ItemTakenOrbEffect : MonoBehaviour
{
	public TrailRenderer trailToColor;

	public ParticleSystem[] particlesToColor;

	public SpriteRenderer[] spritesToColor;

	public SpriteRenderer iconSpriteRenderer;

	private void Start()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		ItemDef itemDef = ItemCatalog.GetItemDef((ItemIndex)Util.UintToIntMinusOne(((Component)this).GetComponent<EffectComponent>().effectData.genericUInt));
		ColorCatalog.ColorIndex colorIndex = ColorCatalog.ColorIndex.Error;
		Sprite sprite = null;
		if ((Object)(object)itemDef != (Object)null)
		{
			colorIndex = itemDef.colorIndex;
			sprite = itemDef.pickupIconSprite;
		}
		Color val = Color32.op_Implicit(ColorCatalog.GetColor(colorIndex));
		trailToColor.startColor *= val;
		trailToColor.endColor *= val;
		for (int i = 0; i < particlesToColor.Length; i++)
		{
			ParticleSystem obj = particlesToColor[i];
			MainModule main = obj.main;
			((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(val);
			obj.Play();
		}
		for (int j = 0; j < spritesToColor.Length; j++)
		{
			spritesToColor[j].color = val;
		}
		iconSpriteRenderer.sprite = sprite;
	}
}
