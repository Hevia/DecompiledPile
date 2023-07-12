using System;
using HG;
using UnityEngine;

namespace RoR2;

public class TimerQueue
{
	public struct TimerHandle : IEquatable<TimerHandle>
	{
		public static readonly TimerHandle invalid = new TimerHandle(-1);

		public readonly int uid;

		public TimerHandle(int uid)
		{
			this.uid = uid;
		}

		public bool Equals(TimerHandle other)
		{
			return uid == other.uid;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is TimerHandle)
			{
				return Equals((TimerHandle)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return uid;
		}
	}

	private struct Timer
	{
		public float time;

		public Action action;

		public TimerHandle handle;
	}

	private float internalTime;

	private int count;

	private Timer[] timers = Array.Empty<Timer>();

	private readonly IndexAllocator indexAllocator = new IndexAllocator();

	private Action[] actionsToCall = Array.Empty<Action>();

	private int actionsToCallCount;

	public TimerHandle CreateTimer(float time, Action action)
	{
		time += internalTime;
		int num = count;
		for (int i = 0; i < count; i++)
		{
			if (time < timers[i].time)
			{
				num = i;
				break;
			}
		}
		TimerHandle timerHandle = new TimerHandle(indexAllocator.RequestIndex());
		Timer timer = default(Timer);
		timer.time = time;
		timer.action = action;
		timer.handle = timerHandle;
		Timer timer2 = timer;
		ArrayUtils.ArrayInsert<Timer>(ref timers, ref count, num, ref timer2);
		return timerHandle;
	}

	public void RemoveTimer(TimerHandle timerHandle)
	{
		for (int i = 0; i < count; i++)
		{
			if (timers[i].handle.Equals(timerHandle))
			{
				RemoveTimerAt(i);
				break;
			}
		}
	}

	private void RemoveTimerAt(int i)
	{
		indexAllocator.FreeIndex(timers[i].handle.uid);
		ArrayUtils.ArrayRemoveAt<Timer>(timers, ref count, i, 1);
	}

	public void Update(float deltaTime)
	{
		internalTime += deltaTime;
		for (int i = 0; i < count && timers[i].time <= internalTime; i++)
		{
			ArrayUtils.ArrayInsert<Action>(ref actionsToCall, ref actionsToCallCount, actionsToCallCount, ref timers[i].action);
		}
		for (int num = actionsToCallCount - 1; num >= 0; num--)
		{
			RemoveTimerAt(num);
		}
		for (int j = 0; j < actionsToCallCount; j++)
		{
			ref Action reference = ref actionsToCall[j];
			try
			{
				reference();
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
			reference = null;
		}
		actionsToCallCount = 0;
	}
}
