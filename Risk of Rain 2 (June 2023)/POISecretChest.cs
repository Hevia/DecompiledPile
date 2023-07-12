using UnityEngine;

public class POISecretChest : MonoBehaviour
{
	public float influence = 5f;

	private void OnDrawGizmos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.matrix = ((Component)this).transform.localToWorldMatrix;
		Gizmos.color = new Color(0f, 1f, 1f, 0.03f);
		Gizmos.DrawCube(Vector3.zero, ((Component)this).transform.localScale / 2f);
		Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
		Gizmos.DrawWireCube(Vector3.zero, ((Component)this).transform.localScale / 2f);
	}
}
