using UnityEngine;

public class RotateItem : MonoBehaviour
{
	public float spinSpeed = 30f;

	public float bobHeight = 0.3f;

	public Vector3 offsetVector = Vector3.zero;

	private float counter;

	private Vector3 initialPosition;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		initialPosition = ((Component)this).transform.position;
	}

	private void Update()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		counter += Time.deltaTime;
		((Component)this).transform.Rotate(new Vector3(0f, spinSpeed * Time.deltaTime, 0f), (Space)0);
		if (Object.op_Implicit((Object)(object)((Component)this).transform.parent))
		{
			((Component)this).transform.localPosition = offsetVector + new Vector3(0f, 0f, Mathf.Sin(counter) * bobHeight);
		}
		else
		{
			((Component)this).transform.position = initialPosition + new Vector3(0f, Mathf.Sin(counter) * bobHeight, 0f);
		}
	}
}
