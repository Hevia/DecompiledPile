using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Merc;

public class PrepAssaulter2 : BaseState
{
	public static float baseDuration;

	public static float smallHopVelocity;

	public static string enterSoundString;

	private float duration;

	public int dashIndex { private get; set; }

	public override void OnEnter()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("FullBody, Override", "AssaulterPrep", "AssaulterPrep.playbackRate", baseDuration);
		((Component)FindModelChild("PreDashEffect")).gameObject.SetActive(true);
		Util.PlaySound(enterSoundString, base.gameObject);
		Ray aimRay = GetAimRay();
		base.characterDirection.forward = aimRay.direction;
		base.characterDirection.moveVector = aimRay.direction;
		SmallHop(base.characterMotor, smallHopVelocity);
		if (NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
		}
	}

	public override void OnExit()
	{
		if (NetworkServer.active)
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
			base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 0.2f);
		}
		((Component)FindModelChild("PreDashEffect")).gameObject.SetActive(false);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextState(new Assaulter2());
		}
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write((byte)dashIndex);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		dashIndex = reader.ReadByte();
	}
}
