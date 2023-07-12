using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using RoR2.ConVar;
using RoR2.Networking;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public static class Chat
{
	public class UserChatMessage : ChatMessageBase
	{
		public GameObject sender;

		public string text;

		public override string ConstructChatString()
		{
			if (Object.op_Implicit((Object)(object)sender))
			{
				NetworkUser component = sender.GetComponent<NetworkUser>();
				if (Object.op_Implicit((Object)(object)component))
				{
					return string.Format(CultureInfo.InvariantCulture, "<color=#e5eefc>{0}: {1}</color>", Util.EscapeRichTextForTextMeshPro(component.userName), Util.EscapeRichTextForTextMeshPro(text));
				}
			}
			return null;
		}

		public override void OnProcessed()
		{
			base.OnProcessed();
			Util.PlaySound("Play_UI_chatMessage", ((Component)RoR2Application.instance).gameObject);
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(sender);
			writer.Write(text);
		}

		public override void Deserialize(NetworkReader reader)
		{
			sender = reader.ReadGameObject();
			text = reader.ReadString();
		}
	}

	public class NpcChatMessage : ChatMessageBase
	{
		public GameObject sender;

		public string baseToken;

		public string sound;

		public string formatStringToken;

		public override string ConstructChatString()
		{
			return Language.GetStringFormatted(formatStringToken, Language.GetString(baseToken));
		}

		public override void OnProcessed()
		{
			base.OnProcessed();
			if (Object.op_Implicit((Object)(object)sender))
			{
				Util.PlaySound(sound, sender);
			}
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(sender);
			writer.Write(baseToken);
			writer.Write(sound);
			writer.Write(formatStringToken);
		}

		public override void Deserialize(NetworkReader reader)
		{
			sender = reader.ReadGameObject();
			baseToken = reader.ReadString();
			sound = reader.ReadString();
			formatStringToken = reader.ReadString();
		}
	}

	public class SimpleChatMessage : ChatMessageBase
	{
		public string baseToken;

		public string[] paramTokens;

		public override string ConstructChatString()
		{
			string text = Language.GetString(baseToken);
			if (paramTokens != null && paramTokens.Length != 0)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				string format = text;
				object[] args = paramTokens;
				text = string.Format(invariantCulture, format, args);
			}
			return text;
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(baseToken);
			GeneratedNetworkCode._WriteArrayString_None(writer, paramTokens);
		}

		public override void Deserialize(NetworkReader reader)
		{
			baseToken = reader.ReadString();
			paramTokens = GeneratedNetworkCode._ReadArrayString_None(reader);
		}
	}

	public class BodyChatMessage : ChatMessageBase
	{
		public GameObject bodyObject;

		public string token;

		public override string ConstructChatString()
		{
			GameObject obj = bodyObject;
			CharacterBody characterBody = ((obj != null) ? obj.GetComponent<CharacterBody>() : null);
			if (Object.op_Implicit((Object)(object)characterBody))
			{
				string bestBodyName = Util.GetBestBodyName(((Component)characterBody).gameObject);
				return string.Format(CultureInfo.InvariantCulture, "<color=#e5eefc>{0}: {1}</color>", Util.EscapeRichTextForTextMeshPro(bestBodyName), Language.GetString(token));
			}
			return null;
		}

		public override void OnProcessed()
		{
			base.OnProcessed();
			Util.PlaySound("Play_UI_chatMessage", ((Component)RoR2Application.instance).gameObject);
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(bodyObject);
			writer.Write(token);
		}

		public override void Deserialize(NetworkReader reader)
		{
			bodyObject = reader.ReadGameObject();
			token = reader.ReadString();
		}
	}

	public class SubjectFormatChatMessage : SubjectChatMessage
	{
		private static readonly string[] empty = new string[0];

		public string[] paramTokens = empty;

		public override string ConstructChatString()
		{
			string @string = Language.GetString(GetResolvedToken());
			string subjectName = GetSubjectName();
			string[] array = new string[1 + paramTokens.Length];
			array[0] = subjectName;
			Array.Copy(paramTokens, 0, array, 1, paramTokens.Length);
			for (int i = 1; i < array.Length; i++)
			{
				array[i] = Language.GetString(array[i]);
			}
			object[] args = array;
			return string.Format(@string, args);
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write((byte)paramTokens.Length);
			for (int i = 0; i < paramTokens.Length; i++)
			{
				writer.Write(paramTokens[i]);
			}
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			paramTokens = new string[reader.ReadByte()];
			for (int i = 0; i < paramTokens.Length; i++)
			{
				paramTokens[i] = reader.ReadString();
			}
		}
	}

	public class PlayerPickupChatMessage : SubjectChatMessage
	{
		public string pickupToken;

		public Color32 pickupColor;

		public uint pickupQuantity;

		public override string ConstructChatString()
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			string subjectName = GetSubjectName();
			string @string = Language.GetString(GetResolvedToken());
			string arg = "";
			if (pickupQuantity != 1)
			{
				arg = "(" + pickupQuantity + ")";
			}
			string str = Language.GetString(pickupToken) ?? "???";
			str = Util.GenerateColoredString(str, pickupColor);
			try
			{
				return string.Format(@string, subjectName, str, arg);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
			return "";
		}

		public override void Serialize(NetworkWriter writer)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			base.Serialize(writer);
			writer.Write(pickupToken);
			writer.Write(pickupColor);
			writer.WritePackedUInt32(pickupQuantity);
		}

		public override void Deserialize(NetworkReader reader)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			base.Deserialize(reader);
			pickupToken = reader.ReadString();
			pickupColor = reader.ReadColor32();
			pickupQuantity = reader.ReadPackedUInt32();
		}
	}

	public class PlayerDeathChatMessage : SubjectFormatChatMessage
	{
		public override string ConstructChatString()
		{
			string text = base.ConstructChatString();
			if (text != null)
			{
				return "<style=cDeath><sprite name=\"Skull\" tint=1> " + text + " <sprite name=\"Skull\" tint=1></style>";
			}
			return text;
		}

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
		}
	}

	public class NamedObjectChatMessage : ChatMessageBase
	{
		public GameObject namedObject;

		public string baseToken;

		public string[] paramTokens;

		public override string ConstructChatString()
		{
			return string.Format(Language.GetString(baseToken), GetObjectName(namedObject));
		}

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(namedObject);
			writer.Write(baseToken);
			GeneratedNetworkCode._WriteArrayString_None(writer, paramTokens);
		}

		public override void Deserialize(NetworkReader reader)
		{
			namedObject = reader.ReadGameObject();
			baseToken = reader.ReadString();
			paramTokens = GeneratedNetworkCode._ReadArrayString_None(reader);
		}
	}

	public class PlayerChatMessage : ChatMessageBase
	{
		public NetworkPlayerName networkPlayerName;

		public string baseToken;

		public override string ConstructChatString()
		{
			return string.Format(Language.GetString(baseToken), networkPlayerName.GetResolvedName());
		}

		public override void Serialize(NetworkWriter writer)
		{
			((MessageBase)this).Serialize(writer);
			writer.Write(networkPlayerName);
			writer.Write(baseToken);
		}

		public override void Deserialize(NetworkReader reader)
		{
			((MessageBase)this).Deserialize(reader);
			networkPlayerName = reader.ReadNetworkPlayerName();
			baseToken = reader.ReadString();
		}
	}

	private static List<string> log = new List<string>();

	private static ReadOnlyCollection<string> _readOnlyLog = log.AsReadOnly();

	private static IntConVar cvChatMaxMessages = new IntConVar("chat_max_messages", ConVarFlags.None, "30", "Maximum number of chat messages to store.");

	public static ReadOnlyCollection<string> readOnlyLog => _readOnlyLog;

	public static event Action onChatChanged;

	public static void AddMessage(string message)
	{
		int num = Mathf.Max(cvChatMaxMessages.value, 1);
		while (log.Count > num)
		{
			log.RemoveAt(0);
		}
		log.Add(message);
		if (Chat.onChatChanged != null)
		{
			Chat.onChatChanged();
		}
		Debug.Log((object)message);
	}

	public static void Clear()
	{
		log.Clear();
		Chat.onChatChanged?.Invoke();
	}

	public static void SendBroadcastChat(ChatMessageBase message)
	{
		SendBroadcastChat(message, QosChannelIndex.chat.intVal);
	}

	public static void SendBroadcastChat(ChatMessageBase message, int channelIndex)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		NetworkWriter val = new NetworkWriter();
		val.StartMessage((short)59);
		val.Write(message.GetTypeIndex());
		val.Write((MessageBase)(object)message);
		val.FinishMessage();
		foreach (NetworkConnection connection in NetworkServer.connections)
		{
			if (connection != null)
			{
				connection.SendWriter(val, channelIndex);
			}
		}
	}

	public static void SendPlayerConnectedMessage(NetworkUser user)
	{
		SendBroadcastChat(new PlayerChatMessage
		{
			networkPlayerName = user.GetNetworkPlayerName(),
			baseToken = "PLAYER_CONNECTED"
		});
	}

	public static void SendPlayerDisconnectedMessage(NetworkUser user)
	{
		SendBroadcastChat(new PlayerChatMessage
		{
			networkPlayerName = user.GetNetworkPlayerName(),
			baseToken = "PLAYER_DISCONNECTED"
		});
	}

	public static void AddPickupMessage(CharacterBody body, string pickupToken, Color32 pickupColor, uint pickupQuantity)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		AddMessage(new PlayerPickupChatMessage
		{
			subjectAsCharacterBody = body,
			baseToken = "PLAYER_PICKUP",
			pickupToken = pickupToken,
			pickupColor = pickupColor,
			pickupQuantity = pickupQuantity
		});
	}

	[NetworkMessageHandler(msgType = 59, client = true)]
	private static void HandleBroadcastChat(NetworkMessage netMsg)
	{
		ChatMessageBase chatMessageBase = ChatMessageBase.Instantiate(netMsg.reader.ReadByte());
		if (chatMessageBase != null)
		{
			((MessageBase)chatMessageBase).Deserialize(netMsg.reader);
			AddMessage(chatMessageBase);
		}
	}

	private static void AddMessage(ChatMessageBase message)
	{
		string text = message.ConstructChatString();
		if (text != null)
		{
			AddMessage(text);
			message.OnProcessed();
		}
	}

	[ConCommand(commandName = "say", flags = ConVarFlags.ExecuteOnServer, helpText = "Sends a chat message.")]
	private static void CCSay(ConCommandArgs args)
	{
		args.CheckArgumentCount(1);
		if (Object.op_Implicit((Object)(object)args.sender))
		{
			SendBroadcastChat(new UserChatMessage
			{
				sender = ((Component)args.sender).gameObject,
				text = args[0]
			});
		}
	}
}
