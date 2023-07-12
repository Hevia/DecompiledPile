using UnityEngine;

namespace RoR2;

internal class LaserPointerController : MonoBehaviour
{
	public InputBankTest source;

	public GameObject dotObject;

	public LineRenderer beam;

	public float minDistanceFromStart = 4f;

	private void LateUpdate()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		bool enabled = false;
		bool active = false;
		if (Object.op_Implicit((Object)(object)source))
		{
			Ray val = default(Ray);
			((Ray)(ref val))._002Ector(source.aimOrigin, source.aimDirection);
			RaycastHit val2 = default(RaycastHit);
			if (Physics.Raycast(val, ref val2, float.PositiveInfinity, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask), (QueryTriggerInteraction)0))
			{
				((Component)this).transform.position = ((RaycastHit)(ref val2)).point;
				((Component)this).transform.forward = -((Ray)(ref val)).direction;
				float num = ((RaycastHit)(ref val2)).distance - minDistanceFromStart;
				if (num >= 0.1f)
				{
					beam.SetPosition(1, new Vector3(0f, 0f, num));
					enabled = true;
				}
				active = true;
			}
		}
		dotObject.SetActive(active);
		((Renderer)beam).enabled = enabled;
	}
}
