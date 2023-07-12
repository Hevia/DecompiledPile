using System;
using System.Collections.Generic;
using System.Globalization;
using Rewired;
using RoR2.UI;
using UnityEngine;

namespace RoR2;

public static class Glyphs
{
	private struct GlyphKey : IEquatable<GlyphKey>
	{
		public readonly string deviceName;

		public readonly int elementId;

		public GlyphKey(string deviceName, int elementId)
		{
			this.deviceName = deviceName;
			this.elementId = elementId;
		}

		public bool Equals(GlyphKey other)
		{
			if (string.Equals(deviceName, other.deviceName))
			{
				return elementId == other.elementId;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is GlyphKey)
			{
				return Equals((GlyphKey)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((deviceName != null) ? deviceName.GetHashCode() : 0) * 397) ^ elementId;
		}
	}

	private static readonly Dictionary<GlyphKey, string> glyphMap;

	private const string xbox360ControllerName = "Xbox 360 Controller";

	private const string xboxOneControllerName = "Xbox One Controller";

	private const string dualshock4ControllerName = "Sony DualShock 4";

	private const string dualshock4ControllerNameAlt = "PlayStation Controller";

	private const string dualSenseController = "Sony DualSense";

	private const string switchControllerName = "Nintendo Controller";

	private const string switchProControllerName = "Nintendo Switch Pro Controller";

	private const string switchProControllerNameAlt = "Pro Controller";

	private const string stadiaControllerName = "Stadia Controller";

	private const string defaultControllerName = "Xbox One Controller";

	private static readonly Dictionary<string, string> keyboardRawNameToGlyphName;

	private static readonly Dictionary<string, string> mouseElementRenameMap;

	private static readonly List<ActionElementMap> resultsList;

	private static void AddGlyph(string controllerName, int elementIndex, string assetName, string glyphName)
	{
		glyphMap[new GlyphKey(controllerName, elementIndex)] = string.Format(CultureInfo.InvariantCulture, "<sprite=\"{0}\" name=\"{1}\">", assetName, glyphName);
	}

