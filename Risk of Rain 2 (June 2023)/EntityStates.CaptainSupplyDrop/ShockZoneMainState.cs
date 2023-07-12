using RoR2;
using UnityEngine;

namespace EntityStates.CaptainSupplyDrop;

public class ShockZoneMainState : BaseMainState
{
	public static GameObject shockEffectPrefab;

	public static float shockRadius;

	public static float shockDamageCoefficient;

	public static float shockFrequency;

	private float shockTimer;

	protected override Interactability GetInteractability(Interactor activator)
	{
		return Interactability.Disabled;
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		shockTimer += Time.fixedDeltaTime;
		if (shockTimer > 1f / shockFrequency)
		{
			shockTimer -= 1f / shockFrequency;
			Shock();
		}
	}

	private void Shock()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		BlastAttack blastAttack = new BlastAttack();
		blastAttack.radius = shockRadius;
		blastAttack.baseDamage = 0f;
		blastAttack.damageType = DamageType.Silent | DamageType.Shock5s;
		blastAttack.falloffModel = BlastAttack.FalloffModel.None;
		blastAttack.attacker = null;
		blastAttack.teamIndex = teamFilter.teamIndex;
		blastAttack.position = base.transform.position;
		blastAttack.Fire();
		if (Object.op_Implicit((Object)(object)shockEffectPrefab))
		{
			EffectManager.SpawnEffect(shockEffectPrefab, new EffectData
			{
				origin = base.transform.position,
				scale = shockRadius
			}, transmit: false);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}
}
