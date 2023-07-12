using UnityEngine;

namespace RoR2;

public class RagdollController : MonoBehaviour
{
	public Transform[] bones;

	public MonoBehaviour[] componentsToDisableOnRagdoll;

	private Animator animator;

	private void Start()
	{
		animator = ((Component)this).GetComponent<Animator>();
		Transform[] array = bones;
		foreach (Transform val in array)
		{
			Collider component = ((Component)val).GetComponent<Collider>();
			Rigidbody component2 = ((Component)val).GetComponent<Rigidbody>();
			if (!Object.op_Implicit((Object)(object)component))
			{
				Debug.LogWarningFormat("Bone {0} is missing a collider!", new object[1] { val });
			}
			else
			{
				component.enabled = false;
				component2.interpolation = (RigidbodyInterpolation)0;
				component2.isKinematic = true;
			}
		}
	}

	public void BeginRagdoll(Vector3 force)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)animator))
		{
			((Behaviour)animator).enabled = false;
		}
		Transform[] array = bones;
		foreach (Transform val in array)
		{
			if (((Component)val).gameObject.layer == LayerIndex.ragdoll.intVal)
			{
				val.parent = ((Component)this).transform;
				Rigidbody component = ((Component)val).GetComponent<Rigidbody>();
				((Component)val).GetComponent<Collider>().enabled = true;
				component.isKinematic = false;
				component.interpolation = (RigidbodyInterpolation)1;
				component.collisionDetectionMode = (CollisionDetectionMode)1;
				component.AddForce(force * Random.Range(0.9f, 1.2f), (ForceMode)2);
			}
		}
		MonoBehaviour[] array2 = componentsToDisableOnRagdoll;
		for (int i = 0; i < array2.Length; i++)
		{
			((Behaviour)array2[i]).enabled = false;
		}
	}
}
