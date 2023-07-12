using System;
using UnityEngine;

namespace RoR2;

public abstract class MiscPickupDef : ScriptableObject
{
	[SerializeField]
	public uint coinValue;

	[SerializeField]
	public string nameToken;

	[SerializeField]
	public GameObject displayPrefab;

	[SerializeField]
	public GameObject dropletDisplayPrefab;

	[SerializeField]
	public ColorCatalog.ColorIndex baseColor;

	[SerializeField]
	public ColorCatalog.ColorIndex darkColor;

	[SerializeField]
	public string interactContextToken;

	[NonSerialized]
	public MiscPickupIndex miscPickupIndex;

	public abstract void GrantPickup(ref PickupDef.GrantContext context);

	public virtual string GetInternalName()
	{
		return "MiscPickupIndex." + ((Object)this).name;
	}

	public virtual PickupDef CreatePickupDef()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		return new PickupDef
		{
			internalName = GetInternalName(),
			coinValue = coinValue,
			nameToken = nameToken,
			displayPrefab = displayPrefab,
			dropletDisplayPrefab = dropletDisplayPrefab,
			baseColor = Color32.op_Implicit(ColorCatalog.GetColor(baseColor)),
			darkColor = Color32.op_Implicit(ColorCatalog.GetColor(darkColor)),
			interactContextToken = interactContextToken,
			attemptGrant = GrantPickup,
			miscPickupIndex = miscPickupIndex
		};
	}
}
