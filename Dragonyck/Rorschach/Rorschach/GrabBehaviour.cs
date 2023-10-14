using KinematicCharacterController;
using RoR2;
using UnityEngine;

namespace Rorschach;

internal class GrabBehaviour : MonoBehaviour
{
	private int intVal;

	internal HurtBox hurtBox;

	internal Transform coreTransform;

	private bool stunned;

	private void Update()
	{
		AttachHurtBox(hurtBox);
	}

	internal void DetachHurtBox()
	{
		if (!Object.op_Implicit((Object)(object)hurtBox))
		{
			return;
		}
		HealthComponent healthComponent = hurtBox.healthComponent;
		if (Object.op_Implicit((Object)(object)healthComponent) && Object.op_Implicit((Object)(object)healthComponent.body) && Object.op_Implicit((Object)(object)((Component)hurtBox).transform))
		{
			CharacterMotor characterMotor = healthComponent.body.characterMotor;
			if (Object.op_Implicit((Object)(object)characterMotor))
			{
				characterMotor.disableAirControlUntilCollision = false;
				((Component)healthComponent.body).gameObject.layer = intVal;
				((BaseCharacterController)characterMotor).Motor.RebuildCollidableLayers();
			}
		}
		hurtBox = null;
		stunned = false;
	}

	protected void AttachHurtBox(HurtBox hurtBox)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)hurtBox))
		{
			return;
		}
		HealthComponent healthComponent = hurtBox.healthComponent;
		if (!Object.op_Implicit((Object)(object)healthComponent) || !Object.op_Implicit((Object)(object)healthComponent.body) || !Object.op_Implicit((Object)(object)((Component)hurtBox).transform))
		{
			return;
		}
		if (!stunned)
		{
			stunned = true;
			SetStateOnHurt.SetStunOnObject(((Component)healthComponent).gameObject, 4f);
		}
		CharacterMotor characterMotor = healthComponent.body.characterMotor;
		if (Object.op_Implicit((Object)(object)characterMotor))
		{
			characterMotor.disableAirControlUntilCollision = true;
			characterMotor.velocity = Vector3.zero;
			characterMotor.rootMotion = Vector3.zero;
			((BaseCharacterController)characterMotor).Motor.SetPosition(coreTransform.position, true);
			if (((Component)healthComponent.body).gameObject.layer != LayerIndex.fakeActor.intVal)
			{
				intVal = ((Component)healthComponent.body).gameObject.layer;
				((Component)healthComponent.body).gameObject.layer = LayerIndex.fakeActor.intVal;
				((BaseCharacterController)characterMotor).Motor.RebuildCollidableLayers();
			}
		}
	}
}
