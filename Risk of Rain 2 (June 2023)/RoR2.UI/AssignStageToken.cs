using TMPro;
using UnityEngine;

namespace RoR2.UI;

public class AssignStageToken : MonoBehaviour
{
	public TextMeshProUGUI titleText;

	public TextMeshProUGUI subtitleText;

	private void Start()
	{
		SceneDef mostRecentSceneDef = SceneCatalog.mostRecentSceneDef;
		((TMP_Text)titleText).SetText(Language.GetString(mostRecentSceneDef.nameToken), true);
		((TMP_Text)subtitleText).SetText(Language.GetString(mostRecentSceneDef.subtitleToken), true);
	}
}
