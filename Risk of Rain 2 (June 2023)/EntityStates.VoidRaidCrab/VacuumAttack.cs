using System.Collections.Generic;
using System.Collections.ObjectModel;
using HG;
using RoR2;
using RoR2.Audio;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidRaidCrab;

public class VacuumAttack : BaseVacuumAttackState
{
	public static AnimationCurve killRadiusCurve;

	public static AnimationCurve pullMagnitudeCurve;

	public static float losObstructionFactor = 0.5f;

	public static GameObject killSphereVfxPrefab;

	public static GameObject environmentVfxPrefab;

	public static LoopSoundDef loopSound;

	private CharacterLosTracker losTracker;

	private VFXHelper killSphereVfxHelper;

	private VFXHelper environmentVfxHelper;

	private SphereSearch killSearch;

	private float killRadius = 1f;

	private BodyIndex jointBodyIndex = BodyIndex.None;

	private LoopSoundManager.SoundLoopPtr loopPtr;

	public override void OnEnter()
	{
		base.OnEnter();
		losTracker = new CharacterLosTracker();
		losTracker.enabled = true;
		killSphereVfxHelper = VFXHelper.Rent();
		killSphereVfxHelper.vfxPrefabReference = killSphereVfxPrefab;
		killSphereVfxHelper.followedTransform = base.vacuumOrigin;
		killSphereVfxHelper.useFollowedTransformScale = false;
		killSphereVfxHelper.enabled = true;
		UpdateKillSphereVfx();
		environmentVfxHelper = VFXHelper.Rent();
		environmentVfxHelper.vfxPrefabReference = environmentVfxPrefab;
		environmentVfxHelper.followedTransform = base.vacuumOrigin;
		environmentVfxHelper.useFollowedTransformScale = false;
		environmentVfxHelper.enabled = true;
		loopPtr = LoopSoundManager.PlaySoundLoopLocal(base.gameObject, loopSound);
		if (NetworkServer.active)
		{
			killSearch = new SphereSearch();
		}
		jointBodyIndex = BodyCatalog.FindBodyIndex("VoidRaidCrabJointBody");
	}

	public override void OnExit()
	{
		killSphereVfxHelper = VFXHelper.Return(killSphereVfxHelper);
		environmentVfxHelper = VFXHelper.Return(environmentVfxHelper);
		losTracker.enabled = false;
		losTracker.Dispose();
		losTracker = null;
		LoopSoundManager.StopSoundLoopLocal(loopPtr);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		float num = Mathf.Clamp01(base.fixedAge / base.duration);
		killRadius = killRadiusCurve.Evaluate(num);
		UpdateKillSphereVfx();
		Vector3 position = base.vacuumOrigin.position;
		losTracker.origin = position;
		losTracker.Step();
		ReadOnlyCollection<CharacterBody> readOnlyInstancesList = CharacterBody.readOnlyInstancesList;
		float num2 = pullMagnitudeCurve.Evaluate(num);
		for (int i = 0; i < readOnlyInstancesList.Count; i++)
		{
			CharacterBody characterBody = readOnlyInstancesList[i];
			if (characterBody == base.characterBody)
			{
				continue;
			}
			bool flag = losTracker.CheckBodyHasLos(characterBody);
			if (characterBody.hasEffectiveAuthority)
			{
				IDisplacementReceiver component = ((Component)characterBody).GetComponent<IDisplacementReceiver>();
				if (component != null)
				{
					float num3 = (flag ? 1f : losObstructionFactor);
					Vector3 val = position - ((Component)characterBody).transform.position;
					component.AddDisplacement(((Vector3)(ref val)).normalized * (num2 * num3 * Time.fixedDeltaTime));
				}
			}
		}
		if (!NetworkServer.active)
		{
			return;
		}
		List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
		List<HealthComponent> list2 = CollectionPool<HealthComponent, List<HealthComponent>>.RentCollection();
		try
		{
			killSearch.radius = killRadius;
			killSearch.origin = position;
			killSearch.mask = LayerIndex.entityPrecise.mask;
			killSearch.RefreshCandidates();
			killSearch.OrderCandidatesByDistance();
			killSearch.FilterCandidatesByDistinctHurtBoxEntities();
			killSearch.GetHurtBoxes(list);
			for (int j = 0; j < list.Count; j++)
			{
				HurtBox hurtBox = list[j];
				if (Object.op_Implicit((Object)(object)hurtBox.healthComponent))
				{
					list2.Add(hurtBox.healthComponent);
				}
			}
			for (int k = 0; k < list2.Count; k++)
			{
				HealthComponent healthComponent = list2[k];
				if (base.healthComponent != healthComponent && healthComponent.body.bodyIndex != jointBodyIndex)
				{
					Debug.Log((object)$"Destroying HealthComponent. healthComponent={base.healthComponent}({((Object)base.healthComponent).GetInstanceID()}) victim={healthComponent}{((Object)healthComponent).GetInstanceID()} bodyIndex={base.healthComponent.body.bodyIndex} jointBodyIndex={jointBodyIndex}");
					healthComponent.Suicide(base.gameObject, base.gameObject, DamageType.VoidDeath);
				}
			}
		}
		finally
		{
			list2 = CollectionPool<HealthComponent, List<HealthComponent>>.ReturnCollection(list2);
			list = CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
		}
	}

	private void UpdateKillSphereVfx()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)killSphereVfxHelper.vfxInstanceTransform))
		{
			killSphereVfxHelper.vfxInstanceTransform.localScale = Vector3.one * killRadius;
		}
	}

	protected override void OnLifetimeExpiredAuthority()
	{
		outer.SetNextState(new VacuumWindDown());
	}
}
