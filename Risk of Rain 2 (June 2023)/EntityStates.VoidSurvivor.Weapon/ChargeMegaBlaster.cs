using RoR2;
using UnityEngine;

namespace EntityStates.VoidSurvivor.Weapon;

public class ChargeMegaBlaster : BaseSkillState
{
	[SerializeField]
	public float minimumDuration = 0.1f;

	[SerializeField]
	public float baseDuration = 2f;

	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public string muzzle;

	[SerializeField]
	public string chargeSoundString;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	private float duration;

	private uint soundID;

	private GameObject chargeEffectInstance;

	public override void OnEnter()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		soundID = Util.PlayAttackSpeedSound(chargeSoundString, base.gameObject, attackSpeedStat);
		PlayAnimation(animationLayerName, animationStateName);
		base.characterBody.SetAimTimer(duration + 1f);
		Transform val = FindModelChild(muzzle);
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
			chargeEffectInstance.transform.parent = val;
			ScaleParticleSystemDuration component = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
			ObjectScaleCurve component2 = chargeEffectInstance.GetComponent<ObjectScaleCurve>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = duration;
			}
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.timeMax = duration;
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			if (base.fixedAge >= duration)
			{
				outer.SetNextState(new ReadyMegaBlaster
				{
					activatorSkillSlot = base.activatorSkillSlot
				});
			}
			else if (!IsKeyDownAuthority() && base.fixedAge > minimumDuration)
			{
				outer.SetNextState(new FireMegaBlasterSmall());
			}
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)chargeEffectInstance))
		{
			EntityState.Destroy((Object)(object)chargeEffectInstance);
		}
		AkSoundEngine.StopPlayingID(soundID);
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
