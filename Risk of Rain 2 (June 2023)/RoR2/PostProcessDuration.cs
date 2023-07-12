using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace RoR2;

public class PostProcessDuration : MonoBehaviour
{
	public PostProcessVolume ppVolume;

	public AnimationCurve ppWeightCurve;

	public float maxDuration;

	public bool destroyOnEnd;

	private float stopwatch;

	private void Update()
	{
		stopwatch += Time.deltaTime;
		UpdatePostProccess();
	}

	private void Awake()
	{
		UpdatePostProccess();
	}

	private void OnEnable()
	{
		stopwatch = 0f;
	}

	private void UpdatePostProccess()
	{
		float num = Mathf.Clamp01(stopwatch / maxDuration);
		ppVolume.weight = ppWeightCurve.Evaluate(num);
		if (num == 1f && destroyOnEnd)
		{
			Object.Destroy((Object)(object)((Component)ppVolume).gameObject);
		}
	}

	private void OnValidate()
	{
		if (maxDuration <= Mathf.Epsilon)
		{
			Debug.LogErrorFormat("{0} has PP of time zero!", new object[1] { ((Component)this).gameObject });
		}
	}
}
