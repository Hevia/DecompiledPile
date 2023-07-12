using System;
using System.Globalization;
using Facepunch.Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class SteamJoinClipboardLobby : MonoBehaviour
{
	public TextMeshProUGUI buttonText;

	public TextMeshProUGUI resultTextComponent;

	public MPButton mpButton;

	private CSteamID clipboardLobbyID;

	private const float resultTextDuration = 4f;

	protected float resultTextTimer;

	private static SteamJoinClipboardLobby instance;

	public bool validClipboardLobbyID { get; private set; }

	private void OnEnable()
	{
		SingletonHelper.Assign<SteamJoinClipboardLobby>(ref instance, this);
	}

	private void OnDisable()
	{
		SingletonHelper.Unassign<SteamJoinClipboardLobby>(ref instance, this);
	}

	[SystemInitializer(new Type[] { })]
	public void Init()
	{
		LobbyManager lobbyManager = PlatformSystems.lobbyManager;
		lobbyManager.onLobbyJoined = (Action<bool>)Delegate.Combine(lobbyManager.onLobbyJoined, new Action<bool>(OnLobbyJoined));
	}

	private static void OnLobbyJoined(bool success)
	{
		if (Object.op_Implicit((Object)(object)instance) && Object.op_Implicit((Object)(object)instance.resultTextComponent))
		{
			instance.resultTextTimer = 4f;
			((Behaviour)instance.resultTextComponent).enabled = true;
			((TMP_Text)instance.resultTextComponent).SetText(Language.GetString(success ? "STEAM_JOIN_LOBBY_CLIPBOARD_SUCCESS" : "STEAM_JOIN_LOBBY_CLIPBOARD_FAIL"), true);
		}
	}

	private static bool IsLobbyIdValid(CSteamID lobbyId)
	{
		return lobbyId != CSteamID.nil;
	}

	private static CSteamID FetchClipboardLobbyId()
	{
		if (PlatformSystems.EgsToggleConVar.value == 1)
		{
			return FetchClipboardLobbyIdEGS();
		}
		return FetchClipboardLobbyIdSTEAM();
	}

	private static CSteamID FetchClipboardLobbyIdSTEAM()
	{
		if (CSteamID.TryParse(GUIUtility.systemCopyBuffer, out var result))
		{
			Client obj = Client.Instance;
			CSteamID cSteamID = new CSteamID((obj != null) ? obj.Lobby.CurrentLobby : CSteamID.nil.steamValue);
			if (result.isLobby && cSteamID != result)
			{
				return result;
			}
		}
		return CSteamID.nil;
	}

	private static CSteamID FetchClipboardLobbyIdEGS()
	{
		string systemCopyBuffer = GUIUtility.systemCopyBuffer;
		if ((PlatformSystems.lobbyManager as EOSLobbyManager).CurrentLobbyId != systemCopyBuffer && systemCopyBuffer != string.Empty)
		{
			return new CSteamID(systemCopyBuffer);
		}
		return CSteamID.nil;
	}

	private void FixedUpdate()
	{
		clipboardLobbyID = FetchClipboardLobbyId();
		validClipboardLobbyID = IsLobbyIdValid(clipboardLobbyID);
		if ((Object)(object)mpButton != (Object)null)
		{
			((Selectable)mpButton).interactable = validClipboardLobbyID;
		}
		if ((Object)(object)resultTextComponent != (Object)null)
		{
			if (resultTextTimer > 0f)
			{
				resultTextTimer -= Time.fixedDeltaTime;
				((Behaviour)resultTextComponent).enabled = true;
			}
			else
			{
				((Behaviour)resultTextComponent).enabled = false;
			}
		}
	}

	public void TryToJoinClipboardLobby()
	{
		Console.instance.SubmitCmd(null, string.Format(CultureInfo.InvariantCulture, "steam_lobby_join {0}", clipboardLobbyID.ToString()), recordSubmit: true);
	}
}
