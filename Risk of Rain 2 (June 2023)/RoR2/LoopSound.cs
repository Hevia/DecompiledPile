using UnityEngine;

namespace RoR2;

public class LoopSound : MonoBehaviour
{
	public string akSoundString;

	public float repeatInterval;

	public Transform soundOwner;

	private float stopwatch;

	private void Update()
	{
		stopwatch += Time.deltaTime;
		if (stopwatch > repeatInterval)
		{
			stopwatch -= repeatInterval;
			if (Object.op_Implicit((Object)(object)soundOwner))
			{
				Util.PlaySound(akSoundString, ((Component)soundOwner).gameObject);
			}
		}
	}
}
