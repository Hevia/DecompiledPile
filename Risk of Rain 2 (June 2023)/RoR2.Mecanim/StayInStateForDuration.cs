using UnityEngine;

namespace RoR2.Mecanim;

public class StayInStateForDuration : StateMachineBehaviour
{
	[Tooltip("The reference float - this is how long we will stay in this state")]
	public string durationFloatParameterName;

	[Tooltip("The counter float - this is exposed incase we want to reset it")]
	public string stopwatchFloatParameterName;

	[Tooltip("The bool that will be set to 'false' once the duration is up, and 'true' when entering this state.")]
	public string deactivationBoolParameterName;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
		animator.SetBool(deactivationBoolParameterName, true);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateUpdate(animator, animatorStateInfo, layerIndex);
		float @float = animator.GetFloat(stopwatchFloatParameterName);
		float float2 = animator.GetFloat(durationFloatParameterName);
		@float += Time.deltaTime;
		if (@float >= float2)
		{
			animator.SetFloat(stopwatchFloatParameterName, 0f);
			animator.SetBool(deactivationBoolParameterName, false);
		}
		else
		{
			animator.SetFloat(stopwatchFloatParameterName, @float);
		}
	}
}
