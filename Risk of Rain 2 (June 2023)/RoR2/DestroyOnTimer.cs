using UnityEngine;

namespace RoR2;

public class DestroyOnTimer : MonoBehaviour
{
	public float duration;

	public bool resetAgeOnDisable;

	private float age;

	private void FixedUpdate()
	{
		age += Time.fixedDeltaTime;
		if (age > duration)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void OnDisable()
	{
		if (resetAgeOnDisable)
		{
			age = 0f;
		}
	}
}
