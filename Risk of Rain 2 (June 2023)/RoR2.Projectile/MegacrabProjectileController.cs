using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

public class MegacrabProjectileController : MonoBehaviour
{
	public enum MegacrabProjectileType
	{
		White,
		Black,
		Count
	}

	[Header("Cached Properties")]
	public ProjectileController projectileController;

	public ProjectileDamage projectileDamage;

	public Rigidbody projectileRigidbody;

	public MegacrabProjectileType megacrabProjectileType;

	[Header("Black Properties")]
	public AnimationCurve blackForceFalloffCurve;

	public float whiteToBlackTransformationRadius;

	public GameObject whiteToBlackTransformedProjectile;

	[Header("White Properties")]
	public AnimationCurve whiteForceFalloffCurve;

	public float minimumWhiteForceMagnitude;

	public float maximumWhiteForceMagnitude;

	public float whiteMinimumForceToFullyRotate;

	public float whiteScaleSmoothtimeMin = 0.1f;

	public float whiteScaleSmoothtimeMax = 0.1f;

	[Range(0f, 1f)]
	public float whitePoisson;

	public static List<MegacrabProjectileController> whiteProjectileList = new List<MegacrabProjectileController>();

	public static List<MegacrabProjectileController> blackProjectileList = new List<MegacrabProjectileController>();

	private float whiteScaleSmoothtime;

	private Vector3 whiteScaleVelocity;

	private float whiteRotationVelocity;

	private void OnEnable()
	{
		switch (megacrabProjectileType)
		{
		case MegacrabProjectileType.White:
			whiteProjectileList.Add(this);
			break;
		case MegacrabProjectileType.Black:
			blackProjectileList.Add(this);
			break;
		}
	}

	private void OnDisable()
	{
		switch (megacrabProjectileType)
		{
		case MegacrabProjectileType.White:
			whiteProjectileList.Remove(this);
			break;
		case MegacrabProjectileType.Black:
			blackProjectileList.Remove(this);
			break;
		}
	}

	private void Start()
	{
		whiteScaleSmoothtime = Random.Range(whiteScaleSmoothtimeMin, whiteScaleSmoothtimeMax);
	}

	private void FixedUpdate()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		Vector3 position = ((Component)this).transform.position;
		switch (megacrabProjectileType)
		{
		case MegacrabProjectileType.White:
		{
			for (int j = 0; j < blackProjectileList.Count; j++)
			{
				Vector3 val4 = ((Component)blackProjectileList[j]).transform.position - position;
				float magnitude2 = ((Vector3)(ref val4)).magnitude;
				Vector3 val5 = whiteForceFalloffCurve.Evaluate(magnitude2) * ((Vector3)(ref val4)).normalized;
				val += val5;
			}
			Quaternion rotation = ((Component)this).transform.rotation;
			Quaternion target = ((Component)this).transform.rotation;
			Vector3 localScale = ((Component)this).transform.localScale;
			Vector3 one = Vector3.one;
			if (((Vector3)(ref val)).magnitude > minimumWhiteForceMagnitude)
			{
				Quaternion val6 = Quaternion.LookRotation(val);
				float num = Mathf.Min(maximumWhiteForceMagnitude, ((Vector3)(ref val)).magnitude);
				float num2 = 1f / Mathf.Lerp(num, 1f, whitePoisson);
				((Vector3)(ref one))._002Ector(num2, num2, num);
				target = Quaternion.Slerp(rotation, val6, whiteMinimumForceToFullyRotate);
			}
			((Component)this).transform.localScale = Vector3.SmoothDamp(localScale, one, ref whiteScaleVelocity, whiteScaleSmoothtime);
			((Component)this).transform.rotation = Util.SmoothDampQuaternion(rotation, target, ref whiteRotationVelocity, whiteScaleSmoothtime);
			break;
		}
		case MegacrabProjectileType.Black:
		{
			for (int i = 0; i < whiteProjectileList.Count; i++)
			{
				Vector3 val2 = ((Component)whiteProjectileList[i]).transform.position - position;
				float magnitude = ((Vector3)(ref val2)).magnitude;
				Vector3 val3 = blackForceFalloffCurve.Evaluate(magnitude) * ((Vector3)(ref val2)).normalized;
				val += val3;
			}
			projectileRigidbody.AddForce(val);
			break;
		}
		}
	}

	public void OnDestroy()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active || megacrabProjectileType != MegacrabProjectileType.Black)
		{
			return;
		}
		Vector3 position = ((Component)this).transform.position;
		for (int i = 0; i < whiteProjectileList.Count; i++)
		{
			MegacrabProjectileController megacrabProjectileController = whiteProjectileList[i];
			Vector3 val = ((Component)whiteProjectileList[i]).transform.position - position;
			if (((Vector3)(ref val)).magnitude <= whiteToBlackTransformationRadius)
			{
				ProjectileExplosion component = ((Component)megacrabProjectileController).GetComponent<ProjectileExplosion>();
				if (component.GetAlive())
				{
					ProjectileManager.instance.FireProjectile(whiteToBlackTransformedProjectile, ((Component)megacrabProjectileController).transform.position, Quaternion.identity, projectileController.owner, projectileDamage.damage, 0f, projectileDamage.crit);
					component.SetAlive(newAlive: false);
				}
			}
		}
	}
}
