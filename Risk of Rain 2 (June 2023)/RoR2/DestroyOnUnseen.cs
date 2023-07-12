using UnityEngine;

namespace RoR2;

public class DestroyOnUnseen : MonoBehaviour
{
	public bool cull;

	private Renderer rend;

	private void Start()
	{
		rend = ((Component)this).GetComponentInChildren<Renderer>();
	}

	private void Update()
	{
		if (cull && Object.op_Implicit((Object)(object)rend) && !rend.isVisible)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
