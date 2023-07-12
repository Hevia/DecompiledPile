using RoR2;
using UnityEngine;

namespace EntityStates.VoidSurvivor.Weapon;

public class ReadyMegaBlaster : BaseSkillState
{
	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public string muzzle;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public string exitSoundString;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	private GameObject chargeEffectInstance;

	public override void OnEnter()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayAnimation(animationLayerName, animationStateName);
		Transform val = FindModelChild(muzzle);
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
			chargeEffectInstance.transform.parent = val;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		base.characterBody.SetAimTimer(3f);
		if (base.isAuthority && !IsKeyDownAuthority())
		{
			outer.SetNextState(new FireMegaBlasterBig());
		}
	}

	public override void OnExit()
	{
		Util.PlaySound(exitSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)chargeEffectInstance))
		{
			EntityState.Destroy((Object)(object)chargeEffectInstance);
		}
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
