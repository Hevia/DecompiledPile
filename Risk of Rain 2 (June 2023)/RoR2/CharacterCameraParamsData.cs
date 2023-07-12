using System;
using HG.BlendableTypes;
using UnityEngine;

namespace RoR2;

[Serializable]
public struct CharacterCameraParamsData
{
	public BlendableFloat minPitch;

	public BlendableFloat maxPitch;

	public BlendableFloat wallCushion;

	public BlendableFloat pivotVerticalOffset;

	public BlendableVector3 idealLocalCameraPos;

	public BlendableFloat fov;

	public BlendableBool isFirstPerson;

	public static readonly CharacterCameraParamsData basic = new CharacterCameraParamsData
	{
		minPitch = BlendableFloat.op_Implicit(-70f),
		maxPitch = BlendableFloat.op_Implicit(70f),
		wallCushion = BlendableFloat.op_Implicit(0.1f),
		pivotVerticalOffset = BlendableFloat.op_Implicit(1.6f),
		idealLocalCameraPos = BlendableVector3.op_Implicit(new Vector3(0f, 0f, -5f)),
		fov = new BlendableFloat
		{
			value = 60f,
			alpha = 0f
		}
	};

	public static void Blend(in CharacterCameraParamsData src, ref CharacterCameraParamsData dest, float alpha)
	{
		BlendableFloat.Blend(ref src.minPitch, ref dest.minPitch, alpha);
		BlendableFloat.Blend(ref src.maxPitch, ref dest.maxPitch, alpha);
		BlendableFloat.Blend(ref src.wallCushion, ref dest.wallCushion, alpha);
		BlendableFloat.Blend(ref src.pivotVerticalOffset, ref dest.pivotVerticalOffset, alpha);
		BlendableVector3.Blend(ref src.idealLocalCameraPos, ref dest.idealLocalCameraPos, alpha);
		BlendableFloat.Blend(ref src.fov, ref dest.fov, alpha);
		BlendableBool.Blend(ref src.isFirstPerson, ref dest.isFirstPerson, alpha);
	}
}
