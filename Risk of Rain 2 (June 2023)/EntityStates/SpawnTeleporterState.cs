using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates;

public class SpawnTeleporterState : BaseState
{
	private float duration = 4f;

	public static string soundString;

	public static float initialDelay;

	private bool hasTeleported;

	private Animator modelAnimator;

	private PrintController printController;

	private CharacterModel characterModel;

	private CameraTargetParams.AimRequest aimRequest;

	public override void OnEnter()
	{
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)base.cameraTargetParams))
		{
			aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
		}
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			GameObject val = ((Component)modelAnimator).gameObject;
			characterModel = val.GetComponent<CharacterModel>();
			characterModel.invisibilityCount++;
		}
		if (NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		if (!hasTeleported)
		{
			characterModel.invisibilityCount--;
		}
		aimRequest?.Dispose();
		if (NetworkServer.active)
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
			base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 3f);
		}
	}

	public override void FixedUpdate()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.fixedAge >= initialDelay && !hasTeleported)
		{
			hasTeleported = true;
			characterModel.invisibilityCount--;
			duration = initialDelay;
			TeleportOutController.AddTPOutEffect(characterModel, 1f, 0f, duration);
			GameObject teleportEffectPrefab = Run.instance.GetTeleportEffectPrefab(base.gameObject);
			if (Object.op_Implicit((Object)(object)teleportEffectPrefab))
			{
				EffectManager.SimpleEffect(teleportEffectPrefab, base.transform.position, Quaternion.identity, transmit: false);
			}
			Util.PlaySound(soundString, base.gameObject);
		}
		if (base.fixedAge >= duration && hasTeleported && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
