using RoR2.Audio;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class RigidbodySoundOnImpact : MonoBehaviour
{
	private Rigidbody rb;

	public string impactSoundString;

	public NetworkSoundEventDef networkedSoundEvent;

	public float minimumRelativeVelocityMagnitude;

	private float ditherTimer;

	private void Start()
	{
		rb = ((Component)this).GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		ditherTimer -= Time.fixedDeltaTime;
	}

	private void OnCollisionEnter(Collision collision)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (ditherTimer > 0f || rb.isKinematic || ((Component)collision.transform).gameObject.layer != LayerIndex.world.intVal)
		{
			return;
		}
		Vector3 relativeVelocity = collision.relativeVelocity;
		if (((Vector3)(ref relativeVelocity)).sqrMagnitude > minimumRelativeVelocityMagnitude * minimumRelativeVelocityMagnitude)
		{
			if (impactSoundString != null)
			{
				Util.PlaySound(impactSoundString, ((Component)this).gameObject);
			}
			if ((Object)(object)networkedSoundEvent != (Object)null)
			{
				PointSoundManager.EmitSoundServer(networkedSoundEvent.index, ((Component)this).transform.position);
			}
			ditherTimer = 0.5f;
		}
	}
}
