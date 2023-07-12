using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileTargetComponent))]
[RequireComponent(typeof(TeamFilter))]
public class ProjectileSphereTargetFinder : MonoBehaviour
{
	[Tooltip("How far ahead the projectile should look to find a target.")]
	public float lookRange;

	[Tooltip("How long before searching for a target.")]
	public float targetSearchInterval = 0.5f;

	[Tooltip("Will not search for new targets once it has one.")]
	public bool onlySearchIfNoTarget;

	[Tooltip("Allows the target to be lost if it's outside the acceptable range.")]
	public bool allowTargetLoss;

	[Tooltip("If set, targets can only be found when there is a free line of sight.")]
	public bool testLoS;

	[Tooltip("Whether or not airborne characters should be ignored.")]
	public bool ignoreAir;

	[Tooltip("The difference in altitude at which a result will be ignored.")]
	[FormerlySerializedAs("altitudeTolerance")]
	public float flierAltitudeTolerance = float.PositiveInfinity;

	public UnityEvent onNewTargetFound;

	public UnityEvent onTargetLost;

	private Transform transform;

	private TeamFilter teamFilter;

	private ProjectileTargetComponent targetComponent;

	private float searchTimer;

	private SphereSearch sphereSearch;

	private bool hasTarget;

	private bool hadTargetLastUpdate;

	private HurtBox lastFoundHurtBox;

	private Transform lastFoundTransform;

	private static readonly List<HurtBox> foundHurtBoxes = new List<HurtBox>();

	private void Start()
	{
		if (!NetworkServer.active)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		transform = ((Component)this).transform;
		teamFilter = ((Component)this).GetComponent<TeamFilter>();
		targetComponent = ((Component)this).GetComponent<ProjectileTargetComponent>();
		sphereSearch = new SphereSearch();
		searchTimer = 0f;
	}

	private void FixedUpdate()
	{
		searchTimer -= Time.fixedDeltaTime;
		if (!(searchTimer <= 0f))
		{
			return;
		}
		searchTimer += targetSearchInterval;
		if (allowTargetLoss && targetComponent.target != null && lastFoundTransform == targetComponent.target && !PassesFilters(lastFoundHurtBox))
		{
			SetTarget(null);
		}
		if (!onlySearchIfNoTarget || (Object)(object)targetComponent.target == (Object)null)
		{
			SearchForTarget();
		}
		hasTarget = (Object)(object)targetComponent.target != (Object)null;
		if (hadTargetLastUpdate != hasTarget)
		{
			if (hasTarget)
			{
				UnityEvent obj = onNewTargetFound;
				if (obj != null)
				{
					obj.Invoke();
				}
			}
			else
			{
				UnityEvent obj2 = onTargetLost;
				if (obj2 != null)
				{
					obj2.Invoke();
				}
			}
		}
		hadTargetLastUpdate = hasTarget;
	}

	private bool PassesFilters(HurtBox result)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		CharacterBody body = result.healthComponent.body;
		if (!Object.op_Implicit((Object)(object)body) || (ignoreAir && body.isFlying))
		{
			return false;
		}
		if (body.isFlying && !float.IsInfinity(flierAltitudeTolerance) && flierAltitudeTolerance < Mathf.Abs(((Component)result).transform.position.y - transform.position.y))
		{
			return false;
		}
		return true;
	}

	private void SearchForTarget()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		sphereSearch.origin = transform.position;
		sphereSearch.radius = lookRange;
		sphereSearch.mask = LayerIndex.entityPrecise.mask;
		sphereSearch.queryTriggerInteraction = (QueryTriggerInteraction)0;
		sphereSearch.RefreshCandidates();
		sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamFilter.teamIndex));
		sphereSearch.OrderCandidatesByDistance();
		sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
		sphereSearch.GetHurtBoxes(foundHurtBoxes);
		HurtBox target = null;
		if (foundHurtBoxes.Count > 0)
		{
			int i = 0;
			for (int count = foundHurtBoxes.Count; i < count; i++)
			{
				if (PassesFilters(foundHurtBoxes[i]))
				{
					target = foundHurtBoxes[i];
					break;
				}
			}
			foundHurtBoxes.Clear();
		}
		SetTarget(target);
	}

	private void SetTarget(HurtBox hurtBox)
	{
		lastFoundHurtBox = hurtBox;
		lastFoundTransform = ((hurtBox != null) ? ((Component)hurtBox).transform : null);
		targetComponent.target = lastFoundTransform;
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.yellow;
		Vector3 position = ((Component)this).transform.position;
		Gizmos.DrawWireSphere(position, lookRange);
		if (!float.IsInfinity(flierAltitudeTolerance))
		{
			Gizmos.DrawWireCube(position, new Vector3(lookRange * 2f, flierAltitudeTolerance * 2f, lookRange * 2f));
		}
	}
}
