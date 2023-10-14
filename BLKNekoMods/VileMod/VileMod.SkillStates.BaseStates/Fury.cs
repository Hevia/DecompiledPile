using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using VileMod.Modules;

namespace VileMod.SkillStates.BaseStates;

public class Fury : GenericCharacterMain
{
	public float Timer = 5f;

	public float ChillTime;

	public float ChillDelay = 1.5f;

	public float PassiveTimer = 0f;

	public bool isHeated;

	public float HeatTime = 5f;

	public float baseDuration = 1f;

	public double MinHP;

	public static bool isCrit;

	public static bool isInFury;

	private float timerFury = 0f;

	private float duration;

	private Animator animator;

	public override void OnEnter()
	{
		((GenericCharacterMain)this).OnEnter();
	}

	public override void OnExit()
	{
		((GenericCharacterMain)this).OnExit();
	}

	public override void Update()
	{
		((GenericCharacterMain)this).Update();
		ChillTime += Time.deltaTime;
		if (((ButtonState)(ref ((EntityState)this).inputBank.skill1)).justReleased)
		{
			ChillDelay = 0.1f;
		}
		if (ChillTime >= 2f)
		{
			if (ChillDelay >= 1.5f - ((BaseState)this).attackSpeedStat / 10f)
			{
				ChillDelay = 1.5f - ((BaseState)this).attackSpeedStat / 10f;
			}
			else
			{
				ChillDelay += 0.15f;
			}
			ChillTime = 0f;
		}
		CherryBlast.Chilldelay = ChillDelay;
		Triple7.Chilldelay = ChillDelay;
	}

	public override void FixedUpdate()
	{
		((GenericCharacterMain)this).FixedUpdate();
		Timer += Time.fixedDeltaTime;
		if (((ButtonState)(ref ((EntityState)this).inputBank.skill2)).justReleased || ((ButtonState)(ref ((EntityState)this).inputBank.skill3)).justReleased || ((ButtonState)(ref ((EntityState)this).inputBank.skill4)).justReleased)
		{
			Timer = 0f;
		}
		if (Timer <= HeatTime)
		{
			CherryBlast.heat = true;
			Triple7.heat = true;
		}
		else
		{
			CherryBlast.heat = false;
			CherryBlast.buffSkillIndex = 0;
			Triple7.heat = false;
			Triple7.buffSkillIndex = 0;
		}
		if (((ButtonState)(ref ((EntityState)this).inputBank.skill2)).justReleased && Timer <= HeatTime)
		{
			CherryBlast.buffSkillIndex = 1;
		}
		if (((ButtonState)(ref ((EntityState)this).inputBank.skill3)).justReleased && Timer <= HeatTime)
		{
			CherryBlast.buffSkillIndex = 2;
		}
		if (((ButtonState)(ref ((EntityState)this).inputBank.skill4)).justReleased && Timer <= HeatTime)
		{
			CherryBlast.buffSkillIndex = 3;
		}
		if (((ButtonState)(ref ((EntityState)this).inputBank.skill2)).justReleased && Timer <= HeatTime)
		{
			Triple7.buffSkillIndex = 1;
		}
		if (((ButtonState)(ref ((EntityState)this).inputBank.skill3)).justReleased && Timer <= HeatTime)
		{
			Triple7.buffSkillIndex = 2;
		}
		if (((ButtonState)(ref ((EntityState)this).inputBank.skill4)).justReleased && Timer <= HeatTime)
		{
			Triple7.buffSkillIndex = 3;
		}
		PassiveTimer -= Time.fixedDeltaTime;
		MinHP = 0.35 + (double)(((EntityState)this).characterBody.level / 200f);
		if ((double)((EntityState)this).characterBody.healthComponent.combinedHealthFraction < MinHP && PassiveTimer < 5f)
		{
			Util.PlaySound(Sounds.vilePassive, ((EntityState)this).gameObject);
			((EntityState)this).healthComponent.AddBarrierAuthority(((EntityState)this).characterBody.healthComponent.fullHealth / 2f);
			if (NetworkServer.active)
			{
				((EntityState)this).characterBody.AddTimedBuff(Buffs.LifeSteal, 10f);
				((EntityState)this).characterBody.AddTimedBuff(Buffs.FullCrit, 10f);
				((EntityState)this).characterBody.AddTimedBuff(Buffs.Warbanner, 10f);
				((EntityState)this).characterBody.AddTimedBuff(Buffs.NoCooldowns, 5f);
				((EntityState)this).characterBody.AddTimedBuff(Buffs.VileFuryBuff, 10f);
			}
			PassiveTimer = 50f;
			timerFury = 10f;
		}
		if (((EntityState)this).characterBody.HasBuff(Buffs.FullCrit))
		{
			isCrit = true;
		}
		else
		{
			isCrit = Util.CheckRoll(((BaseState)this).critStat, ((EntityState)this).characterBody.master);
		}
	}

	public static bool GetHeat()
	{
		Fury fury = new Fury();
		return fury.isHeated;
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)1;
	}
}
