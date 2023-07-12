using UnityEngine;

public class MoveCurve : MonoBehaviour
{
	public bool animateX;

	public bool animateY;

	public bool animateZ;

	public float curveScale = 1f;

	public AnimationCurve moveCurve;

	private float xValue;

	private float yValue;

	private float zValue;

	private void Start()
	{
	}

	private void Update()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		if (animateX)
		{
			xValue = moveCurve.Evaluate(Time.time % (float)moveCurve.length) * curveScale;
		}
		else
		{
			xValue = ((Component)this).transform.localPosition.x;
		}
		if (animateY)
		{
			yValue = moveCurve.Evaluate(Time.time % (float)moveCurve.length) * curveScale;
		}
		else
		{
			yValue = ((Component)this).transform.localPosition.y;
		}
		if (animateZ)
		{
			zValue = moveCurve.Evaluate(Time.time % (float)moveCurve.length) * curveScale;
		}
		else
		{
			zValue = ((Component)this).transform.localPosition.z;
		}
		((Component)this).transform.localPosition = new Vector3(xValue, yValue, zValue);
	}
}
