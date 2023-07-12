using System;
using RoR2.ConVar;
using UnityEngine;

namespace RoR2;

public abstract class UserManager
{
	public enum AvatarSize
	{
		Small,
		Medium,
		Large
	}

	public static BoolConVar P_UseSocialIcon = new BoolConVar("UseSocialIconFlag", ConVarFlags.Archive, "1", "A per-platform flag that indicates whether we display user icons or not.");

	public static event Action OnDisplayNameMappingComplete;

	internal void InvokeDisplayMappingCompleteAction()
	{
		UserManager.OnDisplayNameMappingComplete?.Invoke();
	}

	public abstract void GetAvatar(UserID userID, GameObject requestSender, Texture2D tex, AvatarSize size, Action<Texture2D> onRecieved);

	protected static Texture2D BuildTexture(Texture2D generatedTexture, int width, int height)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)generatedTexture) && (((Texture)generatedTexture).width != width || ((Texture)generatedTexture).height != height))
		{
			generatedTexture.Resize(width, height);
		}
		if ((Object)(object)generatedTexture == (Object)null)
		{
			generatedTexture = new Texture2D(width, height);
		}
		return generatedTexture;
	}

	public virtual string GetUserDisplayName(UserID other)
	{
		return string.Empty;
	}

	public virtual int GetUserID()
	{
		return 0;
	}

	public virtual string GetUserName()
	{
		return "";
	}
}
