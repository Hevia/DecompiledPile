using System;
using UnityEngine;

public class ShrinePlaceTotem : MonoBehaviour
{
	public int totemCount = 5;

	public GameObject totem;

	[Tooltip("Distance from which to form totem ring")]
	public float totemRadius = 2f;

	[Tooltip("Height from which to calculate totem placements.")]
	public float height = 10f;

	[Tooltip("Distance to raycast for totems")]
	public float raycastDistance = 20f;

	[Tooltip("Random bending of totems")]
	public float bendAmount = 15f;

	[Tooltip("Allowed difference from straight up (1) to straight down (-1)")]
	public float dotLimit = 0.8f;

	private void Awake()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		RaycastHit val = default(RaycastHit);
		for (int i = 0; i < totemCount * 2; i++)
		{
			float num2 = ((i >= totemCount) ? ((float)i * 1f * (360f / (float)totemCount)) : (((float)i + 0.5f) * (360f / (float)totemCount)));
			Physics.Raycast(((Component)this).transform.position + new Vector3(0f, height, 0f), new Vector3(Mathf.Cos(num2 * (MathF.PI / 180f)) * totemRadius, 0f - height, Mathf.Sin(num2 * (MathF.PI / 180f)) * totemRadius), ref val, raycastDistance);
			if ((Object)(object)((RaycastHit)(ref val)).collider != (Object)null && Vector3.Dot(((RaycastHit)(ref val)).normal, Vector3.up) > dotLimit)
			{
				GameObject val2 = Object.Instantiate<GameObject>(totem, ((RaycastHit)(ref val)).point, Quaternion.identity);
				val2.transform.parent = ((Component)this).transform;
				val2.transform.rotation = Quaternion.FromToRotation(val2.transform.up, ((RaycastHit)(ref val)).normal);
				Transform transform = val2.transform;
				transform.eulerAngles += new Vector3(Random.Range(0f - bendAmount, bendAmount), Random.Range(0f - bendAmount, bendAmount), Random.Range(0f - bendAmount, bendAmount));
				Transform transform2 = val2.transform;
				transform2.position -= new Vector3(0f, Random.Range(0.1f, 0.2f), 0f);
				num++;
			}
			if (num == totemCount)
			{
				break;
			}
		}
	}

	private void Update()
	{
	}
}
