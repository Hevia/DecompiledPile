using UnityEngine;

public class SetRandomRotation : MonoBehaviour
{
	public bool setRandomXRotation;

	public bool setRandomYRotation;

	public bool setRandomZRotation;

	private void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localEulerAngles = ((Component)this).transform.localEulerAngles;
		if (setRandomXRotation)
		{
			float x = Random.Range(0f, 359f);
			localEulerAngles.x = x;
		}
		if (setRandomYRotation)
		{
			float y = Random.Range(0f, 359f);
			localEulerAngles.y = y;
		}
		if (setRandomZRotation)
		{
			float z = Random.Range(0f, 359f);
			localEulerAngles.z = z;
		}
		((Component)this).transform.localEulerAngles = localEulerAngles;
	}

	private void Update()
	{
	}
}
