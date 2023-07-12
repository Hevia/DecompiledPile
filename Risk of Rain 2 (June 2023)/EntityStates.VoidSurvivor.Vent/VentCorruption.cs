using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidSurvivor.Vent;

public class VentCorruption : GenericCharacterMain
{
	[SerializeField]
	public float minimumDuration;

	[SerializeField]
	public float maximumDuration;

	[SerializeField]
	public string leftVentEffectChildLocatorEntry;

	[SerializeField]
	public string rightVentEffectChildLocatorEntry;

	[SerializeField]
	public string miniVentEffectChildLocatorEntry;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public string exitSoundString;

	[SerializeField]
	public float animationCrossfadeDuration;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string enterAnimationStateName;

	[SerializeField]
	public string exitAnimationStateName;

	[SerializeField]
	public float hoverVelocity;

	[SerializeField]
	public float hoverAcceleration;

	[SerializeField]
	public float healingPercentagePerSecond;

	[SerializeField]
	public float healingTickRate;

	[SerializeField]
	public float corruptionReductionPerSecond;

	[SerializeField]
	public GameObject crosshairOverridePrefab;

	[SerializeField]
	public float turnSpeed;

	private float healPerTick;

	private float healTickStopwatch;

	private float corruptionReductionPerTick;

	private Vector3 liftVector = Vector3.up;

	private VoidSurvivorController voidSurvivorController;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	private float previousTurnSpeed;

	public override void OnEnter()
	{
		base.OnEnter();
		voidSurvivorController = GetComponent<VoidSurvivorController>();
		voidSurvivorController = GetComponent<VoidSurvivorController>();
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayCrossfade(animationLayerName, enterAnimationStateName, animationCrossfadeDuration);
		healPerTick = base.healthComponent.fullHealth * healingPercentagePerSecond / healingTickRate;
		corruptionReductionPerTick = corruptionReductionPerSecond / healingTickRate;
		Transform obj = FindModelChild(leftVentEffectChildLocatorEntry);
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(true);
		}
		Transform obj2 = FindModelChild(rightVentEffectChildLocatorEntry);
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(true);
		}
		Transform obj3 = FindModelChild(miniVentEffectChildLocatorEntry);
		if (obj3 != null)
		{
			((Component)obj3).gameObject.SetActive(true);
		}
		if (Object.op_Implicit((Object)(object)crosshairOverridePrefab))
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
		}
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			previousTurnSpeed = base.characterDirection.turnSpeed;
			base.characterDirection.turnSpeed = turnSpeed;
		}
	}

	public override void FixedUpdate()
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		base.characterBody.SetAimTimer(1f);
		if (NetworkServer.active)
		{
			healTickStopwatch += Time.fixedDeltaTime;
			if (healTickStopwatch > 1f / healingTickRate)
			{
				healTickStopwatch -= 1f / healingTickRate;
				base.healthComponent.Heal(healPerTick, default(ProcChainMask));
				if (Object.op_Implicit((Object)(object)voidSurvivorController))
				{
					voidSurvivorController.AddCorruption(0f - corruptionReductionPerTick);
				}
			}
		}
		if (!base.isAuthority)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			float y = base.characterMotor.velocity.y;
			if (y < hoverVelocity)
			{
				y = Mathf.MoveTowards(y, hoverVelocity, hoverAcceleration * Time.fixedDeltaTime);
				base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, y, base.characterMotor.velocity.z);
			}
		}
		if (base.fixedAge >= maximumDuration || (base.fixedAge >= minimumDuration && Object.op_Implicit((Object)(object)voidSurvivorController) && voidSurvivorController.corruption <= voidSurvivorController.minimumCorruption))
		{
			outer.SetNextStateToMain();
		}
	}

	protected override bool CanExecuteSkill(GenericSkill skillSlot)
	{
		return false;
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.turnSpeed = previousTurnSpeed;
		}
		crosshairOverrideRequest?.Dispose();
		Transform obj = FindModelChild(leftVentEffectChildLocatorEntry);
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(false);
		}
		Transform obj2 = FindModelChild(rightVentEffectChildLocatorEntry);
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(false);
		}
		Transform obj3 = FindModelChild(miniVentEffectChildLocatorEntry);
		if (obj3 != null)
		{
			((Component)obj3).gameObject.SetActive(false);
		}
		PlayCrossfade(animationLayerName, exitAnimationStateName, animationCrossfadeDuration);
		Util.PlaySound(exitSoundString, base.gameObject);
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
