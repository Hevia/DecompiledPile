using UnityEngine;

public class RJShroomBounce : MonoBehaviour
{
	private Animator shroomAnimator;

	public void Start()
	{
		shroomAnimator = ((Component)this).GetComponent<Animator>();
	}

	public void Bounce()
	{
		shroomAnimator.Play("Bounce", 0);
	}
}
