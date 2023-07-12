using System;
using Facepunch.Steamworks;
using UnityEngine;

namespace RoR2;

public class SteamUserManager : UserManager
{
	public override void GetAvatar(UserID id, GameObject sender, Texture2D cachedTexture, AvatarSize size, Action<Texture2D> onRecieved)
	{
		GetSteamAvatar(id, sender, cachedTexture, size, onRecieved);
	}

	public static void GetSteamAvatar(UserID id, GameObject sender, Texture2D cachedTexture, AvatarSize size, Action<Texture2D> onRecieved)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		ulong iD = id.ID;
		Client instance = Client.Instance;
		AvatarSize val = (AvatarSize)(size switch
		{
			AvatarSize.Small => 0, 
			AvatarSize.Medium => 1, 
			_ => 2, 
		});
		if (instance == null)
		{
			return;
		}
		Image cachedAvatar = instance.Friends.GetCachedAvatar(val, iD);
		if (cachedAvatar != null)
		{
			OnSteamAvatarReceived(cachedAvatar, sender, cachedTexture, onRecieved);
			return;
		}
		Action<Image> action = delegate(Image x)
		{
			OnSteamAvatarReceived(x, sender, cachedTexture, onRecieved);
		};
		instance.Friends.GetAvatar(val, iD, action);
	}

	private static void OnSteamAvatarReceived(Image image, GameObject sender, Texture2D tex, Action<Texture2D> onRecieved)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (image == null || (Object)(object)sender == (Object)null)
		{
			return;
		}
		int width = image.Width;
		int height = image.Height;
		tex = UserManager.BuildTexture(tex, width, height);
		byte[] data = image.Data;
		Color32[] array = (Color32[])(object)new Color32[data.Length / 4];
		for (int i = 0; i < height; i++)
		{
			int num = height - 1 - i;
			for (int j = 0; j < width; j++)
			{
				int num2 = (i * width + j) * 4;
				array[num * width + j] = new Color32(data[num2], data[num2 + 1], data[num2 + 2], data[num2 + 3]);
			}
		}
		if (Object.op_Implicit((Object)(object)tex))
		{
			tex.SetPixels32(array);
			tex.Apply();
		}
		onRecieved(tex);
	}
}
