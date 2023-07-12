using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileDamage))]
[RequireComponent(typeof(ProjectileController))]
public class ProjectileMageFirewallWalkerController : MonoBehaviour
{
	public float dropInterval = 0.15f;

	public GameObject firePillarPrefab;

	public float pillarAngle = 45f;

	public bool curveToCenter = true;

	private float moveSign;

	private ProjectileController projectileController;

	private ProjectileDamage projectileDamage;

	private Vector3 lastCenterPosition;

	private float timer;

	private Vector3 currentPillarVector = Vector3.up;

	private void Awake()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
		lastCenterPosition = ((Component)this).transform.position;
		timer = dropInterval / 2f;
		moveSign = 1f;
	}

	private void Start()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)projectileController.owner))
		{
			Vector3 position = projectileController.owner.transform.position;
			Vector3 val = ((Component)this).transform.position - position;
			val.y = 0f;
			if (val.x != 0f && val.z != 0f)
			{
				moveSign = Mathf.Sign(Vector3.Dot(((Component)this).transform.right, val));
			}
		}
		UpdateDirections();
	}

	private void UpdateDirections()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (curveToCenter)
		{
			Vector3 val = ((Component)this).transform.position - lastCenterPosition;
			val.y = 0f;
			if (val.x != 0f && val.z != 0f)
			{
				((Vector3)(ref val)).Normalize();
				Vector3 val2 = Vector3.Cross(Vector3.up, val);
				((Component)this).transform.forward = val2 * moveSign;
				currentPillarVector = Quaternion.AngleAxis(pillarAngle, val2) * Vector3.Cross(val, val2);
			}
		}
	}

	private void FixedUpdate()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)projectileController.owner))
		{
			lastCenterPosition = projectileController.owner.transform.position;
		}
		UpdateDirections();
		if (!NetworkServer.active)
		{
			return;
		}
		timer -= Time.fixedDeltaTime;
		if (timer <= 0f)
		{
			timer = dropInterval;
			if (Object.op_Implicit((Object)(object)firePillarPrefab))
			{
				ProjectileManager.instance.FireProjectile(firePillarPrefab, ((Component)this).transform.position, Util.QuaternionSafeLookRotation(currentPillarVector), projectileController.owner, projectileDamage.damage, projectileDamage.force, projectileDamage.crit, projectileDamage.damageColorIndex);
			}
		}
	}
}
