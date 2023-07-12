using UnityEngine;

namespace RoR2.Mecanim;

public class PlaySoundOnEnter : StateMachineBehaviour
{
	public string soundString;

	public string stopSoundString;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
		Util.PlaySound(soundString, ((Component)animator).gameObject);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateEnter(animator, stateInfo, layerIndex);
		Util.PlaySound(stopSoundString, ((Component)animator).gameObject);
	}
}
