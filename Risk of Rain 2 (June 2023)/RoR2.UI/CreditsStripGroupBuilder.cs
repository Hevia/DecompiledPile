using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;

namespace RoR2.UI;

public class CreditsStripGroupBuilder : MonoBehaviour
{
	[Multiline]
	public string source;

	public GameObject stripPrefab;

	public static string EnglishRoleToToken(string englishRoleString)
	{
		StringBuilder stringBuilder = new StringBuilder(englishRoleString);
		stringBuilder.Replace("&", "AND");
		stringBuilder.Replace(",", "");
		stringBuilder.Replace("(", "");
		stringBuilder.Replace(")", "");
		for (int num = stringBuilder.Length - 1; num >= 0; num--)
		{
			if (char.IsWhiteSpace(stringBuilder[num]))
			{
				stringBuilder[num] = '_';
			}
		}
		for (int num2 = stringBuilder.Length - 1; num2 >= 0; num2--)
		{
			char c = stringBuilder[num2];
			if (!char.IsLetterOrDigit(c) && c != '_' && c != '/')
			{
				stringBuilder.Remove(num2, 1);
			}
		}
		stringBuilder.Insert(0, "CREDITS_ROLE_");
		return stringBuilder.ToString().ToUpper(CultureInfo.InvariantCulture);
	}

	public List<(string name, string englishRoleName)> GetNamesAndEnglishRoles()
	{
		string text = source.Replace("\r", "").Replace("\n", "\t");
		List<(string, string)> list = new List<(string, string)>();
		string[] array = text.Split(new char[1] { '\t' });
		for (int i = 0; i < array.Length; i += 2)
		{
			list.Add((array[i], array[i + 1]));
		}
		return list;
	}

	[ContextMenu("Build")]
	private void Build()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		List<(string, string)> namesAndEnglishRoles = GetNamesAndEnglishRoles();
		List<Transform> list = new List<Transform>(((Component)this).transform.childCount);
		int i = 0;
		for (int childCount = ((Component)this).transform.childCount; i < childCount; i++)
		{
			list.Add(((Component)this).transform.GetChild(i));
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Transform val = list[num];
			if (EditorUtil.PrefabUtilityGetNearestPrefabInstanceRoot((Object)(object)((Component)val).gameObject) != (Object)(object)stripPrefab)
			{
				Object.DestroyImmediate((Object)(object)((Component)val).gameObject);
				list.RemoveAt(num);
			}
		}
		while (list.Count < namesAndEnglishRoles.Count)
		{
			GameObject val2 = (GameObject)EditorUtil.PrefabUtilityInstantiatePrefab((Object)(object)stripPrefab, ((Component)this).transform);
			list.Add(val2.transform);
		}
		while (list.Count > namesAndEnglishRoles.Count)
		{
			int index = list.Count - 1;
			Object.DestroyImmediate((Object)(object)((Component)list[index]).gameObject);
			list.RemoveAt(index);
		}
		int j = 0;
		for (int num2 = Math.Min(namesAndEnglishRoles.Count, list.Count); j < num2; j++)
		{
			(string, string) tuple = namesAndEnglishRoles[j];
			SetStripValues(((Component)list[j]).gameObject, tuple.Item1, tuple.Item2);
		}
		static void SetStripValues(GameObject stripInstance, string name, string rawRoleText)
		{
			((TMP_Text)((Component)stripInstance.transform.Find("CreditNameLabel")).GetComponent<HGTextMeshProUGUI>()).SetText(name, true);
			((Component)stripInstance.transform.Find("CreditRoleLabel")).GetComponent<LanguageTextMeshController>().token = EnglishRoleToToken(rawRoleText);
			((TMP_Text)((Component)stripInstance.transform.Find("CreditRoleLabel")).GetComponent<HGTextMeshProUGUI>()).SetText(rawRoleText, true);
		}
	}
}
