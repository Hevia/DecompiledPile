using UnityEngine;
using UnityEngine.Events;

namespace RoR2.EntityLogic;

public class DelayedEvent : MonoBehaviour
{
	public enum TimeStepType
	{
		Time,
		UnscaledTime,
		FixedTime
	}

	public UnityEvent action;

	public TimeStepType timeStepType;

	public void CallDelayed(float timer)
	{
		TimerQueue timerQueue = null;
		switch (timeStepType)
		{
		case TimeStepType.Time:
			timerQueue = RoR2Application.timeTimers;
			break;
		case TimeStepType.UnscaledTime:
			timerQueue = RoR2Application.unscaledTimeTimers;
			break;
		case TimeStepType.FixedTime:
			timerQueue = RoR2Application.fixedTimeTimers;
			break;
		}
		timerQueue?.CreateTimer(timer, Call);
	}

	public void CallDelayedIfActiveAndEnabled(float timer)
	{
		if (((Behaviour)this).isActiveAndEnabled)
		{
			CallDelayed(timer);
		}
	}

	private void Call()
	{
		if (Object.op_Implicit((Object)(object)this) && ((Behaviour)this).enabled)
		{
			action.Invoke();
		}
	}
}
