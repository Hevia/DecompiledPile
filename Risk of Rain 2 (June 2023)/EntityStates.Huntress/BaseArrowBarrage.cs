using RoR2;
using UnityEngine;

namespace EntityStates.Huntress;

public class BaseArrowBarrage : BaseState
{
	[SerializeField]
	public float maxDuration;

	[SerializeField]
	public string beginLoopSoundString;

	[SerializeField]
	public string endLoopSoundString;

	[SerializeField]
	public string fireSoundString;

	private HuntressTracker huntressTracker;

	private CameraTargetParams.AimRequest aimRequest;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(beginLoopSoundString, base.gameObject);
		huntressTracker = GetComponent<HuntressTracker>();
		if (Object.op_Implicit((Object)(object)huntressTracker))
		{
			((Behaviour)huntressTracker).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)base.cameraTargetParams))
		{
			aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
		}
	}

	public override void FixedUpdate()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity = Vector3.zero;
		}
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.inputBank))
		{
			if (Object.op_Implicit((Object)(object)base.skillLocator) && base.skillLocator.utility.IsReady() && base.inputBank.skill3.justPressed)
			{
				outer.SetNextStateToMain();
			}
			else if (base.fixedAge >= maxDuration || base.inputBank.skill1.justPressed || base.inputBank.skill4.justPressed)
			{
				HandlePrimaryAttack();
			}
		}
	}

	protected virtual void HandlePrimaryAttack()
	{
	}

	public override void OnExit()
	{
		PlayAnimation("FullBody, Override", "FireArrowRain");
		Util.PlaySound(endLoopSoundString, base.gameObject);
		Util.PlaySound(fireSoundString, base.gameObject);
		aimRequest?.Dispose();
		if (Object.op_Implicit((Object)(object)huntressTracker))
		{
			((Behaviour)huntressTracker).enabled = true;
		}
		base.OnExit();
	}
}
