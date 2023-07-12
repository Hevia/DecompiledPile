using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileInstantiateDeployable : MonoBehaviour
{
	[Tooltip("The deployable slot to use.")]
	[SerializeField]
	private DeployableSlot deployableSlot;

	[SerializeField]
	[Tooltip("The prefab to instantiate.")]
	private GameObject prefab;

	[Tooltip("The object upon which the prefab will be positioned.")]
	[SerializeField]
	private Transform targetTransform;

	[Tooltip("The transform upon which to instantiate the prefab.")]
	[SerializeField]
	private bool copyTargetRotation;

	[SerializeField]
	[Tooltip("Whether or not to parent the instantiated prefab to the specified transform.")]
	private bool parentToTarget;

	[Tooltip("Whether or not to instantiate this prefab. If so, this will only run on the server, and will be spawned over the network.")]
	[SerializeField]
	private bool instantiateOnStart = true;

	public void Start()
	{
		if (instantiateOnStart)
		{
			InstantiateDeployable();
		}
	}

	public void InstantiateDeployable()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		GameObject owner = ((Component)this).GetComponent<ProjectileController>().owner;
		if (!Object.op_Implicit((Object)(object)owner))
		{
			return;
		}
		CharacterBody component = owner.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			CharacterMaster master = component.master;
			if (Object.op_Implicit((Object)(object)master))
			{
				Vector3 val = (Object.op_Implicit((Object)(object)targetTransform) ? targetTransform.position : Vector3.zero);
				Quaternion val2 = (copyTargetRotation ? targetTransform.rotation : Quaternion.identity);
				Transform val3 = (parentToTarget ? targetTransform : null);
				GameObject val4 = Object.Instantiate<GameObject>(prefab, val, val2, val3);
				NetworkServer.Spawn(val4);
				master.AddDeployable(val4.GetComponent<Deployable>(), deployableSlot);
			}
		}
	}
}
