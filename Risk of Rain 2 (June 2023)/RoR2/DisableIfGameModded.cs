using UnityEngine;

namespace RoR2;

public class DisableIfGameModded : MonoBehaviour
{
	public void OnEnable()
	{
		if (RoR2Application.isModded)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}
}
