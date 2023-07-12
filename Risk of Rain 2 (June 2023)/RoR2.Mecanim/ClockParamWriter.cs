using System;
using UnityEngine;

namespace RoR2.Mecanim;

public class ClockParamWriter : StateMachineBehaviour
{
	public string targetParamName = "time";

	public float cyclesPerDay = 2f;

	private const float secondsPerDay = 86400f;

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		((StateMachineBehaviour)this).OnStateUpdate(animator, stateInfo, layerIndex);
		float num = 0f;
		num = ((!Object.op_Implicit((Object)(object)Run.instance)) ? ((float)(DateTime.Now - DateTime.Today).TotalSeconds) : Run.instance.GetRunStopwatch());
		animator.SetFloat(targetParamName, cyclesPerDay * num / 86400f);
	}
}
