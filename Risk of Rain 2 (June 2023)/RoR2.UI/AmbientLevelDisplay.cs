using TMPro;
using UnityEngine;

namespace RoR2.UI;

public class AmbientLevelDisplay : MonoBehaviour
{
	public TextMeshProUGUI text;

	private void Update()
	{
		string text = "-";
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			text = Run.instance.ambientLevelFloor.ToString();
		}
		((TMP_Text)this.text).text = Language.GetStringFormatted("AMBIENT_LEVEL_DISPLAY_FORMAT", text);
	}
}
