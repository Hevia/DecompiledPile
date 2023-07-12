using UnityEngine;

namespace RoR2;

public class PaintTerrain : MonoBehaviour
{
	public float splatHeightReference = 60f;

	public float splatRaycastLength = 200f;

	public float splatSlopePower = 1f;

	public float heightPower = 1f;

	public Vector3 snowfallDirection = Vector3.up;

	public Texture2D grassNoiseMap;

	private Terrain terrain;

	private TerrainData data;

	private float[,,] alphamaps;

	private int[,] detailmapGrass;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		snowfallDirection = ((Vector3)(ref snowfallDirection)).normalized;
		terrain = ((Component)this).GetComponent<Terrain>();
		data = terrain.terrainData;
		alphamaps = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
		detailmapGrass = data.GetDetailLayer(0, 0, data.detailWidth, data.detailHeight, 0);
		UpdateAlphaMaps();
		UpdateDetailMaps();
	}

	private void UpdateAlphaMaps()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		for (int i = 0; i < data.alphamapHeight; i++)
		{
			for (int j = 0; j < data.alphamapWidth; j++)
			{
				float num = (float)j / (float)data.alphamapWidth * data.size.x;
				float num2 = (float)i / (float)data.alphamapHeight * data.size.z;
				float num3 = 0f;
				float num4 = 0f;
				float num5 = 0f;
				float num6 = 0f;
				float num7 = Mathf.Pow(Vector3.Dot(Vector3.up, data.GetInterpolatedNormal((float)i / (float)data.alphamapHeight, (float)j / (float)data.alphamapWidth)), splatSlopePower);
				num3 = num7;
				num5 = 1f - num7;
				float interpolatedHeight = data.GetInterpolatedHeight((float)i / (float)data.alphamapHeight, (float)j / (float)data.alphamapWidth);
				if (Physics.Raycast(new Vector3(num2, interpolatedHeight, num), snowfallDirection, ref val, splatRaycastLength, LayerMask.op_Implicit(LayerIndex.world.mask)))
				{
					float num8 = num3;
					float num9 = Mathf.Clamp01(Mathf.Pow(((RaycastHit)(ref val)).distance / splatHeightReference, heightPower));
					num3 = num9 * num8;
					num4 = (1f - num9) * num8;
				}
				alphamaps[j, i, 0] = num3;
				alphamaps[j, i, 1] = num4;
				alphamaps[j, i, 2] = num5;
				alphamaps[j, i, 3] = num6;
			}
		}
		data.SetAlphamaps(0, 0, alphamaps);
	}

	private void UpdateDetailMaps()
	{
		for (int i = 0; i < data.detailHeight; i++)
		{
			for (int j = 0; j < data.detailWidth; j++)
			{
				int num = 0;
				detailmapGrass[j, i] = num;
			}
		}
		data.SetDetailLayer(0, 0, 0, detailmapGrass);
	}
}
