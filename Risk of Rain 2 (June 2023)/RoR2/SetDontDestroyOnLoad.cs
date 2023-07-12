using UnityEngine;

namespace RoR2;

public class SetDontDestroyOnLoad : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
	}
}
