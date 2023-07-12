using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.EngiTurret;

public class DeathState : GenericCharacterDeath
{
	[SerializeField]
	public GameObject initialExplosion;

	[SerializeField]
	public GameObject deathExplosion;

	private float deathDuration;

	protected override bool shouldAutoDestroy => false;

	protected override void PlayDeathAnimation(float crossfadeDuration = 0.1f)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			int layerIndex = modelAnimator.GetLayerIndex("Body");
			modelAnimator.PlayInFixedTime("Death", layerIndex);
			modelAnimator.Update(0f);
			AnimatorStateInfo currentAnimatorStateInfo = modelAnimator.GetCurrentAnimatorStateInfo(layerIndex);
			deathDuration = ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).length;
			if (Object.op_Implicit((Object)(object)initialExplosion))
			{
				Object.Instantiate<GameObject>(initialExplosion, base.transform.position, base.transform.rotation, base.transform);
			}
		}
	}

	public override void FixedUpdate()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.fixedAge > deathDuration && NetworkServer.active && Object.op_Implicit((Object)(object)deathExplosion))
		{
			EffectManager.SpawnEffect(deathExplosion, new EffectData
			{
				origin = base.transform.position,
				scale = 2f
			}, transmit: true);
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}

	public override void OnExit()
	{
		DestroyModel();
		base.OnExit();
	}
}
