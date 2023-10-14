using RoR2;
using UnityEngine;

namespace Rorschach;

internal class CollisionCaller : MonoBehaviour
{
	public bool terrainCollision = false;

	public bool bodyCollision = false;

	private void OnCollisionEnter(Collision collision)
	{
		if (Object.op_Implicit((Object)(object)collision.collider) && Object.op_Implicit((Object)(object)((Component)collision.collider).gameObject))
		{
			if (!Object.op_Implicit((Object)(object)((Component)collision.collider).gameObject.GetComponent<HurtBox>()))
			{
				terrainCollision = true;
			}
			else
			{
				bodyCollision = true;
			}
		}
	}
}
