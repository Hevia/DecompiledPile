using RoR2.Audio;
using UnityEngine;

namespace RoR2;

public class LoopSoundPlayer : MonoBehaviour
{
	[SerializeField]
	private LoopSoundDef loopDef;

	private LoopSoundManager.SoundLoopPtr loopPtr;

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)loopDef))
		{
			loopPtr = LoopSoundManager.PlaySoundLoopLocal(((Component)this).gameObject, loopDef);
		}
	}

	private void OnDisable()
	{
		if (loopPtr.isValid)
		{
			LoopSoundManager.StopSoundLoopLocal(loopPtr);
		}
	}
}
