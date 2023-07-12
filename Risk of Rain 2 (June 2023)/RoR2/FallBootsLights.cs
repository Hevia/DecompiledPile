using EntityStates.Headstompers;
using UnityEngine;

namespace RoR2;

public class FallBootsLights : MonoBehaviour
{
	public GameObject readyEffect;

	public GameObject triggerEffect;

	public GameObject chargingEffect;

	private GameObject readyEffectInstance;

	private GameObject triggerEffectInstance;

	private GameObject chargingEffectInstance;

	private bool isReady;

	private bool isTriggered;

	private bool isCharging;

	private CharacterModel characterModel;

	private EntityStateMachine sourceStateMachine;

	private void Start()
	{
		characterModel = ((Component)this).GetComponentInParent<CharacterModel>();
		FindSourceStateMachine();
	}

	private void FindSourceStateMachine()
	{
		if (Object.op_Implicit((Object)(object)characterModel) && Object.op_Implicit((Object)(object)characterModel.body))
		{
			sourceStateMachine = BaseHeadstompersState.FindForBody(characterModel.body)?.outer;
		}
	}

	private void Update()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)sourceStateMachine))
		{
			FindSourceStateMachine();
		}
		bool flag = Object.op_Implicit((Object)(object)sourceStateMachine) && !(sourceStateMachine.state is HeadstompersCooldown);
		if (flag != isReady)
		{
			if (flag)
			{
				readyEffectInstance = Object.Instantiate<GameObject>(readyEffect, ((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
				Util.PlaySound("Play_item_proc_fallboots_activate", ((Component)this).gameObject);
			}
			else if (Object.op_Implicit((Object)(object)readyEffectInstance))
			{
				Object.Destroy((Object)(object)readyEffectInstance);
			}
			isReady = flag;
		}
		bool flag2 = Object.op_Implicit((Object)(object)sourceStateMachine) && sourceStateMachine.state is HeadstompersFall;
		if (flag2 != isTriggered)
		{
			if (flag2)
			{
				triggerEffectInstance = Object.Instantiate<GameObject>(triggerEffect, ((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
				Util.PlaySound("Play_item_proc_fallboots_activate", ((Component)this).gameObject);
			}
			else if (Object.op_Implicit((Object)(object)triggerEffectInstance))
			{
				Object.Destroy((Object)(object)triggerEffectInstance);
			}
			isTriggered = flag2;
		}
		bool flag3 = Object.op_Implicit((Object)(object)sourceStateMachine) && sourceStateMachine.state is HeadstompersCharge;
		if (flag3 != isCharging)
		{
			if (flag3)
			{
				chargingEffectInstance = Object.Instantiate<GameObject>(chargingEffect, ((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform);
			}
			else if (Object.op_Implicit((Object)(object)chargingEffectInstance))
			{
				Object.Destroy((Object)(object)chargingEffectInstance);
			}
			isCharging = flag3;
		}
	}
}
