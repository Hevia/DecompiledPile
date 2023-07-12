using TMPro;
using UnityEngine;

namespace RoR2.UI;

public class StageCountDisplay : MonoBehaviour
{
	public TextMeshProUGUI text;

	private void Update()
	{
		string text = "-";
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			text = (Run.instance.stageClearCount + 1).ToString();
		}
		((TMP_Text)this.text).text = Language.GetStringFormatted("STAGE_COUNT_FORMAT", text);
	}
}
