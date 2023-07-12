using RoR2;
using UnityEngine;

namespace EntityStates.LaserTurbine;

public class LaserTurbineBaseState : EntityState
{
	private GenericOwnership genericOwnership;

	private SimpleLeash simpleLeash;

	private MemoizedGetComponent<CharacterBody> bodyGetComponent;

	protected LaserTurbineController laserTurbineController { get; private set; }

	protected SimpleRotateToDirection simpleRotateToDirection { get; private set; }

	protected CharacterBody ownerBody => bodyGetComponent.Get(genericOwnership?.ownerObject);

	protected virtual bool shouldFollow => true;

	public override void OnEnter()
	{
		base.OnEnter();
		genericOwnership = GetComponent<GenericOwnership>();
		simpleLeash = GetComponent<SimpleLeash>();
		simpleRotateToDirection = GetComponent<SimpleRotateToDirection>();
		laserTurbineController = GetComponent<LaserTurbineController>();
	}

	protected InputBankTest GetInputBank()
	{
		return ownerBody?.inputBank;
	}

	protected Ray GetAimRay()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return new Ray(base.transform.position, base.transform.forward);
	}

	protected Transform GetMuzzleTransform()
	{
		return base.transform;
	}

	public override void Update()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (Object.op_Implicit((Object)(object)ownerBody) && shouldFollow)
		{
			simpleLeash.leashOrigin = ownerBody.corePosition;
			simpleRotateToDirection.targetRotation = Quaternion.LookRotation(ownerBody.inputBank.aimDirection);
		}
	}

	protected float GetDamage()
	{
		float num = 1f;
		if (Object.op_Implicit((Object)(object)ownerBody))
		{
			num = ownerBody.damage;
			if (Object.op_Implicit((Object)(object)ownerBody.inventory))
			{
				num *= (float)ownerBody.inventory.GetItemCount(RoR2Content.Items.LaserTurbine);
			}
		}
		return num;
	}
}
