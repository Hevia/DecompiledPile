using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Events;

namespace RoR2;

public class JumpVolume : MonoBehaviour
{
	public Transform targetElevationTransform;

	public Vector3 jumpVelocity;

	public float time;

	public string jumpSoundString;

	public UnityEvent onJump;

	public void OnTriggerStay(Collider other)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		CharacterMotor component = ((Component)other).GetComponent<CharacterMotor>();
		if (Object.op_Implicit((Object)(object)component) && component.hasEffectiveAuthority)
		{
			onJump.Invoke();
			if (!component.disableAirControlUntilCollision)
			{
				Util.PlaySound(jumpSoundString, ((Component)this).gameObject);
			}
			component.velocity = jumpVelocity;
			component.disableAirControlUntilCollision = true;
			((BaseCharacterController)component).Motor.ForceUnground();
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		int num = 20;
		float num2 = time / (float)num;
		Vector3 val = ((Component)this).transform.position;
		_ = ((Component)this).transform.position;
		Vector3 val2 = jumpVelocity;
		Gizmos.color = Color.yellow;
		for (int i = 0; i <= num; i++)
		{
			Vector3 val3 = val + val2 * num2;
			val2 += Physics.gravity * num2;
			Gizmos.DrawLine(val3, val);
			val = val3;
		}
	}
}
