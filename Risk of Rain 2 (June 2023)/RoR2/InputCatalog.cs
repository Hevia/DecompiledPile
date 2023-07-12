using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Rewired;

namespace RoR2;

public static class InputCatalog
{
	private struct ActionAxisPair : IEquatable<ActionAxisPair>
	{
		[NotNull]
		private readonly string actionName;

		private readonly AxisRange axisRange;

		public ActionAxisPair([NotNull] string actionName, AxisRange axisRange)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			this.actionName = actionName;
			this.axisRange = axisRange;
		}

		public bool Equals(ActionAxisPair other)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (string.Equals(actionName, other.actionName))
			{
				return axisRange == other.axisRange;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is ActionAxisPair)
			{
				return Equals((ActionAxisPair)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((-1879861323 * -1521134295 + base.GetHashCode()) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(actionName)) * -1521134295 + ((object)(AxisRange)(ref axisRange)).GetHashCode();
		}
	}

	private static readonly Dictionary<ActionAxisPair, string> actionToToken;

	static InputCatalog()
	{
		actionToToken = new Dictionary<ActionAxisPair, string>();
		Add("MoveHorizontal", "ACTION_MOVE_HORIZONTAL", (AxisRange)0);
		Add("MoveVertical", "ACTION_MOVE_VERTICAL", (AxisRange)0);
		Add("AimHorizontalMouse", "ACTION_AIM_HORIZONTAL_MOUSE", (AxisRange)0);
		Add("AimVerticalMouse", "ACTION_AIM_VERTICAL_MOUSE", (AxisRange)0);
		Add("AimHorizontalStick", "ACTION_AIM_HORIZONTAL_STICK", (AxisRange)0);
		Add("AimVerticalStick", "ACTION_AIM_VERTICAL_STICK", (AxisRange)0);
		Add("Jump", "ACTION_JUMP", (AxisRange)0);
		Add("Sprint", "ACTION_SPRINT", (AxisRange)0);
		Add("Interact", "ACTION_INTERACT", (AxisRange)0);
		Add("Equipment", "ACTION_EQUIPMENT", (AxisRange)0);
		Add("PrimarySkill", "ACTION_PRIMARY_SKILL", (AxisRange)0);
		Add("SecondarySkill", "ACTION_SECONDARY_SKILL", (AxisRange)0);
		Add("UtilitySkill", "ACTION_UTILITY_SKILL", (AxisRange)0);
		Add("SpecialSkill", "ACTION_SPECIAL_SKILL", (AxisRange)0);
		Add("Info", "ACTION_INFO", (AxisRange)0);
		Add("Ping", "ACTION_PING", (AxisRange)0);
		Add("MoveHorizontal", "ACTION_MOVE_HORIZONTAL_POSITIVE", (AxisRange)1);
		Add("MoveHorizontal", "ACTION_MOVE_HORIZONTAL_NEGATIVE", (AxisRange)2);
		Add("MoveVertical", "ACTION_MOVE_VERTICAL_POSITIVE", (AxisRange)1);
		Add("MoveVertical", "ACTION_MOVE_VERTICAL_NEGATIVE", (AxisRange)2);
		static void Add(string actionName, string token, AxisRange axisRange)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			actionToToken[new ActionAxisPair(actionName, axisRange)] = token;
		}
	}

	public static string GetActionNameToken(string actionName, AxisRange axisRange = 0)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (actionToToken.TryGetValue(new ActionAxisPair(actionName, axisRange), out var value))
		{
			return value;
		}
		throw new ArgumentException($"Bad action/axis pair {actionName} {axisRange}.");
	}
}