	private static void RegisterXBoxController(string controllerName)
	{
		AddGlyph(controllerName, 0, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_4");
		AddGlyph(controllerName, 1, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_4");
		AddGlyph(controllerName, 2, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_8");
		AddGlyph(controllerName, 3, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_8");
		AddGlyph(controllerName, 4, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_5");
		AddGlyph(controllerName, 5, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_9");
		AddGlyph(controllerName, 10, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_2");
		AddGlyph(controllerName, 11, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_6");
		AddGlyph(controllerName, 6, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_0");
		AddGlyph(controllerName, 7, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_1");
		AddGlyph(controllerName, 8, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_7");
		AddGlyph(controllerName, 9, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_11");
		AddGlyph(controllerName, 12, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_10");
		AddGlyph(controllerName, 13, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_3");
		AddGlyph(controllerName, 14, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_4");
		AddGlyph(controllerName, 15, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_8");
		AddGlyph(controllerName, 19, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_12");
		AddGlyph(controllerName, 17, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_13");
		AddGlyph(controllerName, 16, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_14");
		AddGlyph(controllerName, 18, "tmpsprXboxOneGlyphs", "texXBoxOneGlyphs_15");
	}

	private static void RegisterDS4Controller(string controllerName)
	{
		AddGlyph(controllerName, 0, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_25");
		AddGlyph(controllerName, 1, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_25");
		AddGlyph(controllerName, 2, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_24");
		AddGlyph(controllerName, 3, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_24");
		AddGlyph(controllerName, 4, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_6");
		AddGlyph(controllerName, 5, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_7");
		AddGlyph(controllerName, 10, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_4");
		AddGlyph(controllerName, 11, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_5");
		AddGlyph(controllerName, 6, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_0");
		AddGlyph(controllerName, 7, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_2");
		AddGlyph(controllerName, 8, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_3");
		AddGlyph(controllerName, 9, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_1");
		AddGlyph(controllerName, 15, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_26");
		AddGlyph(controllerName, 16, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_15");
		AddGlyph(controllerName, 17, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_16");
		AddGlyph(controllerName, 18, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_21");
		AddGlyph(controllerName, 19, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_19");
		AddGlyph(controllerName, 20, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_22");
		AddGlyph(controllerName, 21, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_20");
		AddGlyph(controllerName, 13, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_8");
		AddGlyph(controllerName, 12, "tmpsprPS4GlyphsUnified", "texPS4GlyphsUnified_9");
	}

	private static void RegisterSwitchController(string controllerName)
	{
		AddGlyph(controllerName, 1, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_62");
		AddGlyph(controllerName, 0, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_62");
		AddGlyph(controllerName, 3, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_63");
		AddGlyph(controllerName, 2, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_63");
		AddGlyph(controllerName, 10, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_6");
		AddGlyph(controllerName, 11, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_7");
		AddGlyph(controllerName, 8, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_4");
		AddGlyph(controllerName, 9, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_5");
		AddGlyph(controllerName, 4, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_1");
		AddGlyph(controllerName, 5, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_0");
		AddGlyph(controllerName, 6, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_3");
		AddGlyph(controllerName, 7, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_2");
		AddGlyph(controllerName, 12, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_19");
		AddGlyph(controllerName, 13, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_18");
		AddGlyph(controllerName, 16, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_74");
		AddGlyph(controllerName, 17, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_75");
		AddGlyph(controllerName, 18, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_12");
		AddGlyph(controllerName, 19, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_14");
		AddGlyph(controllerName, 20, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_13");
		AddGlyph(controllerName, 21, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_15");
	}

	private static void RegisterSwitchProController(string controllerName)
	{
		AddGlyph(controllerName, 1, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_62");
		AddGlyph(controllerName, 0, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_62");
		AddGlyph(controllerName, 3, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_63");
		AddGlyph(controllerName, 2, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_63");
		AddGlyph(controllerName, 10, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_6");
		AddGlyph(controllerName, 11, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_7");
		AddGlyph(controllerName, 8, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_4");
		AddGlyph(controllerName, 9, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_5");
		AddGlyph(controllerName, 4, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_1");
		AddGlyph(controllerName, 5, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_0");
		AddGlyph(controllerName, 6, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_3");
		AddGlyph(controllerName, 7, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_2");
		AddGlyph(controllerName, 12, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_19");
		AddGlyph(controllerName, 13, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_18");
		AddGlyph(controllerName, 16, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_74");
		AddGlyph(controllerName, 17, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_75");
		AddGlyph(controllerName, 18, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_50");
		AddGlyph(controllerName, 19, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_53");
		AddGlyph(controllerName, 20, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_51");
		AddGlyph(controllerName, 21, "tmpsprSwitchGlyphsUnified", "texSwitchGlyphsUnified_52");
	}

	private static void RegisterStadiaController(string controllerName)
	{
		AddGlyph(controllerName, 0, "tmpsprStadiaGlyphs", "texStadiaGlyphs_28");
		AddGlyph(controllerName, 1, "tmpsprStadiaGlyphs", "texStadiaGlyphs_15");
		AddGlyph(controllerName, 2, "tmpsprStadiaGlyphs", "texStadiaGlyphs_10");
		AddGlyph(controllerName, 3, "tmpsprStadiaGlyphs", "texStadiaGlyphs_0");
		AddGlyph(controllerName, 9, "tmpsprStadiaGlyphs", "texStadiaGlyphs_16");
		AddGlyph(controllerName, 11, "tmpsprStadiaGlyphs", "texStadiaGlyphs_1");
		AddGlyph(controllerName, 8, "tmpsprStadiaGlyphs", "texStadiaGlyphs_21");
		AddGlyph(controllerName, 10, "tmpsprStadiaGlyphs", "texStadiaGlyphs_19");
		AddGlyph(controllerName, 4, "tmpsprStadiaGlyphs", "texStadiaGlyphs_29");
		AddGlyph(controllerName, 5, "tmpsprStadiaGlyphs", "texStadiaGlyphs_31");
		AddGlyph(controllerName, 6, "tmpsprStadiaGlyphs", "texStadiaGlyphs_3");
		AddGlyph(controllerName, 7, "tmpsprStadiaGlyphs", "texStadiaGlyphs_4");
		AddGlyph(controllerName, 13, "tmpsprStadiaGlyphs", "texStadiaGlyphs_17");
		AddGlyph(controllerName, 14, "tmpsprStadiaGlyphs", "texStadiaGlyphs_18");
		AddGlyph(controllerName, 15, "tmpsprStadiaGlyphs", "texStadiaGlyphs_30");
		AddGlyph(controllerName, 16, "tmpsprStadiaGlyphs", "texStadiaGlyphs_32");
		AddGlyph(controllerName, 17, "tmpsprStadiaGlyphs", "texStadiaGlyphs_2");
		AddGlyph(controllerName, 18, "tmpsprStadiaGlyphs", "texStadiaGlyphs_22");
		AddGlyph(controllerName, 19, "tmpsprStadiaGlyphs", "texStadiaGlyphs_20");
		AddGlyph(controllerName, 20, "tmpsprStadiaGlyphs", "texStadiaGlyphs_36");
		AddGlyph(controllerName, 21, "tmpsprStadiaGlyphs", "texStadiaGlyphs_35");
		AddGlyph(controllerName, 22, "tmpsprStadiaGlyphs", "texStadiaGlyphs_33");
		AddGlyph(controllerName, 23, "tmpsprStadiaGlyphs", "texStadiaGlyphs_34");
	}

	private static void RegisterMouse(string controllerName)
	{
		AddGlyph(controllerName, 3, "tmpsprSteamGlyphs", "texSteamGlyphs_17");
		AddGlyph(controllerName, 4, "tmpsprSteamGlyphs", "texSteamGlyphs_18");
		AddGlyph(controllerName, 5, "tmpsprSteamGlyphs", "texSteamGlyphs_19");
	}

	private static void RegisterKeyboard(string controllerName)
	{
	}

	static Glyphs()
	{
		glyphMap = new Dictionary<GlyphKey, string>();
		keyboardRawNameToGlyphName = new Dictionary<string, string>();
		mouseElementRenameMap = new Dictionary<string, string>
		{
			["Left Mouse Button"] = "M1",
			["Right Mouse Button"] = "M2",
			["Mouse Button 3"] = "M3",
			["Mouse Button 4"] = "M4",
			["Mouse Button 5"] = "M5",
			["Mouse Button 6"] = "M6",
			["Mouse Button 7"] = "M7",
			["Mouse Wheel"] = "MW",
			["Mouse Wheel +"] = "MW+",
			["Mouse Wheel -"] = "MW-"
		};
		resultsList = new List<ActionElementMap>();
		RegisterXBoxController("Xbox 360 Controller");
		for (int i = 0; i < 4; i++)
		{
			RegisterXBoxController("XInput Gamepad " + i);
		}
		RegisterXBoxController("Xbox One Controller");
		RegisterXBoxController("Gamepad");
		RegisterDS4Controller("Sony DualShock 4");
		RegisterDS4Controller("PlayStation Controller");
		RegisterDS4Controller("Sony DualSense");
		RegisterSwitchController("Nintendo Controller");
		RegisterSwitchProController("Nintendo Switch Pro Controller");
		RegisterSwitchProController("Pro Controller");
		RegisterStadiaController("Stadia Controller");
	}

	private static string GetKeyboardGlyphString(string actionName)
	{
		if (!keyboardRawNameToGlyphName.TryGetValue(actionName, out var value))
		{
			if (!(actionName == "Left Shift"))
			{
				if (actionName == "Left Control")
				{
					actionName = "Ctrl";
				}
			}
			else
			{
				actionName = "Shift";
			}
			value = actionName;
			keyboardRawNameToGlyphName[actionName] = value;
		}
		return value;
	}

	public static string GetGlyphString(MPEventSystemLocator eventSystemLocator, string actionName)
	{
		MPEventSystem eventSystem = eventSystemLocator.eventSystem;
		if (Object.op_Implicit((Object)(object)eventSystem))
		{
			return GetGlyphString(eventSystem, actionName, (AxisRange)0);
		}
		return "UNKNOWN";
	}

	public static string GetGlyphString(MPEventSystem eventSystem, string actionName, AxisRange axisRange = 0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return GetGlyphString(eventSystem, actionName, axisRange, eventSystem.currentInputSource);
	}

	public static string GetGlyphString(MPEventSystem eventSystem, string actionName, AxisRange axisRange, MPEventSystem.InputSource currentInputSource)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Invalid comparison between Unknown and I4
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Invalid comparison between Unknown and I4
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Invalid comparison between Unknown and I4
		if (!Object.op_Implicit((Object)(object)eventSystem))
		{
			return "???";
		}
		Player inputPlayer = eventSystem.player;
		InputAction action = ReInput.mapping.GetAction(actionName);
		int inputActionId = action.id;
		string controllerName = "Xbox One Controller";
		ControllerType controllerType = (ControllerType)(-1);
		bool axisContributionMatters = (int)axisRange > 0;
		Pole axisContribution = (Pole)0;
		if ((int)axisRange == 2)
		{
			axisContribution = (Pole)1;
		}
		ActionElementMap actionElementMap = default(ActionElementMap);
		switch (currentInputSource)
		{
		case MPEventSystem.InputSource.MouseAndKeyboard:
			SetController((Controller)(object)inputPlayer.controllers.Keyboard);
			if (actionElementMap == null)
			{
				SetController((Controller)(object)inputPlayer.controllers.Mouse);
			}
			if (actionElementMap == null)
			{
				return Language.GetString("INPUT_KEYBOARD_UNBOUND");
			}
			break;
		case MPEventSystem.InputSource.Gamepad:
			controllerType = (ControllerType)2;
			SetController(inputPlayer.controllers.GetLastActiveController((ControllerType)2));
			if (actionElementMap == null)
			{
				foreach (Controller controller in inputPlayer.controllers.Controllers)
				{
					if ((int)controller.type == 2)
					{
						SetController(controller);
						if (actionElementMap != null)
						{
							break;
						}
					}
				}
			}
			if (actionElementMap == null && eventSystem.localUser != null)
			{
				using IEnumerator<ActionElementMap> enumerator2 = ((ControllerMap)eventSystem.localUser.userProfile.joystickMap).ElementMapsWithAction(inputActionId).GetEnumerator();
				if (enumerator2.MoveNext())
				{
					ActionElementMap current2 = enumerator2.Current;
					actionElementMap = current2;
				}
			}
			if (actionElementMap == null)
			{
				return Language.GetString("INPUT_GAMEPAD_UNBOUND");
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		int elementIdentifierId = actionElementMap.elementIdentifierId;
		GlyphKey key = new GlyphKey(controllerName, elementIdentifierId);
		if (glyphMap.TryGetValue(key, out var value))
		{
			return value;
		}
		if ((int)controllerType == 0)
		{
			return GetKeyboardGlyphString(actionElementMap.elementIdentifierName);
		}
		if ((int)controllerType == 1)
		{
			string text = actionElementMap.elementIdentifierName;
			if (mouseElementRenameMap.TryGetValue(text, out var value2))
			{
				text = value2;
			}
			return text;
		}
		return "???";
		void SetController(Controller newController)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			if (newController != null)
			{
				controllerName = newController.name;
				controllerType = newController.type;
			}
			actionElementMap = null;
			if (newController != null)
			{
				resultsList.Clear();
				inputPlayer.controllers.maps.GetElementMapsWithAction(newController.type, newController.id, inputActionId, false, resultsList);
				foreach (ActionElementMap results in resultsList)
				{
					if (!axisContributionMatters || results.axisContribution == axisContribution)
					{
						actionElementMap = results;
						break;
					}
				}
			}
		}
	}
}
