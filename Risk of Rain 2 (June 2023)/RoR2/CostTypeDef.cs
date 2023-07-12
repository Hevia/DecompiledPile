using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace RoR2;

public class CostTypeDef
{
	public delegate void BuildCostStringDelegate(CostTypeDef costTypeDef, BuildCostStringContext context);

	public struct BuildCostStringContext
	{
		public StringBuilder stringBuilder;

		public int cost;
	}

	public delegate Color32 GetCostColorDelegate(CostTypeDef costTypeDef, GetCostColorContext context);

	public struct GetCostColorContext
	{
		public bool forWorldDisplay;
	}

	public delegate void BuildCostStringStyledDelegate(CostTypeDef costTypeDef, BuildCostStringStyledContext context);

	public struct BuildCostStringStyledContext
	{
		public StringBuilder stringBuilder;

		public int cost;

		public bool forWorldDisplay;

		public bool includeColor;
	}

	public delegate bool IsAffordableDelegate(CostTypeDef costTypeDef, IsAffordableContext context);

	public struct IsAffordableContext
	{
		public int cost;

		public Interactor activator;
	}

	public delegate void PayCostDelegate(CostTypeDef costTypeDef, PayCostContext context);

	public struct PayCostContext
	{
		public int cost;

		public Interactor activator;

		public CharacterBody activatorBody;

		public CharacterMaster activatorMaster;

		public GameObject purchasedObject;

		public PayCostResults results;

		public Xoroshiro128Plus rng;

		public ItemIndex avoidedItemIndex;
	}

	public class PayCostResults
	{
		public List<ItemIndex> itemsTaken = new List<ItemIndex>();

		public List<EquipmentIndex> equipmentTaken = new List<EquipmentIndex>();
	}

	public string name;

	public ItemTier itemTier = ItemTier.NoTier;

	public ColorCatalog.ColorIndex colorIndex = ColorCatalog.ColorIndex.Error;

	public string costStringFormatToken;

	public string costStringStyle;

	public bool saturateWorldStyledCostString = true;

	public bool darkenWorldStyledCostString = true;

	public BuildCostStringDelegate buildCostString { private get; set; } = BuildCostStringDefault;


	public GetCostColorDelegate getCostColor { private get; set; } = GetCostColorDefault;


	public BuildCostStringStyledDelegate buildCostStringStyled { private get; set; } = BuildCostStringStyledDefault;


	public IsAffordableDelegate isAffordable { private get; set; }

	public PayCostDelegate payCost { private get; set; }

	public void BuildCostString(int cost, [NotNull] StringBuilder stringBuilder)
	{
		buildCostString(this, new BuildCostStringContext
		{
			cost = cost,
			stringBuilder = stringBuilder
		});
	}

	public static void BuildCostStringDefault(CostTypeDef costTypeDef, BuildCostStringContext context)
	{
		context.stringBuilder.Append(Language.GetStringFormatted(costTypeDef.costStringFormatToken, context.cost));
	}

	public Color32 GetCostColor(bool forWorldDisplay)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return getCostColor(this, new GetCostColorContext
		{
			forWorldDisplay = forWorldDisplay
		});
	}

	public static Color32 GetCostColorDefault(CostTypeDef costTypeDef, GetCostColorContext context)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		Color32 val = ColorCatalog.GetColor(costTypeDef.colorIndex);
		if (context.forWorldDisplay)
		{
			float num = default(float);
			float num2 = default(float);
			float num3 = default(float);
			Color.RGBToHSV(Color32.op_Implicit(val), ref num, ref num2, ref num3);
			if (costTypeDef.saturateWorldStyledCostString && num2 > 0f)
			{
				num2 = 1f;
			}
			if (costTypeDef.darkenWorldStyledCostString)
			{
				num3 *= 0.5f;
			}
			val = Color32.op_Implicit(Color.HSVToRGB(num, num2, num3));
		}
		return val;
	}

	public void BuildCostStringStyled(int cost, [NotNull] StringBuilder stringBuilder, bool forWorldDisplay, bool includeColor = true)
	{
		buildCostStringStyled(this, new BuildCostStringStyledContext
		{
			cost = cost,
			forWorldDisplay = forWorldDisplay,
			stringBuilder = stringBuilder,
			includeColor = includeColor
		});
	}

	public static void BuildCostStringStyledDefault(CostTypeDef costTypeDef, BuildCostStringStyledContext context)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder stringBuilder = context.stringBuilder;
		stringBuilder.Append("<nobr>");
		if (costTypeDef.costStringStyle != null)
		{
			stringBuilder.Append("<style=");
			stringBuilder.Append(costTypeDef.costStringStyle);
			stringBuilder.Append(">");
		}
		if (context.includeColor)
		{
			Color32 costColor = costTypeDef.GetCostColor(context.forWorldDisplay);
			stringBuilder.Append("<color=#");
			stringBuilder.AppendColor32RGBHexValues(costColor);
			stringBuilder.Append(">");
		}
		costTypeDef.BuildCostString(context.cost, context.stringBuilder);
		if (context.includeColor)
		{
			stringBuilder.Append("</color>");
		}
		if (costTypeDef.costStringStyle != null)
		{
			stringBuilder.Append("</style>");
		}
		stringBuilder.Append("</nobr>");
	}

	public bool IsAffordable(int cost, Interactor activator)
	{
		return isAffordable(this, new IsAffordableContext
		{
			cost = cost,
			activator = activator
		});
	}

	public PayCostResults PayCost(int cost, Interactor activator, GameObject purchasedObject, Xoroshiro128Plus rng, ItemIndex avoidedItemIndex)
	{
		PayCostResults payCostResults = new PayCostResults();
		CharacterBody component = ((Component)activator).GetComponent<CharacterBody>();
		payCost(this, new PayCostContext
		{
			cost = cost,
			activator = activator,
			activatorBody = component,
			activatorMaster = (Object.op_Implicit((Object)(object)component) ? component.master : null),
			purchasedObject = purchasedObject,
			results = payCostResults,
			rng = rng,
			avoidedItemIndex = avoidedItemIndex
		});
		return payCostResults;
	}
}
