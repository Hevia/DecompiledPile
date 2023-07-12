using UnityEngine;

namespace RoR2;

public class BoneShrink : MonoBehaviour
{
	private Transform transform;

	private void Awake()
	{
		transform = ((Component)this).transform;
	}

	private void LateUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
	}
}
