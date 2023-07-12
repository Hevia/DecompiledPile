using UnityEngine;

namespace RoR2;

public class UnparentOnStart : MonoBehaviour
{
	private void Start()
	{
		((Component)this).transform.parent = null;
	}
}
