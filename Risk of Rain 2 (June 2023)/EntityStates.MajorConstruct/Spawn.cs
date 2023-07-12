using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.MajorConstruct;

public class Spawn : BaseState
{
	[SerializeField]
	public float duration;

	[SerializeField]
	public string muzzleName;

	[SerializeField]
	public GameObject muzzleEffectPrefab;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	[SerializeField]
	public int numPads;

	[SerializeField]
	public float padRingRadius;

	[SerializeField]
	public float maxRaycastDistance;

	[SerializeField]
	public GameObject padPrefab;

	[SerializeField]
	public float maxPadDistance;

	[SerializeField]
	public GameObject padEffectPrefab;

	[SerializeField]
	public bool alignPadsToNormal;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public bool depleteStocksPrimary;

	[SerializeField]
	public bool depleteStocksSecondary;

	[SerializeField]
	public bool depleteStocksUtility;

	[SerializeField]
	public bool depleteStocksSpecial;

	public override void OnEnter()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
		if (Object.op_Implicit((Object)(object)muzzleEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, muzzleName, transmit: false);
		}
		Util.PlaySound(enterSoundString, base.gameObject);
		MasterSpawnSlotController component = GetComponent<MasterSpawnSlotController>();
		if (!NetworkServer.active || !Object.op_Implicit((Object)(object)padPrefab) || !Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		RaycastHit val = default(RaycastHit);
		for (int i = 0; i < numPads; i++)
		{
			Quaternion val2 = Quaternion.Euler(0f, 360f * ((float)i / (float)numPads), 0f);
			Vector3 val3 = base.characterBody.corePosition + val2 * Vector3.forward * padRingRadius;
			Vector3 val4 = val3 + Vector3.up * maxRaycastDistance;
			if (Physics.Raycast(val3, Vector3.up, ref val, maxRaycastDistance, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				val4 = ((RaycastHit)(ref val)).point;
			}
			if (Physics.Raycast(val4, Vector3.down, ref val, maxRaycastDistance * 2f, LayerMask.op_Implicit(LayerIndex.world.mask)) && Vector3.Distance(base.characterBody.corePosition, ((RaycastHit)(ref val)).point) < maxPadDistance)
			{
				if (alignPadsToNormal)
				{
					val2 = Quaternion.FromToRotation(Vector3.up, ((RaycastHit)(ref val)).normal) * val2;
				}
				GameObject obj = Object.Instantiate<GameObject>(padPrefab, ((RaycastHit)(ref val)).point, val2);
				NetworkedBodySpawnSlot component2 = obj.GetComponent<NetworkedBodySpawnSlot>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					component.slots.Add(component2);
				}
				NetworkServer.Spawn(obj);
				if (Object.op_Implicit((Object)(object)padEffectPrefab))
				{
					EffectData effectData = new EffectData
					{
						origin = ((RaycastHit)(ref val)).point,
						rotation = val2
					};
					EffectManager.SpawnEffect(padEffectPrefab, effectData, transmit: true);
				}
			}
		}
	}

	private void CheckForDepleteStocks(SkillSlot slot, bool deplete)
	{
		GenericSkill skill = base.skillLocator.GetSkill(slot);
		if (deplete && Object.op_Implicit((Object)(object)skill))
		{
			skill.RemoveAllStocks();
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		if (base.isAuthority)
		{
			CheckForDepleteStocks(SkillSlot.Primary, depleteStocksPrimary);
			CheckForDepleteStocks(SkillSlot.Secondary, depleteStocksSecondary);
			CheckForDepleteStocks(SkillSlot.Utility, depleteStocksUtility);
			CheckForDepleteStocks(SkillSlot.Special, depleteStocksSpecial);
		}
		base.OnExit();
	}
}
