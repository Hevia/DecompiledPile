using UnityEngine;

namespace RoR2.Projectile;

public class ProjectileGhostController : MonoBehaviour
{
	private Transform transform;

	private float migration;

	[Tooltip("Sets the ghost's scale to match the base projectile.")]
	public bool inheritScaleFromProjectile;

	public Transform authorityTransform { get; set; }

	public Transform predictionTransform { get; set; }

	private void Awake()
	{
		transform = ((Component)this).transform;
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)authorityTransform) ^ Object.op_Implicit((Object)(object)predictionTransform))
		{
			CopyTransform(Object.op_Implicit((Object)(object)authorityTransform) ? authorityTransform : predictionTransform);
		}
		else if (Object.op_Implicit((Object)(object)authorityTransform))
		{
			LerpTransform(predictionTransform, authorityTransform, migration);
			if (migration == 1f)
			{
				predictionTransform = null;
			}
		}
		else
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void LerpTransform(Transform a, Transform b, float t)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		transform.position = Vector3.LerpUnclamped(a.position, b.position, t);
		transform.rotation = Quaternion.SlerpUnclamped(a.rotation, b.rotation, t);
		if (inheritScaleFromProjectile)
		{
			transform.localScale = Vector3.Lerp(a.localScale, b.localScale, t);
		}
	}

	private void CopyTransform(Transform src)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		transform.position = src.position;
		transform.rotation = src.rotation;
		if (inheritScaleFromProjectile)
		{
			transform.localScale = src.localScale;
		}
	}
}
