using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(CharacterBody))]
[RequireComponent(typeof(CharacterMotor))]
public class LoopSoundWhileCharacterMoving : MonoBehaviour
{
	[SerializeField]
	private string startSoundName;

	[SerializeField]
	private string stopSoundName;

	[SerializeField]
	private float minSpeed;

	[SerializeField]
	private bool requireGrounded;

	[SerializeField]
	private bool disableWhileSprinting;

	[SerializeField]
	private bool applyScale;

	private CharacterBody body;

	private CharacterMotor motor;

	private bool isLooping;

	private uint soundId;

	private void Start()
	{
		isLooping = false;
		body = ((Component)this).GetComponent<CharacterBody>();
		motor = ((Component)this).GetComponent<CharacterMotor>();
	}

	private void OnDestroy()
	{
		if (isLooping)
		{
			StopLoop();
		}
	}

	private void FixedUpdate()
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)body) || !Object.op_Implicit((Object)(object)motor))
		{
			return;
		}
		if (isLooping)
		{
			float magnitude = ((Vector3)(ref motor.velocity)).magnitude;
			if (applyScale)
			{
				float num = body.moveSpeed;
				float num2 = num;
				if (!body.isSprinting)
				{
					num *= body.sprintingSpeedMultiplier;
				}
				else
				{
					num2 /= body.sprintingSpeedMultiplier;
				}
				float num3 = Mathf.Lerp(0f, 50f, magnitude / num2) + Mathf.Lerp(0f, 50f, (magnitude - num2) / (num - num2));
				AkSoundEngine.SetRTPCValueByPlayingID("charMultSpeed", num3, soundId);
			}
			if ((body.isSprinting && disableWhileSprinting) || (requireGrounded && !motor.isGrounded) || magnitude < minSpeed)
			{
				StopLoop();
			}
		}
		else if ((!body.isSprinting || !disableWhileSprinting) && (!requireGrounded || motor.isGrounded) && ((Vector3)(ref motor.velocity)).sqrMagnitude >= minSpeed)
		{
			StartLoop();
		}
	}

	private void StartLoop()
	{
		soundId = Util.PlaySound(startSoundName, ((Component)this).gameObject);
		isLooping = true;
	}

	private void StopLoop()
	{
		Util.PlaySound(stopSoundName, ((Component)this).gameObject);
		isLooping = false;
	}
}
