using System;
using System.Collections.Generic;

namespace VileMod.Modules;

internal static class Helpers
{
	internal const string agilePrefix = "<style=cIsUtility>Agile.</style> ";

	internal static string ScepterDescription(string desc)
	{
		return "\n<color=#d299ff>SCEPTER: " + desc + "</color>";
	}

	public static T[] Append<T>(ref T[] array, List<T> list)
	{
		int num = array.Length;
		int count = list.Count;
		Array.Resize(ref array, num + count);
		list.CopyTo(array, num);
		return array;
	}

	public static Func<T[], T[]> AppendDel<T>(List<T> list)
	{
		return (T[] r) => Append(ref r, list);
	}
}
