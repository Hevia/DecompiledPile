using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class ColoredTokenChatMessage : SubjectChatMessage
{
	private static readonly string[] empty = new string[0];

	public string[] paramTokens = empty;

	public Color32[] paramColors = (Color32[])(object)new Color32[0];

	public override string ConstructChatString()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		string @string = Language.GetString(GetResolvedToken());
		string subjectName = GetSubjectName();
		string[] array = new string[1 + paramTokens.Length];
		array[0] = subjectName;
		Array.Copy(paramTokens, 0, array, 1, paramTokens.Length);
		for (int i = 1; i < array.Length; i++)
		{
			int num = i - 1;
			if (num < paramColors.Length)
			{
				array[i] = Util.GenerateColoredString(Language.GetString(array[i]), paramColors[num]);
			}
			else
			{
				array[i] = Language.GetString(array[i]);
			}
		}
		object[] args = array;
		return string.Format(@string, args);
	}

	public override void Serialize(NetworkWriter writer)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		base.Serialize(writer);
		writer.Write((byte)paramTokens.Length);
		for (int i = 0; i < paramTokens.Length; i++)
		{
			writer.Write(paramTokens[i]);
		}
		writer.Write((byte)paramColors.Length);
		for (int j = 0; j < paramColors.Length; j++)
		{
			writer.Write(paramColors[j]);
		}
	}

	public override void Deserialize(NetworkReader reader)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		base.Deserialize(reader);
		paramTokens = new string[reader.ReadByte()];
		for (int i = 0; i < paramTokens.Length; i++)
		{
			paramTokens[i] = reader.ReadString();
		}
		paramColors = (Color32[])(object)new Color32[reader.ReadByte()];
		for (int j = 0; j < paramColors.Length; j++)
		{
			paramColors[j] = reader.ReadColor32();
		}
	}
}
