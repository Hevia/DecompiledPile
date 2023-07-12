using UnityEngine;

public class SetRandomScale : MonoBehaviour
{
	public float minimumScale;

	public float maximumScale;

	private void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(minimumScale, maximumScale);
		((Component)this).transform.localScale = Vector3.one * num;
	}
}
