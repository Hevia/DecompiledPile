using UnityEngine;

namespace RoR2.Mecanim;

public class SetCorrectiveLayer : StateMachineBehaviour
{
	public string referenceOverrideLayerName;

	public float maxWeight = 1f;

	private float smoothVelocity;

	public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
	{
		((StateMachineBehaviour)this).OnStateMachineEnter(animator, stateMachinePathHash);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		int layerIndex2 = animator.GetLayerIndex(referenceOverrideLayerName);
		float num = Mathf.Min(animator.GetLayerWeight(layerIndex2), maxWeight);
		float num2 = Mathf.SmoothDamp(animator.GetLayerWeight(layerIndex), num, ref smoothVelocity, 0.2f);
		animator.SetLayerWeight(layerIndex, num2);
		((StateMachineBehaviour)this).OnStateUpdate(animator, stateInfo, layerIndex);
	}
}
