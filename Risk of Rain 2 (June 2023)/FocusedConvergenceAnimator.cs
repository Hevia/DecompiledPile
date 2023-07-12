using UnityEngine;

public class FocusedConvergenceAnimator : MonoBehaviour
{
	public bool animateX;

	public bool animateY;

	public bool animateZ;

	private AnimationCurve animCurve;

	private float xScale;

	private float yScale;

	private float zScale;

	public float scaleTime = 4f;

	private float tempScaleTimer = 4f;

	public float rotateTime = 4f;

	private float tempRotateTimer = 4f;

	private float scaleMax = 1.2f;

	private float scaleMin = 0.8f;

	private bool isScaling;

	private float xRotate;

	private float xRotateTop;

	private float yRotate;

	private float yRotateTop;

	private float zRotate;

	private float zRotateTop;

	private void Start()
	{
		tempScaleTimer = 0f;
		tempRotateTimer = 0f;
		animCurve = AnimationCurve.EaseInOut(0f, scaleMin, 1f, scaleMax);
	}

	private void FixedUpdate()
	{
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		if (isScaling)
		{
			tempScaleTimer += Time.deltaTime;
			if (tempScaleTimer <= scaleTime * 0.333f)
			{
				float num = tempScaleTimer / (scaleTime * 0.333f);
				xScale = Mathf.Lerp(1f, scaleMax, num);
				yScale = Mathf.Lerp(1f, scaleMax, num);
				zScale = Mathf.Lerp(1f, scaleMax, num);
			}
			if (tempScaleTimer > scaleTime * 0.333f && tempScaleTimer <= scaleTime * 0.666f)
			{
				float num = (tempScaleTimer - scaleTime * 0.333f) / (scaleTime * 0.333f);
				xScale = Mathf.Lerp(scaleMax, scaleMin, num);
				yScale = Mathf.Lerp(scaleMax, scaleMin, num);
				zScale = Mathf.Lerp(scaleMax, scaleMin, num);
			}
			if (tempScaleTimer > scaleTime * 0.666f)
			{
				float num = (tempScaleTimer - scaleTime * 0.666f) / (scaleTime * 0.333f);
				xScale = Mathf.Lerp(scaleMin, 1f, num);
				yScale = Mathf.Lerp(scaleMin, 1f, num);
				zScale = Mathf.Lerp(scaleMin, 1f, num);
			}
			if (tempScaleTimer >= scaleTime)
			{
				isScaling = false;
				tempRotateTimer = 0f;
				xRotateTop = Random.Range(0f, 10f);
				yRotateTop = Random.Range(0f, 10f);
				zRotateTop = Random.Range(0f, 10f);
			}
			else
			{
				((Component)this).transform.localScale = new Vector3(xScale, yScale, zScale);
			}
		}
		else
		{
			tempRotateTimer += Time.deltaTime;
			if (tempRotateTimer <= rotateTime * 0.5f)
			{
				float num2 = tempRotateTimer / (rotateTime * 0.5f);
				xRotate = Mathf.Lerp(0f, xRotateTop, num2);
				yRotate = Mathf.Lerp(0f, yRotateTop, num2);
				zRotate = Mathf.Lerp(0f, zRotateTop, num2);
			}
			else
			{
				float num2 = (tempRotateTimer - rotateTime * 0.5f) / (rotateTime * 0.5f);
				xRotate = Mathf.Lerp(xRotateTop, 0f, num2);
				yRotate = Mathf.Lerp(yRotateTop, 0f, num2);
				zRotate = Mathf.Lerp(zRotateTop, 0f, num2);
			}
			if (tempRotateTimer >= rotateTime)
			{
				isScaling = true;
				tempScaleTimer = 0f;
			}
			else
			{
				((Component)this).transform.Rotate(new Vector3(xRotate, yRotate, zRotate));
			}
		}
	}
}
