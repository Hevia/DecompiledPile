using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileTargetComponent))]
public class ProjectileSteerTowardTarget : MonoBehaviour
{
	[Tooltip("Constrains rotation to the Y axis only.")]
	public bool yAxisOnly;

	[Tooltip("How fast to rotate in degrees per second. Rotation is linear.")]
	public float rotationSpeed;

	private Transform transform;

	private ProjectileTargetComponent targetComponent;

	private void Start()
	{
		if (!NetworkServer.active)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		transform = ((Component)this).transform;
		targetComponent = ((Component)this).GetComponent<ProjectileTargetComponent>();
	}

	private void FixedUpdate()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)targetComponent.target))
		{
			Vector3 val = ((Component)targetComponent.target).transform.position - transform.position;
			if (yAxisOnly)
			{
				val.y = 0f;
			}
			if (val != Vector3.zero)
			{
				transform.forward = Vector3.RotateTowards(transform.forward, val, rotationSpeed * (MathF.PI / 180f) * Time.fixedDeltaTime, 0f);
			}
		}
	}
}
