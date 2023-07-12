using UnityEngine;

namespace RoR2;

public class SnailAnimator : MonoBehaviour
{
	public ParticleSystem healEffectSystem;

	private bool lastOutOfDanger;

	private Animator animator;

	private CharacterModel characterModel;

	private void Start()
	{
		animator = ((Component)this).GetComponent<Animator>();
		characterModel = ((Component)this).GetComponentInParent<CharacterModel>();
	}

	private void FixedUpdate()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)characterModel))
		{
			return;
		}
		CharacterBody body = characterModel.body;
		if (Object.op_Implicit((Object)(object)body))
		{
			bool outOfDanger = body.outOfDanger;
			if (outOfDanger && !lastOutOfDanger)
			{
				animator.SetBool("spawn", true);
				animator.SetBool("hide", false);
				Util.PlaySound("Play_item_proc_slug_emerge", ((Component)characterModel).gameObject);
				MainModule main = healEffectSystem.main;
				((MainModule)(ref main)).loop = true;
				healEffectSystem.Play();
			}
			else if (!outOfDanger && lastOutOfDanger)
			{
				animator.SetBool("hide", true);
				animator.SetBool("spawn", false);
				Util.PlaySound("Play_item_proc_slug_hide", ((Component)characterModel).gameObject);
				MainModule main2 = healEffectSystem.main;
				((MainModule)(ref main2)).loop = false;
			}
			lastOutOfDanger = outOfDanger;
		}
	}
}
