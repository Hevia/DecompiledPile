using System.Collections.Generic;
using HG;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class RadialForce : MonoBehaviour
{
	public float radius;

	public float damping = 0.2f;

	public float forceMagnitude;

	public float forceCoefficientAtEdge = 0.5f;

	public TetherVfxOrigin tetherVfxOrigin;

	private SphereSearch sphereSearch;

	protected Transform transform { get; private set; }

	protected TeamFilter teamFilter { get; private set; }

	protected void Awake()
	{
		transform = ((Component)this).GetComponent<Transform>();
		teamFilter = ((Component)this).GetComponent<TeamFilter>();
		sphereSearch = new SphereSearch();
	}

	protected void FixedUpdate()
	{
		List<HurtBox> list = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
		SearchForTargets(list);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			ApplyPullToHurtBox(list[i]);
		}
		if (Object.op_Implicit((Object)(object)tetherVfxOrigin))
		{
			List<Transform> list2 = CollectionPool<Transform, List<Transform>>.RentCollection();
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				HurtBox hurtBox = list[j];
				if (!Object.op_Implicit((Object)(object)hurtBox))
				{
					continue;
				}
				Transform item = ((Component)hurtBox).transform;
				HealthComponent healthComponent = hurtBox.healthComponent;
				if (Object.op_Implicit((Object)(object)healthComponent))
				{
					Transform coreTransform = healthComponent.body.coreTransform;
					if (Object.op_Implicit((Object)(object)coreTransform))
					{
						item = coreTransform;
					}
				}
				list2.Add(item);
			}
			tetherVfxOrigin.SetTetheredTransforms(list2);
			CollectionPool<Transform, List<Transform>>.ReturnCollection(list2);
		}
		CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(list);
	}

	protected void SearchForTargets(List<HurtBox> dest)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		sphereSearch.mask = LayerIndex.entityPrecise.mask;
		sphereSearch.origin = transform.position;
		sphereSearch.radius = radius;
		sphereSearch.queryTriggerInteraction = (QueryTriggerInteraction)0;
		sphereSearch.RefreshCandidates();
		sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamFilter.teamIndex));
		sphereSearch.OrderCandidatesByDistance();
		sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
		sphereSearch.GetHurtBoxes(dest);
		sphereSearch.ClearCandidates();
	}

	protected void ApplyPullToHurtBox(HurtBox hurtBox)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)hurtBox))
		{
			return;
		}
		HealthComponent healthComponent = hurtBox.healthComponent;
		if (!Object.op_Implicit((Object)(object)healthComponent) || !Object.op_Implicit((Object)(object)healthComponent.body) || !Object.op_Implicit((Object)(object)((Component)hurtBox).transform) || !Object.op_Implicit((Object)(object)transform) || !NetworkServer.active)
		{
			return;
		}
		CharacterMotor characterMotor = healthComponent.body.characterMotor;
		Vector3 val = ((Component)hurtBox).transform.position - transform.position;
		float num = 1f - Mathf.Clamp(((Vector3)(ref val)).magnitude / radius, 0f, 1f - forceCoefficientAtEdge);
		val = ((Vector3)(ref val)).normalized * forceMagnitude * (1f - num);
		Vector3 val2 = Vector3.zero;
		float num2 = 0f;
		if (Object.op_Implicit((Object)(object)characterMotor))
		{
			val2 = characterMotor.velocity;
			num2 = characterMotor.mass;
		}
		else
		{
			Rigidbody rigidbody = healthComponent.body.rigidbody;
			if (Object.op_Implicit((Object)(object)rigidbody))
			{
				val2 = rigidbody.velocity;
				num2 = rigidbody.mass;
			}
		}
		val2.y += Physics.gravity.y * Time.fixedDeltaTime;
		healthComponent.TakeDamageForce(val - val2 * damping * num2 * num, alwaysApply: true);
	}
}
