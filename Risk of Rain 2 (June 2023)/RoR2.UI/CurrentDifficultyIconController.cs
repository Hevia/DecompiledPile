using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(Image))]
internal class CurrentDifficultyIconController : MonoBehaviour
{
	private void Start()
	{
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty);
			((Component)this).GetComponent<Image>().sprite = difficultyDef.GetIconSprite();
		}
	}
}
