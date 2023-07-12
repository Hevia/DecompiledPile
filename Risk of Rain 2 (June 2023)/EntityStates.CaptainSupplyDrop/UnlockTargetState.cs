using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.CaptainSupplyDrop;

public class UnlockTargetState : BaseMainState
{
	public static GameObject unlockEffectPrefab;

	public static float baseDuration;

	public static string soundString;

	public PurchaseInteraction target;

	private float duration;

	public override void OnEnter()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)target) && target.available)
		{
			target.Networkcost = 0;
			GameObject ownerObject = GetComponent<GenericOwnership>().ownerObject;
			if (Object.op_Implicit((Object)(object)ownerObject))
			{
				Interactor component = ownerObject.GetComponent<Interactor>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.AttemptInteraction(((Component)target).gameObject);
				}
			}
			EffectManager.SpawnEffect(unlockEffectPrefab, new EffectData
			{
				origin = ((Component)target).transform.position
			}, transmit: true);
		}
		Util.PlaySound(soundString, base.gameObject);
		duration = baseDuration;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write(Object.op_Implicit((Object)(object)target) ? ((Component)target).gameObject : null);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		GameObject val = reader.ReadGameObject();
		target = (Object.op_Implicit((Object)(object)val) ? val.GetComponent<PurchaseInteraction>() : null);
	}
}
