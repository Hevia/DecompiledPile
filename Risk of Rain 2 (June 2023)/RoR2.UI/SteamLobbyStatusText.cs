using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(LanguageTextMeshController))]
public class SteamLobbyStatusText : MonoBehaviour
{
	private LanguageTextMeshController languageTextMeshController;

	private void Start()
	{
		languageTextMeshController = ((Component)this).GetComponent<LanguageTextMeshController>();
	}

	private void Update()
	{
		LobbyType currentLobbyType = PlatformSystems.lobbyManager.currentLobbyType;
		for (int i = 0; i < LobbyUserList.lobbyStateChoices.Length; i++)
		{
			if (currentLobbyType == LobbyUserList.lobbyStateChoices[i].lobbyType)
			{
				languageTextMeshController.token = LobbyUserList.lobbyStateChoices[i].token;
				break;
			}
		}
	}
}
