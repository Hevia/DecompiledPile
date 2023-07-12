using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileTargetComponent))]
[RequireComponent(typeof(Rigidbody))]
public class MissileController : MonoBehaviour
{
	private Transform transform;

	private Rigidbody rigidbody;

	private TeamFilter teamFilter;

	private ProjectileTargetComponent targetComponent;

	public float maxVelocity;

	public float rollVelocity;

	public float acceleration;

	public float delayTimer;

	public float giveupTimer = 8f;

	public float deathTimer = 10f;

	private float timer;

	private QuaternionPID torquePID;

	public float turbulence;

	public float maxSeekDistance = 40f;

	private BullseyeSearch search = new BullseyeSearch();

	private void Awake()
	{
		if (!NetworkServer.active)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		transform = ((Component)this).transform;
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
		torquePID = ((Component)this).GetComponent<QuaternionPID>();
		teamFilter = ((Component)this).GetComponent<TeamFilter>();
		targetComponent = ((Component)this).GetComponent<ProjectileTargetComponent>();
	}

	private void FixedUpdate()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.fixedDeltaTime;
		if (timer < giveupTimer)
		{
			rigidbody.velocity = transform.forward * maxVelocity;
			if (Object.op_Implicit((Object)(object)targetComponent.target) && timer >= delayTimer)
			{
				rigidbody.velocity = transform.forward * (maxVelocity + timer * acceleration);
				Vector3 val = targetComponent.target.position + Random.insideUnitSphere * turbulence - transform.position;
				if (val != Vector3.zero)
				{
					Quaternion rotation = transform.rotation;
					Quaternion targetQuat = Util.QuaternionSafeLookRotation(val);
					torquePID.inputQuat = rotation;
					torquePID.targetQuat = targetQuat;
					rigidbody.angularVelocity = torquePID.UpdatePID();
				}
			}
		}
		if (!Object.op_Implicit((Object)(object)targetComponent.target))
		{
			targetComponent.target = FindTarget();
		}
		else
		{
			HealthComponent component = ((Component)targetComponent.target).GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component) && !component.alive)
			{
				targetComponent.target = FindTarget();
			}
		}
		if (timer > deathTimer)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private Transform FindTarget()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		search.searchOrigin = transform.position;
		search.searchDirection = transform.forward;
		search.teamMaskFilter.RemoveTeam(teamFilter.teamIndex);
		search.RefreshCandidates();
		HurtBox hurtBox = search.GetResults().FirstOrDefault();
		if (hurtBox == null)
		{
			return null;
		}
		return ((Component)hurtBox).transform;
	}
}
