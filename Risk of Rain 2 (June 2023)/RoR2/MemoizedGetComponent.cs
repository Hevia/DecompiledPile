using UnityEngine;

namespace RoR2;

public struct MemoizedGetComponent<TComponent> where TComponent : Component
{
	private GameObject cachedGameObject;

	private TComponent cachedValue;

	public TComponent Get(GameObject gameObject)
	{
		if ((Object)(object)cachedGameObject != (Object)(object)gameObject)
		{
			cachedGameObject = gameObject;
			cachedValue = (Object.op_Implicit((Object)(object)gameObject) ? gameObject.GetComponent<TComponent>() : default(TComponent));
		}
		return cachedValue;
	}
}
