using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Railgunner.Scope;

public class BaseScopeState : BaseSkillState
{
	[SerializeField]
	public GameObject crosshairOverridePrefab;

	[SerializeField]
	public GameObject scopeOverlayPrefab;

	[SerializeField]
	public CharacterCameraParams cameraParams;

	[SerializeField]
	public float cameraOverridePriority;

	public static string mecanimBoolName;

	private OverlayController overlayController;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	private CameraTargetParams.CameraParamsOverrideHandle cameraParamsOverrideHandle;

	public override void OnEnter()
	{
		base.OnEnter();
		overlayController = HudOverlayManager.AddOverlay(base.gameObject, new OverlayCreationParams
		{
			prefab = scopeOverlayPrefab,
			childLocatorEntry = "ScopeContainer"
		});
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.SetBool(mecanimBoolName, true);
		}
		if (Object.op_Implicit((Object)(object)crosshairOverridePrefab))
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
		}
	}

	public override void OnExit()
	{
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.SetBool(mecanimBoolName, false);
		}
		RemoveOverlay(0f);
		crosshairOverrideRequest?.Dispose();
		base.OnExit();
	}

	protected void SetScopeAlpha(float alpha)
	{
		if (overlayController != null)
		{
			overlayController.alpha = alpha;
		}
	}

	protected void RemoveOverlay(float transitionDuration)
	{
		if (overlayController != null)
		{
			HudOverlayManager.RemoveOverlay(overlayController);
			overlayController = null;
		}
	}

	protected void StartScopeParamsOverride(float transitionDuration)
	{
		if (!cameraParamsOverrideHandle.isValid)
		{
			cameraParamsOverrideHandle = base.cameraTargetParams.AddParamsOverride(new CameraTargetParams.CameraParamsOverrideRequest
			{
				cameraParamsData = cameraParams.data,
				priority = cameraOverridePriority
			}, transitionDuration);
		}
	}

	protected void EndScopeParamsOverride(float transitionDuration)
	{
		if (cameraParamsOverrideHandle.isValid)
		{
			base.cameraTargetParams.RemoveParamsOverride(cameraParamsOverrideHandle, transitionDuration);
			cameraParamsOverrideHandle = default(CameraTargetParams.CameraParamsOverrideHandle);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	protected virtual CharacterCameraParams GetCameraParams()
	{
		return cameraParams;
	}

	protected virtual float GetScopeEntryDuration()
	{
		return 0f;
	}
}
