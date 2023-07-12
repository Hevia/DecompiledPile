using EntityStates;
using KinematicCharacterController;
using UnityEngine;

namespace RoR2;

public class CharacterDeathBehavior : MonoBehaviour
{
	[Tooltip("The state machine to set the state of when this character is killed.")]
	public EntityStateMachine deathStateMachine;

	[Tooltip("The state to enter when this character is killed.")]
	public SerializableEntityStateType deathState;

	[Tooltip("The state machine(s) to set to idle when this character is killed.")]
	public EntityStateMachine[] idleStateMachine;

	public void OnDeath()
	{
		if (Util.HasEffectiveAuthority(((Component)this).gameObject))
		{
			if (Object.op_Implicit((Object)(object)deathStateMachine))
			{
				deathStateMachine.SetNextState(EntityStateCatalog.InstantiateState(deathState));
			}
			EntityStateMachine[] array = idleStateMachine;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetNextState(new Idle());
			}
		}
		((Component)this).gameObject.layer = LayerIndex.debris.intVal;
		CharacterMotor component = ((Component)this).GetComponent<CharacterMotor>();
		if (Object.op_Implicit((Object)(object)component))
		{
			((BaseCharacterController)component).Motor.RebuildCollidableLayers();
		}
		ILifeBehavior[] components = ((Component)this).GetComponents<ILifeBehavior>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].OnDeathStart();
		}
		ModelLocator component2 = ((Component)this).GetComponent<ModelLocator>();
		if (!Object.op_Implicit((Object)(object)component2))
		{
			return;
		}
		Transform modelTransform = component2.modelTransform;
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			components = ((Component)modelTransform).GetComponents<ILifeBehavior>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnDeathStart();
			}
		}
	}
}
