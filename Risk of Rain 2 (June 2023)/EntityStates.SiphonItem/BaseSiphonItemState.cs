using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;

namespace EntityStates.SiphonItem;

public class BaseSiphonItemState : BaseBodyAttachmentState
{
	private List<ParticleSystem> FXParticles = new List<ParticleSystem>();

	protected int GetItemStack()
	{
		if (!Object.op_Implicit((Object)(object)base.attachedBody) || !Object.op_Implicit((Object)(object)base.attachedBody.inventory))
		{
			return 1;
		}
		return base.attachedBody.inventory.GetItemCount(RoR2Content.Items.SiphonOnLowHealth);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		FXParticles = base.gameObject.GetComponentsInChildren<ParticleSystem>().ToList();
	}

	public void TurnOffHealingFX()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < FXParticles.Count; i++)
		{
			EmissionModule emission = FXParticles[i].emission;
			((EmissionModule)(ref emission)).enabled = false;
		}
	}

	public void TurnOnHealingFX()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < FXParticles.Count; i++)
		{
			EmissionModule emission = FXParticles[i].emission;
			((EmissionModule)(ref emission)).enabled = true;
		}
	}
}
