using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HG;
using UnityEngine;

namespace RoR2.ContentManagement;

[CreateAssetMenu(menuName = "RoR2/ContentAssetRefResolver")]
public class ContentAssetRefResolver : ScriptableObject
{
	[Serializable]
	public struct FieldAssignmentInfo
	{
		public Object targetObject;

		public string fieldPath;

		public ContentAssetPath contentAssetPath;
	}

	[Serializable]
	public struct ContentAssetPath
	{
		public string contentPackIdentifier;

		public string collectionName;

		public string assetName;
	}

	public bool applyOnEnable = true;

	public FieldAssignmentInfo[] fieldAssignmentInfos;

	private static bool contentPacksLoaded;

	private static Queue<ContentAssetRefResolver> pendingResolutions;

	private static ReadOnlyArray<ReadOnlyContentPack> loadedContentPacks;

	[ContextMenu("Run Test")]
	public void Apply()
	{
		for (int i = 0; i < fieldAssignmentInfos.Length; i++)
		{
			ApplyFieldAssignmentInfo(in fieldAssignmentInfos[i]);
		}
	}

	private unsafe object FindContentAsset(in ContentAssetPath contentAssetPath)
	{
		for (int i = 0; i < loadedContentPacks.Length; i++)
		{
			if (((ReadOnlyContentPack)loadedContentPacks[i]).identifier.Equals(contentAssetPath.contentPackIdentifier, StringComparison.Ordinal))
			{
				ReadOnlyContentPack readOnlyContentPack = *(ReadOnlyContentPack*)loadedContentPacks[i];
				readOnlyContentPack.FindAsset(contentAssetPath.collectionName, contentAssetPath.assetName, out var result);
				return result;
			}
		}
		return null;
	}

