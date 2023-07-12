using TMPro;
using UnityEngine;

namespace RoR2;

public class TimerHologramContent : MonoBehaviour
{
	public float displayValue;

	public TextMeshPro targetTextMesh;

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)targetTextMesh))
		{
			int num = Mathf.FloorToInt(displayValue);
			int num2 = Mathf.FloorToInt((displayValue - (float)num) * 100f);
			((TMP_Text)targetTextMesh).text = $"{num:D}.{num2:D2}";
		}
	}
}
