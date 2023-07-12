using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class InfiniteTowerWaveProgressBar : MonoBehaviour
{
	[Tooltip("The bar we're filling up")]
	[SerializeField]
	private Image barImage;

	[SerializeField]
	private Animator animator;

	private InfiniteTowerWaveController waveController;

	private float previousFillAmount;

	private void OnEnable()
	{
		InfiniteTowerRun infiniteTowerRun = Run.instance as InfiniteTowerRun;
		if (Object.op_Implicit((Object)(object)infiniteTowerRun))
		{
			waveController = infiniteTowerRun.waveController;
		}
	}

	private void Update()
	{
		if (!Object.op_Implicit((Object)(object)waveController))
		{
			return;
		}
		float normalizedProgress = waveController.GetNormalizedProgress();
		if (normalizedProgress > previousFillAmount)
		{
			previousFillAmount = normalizedProgress;
			if (Object.op_Implicit((Object)(object)animator))
			{
				int layerIndex = animator.GetLayerIndex("Base");
				animator.Play("Idle", layerIndex);
				animator.Update(0f);
				animator.Play((normalizedProgress >= 1f) ? "Ready" : "Pulse", layerIndex);
			}
		}
		if (Object.op_Implicit((Object)(object)barImage))
		{
			barImage.fillAmount = normalizedProgress;
		}
	}
}
