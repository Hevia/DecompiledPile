using RoR2;
using UnityEngine;

namespace EntityStates.VagrantNovaItem;

public class BaseVagrantNovaItemState : BaseBodyAttachmentState
{
	protected ParticleSystem chargeSparks;

	protected int GetItemStack()
	{
		if (!Object.op_Implicit((Object)(object)base.attachedBody) || !Object.op_Implicit((Object)(object)base.attachedBody.inventory))
		{
			return 1;
		}
		return base.attachedBody.inventory.GetItemCount(RoR2Content.Items.NovaOnLowHealth);
	}

	public override void OnEnter()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		ChildLocator component = GetComponent<ChildLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		Transform obj = component.FindChild("ChargeSparks");
		chargeSparks = ((obj != null) ? ((Component)obj).GetComponent<ParticleSystem>() : null);
		if (Object.op_Implicit((Object)(object)chargeSparks))
		{
			ShapeModule shape = chargeSparks.shape;
			SkinnedMeshRenderer val = FindAttachedBodyMainRenderer();
			if (Object.op_Implicit((Object)(object)val))
			{
				((ShapeModule)(ref shape)).skinnedMeshRenderer = FindAttachedBodyMainRenderer();
				MainModule main = chargeSparks.main;
				float x = ((Component)val).transform.lossyScale.x;
				((MainModule)(ref main)).startSize = MinMaxCurve.op_Implicit(0.5f / x);
			}
		}
	}

	protected void SetChargeSparkEmissionRateMultiplier(float multiplier)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)chargeSparks))
		{
			EmissionModule emission = chargeSparks.emission;
			((EmissionModule)(ref emission)).rateOverTimeMultiplier = multiplier * 20f;
		}
	}

	private SkinnedMeshRenderer FindAttachedBodyMainRenderer()
	{
		if (!Object.op_Implicit((Object)(object)base.attachedBody))
		{
			return null;
		}
		ModelLocator obj = base.attachedBody.modelLocator;
		CharacterModel.RendererInfo[] array = ((obj == null) ? null : ((Component)obj.modelTransform).GetComponent<CharacterModel>()?.baseRendererInfos);
		if (array == null)
		{
			return null;
		}
		for (int i = 0; i < array.Length; i++)
		{
			Renderer renderer = array[i].renderer;
			SkinnedMeshRenderer result;
			if ((result = (SkinnedMeshRenderer)(object)((renderer is SkinnedMeshRenderer) ? renderer : null)) != null)
			{
				return result;
			}
		}
		return null;
	}
}
