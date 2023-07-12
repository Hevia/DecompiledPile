using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2;

[RequireComponent(typeof(RawImage))]
public class SocialUserIcon : UIBehaviour
{
	private enum SourceType
	{
		Local,
		Network
	}

	private RawImage rawImageComponent;

	protected Texture2D generatedTexture;

	private UserID userID;

	[SerializeField]
	private UserManager.AvatarSize avatarSize;

	private SourceType sourceType;

	private Texture defaultTexture => LegacyResourcesAPI.Load<Texture>("Textures/UI/texDefaultSocialUserIcon");

	protected override void OnDestroy()
	{
		Object.Destroy((Object)(object)generatedTexture);
		generatedTexture = null;
		((UIBehaviour)this).OnDestroy();
	}

	protected override void Awake()
	{
		((UIBehaviour)this).Awake();
		rawImageComponent = ((Component)this).GetComponent<RawImage>();
		rawImageComponent.texture = defaultTexture;
		if (!UserManager.P_UseSocialIcon.value)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void HandleNewTexture(Texture2D tex)
	{
		generatedTexture = tex;
		rawImageComponent.texture = (Texture)(object)tex;
	}

	public virtual void Refresh()
	{
		if (PlatformSystems.lobbyManager.HasMPLobbyFeature(MPLobbyFeatures.UserIcon))
		{
			PlatformSystems.userManager.GetAvatar(userID, ((Component)this).gameObject, generatedTexture, avatarSize, HandleNewTexture);
			if (!Object.op_Implicit((Object)(object)generatedTexture))
			{
				rawImageComponent.texture = defaultTexture;
			}
		}
	}

	public virtual void SetFromMaster(CharacterMaster master)
	{
		if (!PlatformSystems.lobbyManager.HasMPLobbyFeature(MPLobbyFeatures.UserIcon))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)master))
		{
			PlayerCharacterMasterController component = ((Component)master).GetComponent<PlayerCharacterMasterController>();
			if (Object.op_Implicit((Object)(object)component))
			{
				NetworkUser networkUser = component.networkUser;
				RefreshWithUser(new UserID(new CSteamID(networkUser.id.value)));
				return;
			}
		}
		userID = default(UserID);
		sourceType = SourceType.Local;
		Refresh();
	}

	public void RefreshWithUser(UserID newUserID)
	{
		if (PlatformSystems.lobbyManager.HasMPLobbyFeature(MPLobbyFeatures.UserIcon) && (sourceType != SourceType.Network || !newUserID.Equals(userID)))
		{
			sourceType = SourceType.Network;
			userID = newUserID;
			Refresh();
		}
	}
}
