using TMPro;
using UnityEngine;

namespace RoR2.UI;

public class InfiniteTowerEnemyCounter : MonoBehaviour
{
	[Tooltip("The root we're toggling")]
	[SerializeField]
	private GameObject rootObject;

	[Tooltip("The text we're setting")]
	[SerializeField]
	private TextMeshProUGUI counterText;

	[SerializeField]
	[Tooltip("The language token for the text field")]
	private string token;

	private InfiniteTowerWaveController waveController;

	private CombatSquad combatSquad;

	private void OnEnable()
	{
		InfiniteTowerRun infiniteTowerRun = Run.instance as InfiniteTowerRun;
		if (!Object.op_Implicit((Object)(object)infiniteTowerRun))
		{
			return;
		}
		waveController = infiniteTowerRun.waveController;
		if (Object.op_Implicit((Object)(object)waveController))
		{
			combatSquad = ((Component)waveController).GetComponent<CombatSquad>();
			if (Object.op_Implicit((Object)(object)combatSquad))
			{
				rootObject.SetActive(waveController.HasFullProgress() && combatSquad.memberCount > 0);
			}
			else
			{
				rootObject.SetActive(false);
			}
		}
		else
		{
			rootObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)combatSquad))
		{
			rootObject.SetActive(waveController.HasFullProgress() && combatSquad.memberCount > 0);
			if (Object.op_Implicit((Object)(object)counterText))
			{
				string text = combatSquad.memberCount.ToString();
				((TMP_Text)counterText).text = Language.GetStringFormatted(token, text);
			}
		}
	}
}
