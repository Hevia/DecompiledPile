using RoR2;
using UnityEngine;

namespace EntityStates.TitanMonster;

public class FireGoldFist : FireFist
{
	public static int fistCount;

	public static float distanceBetweenFists;

	public static float delayBetweenFists;

	protected override void PlacePredictedAttack()
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Vector3 val = predictedTargetPosition;
		Vector3 val2 = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * Vector3.forward;
		RaycastHit val4 = default(RaycastHit);
		for (int i = -(fistCount / 2); i < fistCount / 2; i++)
		{
			Vector3 val3 = val + val2 * distanceBetweenFists * (float)i;
			float num2 = 60f;
			if (Physics.Raycast(new Ray(val3 + Vector3.up * (num2 / 2f), Vector3.down), ref val4, num2, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
			{
				val3 = ((RaycastHit)(ref val4)).point;
			}
			PlaceSingleDelayBlast(val3, delayBetweenFists * (float)num);
			num++;
		}
	}
}
