using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(ItemFollower))]
public class GravCubeController : MonoBehaviour
{
	private ItemFollower itemFollower;

	private float activeTimer;

	private Animator itemFollowerAnimator;

	private void Start()
	{
		itemFollower = ((Component)this).GetComponent<ItemFollower>();
	}

	public void ActivateCube(float duration)
	{
		activeTimer = duration;
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)itemFollower) && Object.op_Implicit((Object)(object)itemFollower.followerInstance))
		{
			if (!Object.op_Implicit((Object)(object)itemFollowerAnimator))
			{
				itemFollowerAnimator = itemFollower.followerInstance.GetComponentInChildren<Animator>();
			}
			activeTimer -= Time.deltaTime;
			itemFollowerAnimator.SetBool("active", activeTimer > 0f);
		}
	}
}
