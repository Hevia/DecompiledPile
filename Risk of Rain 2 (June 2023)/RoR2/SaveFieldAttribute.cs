using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rewired;
using UnityEngine;

namespace RoR2;

public class SaveFieldAttribute : Attribute
{
	public Action<UserProfile, string> setter;

	public Func<UserProfile, string> getter;

	public Action<UserProfile, UserProfile> copier;

	public string defaultValue = string.Empty;

	public string fieldName;

	public string explicitSetupMethod;

	private FieldInfo fieldInfo;

	public void Setup(FieldInfo fieldInfo)
	{
		this.fieldInfo = fieldInfo;
		Type fieldType = fieldInfo.FieldType;
		fieldName = fieldInfo.Name;
		if (explicitSetupMethod != null)
		{
			MethodInfo method = typeof(SaveFieldAttribute).GetMethod(explicitSetupMethod, BindingFlags.Instance | BindingFlags.Public);
			if (method == null)
			{
				Debug.LogErrorFormat("Explicit setup {0} specified by field {1} could not be found. Use the nameof() operator to ensure the method exists.", Array.Empty<object>());
			}
			else if (method.GetParameters().Length > 1)
			{
				Debug.LogErrorFormat("Explicit setup method {0} for field {1} must have one parameter.", new object[2] { explicitSetupMethod, fieldInfo.Name });
			}
			else
			{
				method.Invoke(this, new object[1] { fieldInfo });
			}
		}
		else if (fieldType == typeof(string))
		{
			SetupString(fieldInfo);
		}
		else if (fieldType == typeof(int))
		{
			SetupInt(fieldInfo);
		}
		else if (fieldType == typeof(uint))
		{
			SetupUint(fieldInfo);
		}
		else if (fieldType == typeof(float))
		{
			SetupFloat(fieldInfo);
		}
		else if (fieldType == typeof(bool))
		{
			SetupBool(fieldInfo);
		}
		else if (fieldType == typeof(SurvivorDef))
		{
			SetupSurvivorDef(fieldInfo);
		}
		else
		{
			Debug.LogErrorFormat("No explicit setup method or supported type for save field {0}", new object[1] { fieldInfo.Name });
		}
	}

	public void SetupString(FieldInfo fieldInfo)
	{
		getter = (UserProfile userProfile) => (string)fieldInfo.GetValue(userProfile);
		setter = delegate(UserProfile userProfile, string valueString)
		{
			fieldInfo.SetValue(userProfile, valueString);
		};
		copier = delegate(UserProfile srcProfile, UserProfile destProfile)
		{
			fieldInfo.SetValue(destProfile, fieldInfo.GetValue(srcProfile));
		};
	}

	public void SetupFloat(FieldInfo fieldInfo)
	{
		getter = (UserProfile userProfile) => TextSerialization.ToStringInvariant((float)fieldInfo.GetValue(userProfile));
		setter = delegate(UserProfile userProfile, string valueString)
		{
			if (TextSerialization.TryParseInvariant(valueString, out float result))
			{
				fieldInfo.SetValue(userProfile, result);
			}
		};
		copier = delegate(UserProfile srcProfile, UserProfile destProfile)
		{
			fieldInfo.SetValue(destProfile, fieldInfo.GetValue(srcProfile));
		};
	}

	public void SetupInt(FieldInfo fieldInfo)
	{
		getter = (UserProfile userProfile) => TextSerialization.ToStringInvariant((int)fieldInfo.GetValue(userProfile));
		setter = delegate(UserProfile userProfile, string valueString)
		{
			if (TextSerialization.TryParseInvariant(valueString, out int result))
			{
				fieldInfo.SetValue(userProfile, result);
			}
		};
		copier = delegate(UserProfile srcProfile, UserProfile destProfile)
		{
			fieldInfo.SetValue(destProfile, fieldInfo.GetValue(srcProfile));
		};
	}

	public void SetupUint(FieldInfo fieldInfo)
	{
		getter = (UserProfile userProfile) => TextSerialization.ToStringInvariant((uint)fieldInfo.GetValue(userProfile));
		setter = delegate(UserProfile userProfile, string valueString)
		{
			if (TextSerialization.TryParseInvariant(valueString, out uint result))
			{
				fieldInfo.SetValue(userProfile, result);
			}
		};
		copier = delegate(UserProfile srcProfile, UserProfile destProfile)
		{
			fieldInfo.SetValue(destProfile, fieldInfo.GetValue(srcProfile));
		};
	}

