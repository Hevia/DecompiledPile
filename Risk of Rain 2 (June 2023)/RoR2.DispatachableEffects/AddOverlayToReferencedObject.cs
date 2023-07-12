using UnityEngine;

namespace RoR2.DispatachableEffects;

[RequireComponent(typeof(TemporaryOverlay))]
[RequireComponent(typeof(EffectComponent))]
public class AddOverlayToReferencedObject : MonoBehaviour
{
	public bool effectDataGenericFloatOverridesDuration = true;

	protected void Start()
	{
		EffectComponent component = ((Component)this).GetComponent<EffectComponent>();
		ApplyOverlay(component.GetReferencedObject(), component.effectData.genericFloat);
	}

	protected void ApplyOverlay(GameObject targetObject, float duration)
	{
		if (!Object.op_Implicit((Object)(object)targetObject))
		{
			return;
		}
		ModelLocator component = targetObject.GetComponent<ModelLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		Transform modelTransform = component.modelTransform;
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		CharacterModel component2 = ((Component)modelTransform).GetComponent<CharacterModel>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			TemporaryOverlay component3 = ((Component)this).GetComponent<TemporaryOverlay>();
			component3.AddToCharacerModel(component2);
			if (effectDataGenericFloatOverridesDuration)
			{
				component3.duration = duration;
			}
		}
	}
}
