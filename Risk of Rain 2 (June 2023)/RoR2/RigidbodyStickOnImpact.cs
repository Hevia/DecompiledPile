using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class RigidbodyStickOnImpact : MonoBehaviour
{
	private Rigidbody rb;

	public string stickSoundString;

	public GameObject stickEffectPrefab;

	public float minimumRelativeVelocityMagnitude;

	public AnimationCurve embedDistanceCurve;

	private bool stuck;

	private float stopwatchSinceStuck;

	private Vector3 contactNormal;

	private Vector3 contactPosition;

	private Vector3 transformPositionWhenContacted;

	private void Awake()
	{
		rb = ((Component)this).GetComponent<Rigidbody>();
	}

	private void Update()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (stuck)
		{
			stopwatchSinceStuck += Time.deltaTime;
			((Component)this).transform.position = transformPositionWhenContacted + embedDistanceCurve.Evaluate(stopwatchSinceStuck) * contactNormal;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		if (!stuck && !rb.isKinematic && ((Component)collision.transform).gameObject.layer == LayerIndex.world.intVal)
		{
			Vector3 relativeVelocity = collision.relativeVelocity;
			if (((Vector3)(ref relativeVelocity)).sqrMagnitude > minimumRelativeVelocityMagnitude * minimumRelativeVelocityMagnitude)
			{
				stuck = true;
				ContactPoint contact = collision.GetContact(0);
				contactNormal = ((ContactPoint)(ref contact)).normal;
				contactPosition = ((ContactPoint)(ref contact)).point;
				transformPositionWhenContacted = ((Component)this).transform.position;
				EffectManager.SpawnEffect(stickEffectPrefab, new EffectData
				{
					origin = contactPosition,
					rotation = Util.QuaternionSafeLookRotation(contactNormal)
				}, transmit: false);
				Util.PlaySound(stickSoundString, ((Component)this).gameObject);
				rb.collisionDetectionMode = (CollisionDetectionMode)3;
				rb.detectCollisions = false;
				rb.isKinematic = true;
				rb.velocity = Vector3.zero;
			}
		}
	}
}
