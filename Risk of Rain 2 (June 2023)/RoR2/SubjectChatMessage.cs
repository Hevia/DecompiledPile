using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class SubjectChatMessage : ChatMessageBase
{
	private GameObject subjectNetworkUserObject;

	private GameObject subjectCharacterBodyGameObject;

	public string baseToken;

	private MemoizedGetComponent<NetworkUser> subjectNetworkUserGetComponent;

	private MemoizedGetComponent<CharacterBody> subjectCharacterBodyGetComponent;

	public NetworkUser subjectAsNetworkUser
	{
		get
		{
			return subjectNetworkUserGetComponent.Get(subjectNetworkUserObject);
		}
		set
		{
			subjectNetworkUserObject = (Object.op_Implicit((Object)(object)value) ? ((Component)value).gameObject : null);
			CharacterBody characterBody = null;
			if (Object.op_Implicit((Object)(object)value))
			{
				characterBody = value.GetCurrentBody();
			}
			subjectCharacterBodyGameObject = (Object.op_Implicit((Object)(object)characterBody) ? ((Component)characterBody).gameObject : null);
		}
	}

	public CharacterBody subjectAsCharacterBody
	{
		get
		{
			return subjectCharacterBodyGetComponent.Get(subjectCharacterBodyGameObject);
		}
		set
		{
			subjectCharacterBodyGameObject = (Object.op_Implicit((Object)(object)value) ? ((Component)value).gameObject : null);
			NetworkUser networkUser = Util.LookUpBodyNetworkUser(value);
			subjectNetworkUserObject = ((networkUser != null) ? ((Component)networkUser).gameObject : null);
		}
	}

	protected string GetSubjectName()
	{
		if (Object.op_Implicit((Object)(object)subjectAsNetworkUser))
		{
			return Util.EscapeRichTextForTextMeshPro(subjectAsNetworkUser.userName);
		}
		if (Object.op_Implicit((Object)(object)subjectAsCharacterBody))
		{
			return subjectAsCharacterBody.GetDisplayName();
		}
		return "???";
	}

	protected bool IsSecondPerson()
	{
		if (LocalUserManager.readOnlyLocalUsersList.Count == 1 && Object.op_Implicit((Object)(object)subjectAsNetworkUser) && subjectAsNetworkUser.localUser != null)
		{
			return true;
		}
		return false;
	}

	protected string GetResolvedToken()
	{
		if (!IsSecondPerson())
		{
			return baseToken;
		}
		return baseToken + "_2P";
	}

	public override string ConstructChatString()
	{
		return string.Format(Language.GetString(GetResolvedToken()), GetSubjectName());
	}

	public override void Serialize(NetworkWriter writer)
	{
		((MessageBase)this).Serialize(writer);
		writer.Write(subjectNetworkUserObject);
		writer.Write(subjectCharacterBodyGameObject);
		writer.Write(baseToken);
	}

	public override void Deserialize(NetworkReader reader)
	{
		((MessageBase)this).Deserialize(reader);
		subjectNetworkUserObject = reader.ReadGameObject();
		subjectCharacterBodyGameObject = reader.ReadGameObject();
		baseToken = reader.ReadString();
	}
}
