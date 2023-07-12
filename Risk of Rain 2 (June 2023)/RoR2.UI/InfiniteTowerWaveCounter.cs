using TMPro;
using UnityEngine;

namespace RoR2.UI;

public class InfiniteTowerWaveCounter : MonoBehaviour
{
	[Tooltip("The text we're setting")]
	[SerializeField]
	private TextMeshProUGUI counterText;

	[Tooltip("The language token for the text field")]
	[SerializeField]
	private string token;

	private InfiniteTowerRun runInstance;

	private void OnEnable()
	{
		runInstance = Run.instance as InfiniteTowerRun;
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)runInstance) && Object.op_Implicit((Object)(object)counterText))
		{
			((TMP_Text)counterText).text = Language.GetStringFormatted(token, runInstance.waveIndex);
		}
	}
}
