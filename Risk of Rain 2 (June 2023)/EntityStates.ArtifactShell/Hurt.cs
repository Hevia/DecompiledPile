using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ArtifactShell;

public class Hurt : ArtifactShellBaseState
{
	public static float baseDuration = 2f;

	public static float blastRadius = 50f;

	public static float knockbackForce = 5000f;

	public static float knockbackLiftForce = 2000f;

	public static float blastOriginOffset = -10f;

	public static GameObject novaEffectPrefab;

	private float duration;

	public override void OnEnter()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration;
		Vector3 position = base.transform.position;
		if (NetworkServer.active)
		{
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.position = position;
			damageInfo.attacker = null;
			damageInfo.inflictor = null;
			damageInfo.damage = Mathf.Ceil(base.healthComponent.fullCombinedHealth * 0.25f);
			damageInfo.damageType = DamageType.BypassArmor | DamageType.Silent;
			damageInfo.procCoefficient = 0f;
			base.healthComponent.TakeDamage(damageInfo);
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.position = position + blastOriginOffset * Vector3.up;
			blastAttack.attacker = base.gameObject;
			blastAttack.inflictor = base.gameObject;
			blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
			blastAttack.baseDamage = 0f;
			blastAttack.baseForce = knockbackForce;
			blastAttack.bonusForce = Vector3.up * knockbackLiftForce;
			blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
			blastAttack.radius = blastRadius;
			blastAttack.procCoefficient = 0f;
			blastAttack.teamIndex = TeamIndex.None;
			blastAttack.Fire();
			ArtifactTrialMissionController.RemoveAllMissionKeys();
		}
		EffectData effectData = new EffectData
		{
			origin = position,
			scale = blastRadius
		};
		EffectManager.SpawnEffect(novaEffectPrefab, effectData, transmit: false);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && base.fixedAge >= duration)
		{
			outer.SetNextState(new WaitForKey());
		}
	}
}