	public void SetupBool(FieldInfo fieldInfo)
	{
		getter = (UserProfile userProfile) => (!(bool)fieldInfo.GetValue(userProfile)) ? "0" : "1";
		setter = delegate(UserProfile userProfile, string valueString)
		{
			if (TextSerialization.TryParseInvariant(valueString, out int result))
			{
				fieldInfo.SetValue(userProfile, result > 0);
			}
		};
		copier = delegate(UserProfile srcProfile, UserProfile destProfile)
		{
			fieldInfo.SetValue(destProfile, fieldInfo.GetValue(srcProfile));
		};
	}

	public void SetupTokenList(FieldInfo fieldInfo)
	{
		getter = (UserProfile userProfile) => string.Join(" ", (List<string>)fieldInfo.GetValue(userProfile));
		setter = delegate(UserProfile userProfile, string valueString)
		{
			List<string> list = (List<string>)fieldInfo.GetValue(userProfile);
			list.Clear();
			string[] array = valueString.Split(new char[1] { ' ' });
			foreach (string item in array)
			{
				list.Add(item);
			}
		};
		copier = delegate(UserProfile srcProfile, UserProfile destProfile)
		{
			List<string> src = (List<string>)fieldInfo.GetValue(srcProfile);
			List<string> dest = (List<string>)fieldInfo.GetValue(destProfile);
			Util.CopyList(src, dest);
		};
	}

	public void SetupPickupsSet(FieldInfo fieldInfo)
	{
		getter = delegate(UserProfile userProfile)
		{
			bool[] pickupsSet = (bool[])fieldInfo.GetValue(userProfile);
			return string.Join(" ", from pickupDef in PickupCatalog.allPickups
				where pickupsSet[pickupDef.pickupIndex.value]
				select pickupDef.internalName);
		};
		setter = delegate(UserProfile userProfile, string valueString)
		{
			bool[] array2 = (bool[])fieldInfo.GetValue(userProfile);
			Array.Clear(array2, 0, 0);
			string[] array3 = valueString.Split(new char[1] { ' ' });
			for (int i = 0; i < array3.Length; i++)
			{
				PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(array3[i]);
				if (pickupIndex.isValid)
				{
					array2[pickupIndex.value] = true;
				}
			}
		};
		copier = delegate(UserProfile srcProfile, UserProfile destProfile)
		{
			bool[] sourceArray = (bool[])fieldInfo.GetValue(srcProfile);
			bool[] array = (bool[])fieldInfo.GetValue(destProfile);
			Array.Copy(sourceArray, array, array.Length);
		};
	}

	public void SetupSurvivorDef(FieldInfo fieldInfo)
	{
		getter = (UserProfile userProfile) => ((SurvivorDef)fieldInfo.GetValue(userProfile)).cachedName;
		setter = delegate(UserProfile userProfile, string valueString)
		{
			SurvivorDef value = SurvivorCatalog.FindSurvivorDef(valueString);
			fieldInfo.SetValue(userProfile, value);
		};
		copier = DefaultCopier;
	}

	public void SetupKeyboardMap(FieldInfo fieldInfo)
	{
		SetupControllerMap(fieldInfo, (ControllerType)0);
	}

	public void SetupMouseMap(FieldInfo fieldInfo)
	{
		SetupControllerMap(fieldInfo, (ControllerType)1);
	}

	public void SetupJoystickMap(FieldInfo fieldInfo)
	{
		SetupControllerMap(fieldInfo, (ControllerType)2);
	}

	private void SetupControllerMap(FieldInfo fieldInfo, ControllerType controllerType)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		getter = delegate(UserProfile userProfile)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			ControllerMap val2 = (ControllerMap)fieldInfo.GetValue(userProfile);
			return (((int)val2 != 0) ? val2.ToXmlString() : null) ?? string.Empty;
		};
		setter = delegate(UserProfile userProfile, string valueString)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			fieldInfo.SetValue(userProfile, ControllerMap.CreateFromXml(controllerType, valueString));
		};
		copier = delegate(UserProfile srcProfile, UserProfile destProfile)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected I4, but got Unknown
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected O, but got Unknown
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Expected O, but got Unknown
			ControllerType val = controllerType;
			switch ((int)val)
			{
			case 2:
				fieldInfo.SetValue(destProfile, (object?)new JoystickMap((JoystickMap)fieldInfo.GetValue(srcProfile)));
				break;
			case 0:
				fieldInfo.SetValue(destProfile, (object?)new KeyboardMap((KeyboardMap)fieldInfo.GetValue(srcProfile)));
				break;
			case 1:
				fieldInfo.SetValue(destProfile, (object?)new MouseMap((MouseMap)fieldInfo.GetValue(srcProfile)));
				break;
			default:
				throw new NotImplementedException();
			}
		};
	}

	private void DefaultCopier(UserProfile srcProfile, UserProfile destProfile)
	{
		fieldInfo.SetValue(destProfile, fieldInfo.GetValue(srcProfile));
	}
}
