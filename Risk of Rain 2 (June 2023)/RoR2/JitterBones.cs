using System;
using UnityEngine;

namespace RoR2;

public class JitterBones : MonoBehaviour
{
	private struct BoneInfo
	{
		public Transform transform;

		public bool isHead;

		public bool isRoot;
	}

	[SerializeField]
	private SkinnedMeshRenderer _skinnedMeshRenderer;

	private BoneInfo[] bones = Array.Empty<BoneInfo>();

	public float perlinNoiseFrequency;

	public float perlinNoiseStrength;

	public float perlinNoiseMinimumCutoff;

	public float perlinNoiseMaximumCutoff = 1f;

	public float headBonusStrength;

	private float age;

	public SkinnedMeshRenderer skinnedMeshRenderer
	{
		get
		{
			return _skinnedMeshRenderer;
		}
		set
		{
			if (_skinnedMeshRenderer != value)
			{
				_skinnedMeshRenderer = value;
				RebuildBones();
			}
		}
	}

	private void RebuildBones()
	{
		if (!Object.op_Implicit((Object)(object)_skinnedMeshRenderer))
		{
			bones = Array.Empty<BoneInfo>();
			return;
		}
		Transform[] array = _skinnedMeshRenderer.bones;
		Array.Resize(ref bones, array.Length);
		for (int i = 0; i < bones.Length; i++)
		{
			Transform val = array[i];
			string text = ((Object)val).name.ToLower();
			bones[i] = new BoneInfo
			{
				transform = val,
				isHead = text.Contains("head"),
				isRoot = text.Contains("root")
			};
		}
	}

	private void Start()
	{
		RebuildBones();
	}

	private void LateUpdate()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)skinnedMeshRenderer))
		{
			return;
		}
		age += Time.deltaTime;
		Vector3 val = default(Vector3);
		for (int i = 0; i < bones.Length; i++)
		{
			BoneInfo boneInfo = bones[i];
			if (!boneInfo.isRoot)
			{
				float num = age * perlinNoiseFrequency;
				float num2 = i;
				((Vector3)(ref val))._002Ector(Mathf.PerlinNoise(num, num2), Mathf.PerlinNoise(num + 4f, num2 + 3f), Mathf.PerlinNoise(num + 6f, num2 - 7f));
				val = HGMath.Remap(val, perlinNoiseMinimumCutoff, perlinNoiseMaximumCutoff, -1f, 1f);
				val = HGMath.Clamp(val, 0f, 1f);
				val *= perlinNoiseStrength;
				if (headBonusStrength >= 0f && boneInfo.isHead)
				{
					val *= headBonusStrength;
				}
				Transform transform = boneInfo.transform;
				transform.rotation *= Quaternion.Euler(val);
			}
		}
	}
}
