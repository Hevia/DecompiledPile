using UnityEngine;

namespace RoR2.UI;

public class RunTimerUIController : MonoBehaviour
{
	public TimerText runStopwatchTimerTextController;

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)runStopwatchTimerTextController))
		{
			runStopwatchTimerTextController.seconds = (Object.op_Implicit((Object)(object)Run.instance) ? Run.instance.GetRunStopwatch() : 0f);
		}
	}
}