	static ContentAssetRefResolver()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		contentPacksLoaded = false;
		pendingResolutions = new Queue<ContentAssetRefResolver>();
		loadedContentPacks = ReadOnlyArray<ReadOnlyContentPack>.op_Implicit(Array.Empty<ReadOnlyContentPack>());
		ContentManager.onContentPacksAssigned += OnContentPacksLoaded;
	}

	private static void OnContentPacksLoaded(ReadOnlyArray<ReadOnlyContentPack> newLoadedContentPacks)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		loadedContentPacks = newLoadedContentPacks;
		contentPacksLoaded = true;
		ApplyQueued();
	}

	private static void ApplyQueued()
	{
		while (pendingResolutions.Count > 0)
		{
			ContentAssetRefResolver contentAssetRefResolver = pendingResolutions.Dequeue();
			try
			{
				contentAssetRefResolver.Apply();
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
		}
	}

	private void OnEnable()
	{
		if (applyOnEnable)
		{
			if (contentPacksLoaded)
			{
				Apply();
			}
			else
			{
				pendingResolutions.Enqueue(this);
			}
		}
	}

	private bool ApplyFieldAssignmentInfo(in FieldAssignmentInfo fieldAssignmentInfo)
	{
		object obj2 = fieldAssignmentInfo.targetObject;
		string fieldPath = fieldAssignmentInfo.fieldPath;
		object obj3 = FindContentAsset(in fieldAssignmentInfo.contentAssetPath);
		Object valueToAssign = (Object)((obj3 is Object) ? obj3 : null);
		int currentReadPos = 0;
		try
		{
			if (obj2 == null)
			{
				throw new NullReferenceException("targetObject is null");
			}
			ProcessObject(ref obj2);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError((object)$"Could not assign {obj2}.{fieldPath}={valueToAssign}: {ex}");
			return false;
		}
		void HandleInnerValue(ref object innerValue)
		{
			if (currentReadPos < fieldPath.Length)
			{
				if (innerValue == null)
				{
					throw new Exception(fieldPath.Substring(0, currentReadPos) + " is null.");
				}
				if (innerValue is Object)
				{
					throw new Exception("Assignment cannot propagate through UnityEngine.Object.");
				}
				ProcessObject(ref innerValue);
			}
			else
			{
				innerValue = valueToAssign;
			}
		}
		static bool IsIdentifierChar(char c)
		{
			if (!char.IsLetterOrDigit(c))
			{
				return c == '_';
			}
			return true;
		}
		void ProcessObject(ref object obj)
		{
			Type type = obj.GetType();
			if (obj is IList list)
			{
				int index = TakeArrayIndex();
				object innerValue2 = list[index];
				HandleInnerValue(ref innerValue2);
				list[index] = innerValue2;
			}
			else
			{
				string text2 = TakeIdentifier();
				FieldInfo field = type.GetField(text2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field == null)
				{
					throw new Exception("Field \"" + type.Name + "." + text2 + "\" could not be found.");
				}
				object innerValue3 = field.GetValue(obj);
				HandleInnerValue(ref innerValue3);
				field.SetValue(obj, innerValue3);
			}
		}
		int TakeArrayIndex()
		{
			char c4 = fieldPath[currentReadPos];
			if (c4 != '[')
			{
				throw new FormatException($"Expected char '[' but got '{c4}'.");
			}
			int num5 = currentReadPos + 1;
			currentReadPos = num5;
			int num6 = TakeInt();
			if (num6 < 0)
			{
				throw new FormatException($"Array index {num6} cannot be negative.");
			}
			char c5 = fieldPath[currentReadPos];
			if (c5 != ']')
			{
				throw new FormatException($"Expected char ']' but got '{c5}'.");
			}
			num5 = currentReadPos + 1;
			currentReadPos = num5;
			return num6;
		}
		string TakeIdentifier()
		{
			int num = currentReadPos;
			string text = null;
			char c2 = fieldPath[currentReadPos];
			if (c2 == '.')
			{
				num++;
				int num2 = currentReadPos + 1;
				currentReadPos = num2;
			}
			else if (currentReadPos != 0)
			{
				throw new Exception($"Expected '.' at {num}, but encountered '{c2}'");
			}
			while (currentReadPos < fieldPath.Length && IsIdentifierChar(fieldPath[currentReadPos]))
			{
				int num2 = currentReadPos + 1;
				currentReadPos = num2;
			}
			text = fieldPath.Substring(num, currentReadPos - num);
			if (text == null)
			{
				throw new FormatException($"Expected identifier at {num}, but encountered end of string.");
			}
			if (text.Length == 0)
			{
				throw new FormatException($"Expected identifier at {num}, but encountered no valid characters (a-zA-Z0-9_).");
			}
			if (char.IsDigit(text[0]))
			{
				throw new FormatException($"Expected identifier at {num}, but an identifier cannot begin with a digit. digit={text[0]} substring={text}");
			}
			return text;
		}
		int TakeInt()
		{
			int num3 = currentReadPos;
			if (num3 >= fieldPath.Length)
			{
				throw new FormatException($"Expected integer at {num3}, but encountered end of string.");
			}
			char c3 = fieldPath[currentReadPos];
			if (c3 != '-' && !char.IsDigit(c3))
			{
				throw new FormatException($"Expected integer at {num3}, but an integer cannot begin with character '{c3}'");
			}
			if (c3 == '-')
			{
				int num4 = currentReadPos + 1;
				currentReadPos = num4;
			}
			while (currentReadPos < fieldPath.Length)
			{
				if (!char.IsDigit(fieldPath[currentReadPos]) || currentReadPos == fieldPath.Length - 1)
				{
					fieldPath.Substring(num3, currentReadPos + 1 - num3);
					break;
				}
				int num4 = currentReadPos + 1;
				currentReadPos = num4;
			}
			string obj4 = fieldPath.Substring(num3, currentReadPos - num3) ?? throw new FormatException($"Expected integer at {num3}, but encountered end of string.");
			if (obj4.Length == 0)
			{
				throw new FormatException($"Expected integer at {num3}, but encountered no valid characters (-0-9).");
			}
			return int.Parse(obj4);
		}
	}
}
