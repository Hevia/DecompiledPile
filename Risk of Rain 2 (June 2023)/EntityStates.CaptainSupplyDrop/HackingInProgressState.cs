using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.CaptainSupplyDrop;

public class HackingInProgressState : BaseMainState
{
	public static int baseGoldForBaseDuration = 25;

	public static float baseDuration = 15f;

	public static GameObject targetIndicatorVfxPrefab;

	public PurchaseInteraction target;

	private GameObject targetIndicatorVfxInstance;

	private Run.FixedTimeStamp startTime;

	private Run.FixedTimeStamp endTime;

	protected override bool shouldShowEnergy => true;

	public override void OnEnter()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (base.isAuthority)
		{
			int difficultyScaledCost = Run.instance.GetDifficultyScaledCost(baseGoldForBaseDuration, Stage.instance.entryDifficultyCoefficient);
			float num = (float)((double)target.cost / (double)difficultyScaledCost * (double)baseDuration);
			startTime = Run.FixedTimeStamp.now;
			endTime = startTime + num;
		}
		energyComponent.normalizedChargeRate = 1f / (endTime - startTime);
		if (NetworkServer.active)
		{
			energyComponent.energy = 0f;
		}
		if (!Object.op_Implicit((Object)(object)targetIndicatorVfxPrefab) || !Object.op_Implicit((Object)(object)target))
		{
			return;
		}
		targetIndicatorVfxInstance = Object.Instantiate<GameObject>(targetIndicatorVfxPrefab, ((Component)target).transform.position, Quaternion.identity);
		ChildLocator component = targetIndicatorVfxInstance.GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild("LineEnd");
			if (Object.op_Implicit((Object)(object)val))
			{
				val.position = FindModelChild("ShaftTip").position;
			}
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)targetIndicatorVfxInstance))
		{
			EntityState.Destroy((Object)(object)targetIndicatorVfxInstance);
			targetIndicatorVfxInstance = null;
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			if (energyComponent.normalizedEnergy >= 1f)
			{
				outer.SetNextState(new UnlockTargetState
				{
					target = target
				});
			}
			else if (!HackingMainState.PurchaseInteractionIsValidTarget(target))
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write(Object.op_Implicit((Object)(object)target) ? ((Component)target).gameObject : null);
		writer.Write(startTime);
		writer.Write(endTime);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		GameObject val = reader.ReadGameObject();
		target = (Object.op_Implicit((Object)(object)val) ? val.GetComponent<PurchaseInteraction>() : null);
		startTime = reader.ReadFixedTimeStamp();
		endTime = reader.ReadFixedTimeStamp();
	}
}
