using EntityStates;

namespace VileMod.SkillStates.BaseStates;

public class BaseTimedSkillState : BaseSkillState
{
	public static float TimedBaseDuration;

	public static float TimedBaseCastStartTime;

	public static float TimedBaseCastEndTime;

	protected float duration;

	protected float castStartTime;

	protected float castEndTime;

	protected bool hasFired;

	protected bool isFiring;

	protected bool hasExited;

	protected virtual void InitDurationValues(float baseDuration, float baseCastStartTime, float baseCastEndTime = 1f)
	{
		TimedBaseDuration = baseDuration;
		TimedBaseCastStartTime = baseCastStartTime;
		TimedBaseCastEndTime = baseCastEndTime;
		duration = TimedBaseDuration / ((BaseState)this).attackSpeedStat;
		castStartTime = baseCastStartTime * duration;
		castEndTime = baseCastEndTime * duration;
	}

	protected virtual void OnCastEnter()
	{
	}

	protected virtual void OnCastFixedUpdate()
	{
	}

	protected virtual void OnCastUpdate()
	{
	}

	protected virtual void OnCastExit()
	{
	}

	public override void FixedUpdate()
	{
		((EntityState)this).FixedUpdate();
		if (!hasFired && ((EntityState)this).fixedAge > castStartTime)
		{
			hasFired = true;
			OnCastEnter();
		}
		bool flag = ((EntityState)this).fixedAge >= castStartTime;
		bool flag2 = ((EntityState)this).fixedAge >= castEndTime;
		isFiring = false;
		if ((flag && !flag2) || (flag && flag2 && !hasFired))
		{
			isFiring = true;
			OnCastFixedUpdate();
		}
		if (flag2 && !hasExited)
		{
			hasExited = true;
			OnCastExit();
		}
		if (((EntityState)this).fixedAge > duration)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	public override void Update()
	{
		((EntityState)this).Update();
		if (isFiring)
		{
			OnCastUpdate();
		}
	}
}
