using System.Collections;
using System.Collections.Generic;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

public class MaulingRockZoneManager : MonoBehaviour
{
	public List<GameObject> maulingRockProjectilePrefabs = new List<GameObject>();

	public Transform startZonePoint1;

	public Transform startZonePoint2;

	public Transform endZonePoint1;

	public Transform endZonePoint2;

	public static float baseDuration = 60f;

	public float knockbackForce = 10000f;

	private Vector3 vectorBetweenStartPoints = Vector3.zero;

	private Vector3 vectorBetweenEndPoints = Vector3.zero;

	private Vector3 MediumRockBump = Vector3.zero;

	private Vector3 LargeRockBump = Vector3.zero;

	private int salvoMaximumCount = 10;

	private float timeBetweenSalvoShotsLow = 0.1f;

	private float timeBetweenSalvoShotsHigh = 1f;

	private float timeBetweenSalvosLow = 3f;

	private float timeBetweenSalvosHigh = 5f;

	private int salvoRockCount;

	private int currentSalvoCount;

	private void Awake()
	{
		if (!NetworkServer.active)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		vectorBetweenStartPoints = startZonePoint2.position - startZonePoint1.position;
		vectorBetweenEndPoints = endZonePoint2.position - endZonePoint1.position;
		FireSalvo();
	}

	private void FireSalvo()
	{
		salvoRockCount = Random.Range(0, salvoMaximumCount);
		currentSalvoCount = 0;
		FireRock();
	}

	private void FireRock()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = maulingRockProjectilePrefabs[Random.Range(0, maulingRockProjectilePrefabs.Count)];
		Vector3 val2 = startZonePoint1.position + Random.Range(0f, 1f) * vectorBetweenStartPoints;
		Vector3 val3 = endZonePoint1.position + Random.Range(0f, 1f) * vectorBetweenEndPoints;
		MaulingRock component = val.GetComponent<MaulingRock>();
		float num = Random.Range(0f, 4f);
		num += component.verticalOffset;
		((Vector3)(ref val2))._002Ector(val2.x, val2.y + num, val2.z);
		num = Random.Range(0f, 4f);
		num += component.verticalOffset;
		((Vector3)(ref val3))._002Ector(val3.x, val3.y + num, val3.z);
		Ray val4 = default(Ray);
		((Ray)(ref val4))._002Ector(val2, val3 - val2);
		FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
		fireProjectileInfo.projectilePrefab = val;
		fireProjectileInfo.position = ((Ray)(ref val4)).origin;
		fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(((Ray)(ref val4)).direction);
		fireProjectileInfo.owner = null;
		fireProjectileInfo.damage = component.damage * component.damageCoefficient;
		fireProjectileInfo.force = knockbackForce;
		fireProjectileInfo.crit = false;
		fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
		fireProjectileInfo.target = null;
		FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
		ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
		currentSalvoCount++;
		((MonoBehaviour)this).StartCoroutine(WaitToFireAnotherRock());
	}

	public IEnumerator WaitToFireAnotherSalvo()
	{
		float num = Random.Range(timeBetweenSalvosLow, timeBetweenSalvosHigh);
		yield return (object)new WaitForSeconds(num);
		FireSalvo();
	}

	public IEnumerator WaitToFireAnotherRock()
	{
		float num = Random.Range(timeBetweenSalvoShotsLow, timeBetweenSalvoShotsHigh);
		if (currentSalvoCount >= salvoRockCount)
		{
			yield return null;
			((MonoBehaviour)this).StartCoroutine(WaitToFireAnotherSalvo());
		}
		else
		{
			yield return (object)new WaitForSeconds(num);
			FireRock();
		}
	}
}
