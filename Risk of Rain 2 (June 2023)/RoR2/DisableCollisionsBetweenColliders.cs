using UnityEngine;

namespace RoR2;

public class DisableCollisionsBetweenColliders : MonoBehaviour
{
	public Collider[] collidersA;

	public Collider[] collidersB;

	public void Awake()
	{
		Collider[] array = collidersA;
		foreach (Collider val in array)
		{
			Collider[] array2 = collidersB;
			foreach (Collider val2 in array2)
			{
				Physics.IgnoreCollision(val, val2);
			}
		}
	}
}
