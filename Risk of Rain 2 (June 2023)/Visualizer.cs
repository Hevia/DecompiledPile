using UnityEngine;

public class Visualizer : MonoBehaviour
{
	public float yscale;

	public GameObject particleObject;

	public float yvalue;

	private Vector3 initialPos;

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		initialPos = particleObject.transform.localPosition;
	}

	private void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		particleObject.transform.localPosition = initialPos + new Vector3(0f, yvalue / yscale, 0f);
	}
}
