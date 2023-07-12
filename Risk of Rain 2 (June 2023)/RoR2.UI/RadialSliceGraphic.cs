using System;
using HG;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class RadialSliceGraphic : MaskableGraphic
{
	public struct DisplayData : IEquatable<DisplayData>
	{
		public Radians start;

		public Radians end;

		public float startU;

		public float endU;

		public Material material;

		public Texture texture;

		public Color color;

		public float normalizedInnerRadius;

		public Radians maxQuadWidth;

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is DisplayData other)
			{
				return Equals(other);
			}
			return false;
		}

		public static bool operator ==(DisplayData left, DisplayData right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DisplayData left, DisplayData right)
		{
			return !left.Equals(right);
		}

		public bool Equals(DisplayData other)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			if (((Radians)(ref start)).Equals(other.start) && ((Radians)(ref end)).Equals(other.end) && startU.Equals(other.startU) && endU.Equals(other.endU) && object.Equals(material, other.material) && object.Equals(texture, other.texture) && ((Color)(ref color)).Equals(other.color) && normalizedInnerRadius.Equals(other.normalizedInnerRadius))
			{
				return ((Radians)(ref maxQuadWidth)).Equals(other.maxQuadWidth);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((((((((((((((((object)(Radians)(ref start)).GetHashCode() * 397) ^ ((object)(Radians)(ref end)).GetHashCode()) * 397) ^ startU.GetHashCode()) * 397) ^ endU.GetHashCode()) * 397) ^ (((Object)(object)material != (Object)null) ? ((object)material).GetHashCode() : 0)) * 397) ^ (((Object)(object)texture != (Object)null) ? ((object)texture).GetHashCode() : 0)) * 397) ^ ((object)(Color)(ref color)).GetHashCode()) * 397) ^ normalizedInnerRadius.GetHashCode()) * 397) ^ ((object)(Radians)(ref maxQuadWidth)).GetHashCode();
		}
	}

	public RectTransform sliceCenterSticker;

	private static readonly DisplayData defaultDisplayData;

	private DisplayData displayData = defaultDisplayData;

	private float currentRadius;

	public override Texture mainTexture
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)displayData.texture))
			{
				if (!Object.op_Implicit((Object)(object)displayData.material))
				{
					return null;
				}
				return displayData.material.mainTexture;
			}
			return displayData.texture;
		}
	}

	protected RadialSliceGraphic()
	{
		((Graphic)this).useLegacyMeshGeneration = false;
	}

	public void SetDisplayData(in DisplayData newDisplayData)
	{
		if (!(newDisplayData == displayData))
		{
			displayData = newDisplayData;
			UpdateDisplayData();
		}
	}

	private void UpdateDisplayData()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)((Graphic)this).material != (Object)(object)displayData.material)
		{
			((Graphic)this).material = displayData.material;
		}
		if (((Graphic)this).color != displayData.color)
		{
			((Graphic)this).color = displayData.color;
		}
		((Graphic)this).SetVerticesDirty();
	}

	private static Vector2 GetDirection(Radians radians)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(Mathf.Cos((float)(ref radians)), Mathf.Sin((float)(ref radians)));
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		vh.Clear();
		Rect rect = ((Graphic)this).rectTransform.rect;
		currentRadius = Mathf.Min(((Rect)(ref rect)).width, ((Rect)(ref rect)).height) * 0.5f;
		float outerRadius = currentRadius;
		float innerRadius = outerRadius * displayData.normalizedInnerRadius;
		Radians a = displayData.start;
		Radians b = displayData.end;
		float a2 = displayData.startU;
		float b2 = displayData.endU;
		if ((ref b) < (ref a))
		{
			Util.Swap(ref a, ref b);
			Util.Swap(ref a2, ref b2);
		}
		Radians val = (ref b) - (ref a);
		if (!((float)(ref val) <= 0f) && !((float)(ref displayData.maxQuadWidth) <= 0f))
		{
			int num = Mathf.CeilToInt((float)(ref val) / (float)(ref displayData.maxQuadWidth));
			Radians val2 = (ref val) / (ref num);
			float num2 = 1f / (float)num;
			float num3 = b2 - a2;
			float quadStartU2 = a2;
			Vector2 startDirection2 = GetDirection(a);
			Color color2 = ((Graphic)this).color * displayData.color;
			for (int i = 0; i < num; i++)
			{
				int num4 = i + 1;
				Radians val3 = (ref num4) * (ref val2);
				Vector2 endDirection2 = GetDirection((ref a) + (ref val3));
				float num5 = a2 + num3 * ((float)(i + 1) * num2);
				AddQuad(in startDirection2, in endDirection2, in color2, quadStartU2, num5);
				startDirection2 = endDirection2;
				quadStartU2 = num5;
			}
		}
		void AddQuad(in Vector2 startDirection, in Vector2 endDirection, in Color color, float quadStartU, float quadEndU)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			int currentVertCount = vh.currentVertCount;
			vh.AddVert(Vector2.op_Implicit(startDirection * innerRadius), Color32.op_Implicit(color), new Vector2(quadStartU, 0f));
			vh.AddVert(Vector2.op_Implicit(startDirection * outerRadius), Color32.op_Implicit(color), new Vector2(quadStartU, 1f));
			vh.AddVert(Vector2.op_Implicit(endDirection * outerRadius), Color32.op_Implicit(color), new Vector2(quadEndU, 1f));
			vh.AddVert(Vector2.op_Implicit(endDirection * innerRadius), Color32.op_Implicit(color), new Vector2(quadEndU, 0f));
			vh.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
			vh.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
		}
	}

	public override bool Raycast(Vector2 sp, Camera eventCamera)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Graphic)this).Raycast(sp, eventCamera))
		{
			return false;
		}
		Vector2 point = default(Vector2);
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(((Graphic)this).rectTransform, sp, eventCamera, ref point))
		{
			return false;
		}
		Color white = Color.white;
		bool result = PointInArc(point, displayData.start, displayData.end, displayData.normalizedInnerRadius * currentRadius, currentRadius);
		((Graphic)this).color = white;
		return result;
	}

	private static bool PointInArc(Vector2 point, Radians arcStart, Radians arcEnd, float arcInnerRadius, float arcOuterRadius)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Radians val = (ref arcStart) + (ref arcEnd);
		float num = 0.5f;
		Vector2 direction = GetDirection((ref val) * (ref num));
		val = (ref arcEnd) - (ref arcStart);
		Radians absolute = ((Radians)(ref val)).absolute;
		Vector2 normalized = ((Vector2)(ref point)).normalized;
		float num2 = Vector2.Dot(direction, normalized);
		float num3 = Mathf.Cos((float)(ref absolute) * 0.5f);
		if (num2 < num3)
		{
			return false;
		}
		float sqrMagnitude = ((Vector2)(ref point)).sqrMagnitude;
		if (sqrMagnitude < arcInnerRadius * arcInnerRadius)
		{
			return false;
		}
		if (sqrMagnitude > arcOuterRadius * arcOuterRadius)
		{
			return false;
		}
		return true;
	}

	public override void GraphicUpdateComplete()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)this).GraphicUpdateComplete();
		if (Object.op_Implicit((Object)(object)sliceCenterSticker))
		{
			float num = Mathf.LerpAngle((float)(ref displayData.start), (float)(ref displayData.end), 0.5f);
			Radians val = (Radians)(ref num);
			float num2 = Mathf.Lerp(displayData.normalizedInnerRadius, 1f, 0.5f) * currentRadius;
			Vector3 localPosition = default(Vector3);
			((Vector3)(ref localPosition))._002Ector(((Radians)(ref val)).cos * num2, ((Radians)(ref val)).sin * num2);
			((Transform)sliceCenterSticker).localPosition = localPosition;
		}
	}

	static RadialSliceGraphic()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		DisplayData displayData = new DisplayData
		{
			color = Color.white,
			start = Radians.FromRevolutions(0f),
			end = Radians.FromRevolutions(-0.4f),
			startU = 0f,
			endU = 1f
		};
		float num = MathF.PI / 10f;
		displayData.maxQuadWidth = (Radians)(ref num);
		displayData.normalizedInnerRadius = 0.2f;
		defaultDisplayData = displayData;
	}
}
