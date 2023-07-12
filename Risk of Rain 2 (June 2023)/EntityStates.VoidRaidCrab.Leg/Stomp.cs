using RoR2;
using UnityEngine;

namespace EntityStates.VoidRaidCrab.Leg;

public class Stomp : BaseStompState
{
	public static float blastDamageCoefficient;

	public static float blastRadius;

	public static float blastForce;

	public static GameObject blastEffectPrefab;

	private Vector3? previousToePosition;

	private bool hasDoneBlast;

	protected override void OnLifetimeExpiredAuthority()
	{
		outer.SetNextState(new PostStompReturnToBase
		{
			target = target
		});
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!hasDoneBlast && Object.op_Implicit((Object)(object)base.legController.mainBody) && base.legController.mainBody.hasEffectiveAuthority)
		{
			TryStompCollisionAuthority();
		}
	}

	private void TryStompCollisionAuthority()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3 currentToePosition = base.legController.toeTipTransform.position;
		TryAttack();
		previousToePosition = currentToePosition;
		void TryAttack()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			if (previousToePosition.HasValue)
			{
				Vector3 value = previousToePosition.Value;
				Vector3 val = currentToePosition - value;
				RaycastHit val2 = default(RaycastHit);
				if (!(((Vector3)(ref val)).sqrMagnitude < 0.0025000002f) && Physics.Linecast(previousToePosition.Value, currentToePosition, ref val2, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
				{
					DoBlastAuthority(((RaycastHit)(ref val2)).point);
					base.legController.DoToeConcussionBlastAuthority(((RaycastHit)(ref val2)).point, useEffect: false);
					hasDoneBlast = true;
				}
			}
		}
	}

	private void DoBlastAuthority(Vector3 blastOrigin)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		CharacterBody mainBody = base.legController.mainBody;
		EffectData effectData = new EffectData();
		effectData.origin = blastOrigin;
		effectData.scale = blastRadius;
		effectData.rotation = Quaternion.LookRotation(Vector3.up);
		EffectManager.SpawnEffect(blastEffectPrefab, effectData, transmit: true);
		BlastAttack obj = new BlastAttack
		{
			attacker = ((Component)mainBody).gameObject
		};
		obj.inflictor = obj.attacker;
		obj.baseDamage = blastDamageCoefficient * mainBody.damage;
		obj.baseForce = blastForce;
		obj.position = blastOrigin;
		obj.radius = blastRadius;
		obj.falloffModel = BlastAttack.FalloffModel.SweetSpot;
		obj.teamIndex = mainBody.teamComponent.teamIndex;
		obj.attackerFiltering = AttackerFiltering.NeverHitSelf;
		obj.damageType = DamageType.Generic;
		obj.crit = mainBody.RollCrit();
		obj.damageColorIndex = DamageColorIndex.Default;
		obj.impactEffect = EffectIndex.Invalid;
		obj.losType = BlastAttack.LoSType.None;
		obj.procChainMask = default(ProcChainMask);
		obj.procCoefficient = 1f;
		obj.Fire();
	}

	private void DoStompAttackAuthority(Vector3 hitPosition)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		CharacterBody mainBody = base.legController.mainBody;
		if (Object.op_Implicit((Object)(object)mainBody))
		{
			Vector3 position = base.legController.footTranform.position;
			Vector3 position2 = base.legController.toeTipTransform.position;
			RaycastHit val = default(RaycastHit);
			Vector3 val2 = (Physics.Linecast(position, position2, ref val, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1) ? ((RaycastHit)(ref val)).point : position2);
			EffectData effectData = new EffectData();
			effectData.origin = val2;
			effectData.scale = blastRadius;
			effectData.rotation = Quaternion.LookRotation(-((RaycastHit)(ref val)).normal);
			EffectManager.SpawnEffect(blastEffectPrefab, effectData, transmit: true);
			BlastAttack obj = new BlastAttack
			{
				attacker = ((Component)mainBody).gameObject
			};
			obj.inflictor = obj.attacker;
			obj.baseDamage = blastDamageCoefficient * mainBody.damage;
			obj.baseForce = blastForce;
			obj.position = val2;
			obj.radius = blastRadius;
			obj.falloffModel = BlastAttack.FalloffModel.Linear;
			obj.teamIndex = mainBody.teamComponent.teamIndex;
			obj.attackerFiltering = AttackerFiltering.NeverHitSelf;
			obj.damageType = DamageType.Generic;
			obj.crit = mainBody.RollCrit();
			obj.damageColorIndex = DamageColorIndex.Default;
			obj.impactEffect = EffectIndex.Invalid;
			obj.losType = BlastAttack.LoSType.None;
			obj.procChainMask = default(ProcChainMask);
			obj.procCoefficient = 1f;
			obj.Fire();
		}
	}
}
