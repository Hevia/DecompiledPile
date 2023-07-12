using UnityEngine;

namespace RoR2.Mecanim;

public class SetRandomIntOnEnter : StateMachineBehaviour
{
	public string intParameterName;

	public int rangeMin;

	public int rangeMax;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
		animator.SetInteger(intParameterName, Random.Range(rangeMin, rangeMax + 1));
	}
}
