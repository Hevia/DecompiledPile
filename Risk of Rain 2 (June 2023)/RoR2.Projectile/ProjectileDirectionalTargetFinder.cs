using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace RoR2.Projectile;

[RequireComponent(typeof(TeamFilter))]
[RequireComponent(typeof(ProjectileTargetComponent))]
public class ProjectileDirectionalTargetFinder : MonoBehaviour
{
	[Tooltip("How far ahead the projectile should look to find a target.")]
	public float lookRange;

	[Tooltip("How wide the cone of vision for this projectile is in degrees. Limit is 180.")]
	[Range(0f, 180f)]
	public float lookCone;

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

	[FormerlySerializedAs("ontargetLost")]
	public UnityEvent onTargetLost;

	private Transform transform;

	private TeamFilter teamFilter;

	private ProjectileTargetComponent targetComponent;

	private float searchTimer;

	private bool hasTarget;

	private bool hadTargetLastUpdate;

	private BullseyeSearch bullseyeSearch;

	private HurtBox lastFoundHurtBox;

	private Transform lastFoundTransform;

	private void Start()
	{
		if (!NetworkServer.active)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		bullseyeSearch = new BullseyeSearch();
		teamFilter = ((Component)this).GetComponent<TeamFilter>();
		targetComponent = ((Component)this).GetComponent<ProjectileTargetComponent>();
		transform = ((Component)this).transform;
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
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
		bullseyeSearch.teamMaskFilter.RemoveTeam(teamFilter.teamIndex);
		bullseyeSearch.filterByLoS = testLoS;
		bullseyeSearch.searchOrigin = transform.position;
		bullseyeSearch.searchDirection = transform.forward;
		bullseyeSearch.maxDistanceFilter = lookRange;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
		bullseyeSearch.maxAngleFilter = lookCone;
		bullseyeSearch.RefreshCandidates();
		IEnumerable<HurtBox> source = bullseyeSearch.GetResults().Where(PassesFilters);
		SetTarget(source.FirstOrDefault());
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
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.yellow;
		Transform val = ((Component)this).transform;
		Vector3 position = val.position;
		Gizmos.DrawWireSphere(position, lookRange);
		Gizmos.DrawRay(position, val.forward * lookRange);
		Gizmos.DrawFrustum(position, lookCone * 2f, lookRange, 0f, 1f);
		if (!float.IsInfinity(flierAltitudeTolerance))
		{
			Gizmos.DrawWireCube(position, new Vector3(lookRange * 2f, flierAltitudeTolerance * 2f, lookRange * 2f));
		}
	}
}
