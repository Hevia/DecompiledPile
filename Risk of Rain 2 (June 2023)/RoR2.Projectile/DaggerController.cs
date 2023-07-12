using System.Collections.ObjectModel;
using UnityEngine;

namespace RoR2.Projectile;

[RequireComponent(typeof(Rigidbody))]
public class DaggerController : MonoBehaviour
{
	private Transform transform;

	private Rigidbody rigidbody;

	public Transform target;

	public float acceleration;

	public float delayTimer;

	public float giveupTimer = 8f;

	public float deathTimer = 10f;

	private float timer;

	public float turbulence;

	private bool hasPlayedSound;

	private void Awake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		transform = ((Component)this).transform;
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
		rigidbody.AddRelativeForce(Random.insideUnitSphere * 50f);
	}

	private void FixedUpdate()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.fixedDeltaTime;
		if (timer < giveupTimer)
		{
			if (Object.op_Implicit((Object)(object)target))
			{
				Vector3 val = ((Component)target).transform.position - transform.position;
				if (val != Vector3.zero)
				{
					transform.rotation = Util.QuaternionSafeLookRotation(val);
				}
				if (timer >= delayTimer)
				{
					rigidbody.AddForce(transform.forward * acceleration);
					if (!hasPlayedSound)
					{
						Util.PlaySound("Play_item_proc_dagger_fly", ((Component)this).gameObject);
						hasPlayedSound = true;
					}
				}
			}
		}
		else
		{
			rigidbody.useGravity = true;
		}
		if (!Object.op_Implicit((Object)(object)target))
		{
			target = FindTarget();
		}
		else
		{
			HealthComponent component = ((Component)target).GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component) && !component.alive)
			{
				target = FindTarget();
			}
		}
		if (timer > deathTimer)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private Transform FindTarget()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(TeamIndex.Monster);
		float num = 99999f;
		Transform result = null;
		for (int i = 0; i < teamMembers.Count; i++)
		{
			float num2 = Vector3.SqrMagnitude(((Component)teamMembers[i]).transform.position - transform.position);
			if (num2 < num)
			{
				num = num2;
				result = ((Component)teamMembers[i]).transform;
			}
		}
		return result;
	}
}
