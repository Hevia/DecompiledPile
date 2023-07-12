using System.Collections.Generic;
using HG;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Missions.Moon;

public class MoonBatteryDesignActive : MoonBatteryActive
{
	private TeamFilter teamFilter;

	public static GameObject pulsePrefab;

	public static float pulseInterval;

	public static BuffDef buffDef;

	public static float buffDuration;

	public static float baseForce;

	private float pulseTimer;

	public override void OnEnter()
	{
		base.OnEnter();
		teamFilter = GetComponent<TeamFilter>();
		pulseTimer = 0f;
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active)
		{
			pulseTimer -= Time.fixedDeltaTime;
			if (pulseTimer < 0f)
			{
				pulseTimer = pulseInterval;
				CreatePulseServer();
			}
		}
	}

	private void CreatePulseServer()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)FindModelChild("PulseOrigin")))
		{
			_ = base.transform;
		}
		GameObject obj = Object.Instantiate<GameObject>(pulsePrefab, base.transform.position, base.transform.rotation);
		SphereSearch sphereSearch = new SphereSearch();
		PulseController component = obj.GetComponent<PulseController>();
		component.performSearch += PerformPulseSearch;
		component.onPulseHit += OnPulseHit;
		component.StartPulseServer();
		NetworkServer.Spawn(obj);
		static void OnPulseHit(PulseController _, PulseController.PulseHit hitInfo)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			HealthComponent healthComponent = hitInfo.hitObject as HealthComponent;
			if (Object.op_Implicit((Object)(object)healthComponent))
			{
				CharacterBody body = healthComponent.body;
				if (Object.op_Implicit((Object)(object)body.characterMotor) && body.characterMotor.isGrounded)
				{
					Vector3 val = hitInfo.hitPos - hitInfo.pulseOrigin;
					Vector3 normalized = ((Vector3)(ref val)).normalized;
					body.characterMotor.ApplyForce(normalized * (baseForce * hitInfo.hitSeverity));
					body.AddTimedBuff(buffDef, buffDuration);
				}
			}
		}
		void PerformPulseSearch(PulseController _, Vector3 origin, float radius, List<PulseController.PulseSearchResult> dest)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			TeamMask none = TeamMask.none;
			none.AddTeam(teamFilter.teamIndex);
			sphereSearch.origin = origin;
			sphereSearch.radius = radius;
			sphereSearch.queryTriggerInteraction = (QueryTriggerInteraction)1;
			sphereSearch.mask = LayerIndex.entityPrecise.mask;
			sphereSearch.RefreshCandidates();
			sphereSearch.FilterCandidatesByHurtBoxTeam(none);
			sphereSearch.OrderCandidatesByDistance();
			sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
			List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
			sphereSearch.GetHurtBoxes(list);
			for (int i = 0; i < list.Count; i++)
			{
				HurtBox hurtBox = list[i];
				if (Object.op_Implicit((Object)(object)hurtBox))
				{
					_ = hurtBox.healthComponent;
					if (Object.op_Implicit((Object)(object)hurtBox.healthComponent))
					{
						Vector3 val2 = hurtBox.collider.ClosestPoint(origin);
						Vector3.Distance(origin, val2);
						PulseController.PulseSearchResult pulseSearchResult = default(PulseController.PulseSearchResult);
						pulseSearchResult.hitObject = (Object)(object)hurtBox.healthComponent;
						pulseSearchResult.hitPos = val2;
						PulseController.PulseSearchResult item = pulseSearchResult;
						dest.Add(item);
					}
				}
			}
			list = CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
		}
	}
}
