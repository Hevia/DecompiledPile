using UnityEngine;

public class RotateObject : MonoBehaviour
{
	public Vector3 rotationSpeed;

	private void Start()
	{
	}

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.Rotate(rotationSpeed * Time.deltaTime);
	}
}
