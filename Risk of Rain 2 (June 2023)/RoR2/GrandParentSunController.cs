using System.Collections.Generic;
using RoR2.Audio;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(TeamFilter))]
[RequireComponent(typeof(GenericOwnership))]
public class GrandParentSunController : MonoBehaviour
{
	private TeamFilter teamFilter;

	private GenericOwnership ownership;

	public BuffDef buffDef;

	[Min(0.001f)]
	public float cycleInterval = 1f;

	[Min(0.001f)]
	public float nearBuffDuration = 1f;

	[Min(0.001f)]
	public float maxDistance = 1f;

	public int minimumStacksBeforeApplyingBurns = 4;

	public float burnDuration = 5f;

	public GameObject buffApplyEffect;

	[SerializeField]
	private LoopSoundDef activeLoopDef;

	[SerializeField]
	private LoopSoundDef damageLoopDef;

	[SerializeField]
	private string stopSoundName;

	private Run.FixedTimeStamp previousCycle = Run.FixedTimeStamp.negativeInfinity;

	private int cycleIndex;

	private List<HurtBox> cycleTargets = new List<HurtBox>();

	private BullseyeSearch bullseyeSearch = new BullseyeSearch();

	private bool isLocalPlayerDamaged;

	private void Awake()
	{
		teamFilter = ((Component)this).GetComponent<TeamFilter>();
		ownership = ((Component)this).GetComponent<GenericOwnership>();
	}

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)activeLoopDef))
		{
			Util.PlaySound(activeLoopDef.startSoundName, ((Component)this).gameObject);
		}
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)activeLoopDef))
		{
			Util.PlaySound(activeLoopDef.stopSoundName, ((Component)this).gameObject);
		}
		if (Object.op_Implicit((Object)(object)damageLoopDef))
		{
			Util.PlaySound(damageLoopDef.stopSoundName, ((Component)this).gameObject);
		}
		if (stopSoundName != null)
		{
			Util.PlaySound(stopSoundName, ((Component)this).gameObject);
		}
	}

	private void FixedUpdate()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			ServerFixedUpdate();
		}
		if (!Object.op_Implicit((Object)(object)damageLoopDef))
		{
			return;
		}
		bool flag = isLocalPlayerDamaged;
		isLocalPlayerDamaged = false;
		RaycastHit val = default(RaycastHit);
		foreach (HurtBox cycleTarget in cycleTargets)
		{
			CharacterBody characterBody = null;
			if (Object.op_Implicit((Object)(object)cycleTarget) && Object.op_Implicit((Object)(object)cycleTarget.healthComponent))
			{
				characterBody = cycleTarget.healthComponent.body;
			}
			if (Object.op_Implicit((Object)(object)characterBody) && (characterBody.bodyFlags & CharacterBody.BodyFlags.OverheatImmune) != 0 && characterBody.hasEffectiveAuthority)
			{
				Vector3 position = ((Component)this).transform.position;
				Vector3 corePosition = characterBody.corePosition;
				if (!Physics.Linecast(position, corePosition, ref val, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
				{
					isLocalPlayerDamaged = true;
				}
			}
		}
		if (isLocalPlayerDamaged && !flag)
		{
			Util.PlaySound(damageLoopDef.startSoundName, ((Component)this).gameObject);
		}
		else if (!isLocalPlayerDamaged && flag)
		{
			Util.PlaySound(damageLoopDef.stopSoundName, ((Component)this).gameObject);
		}
	}

	private void ServerFixedUpdate()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01(previousCycle.timeSince / cycleInterval);
		int num2 = ((num == 1f) ? cycleTargets.Count : Mathf.FloorToInt((float)cycleTargets.Count * num));
		Vector3 position = ((Component)this).transform.position;
		Ray val = default(Ray);
		RaycastHit val2 = default(RaycastHit);
		while (cycleIndex < num2)
		{
			HurtBox hurtBox = cycleTargets[cycleIndex];
			if (Object.op_Implicit((Object)(object)hurtBox) && Object.op_Implicit((Object)(object)hurtBox.healthComponent))
			{
				CharacterBody body = hurtBox.healthComponent.body;
				if ((body.bodyFlags & CharacterBody.BodyFlags.OverheatImmune) == 0)
				{
					Vector3 corePosition = body.corePosition;
					((Ray)(ref val))._002Ector(position, corePosition - position);
					if (!Physics.Linecast(position, corePosition, ref val2, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
					{
						float num3 = Mathf.Max(1f, ((RaycastHit)(ref val2)).distance);
						body.AddTimedBuff(buffDef, nearBuffDuration / num3);
						if (Object.op_Implicit((Object)(object)buffApplyEffect))
						{
							EffectData effectData = new EffectData
							{
								origin = corePosition,
								rotation = Util.QuaternionSafeLookRotation(-((Ray)(ref val)).direction),
								scale = body.bestFitRadius
							};
							effectData.SetHurtBoxReference(hurtBox);
							EffectManager.SpawnEffect(buffApplyEffect, effectData, transmit: true);
						}
						int num4 = body.GetBuffCount(buffDef) - minimumStacksBeforeApplyingBurns;
						if (num4 > 0)
						{
							InflictDotInfo dotInfo = default(InflictDotInfo);
							dotInfo.dotIndex = DotController.DotIndex.Burn;
							dotInfo.attackerObject = ownership.ownerObject;
							dotInfo.victimObject = ((Component)body).gameObject;
							dotInfo.damageMultiplier = 1f;
							GenericOwnership genericOwnership = ownership;
							object obj;
							if (genericOwnership == null)
							{
								obj = null;
							}
							else
							{
								GameObject ownerObject = genericOwnership.ownerObject;
								obj = ((ownerObject != null) ? ownerObject.GetComponent<CharacterBody>() : null);
							}
							CharacterBody characterBody = (CharacterBody)obj;
							if (Object.op_Implicit((Object)(object)characterBody) && Object.op_Implicit((Object)(object)characterBody.inventory))
							{
								dotInfo.totalDamage = 0.5f * characterBody.damage * burnDuration * (float)num4;
								StrengthenBurnUtils.CheckDotForUpgrade(characterBody.inventory, ref dotInfo);
							}
							DotController.InflictDot(ref dotInfo);
						}
					}
				}
			}
			cycleIndex++;
		}
		if (previousCycle.timeSince >= cycleInterval)
		{
			previousCycle = Run.FixedTimeStamp.now;
			cycleIndex = 0;
			cycleTargets.Clear();
			SearchForTargets(cycleTargets);
		}
	}

	private void SearchForTargets(List<HurtBox> dest)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		bullseyeSearch.searchOrigin = ((Component)this).transform.position;
		bullseyeSearch.minAngleFilter = 0f;
		bullseyeSearch.maxAngleFilter = 180f;
		bullseyeSearch.maxDistanceFilter = maxDistance;
		bullseyeSearch.filterByDistinctEntity = true;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
		bullseyeSearch.viewer = null;
		bullseyeSearch.RefreshCandidates();
		dest.AddRange(bullseyeSearch.GetResults());
	}
}
