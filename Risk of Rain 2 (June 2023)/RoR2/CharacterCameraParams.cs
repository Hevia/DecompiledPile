using System;
using HG;
using HG.BlendableTypes;
using UnityEngine;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/CharacterCameraParams")]
public class CharacterCameraParams : ScriptableObject
{
	[HideInInspector]
	[SerializeField]
	private int version;

	public CharacterCameraParamsData data;

	private static readonly float minPitchDefault = -70f;

	private static readonly float maxPitchDefault = 70f;

	private static readonly float wallCushionDefault = 0.1f;

	private static readonly float pivotVerticalOffsetDefault = 1.6f;

	private static readonly Vector3 standardLocalCameraPosDefault = new Vector3(0f, 0f, -5f);

	[Obsolete("Use data.minPitch instead.", false)]
	[ShowFieldObsolete]
	public float minPitch = minPitchDefault;

	[Obsolete("Use data.maxPitch instead.", false)]
	[ShowFieldObsolete]
	public float maxPitch = maxPitchDefault;

	[Obsolete("Use data.wallCushion instead.", false)]
	[ShowFieldObsolete]
	public float wallCushion = wallCushionDefault;

	[ShowFieldObsolete]
	[Obsolete("Use data.pivotVerticalOffset instead.", false)]
	public float pivotVerticalOffset = pivotVerticalOffsetDefault;

	[ShowFieldObsolete]
	[Obsolete("Use data.standardLocalCameraPos instead.", false)]
	public Vector3 standardLocalCameraPos = standardLocalCameraPosDefault;

	private void Awake()
	{
		UpdateVersion();
	}

	private void UpdateVersion()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (version < 1)
		{
			SetBlendableFromValue(out data.minPitch, minPitch, minPitchDefault);
			SetBlendableFromValue(out data.maxPitch, maxPitch, maxPitchDefault);
			SetBlendableFromValue(out data.wallCushion, wallCushion, wallCushionDefault);
			SetBlendableFromValue(out data.pivotVerticalOffset, pivotVerticalOffset, pivotVerticalOffsetDefault);
			SetBlendableFromValue(out data.idealLocalCameraPos, standardLocalCameraPos, standardLocalCameraPosDefault);
			version = 1;
		}
		else
		{
			minPitch = data.minPitch.value;
			maxPitch = data.maxPitch.value;
			wallCushion = data.wallCushion.value;
			pivotVerticalOffset = data.pivotVerticalOffset.value;
			standardLocalCameraPos = data.idealLocalCameraPos.value;
		}
	}

	private static void SetBlendableFromValue(out BlendableFloat dest, float src, float defaultValue)
	{
		dest.value = src;
		dest.alpha = (src.Equals(defaultValue) ? 0f : 1f);
	}

	private static void SetBlendableFromValue(out BlendableVector3 dest, Vector3 src, Vector3 defaultValue)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		dest.value = src;
		dest.alpha = (((Vector3)(ref src)).Equals(defaultValue) ? 0f : 1f);
	}

	private static void SetBlendableFromValue(out BlendableBool dest, bool src, bool defaultValue)
	{
		dest.value = src;
		dest.alpha = (src.Equals(defaultValue) ? 0f : 1f);
	}
}
