using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.QuestVolatileBattery;

public class CountDown : QuestVolatileBatteryBaseState
{
	public static float duration;

	public static GameObject vfxPrefab;

	public static float explosionRadius;

	public static GameObject explosionEffectPrefab;

	private GameObject[] vfxInstances = Array.Empty<GameObject>();

	private bool detonated;

	public override void OnEnter()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (!Object.op_Implicit((Object)(object)vfxPrefab) || !Object.op_Implicit((Object)(object)base.attachedCharacterModel))
		{
			return;
		}
		List<GameObject> equipmentDisplayObjects = base.attachedCharacterModel.GetEquipmentDisplayObjects(RoR2Content.Equipment.QuestVolatileBattery.equipmentIndex);
		if (equipmentDisplayObjects.Count > 0)
		{
			vfxInstances = (GameObject[])(object)new GameObject[equipmentDisplayObjects.Count];
			for (int i = 0; i < vfxInstances.Length; i++)
			{
				GameObject val = Object.Instantiate<GameObject>(vfxPrefab, equipmentDisplayObjects[i].transform);
				val.transform.localPosition = Vector3.zero;
				val.transform.localRotation = Quaternion.identity;
				vfxInstances[i] = val;
			}
		}
	}

	public override void OnExit()
	{
		GameObject[] array = vfxInstances;
		for (int i = 0; i < array.Length; i++)
		{
			EntityState.Destroy((Object)(object)array[i]);
		}
		vfxInstances = Array.Empty<GameObject>();
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active)
		{
			FixedUpdateServer();
		}
	}

	private void FixedUpdateServer()
	{
		if (base.fixedAge >= duration && !detonated)
		{
			detonated = true;
			Detonate();
		}
	}

	public void Detonate()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.networkedBodyAttachment.attachedBody))
		{
			Vector3 corePosition = base.networkedBodyAttachment.attachedBody.corePosition;
			float baseDamage = 0f;
			if (Object.op_Implicit((Object)(object)base.attachedHealthComponent))
			{
				baseDamage = base.attachedHealthComponent.fullCombinedHealth * 3f;
			}
			EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
			{
				origin = corePosition,
				scale = explosionRadius
			}, transmit: true);
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.position = corePosition + Random.onUnitSphere;
			blastAttack.radius = explosionRadius;
			blastAttack.falloffModel = BlastAttack.FalloffModel.None;
			blastAttack.attacker = base.networkedBodyAttachment.attachedBodyObject;
			blastAttack.inflictor = ((Component)base.networkedBodyAttachment).gameObject;
			blastAttack.damageColorIndex = DamageColorIndex.Item;
			blastAttack.baseDamage = baseDamage;
			blastAttack.baseForce = 5000f;
			blastAttack.bonusForce = Vector3.zero;
			blastAttack.attackerFiltering = AttackerFiltering.AlwaysHit;
			blastAttack.crit = false;
			blastAttack.procChainMask = default(ProcChainMask);
			blastAttack.procCoefficient = 0f;
			blastAttack.teamIndex = base.networkedBodyAttachment.attachedBody.teamComponent.teamIndex;
			blastAttack.Fire();
			base.networkedBodyAttachment.attachedBody.inventory.SetEquipmentIndex(EquipmentIndex.None);
			outer.SetNextState(new Idle());
		}
	}
}
