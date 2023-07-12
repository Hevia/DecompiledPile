using UnityEngine;

public class ScaleProjector : MonoBehaviour
{
	private Projector projector;

	private void Start()
	{
		projector = ((Component)this).GetComponent<Projector>();
	}

	private void Update()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)projector))
		{
			projector.orthographicSize = ((Component)this).transform.lossyScale.x;
		}
	}
}
