using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidSurvivor.Weapon;

public class ChargeCrabCannon : BaseSkillState
{
	[SerializeField]
	public float baseDurationPerGrenade;

	[SerializeField]
	public float minimumDuration;

	[SerializeField]
	public string muzzle;

	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public string chargeStockSoundString;

	[SerializeField]
	public string chargeLoopStartSoundString;

	[SerializeField]
	public string chargeLoopStopSoundString;

	[SerializeField]
	public float bloomPerGrenade;

	[SerializeField]
	public float corruptionPerGrenade;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	private VoidSurvivorController voidSurvivorController;

	private GameObject chargeEffectInstance;

	private int grenadeCount;

	private int lastGrenadeCount;

	private float durationPerGrenade;

	private float nextGrenadeStopwatch;

	public override void OnEnter()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		voidSurvivorController = GetComponent<VoidSurvivorController>();
		PlayAnimation(animationLayerName, animationStateName);
		durationPerGrenade = baseDurationPerGrenade / attackSpeedStat;
		Util.PlaySound(chargeLoopStartSoundString, base.gameObject);
		AddGrenade();
		Transform modelTransform = GetModelTransform();
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		Transform val = component.FindChild(muzzle);
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
			chargeEffectInstance.transform.parent = val;
			ScaleParticleSystemDuration component2 = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.newDuration = durationPerGrenade;
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		Util.PlaySound(chargeLoopStopSoundString, base.gameObject);
		PlayAnimation(animationLayerName, "BufferEmpty");
		EntityState.Destroy((Object)(object)chargeEffectInstance);
	}

	private void AddGrenade()
	{
		grenadeCount++;
		if (Object.op_Implicit((Object)(object)voidSurvivorController) && NetworkServer.active)
		{
			voidSurvivorController.AddCorruption(corruptionPerGrenade);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		base.characterBody.SetAimTimer(3f);
		nextGrenadeStopwatch += Time.fixedDeltaTime;
		if (nextGrenadeStopwatch > durationPerGrenade && base.activatorSkillSlot.stock > 0)
		{
			AddGrenade();
			nextGrenadeStopwatch -= durationPerGrenade;
			base.activatorSkillSlot.DeductStock(1);
		}
		float value = bloomPerGrenade * (float)lastGrenadeCount;
		base.characterBody.SetSpreadBloom(value);
		if (lastGrenadeCount < grenadeCount)
		{
			Util.PlaySound(chargeStockSoundString, base.gameObject);
		}
		if (!IsKeyDownAuthority() && base.fixedAge > minimumDuration / attackSpeedStat && base.isAuthority)
		{
			FireCrabCannon fireCrabCannon = new FireCrabCannon();
			fireCrabCannon.grenadeCountMax = grenadeCount;
			outer.SetNextState(fireCrabCannon);
		}
		else
		{
			lastGrenadeCount = grenadeCount;
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
