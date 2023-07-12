using UnityEngine;

namespace RoR2.Mecanim;

public class ClearLayerWeight : StateMachineBehaviour
{
	public bool resetOnExit = true;

	public string layerName;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
		int num = layerIndex;
		if (layerName.Length > 0)
		{
			num = animator.GetLayerIndex(layerName);
		}
		animator.SetLayerWeight(num, 0f);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateExit(animator, stateInfo, layerIndex);
		if (resetOnExit)
		{
			int num = layerIndex;
			if (layerName.Length > 0)
			{
				num = animator.GetLayerIndex(layerName);
			}
			animator.SetLayerWeight(num, 1f);
		}
	}
}
