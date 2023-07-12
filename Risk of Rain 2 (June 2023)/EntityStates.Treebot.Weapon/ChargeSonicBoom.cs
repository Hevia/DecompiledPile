using RoR2;
using UnityEngine;

namespace EntityStates.Treebot.Weapon;

public class ChargeSonicBoom : BaseState
{
	[SerializeField]
	public string sound;

	[SerializeField]
	public GameObject chargeEffectPrefab;

	public static string muzzleName;

	public static float baseDuration;

	private float duration;

	private GameObject chargeEffect;

	public override void OnEnter()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Util.PlaySound(sound, base.gameObject);
		base.characterBody.SetAimTimer(3f);
		if (Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			Transform val = FindModelChild(muzzleName);
			if (Object.op_Implicit((Object)(object)val))
			{
				chargeEffect = Object.Instantiate<GameObject>(chargeEffectPrefab, val);
				chargeEffect.transform.localPosition = Vector3.zero;
				chargeEffect.transform.localRotation = Quaternion.identity;
			}
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)chargeEffect))
		{
			EntityState.Destroy((Object)(object)chargeEffect);
			chargeEffect = null;
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(GetNextState());
		}
	}

	protected virtual EntityState GetNextState()
	{
		return new FireSonicBoom();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
