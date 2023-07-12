using System.Collections.Generic;
using System.Text;
using HG;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.RemoteGameBrowser;

public class RemoteGameCardController : MonoBehaviour
{
	public TextMeshProUGUI nameLabel;

	public TextMeshProUGUI playerCountLabel;

	public TextMeshProUGUI pingLabel;

	public TextMeshProUGUI tagsLabel;

	public TextMeshProUGUI typeLabel;

	public ArtifactDisplayPanelController artifactDisplayPanelController;

	public RawImage mapImage;

	public GameObject passwordIconObject;

	public GameObject difficultyIconObject;

	public Image difficultyIcon;

	private static List<ArtifactDef> artifactBuffer = new List<ArtifactDef>();

	private RemoteGameInfo currentGameInfo;

	public void OpenCurrentGameDetails()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		RectTransform val = (RectTransform)((Component)RoR2Application.instance.mainCanvas).transform;
		GameObject obj = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/RemoteGameBrowser/RemoteGameDetailsPanel"), (Transform)(object)val);
		RectTransform val2 = (RectTransform)obj.transform;
		Rect rect = val2.rect;
		Vector2 val3 = -(((Rect)(ref rect)).size / 2f);
		val3.y = 0f - val3.y;
		((Transform)val2).localPosition = Vector2.op_Implicit(Vector2.zero + val3);
		obj.GetComponent<RemoteGameDetailsPanelController>().SetGameInfo(currentGameInfo);
	}

	public void SetDisplayData(RemoteGameInfo remoteGameInfo)
	{
		currentGameInfo = remoteGameInfo;
		StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
		Texture val = null;
		if (remoteGameInfo.currentSceneIndex.HasValue)
		{
			val = SceneCatalog.GetSceneDef(remoteGameInfo.currentSceneIndex.Value)?.previewTexture;
		}
		((Behaviour)mapImage).enabled = (Object)(object)val != (Object)null;
		mapImage.texture = val;
		passwordIconObject.SetActive(remoteGameInfo.hasPassword ?? false);
		((TMP_Text)playerCountLabel).SetText(stringBuilder.Clear().AppendInt(remoteGameInfo.lobbyPlayerCount ?? remoteGameInfo.serverPlayerCount ?? 0).Append("/")
			.AppendInt(remoteGameInfo.lobbyMaxPlayers ?? remoteGameInfo.serverMaxPlayers ?? 0));
		if (remoteGameInfo.currentDifficultyIndex.HasValue)
		{
			DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(remoteGameInfo.currentDifficultyIndex.Value);
			difficultyIcon.sprite = difficultyDef?.GetIconSprite();
			((Behaviour)difficultyIcon).enabled = true;
		}
		else
		{
			((Behaviour)difficultyIcon).enabled = false;
		}
		((TMP_Text)nameLabel).SetText(remoteGameInfo.serverName ?? remoteGameInfo.lobbyName, true);
		stringBuilder.Clear();
		if (remoteGameInfo.ping.HasValue)
		{
			stringBuilder.AppendInt(remoteGameInfo.ping ?? (-1));
		}
		else
		{
			stringBuilder.Append("N/A");
		}
		((TMP_Text)pingLabel).SetText(stringBuilder);
		stringBuilder.Clear();
		if (remoteGameInfo.tags != null && remoteGameInfo.tags.Length != 0)
		{
			stringBuilder.Append(remoteGameInfo.tags[0]);
			for (int i = 1; i < remoteGameInfo.tags.Length; i++)
			{
				stringBuilder.Append(", ").Append(remoteGameInfo.tags[i]);
			}
		}
		((TMP_Text)tagsLabel).SetText(stringBuilder);
		artifactBuffer.Clear();
		foreach (ArtifactDef enabledArtifact in remoteGameInfo.GetEnabledArtifacts())
		{
			artifactBuffer.Add(enabledArtifact);
		}
		List<ArtifactDef>.Enumerator enabledArtifacts = artifactBuffer.GetEnumerator();
		if (Object.op_Implicit((Object)(object)artifactDisplayPanelController))
		{
			artifactDisplayPanelController.SetDisplayData(ref enabledArtifacts);
		}
		StringBuilderPool.ReturnStringBuilder(stringBuilder);
	}
}
