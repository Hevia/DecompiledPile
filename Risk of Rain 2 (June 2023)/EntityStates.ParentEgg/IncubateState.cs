using RoR2;

namespace EntityStates.ParentEgg;

public class IncubateState : BaseEggState
{
	private float duration;

	public override void OnEnter()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = base.controller.incubationDuration;
		PlayAnimation("Body", "Spawn");
		Util.PlaySound(base.controller.podSpawnSound, base.gameObject);
		EffectManager.SimpleEffect(base.controller.spawnEffect, base.gameObject.transform.position, base.transform.rotation, transmit: true);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextState(new PreHatch());
		}
	}
}
