using UnityEngine;

namespace RoR2.UI;

public class OutroFlavorTextController : MonoBehaviour
{
	public LanguageTextMeshController languageTextMeshController;

	protected void Start()
	{
		UpdateFlavorText();
	}

	protected void UpdateFlavorText()
	{
		string text = null;
		LocalUser localUser = ((Component)this).GetComponent<MPEventSystemLocator>().eventSystem?.localUser;
		RunReport runReport = GameOverController.instance?.runReport;
		if (localUser != null && runReport != null)
		{
			RunReport.PlayerInfo playerInfo = runReport.FindPlayerInfo(localUser);
			if (playerInfo != null)
			{
				SurvivorDef survivorDef = SurvivorCatalog.GetSurvivorDef(SurvivorCatalog.GetSurvivorIndexFromBodyIndex(playerInfo.bodyIndex));
				if (Object.op_Implicit((Object)(object)survivorDef))
				{
					text = ((!playerInfo.isDead) ? survivorDef.outroFlavorToken : survivorDef.mainEndingEscapeFailureFlavorToken);
				}
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "GENERIC_OUTRO_FLAVOR";
		}
		if (Object.op_Implicit((Object)(object)languageTextMeshController))
		{
			languageTextMeshController.token = text;
		}
	}
}
