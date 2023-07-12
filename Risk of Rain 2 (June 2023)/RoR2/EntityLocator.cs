using UnityEngine;

namespace RoR2;

[DisallowMultipleComponent]
public class EntityLocator : MonoBehaviour
{
	[Tooltip("The root gameobject of the entity.")]
	public GameObject entity;

	public static GameObject GetEntity(GameObject gameObject)
	{
		if ((Object)(object)gameObject == (Object)null)
		{
			return null;
		}
		EntityLocator component = gameObject.GetComponent<EntityLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return null;
		}
		return component.entity;
	}
}
