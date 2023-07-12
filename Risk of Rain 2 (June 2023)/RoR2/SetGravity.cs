using UnityEngine;

namespace RoR2;

public class SetGravity : MonoBehaviour
{
	public float newGravity = -30f;

	private void OnEnable()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Physics.gravity = new Vector3(0f, newGravity, 0f);
	}

	private void OnDisable()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Physics.gravity = new Vector3(0f, Run.baseGravity, 0f);
	}
}
