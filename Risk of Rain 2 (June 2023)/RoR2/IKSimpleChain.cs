using UnityEngine;

namespace RoR2;

public class IKSimpleChain : MonoBehaviour
{
	public enum InnerAxis
	{
		Left,
		Right,
		Forward,
		Backward
	}

	public float scale = 1f;

	public int maxIterations = 100;

	public float positionAccuracy = 0.001f;

	private float posAccuracy = 0.001f;

	public float bendingLow;

	public float bendingHigh;

	public int chainResolution;

	private int startBone;

	private bool minIsFound;

	private bool bendMore;

	private Vector3 targetPosition;

	public float legLength;

	public float poleAngle;

	public InnerAxis innerAxis = InnerAxis.Right;

	private Transform tmpBone;

	public Transform ikPole;

	public Transform[] boneList;

	private bool firstRun = true;

	private IIKTargetBehavior ikTarget;

	private void Start()
	{
		ikTarget = ((Component)this).GetComponent<IIKTargetBehavior>();
	}

	private void LateUpdate()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (firstRun)
		{
			tmpBone = boneList[startBone];
		}
		if (ikTarget != null)
		{
			ikTarget.UpdateIKTargetPosition();
		}
		targetPosition = ((Component)this).transform.position;
		legLength = CalculateLegLength(boneList);
		Solve(boneList, targetPosition);
		firstRun = false;
	}

	public bool LegTooShort(float legScale = 1f)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		Vector3 val = targetPosition - ((Component)boneList[0]).transform.position;
		if (((Vector3)(ref val)).sqrMagnitude >= legLength * legLength * legScale * legScale)
		{
			result = true;
		}
		return result;
	}

	private float CalculateLegLength(Transform[] bones)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		float[] array = new float[bones.Length - 1];
		float num = 0f;
		for (int i = startBone; i < bones.Length - 1; i++)
		{
			int num2 = i;
			Vector3 val = bones[i + 1].position - bones[i].position;
			array[num2] = ((Vector3)(ref val)).magnitude;
			num += array[i];
		}
		return num;
	}

	public void Solve(Transform[] bones, Vector3 target)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_0499: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04db: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		Transform val = bones[^1];
		Vector3[] array = (Vector3[])(object)new Vector3[bones.Length - 2];
		float[] array2 = new float[bones.Length - 2];
		Quaternion[] array3 = (Quaternion[])(object)new Quaternion[bones.Length - 2];
		for (int i = startBone; i < bones.Length - 2; i++)
		{
			array[i] = Vector3.Cross(bones[i + 1].position - bones[i].position, bones[i + 2].position - bones[i + 1].position);
			array[i] = Quaternion.Inverse(bones[i].rotation) * array[i];
			array[i] = ((Vector3)(ref array[i])).normalized;
			array2[i] = Vector3.Angle(bones[i + 1].position - bones[i].position, bones[i + 1].position - bones[i + 2].position);
			array3[i] = bones[i + 1].localRotation;
		}
		positionAccuracy = legLength * posAccuracy;
		Vector3 val2 = val.position - bones[startBone].position;
		float magnitude = ((Vector3)(ref val2)).magnitude;
		val2 = target - bones[startBone].position;
		float magnitude2 = ((Vector3)(ref val2)).magnitude;
		minIsFound = false;
		bendMore = false;
		if (magnitude2 >= magnitude)
		{
			minIsFound = true;
			bendingHigh = 1f;
			bendingLow = 0f;
		}
		else
		{
			bendMore = true;
			bendingHigh = 1f;
			bendingLow = 0f;
		}
		_ = array3.Length;
		int num = 0;
		while (Mathf.Abs(magnitude - magnitude2) > positionAccuracy && num < maxIterations)
		{
			num++;
			float num2 = (minIsFound ? ((bendingLow + bendingHigh) / 2f) : bendingHigh);
			for (int j = startBone; j < bones.Length - 2; j++)
			{
				float num3 = (bendMore ? (array2[j] * (1f - num2) + (array2[j] - 30f) * num2) : Mathf.Lerp(180f, array2[j], num2));
				Quaternion localRotation = Quaternion.AngleAxis(array2[j] - num3, array[j]) * array3[j];
				bones[j + 1].localRotation = localRotation;
			}
			val2 = val.position - bones[startBone].position;
			magnitude = ((Vector3)(ref val2)).magnitude;
			if (magnitude2 > magnitude)
			{
				minIsFound = true;
			}
			if (minIsFound)
			{
				if (magnitude2 > magnitude)
				{
					bendingHigh = num2;
				}
				else
				{
					bendingLow = num2;
				}
				if (bendingHigh < 0.01f)
				{
					break;
				}
			}
			else
			{
				bendingLow = bendingHigh;
				bendingHigh += 1f;
			}
		}
		if (firstRun)
		{
			tmpBone.rotation = bones[startBone].rotation;
		}
		bones[startBone].rotation = Quaternion.AngleAxis(Vector3.Angle(val.position - bones[startBone].position, target - bones[startBone].position), Vector3.Cross(val.position - bones[startBone].position, target - bones[startBone].position)) * bones[startBone].rotation;
		if (Object.op_Implicit((Object)(object)ikPole))
		{
			Vector3 position = bones[startBone].position;
			Vector3 up = ((Component)bones[startBone]).transform.up;
			Vector3 position2 = val.position;
			Vector3 position3 = ikPole.position;
			Vector3 val3 = Vector3.Cross(position2 - position, position3 - position);
			Vector3 val4 = Vector3.Cross(val3, up);
			Vector3 vecU = Vector3.zero;
			switch (innerAxis)
			{
			case InnerAxis.Right:
				vecU = ((Component)bones[startBone]).transform.right;
				break;
			case InnerAxis.Left:
				vecU = -((Component)bones[startBone]).transform.right;
				break;
			case InnerAxis.Forward:
				vecU = ((Component)bones[startBone]).transform.forward;
				break;
			case InnerAxis.Backward:
				vecU = -((Component)bones[startBone]).transform.forward;
				break;
			}
			float num4 = SignedAngle(vecU, val4, up);
			num4 += poleAngle;
			bones[startBone].rotation = Quaternion.AngleAxis(num4, val.position - bones[startBone].position) * bones[startBone].rotation;
			Debug.DrawLine(val.position, bones[startBone].position, Color.red);
			Debug.DrawRay(bones[startBone].position, val3, Color.blue);
			Debug.DrawRay(bones[startBone].position, val4, Color.yellow);
		}
		tmpBone = bones[startBone];
	}

	private float SignedAngle(Vector3 vecU, Vector3 vecV, Vector3 normal)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Angle(vecU, vecV);
		if (Vector3.Angle(Vector3.Cross(vecU, vecV), normal) < 1f)
		{
			num *= -1f;
		}
		return 0f - num;
	}

	private float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(Vector3.Cross(fwd, targetDir), up);
		if (num > 0f)
		{
			return 1f;
		}
		if (num < 0f)
		{
			return -1f;
		}
		return 0f;
	}
}
