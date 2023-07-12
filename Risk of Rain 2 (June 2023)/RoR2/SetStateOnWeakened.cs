using EntityStates;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class SetStateOnWeakened : NetworkBehaviour, IOnTakeDamageServerReceiver
{
	[Tooltip("The percentage of their max HP they need to take to get weakened. Ranges from 0-1.")]
	public float weakenPercentage = 0.1f;

	[Tooltip("The percentage of their max HP they deal to themselves once weakened. Ranges from 0-1.")]
	public float selfDamagePercentage;

	[Tooltip("The state machine to set the state of when this character is hurt.")]
	public EntityStateMachine targetStateMachine;

	[Tooltip("The state machine to set to idle when this character is hurt.")]
	public EntityStateMachine[] idleStateMachine;

	[Tooltip("The hurtboxes to set to not a weak point once consumed")]
	public HurtBox[] weakHurtBox;

	[Tooltip("The state to enter when this character is hurt.")]
	public SerializableEntityStateType hurtState;

	private float accumulatedDamage;

	private bool consumed;

	private CharacterBody characterBody;

	private void Start()
	{
		characterBody = ((Component)this).GetComponent<CharacterBody>();
	}

	public void OnTakeDamageServer(DamageReport damageReport)
	{
		if (consumed || !Object.op_Implicit((Object)(object)targetStateMachine) || !((NetworkBehaviour)this).isServer || !Object.op_Implicit((Object)(object)characterBody))
		{
			return;
		}
		DamageInfo damageInfo = damageReport.damageInfo;
		float num = (damageInfo.crit ? (damageInfo.damage * 2f) : damageInfo.damage);
		if ((damageInfo.damageType & DamageType.WeakPointHit) != 0)
		{
			accumulatedDamage += num;
			if (accumulatedDamage > characterBody.maxHealth * weakenPercentage)
			{
				consumed = true;
				SetWeak(damageInfo);
			}
		}
	}

	public void SetWeak(DamageInfo damageInfo)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)targetStateMachine))
		{
			targetStateMachine.SetInterruptState(EntityStateCatalog.InstantiateState(hurtState), InterruptPriority.Pain);
		}
		EntityStateMachine[] array = idleStateMachine;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetNextState(new Idle());
		}
		HurtBox[] array2 = weakHurtBox;
		for (int i = 0; i < array2.Length; i++)
		{
			((Component)array2[i]).gameObject.SetActive(false);
		}
		if (selfDamagePercentage > 0f)
		{
			DamageInfo damageInfo2 = new DamageInfo();
			damageInfo2.damage = characterBody.maxHealth * selfDamagePercentage / 3f;
			damageInfo2.attacker = damageInfo.attacker;
			damageInfo2.crit = true;
			damageInfo2.position = damageInfo.position;
			damageInfo2.damageType = DamageType.NonLethal | DamageType.WeakPointHit;
			characterBody.healthComponent.TakeDamage(damageInfo2);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
