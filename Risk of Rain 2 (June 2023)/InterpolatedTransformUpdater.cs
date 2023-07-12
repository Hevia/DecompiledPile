using UnityEngine;

public class InterpolatedTransformUpdater : MonoBehaviour
{
	private InterpolatedTransform m_interpolatedTransform;

	private void Awake()
	{
		m_interpolatedTransform = ((Component)this).GetComponent<InterpolatedTransform>();
	}

	private void FixedUpdate()
	{
		m_interpolatedTransform.LateFixedUpdate();
	}
}
