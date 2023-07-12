using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.AffixVoid;

public class SelfDestruct : BaseState
{
	[SerializeField]
	public float duration;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string enterSoundString;

	public override void OnEnter()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayCrossfade(animationLayerName, animationStateName, duration);
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.isSprinting = false;
		}
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.moveVector = base.characterDirection.forward;
		}
		if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
		{
			base.rigidbodyMotor.moveVector = Vector3.zero;
		}
		Util.PlaySound(enterSoundString, base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && base.fixedAge >= duration)
		{
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}
}
