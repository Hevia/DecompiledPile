using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Merc;

public class FocusedAssaultPrep : BaseState
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public float smallHopVelocity;

	[SerializeField]
	public string enterSoundString;

	private float duration;

	public int dashIndex { private get; set; }

	public override void OnEnter()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("FullBody, Override", "AssaulterPrep", "AssaulterPrep.playbackRate", baseDuration);
		GameObject obj = FindModelChildGameObject("PreDashEffect");
		if (obj != null)
		{
			obj.SetActive(true);
		}
		Util.PlaySound(enterSoundString, base.gameObject);
		Ray aimRay = GetAimRay();
		base.characterDirection.forward = ((Ray)(ref aimRay)).direction;
		base.characterDirection.moveVector = ((Ray)(ref aimRay)).direction;
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
		GameObject obj = FindModelChildGameObject("PreDashEffect");
		if (obj != null)
		{
			obj.SetActive(false);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextState(new FocusedAssaultDash());
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
