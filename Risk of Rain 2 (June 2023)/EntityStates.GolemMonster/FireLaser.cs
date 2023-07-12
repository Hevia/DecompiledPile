using RoR2;
using UnityEngine;

namespace EntityStates.GolemMonster;

public class FireLaser : BaseState
{
	public static GameObject effectPrefab;

	public static GameObject hitEffectPrefab;

	public static GameObject tracerEffectPrefab;

	public static float damageCoefficient;

	public static float blastRadius;

	public static float force;

	public static float minSpread;

	public static float maxSpread;

	public static int bulletCount;

	public static float baseDuration = 2f;

	public static string attackSoundString;

	public Vector3 laserDirection;

	private float duration;

	private Ray modifiedAimRay;

	public override void OnEnter()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modifiedAimRay = GetAimRay();
		((Ray)(ref modifiedAimRay)).direction = laserDirection;
		GetModelAnimator();
		Transform modelTransform = GetModelTransform();
		Util.PlaySound(attackSoundString, base.gameObject);
		string text = "MuzzleLaser";
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(2f);
		}
		PlayAnimation("Gesture", "FireLaser", "FireLaser.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, text, transmit: false);
		}
		if (!base.isAuthority)
		{
			return;
		}
		float num = 1000f;
		Vector3 val = ((Ray)(ref modifiedAimRay)).origin + ((Ray)(ref modifiedAimRay)).direction * num;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(modifiedAimRay, ref val2, num, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.defaultLayer.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)))
		{
			val = ((RaycastHit)(ref val2)).point;
		}
		BlastAttack blastAttack = new BlastAttack();
		blastAttack.attacker = base.gameObject;
		blastAttack.inflictor = base.gameObject;
		blastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
		blastAttack.baseDamage = damageStat * damageCoefficient;
		blastAttack.baseForce = force * 0.2f;
		blastAttack.position = val;
		blastAttack.radius = blastRadius;
		blastAttack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
		blastAttack.bonusForce = force * ((Ray)(ref modifiedAimRay)).direction;
		blastAttack.Fire();
		_ = ((Ray)(ref modifiedAimRay)).origin;
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			int childIndex = component.FindChildIndex(text);
			if (Object.op_Implicit((Object)(object)tracerEffectPrefab))
			{
				EffectData effectData = new EffectData
				{
					origin = val,
					start = ((Ray)(ref modifiedAimRay)).origin
				};
				effectData.SetChildLocatorTransformReference(base.gameObject, childIndex);
				EffectManager.SpawnEffect(tracerEffectPrefab, effectData, transmit: true);
				EffectManager.SpawnEffect(hitEffectPrefab, effectData, transmit: true);
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
