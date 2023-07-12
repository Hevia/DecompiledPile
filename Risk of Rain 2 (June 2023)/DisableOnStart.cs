using UnityEngine;

public class DisableOnStart : MonoBehaviour
{
	private void Start()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
