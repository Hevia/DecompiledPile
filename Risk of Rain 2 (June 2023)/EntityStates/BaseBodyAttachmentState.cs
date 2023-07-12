using RoR2;
using UnityEngine;

namespace EntityStates;

public class BaseBodyAttachmentState : EntityState
{
	protected NetworkedBodyAttachment bodyAttachment { get; private set; }

	protected CharacterBody attachedBody { get; private set; }

	public override void OnEnter()
	{
		base.OnEnter();
		bodyAttachment = GetComponent<NetworkedBodyAttachment>();
		attachedBody = (Object.op_Implicit((Object)(object)bodyAttachment) ? bodyAttachment.attachedBody : null);
	}
}
