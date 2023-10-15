using System.Linq;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(InputBankTest))]
[RequireComponent(typeof(CharacterBody))]
[RequireComponent(typeof(TeamComponent))]
public class HuntressTracker : MonoBehaviour
{
	public float maxTrackingDistance = 20f;

	public float maxTrackingAngle = 20f;

	public float trackerUpdateFrequency = 10f;

	private HurtBox trackingTarget;

	private CharacterBody characterBody;

	private TeamComponent teamComponent;

	private InputBankTest inputBank;

	private float trackerUpdateStopwatch;

	private Indicator indicator;

	private readonly BullseyeSearch search = new BullseyeSearch();

	private void Awake()
	{
		indicator = new Indicator(((Component)this).gameObject, LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator"));
	}

	private void Start()
	{
		characterBody = ((Component)this).GetComponent<CharacterBody>();
		inputBank = ((Component)this).GetComponent<InputBankTest>();
		teamComponent = ((Component)this).GetComponent<TeamComponent>();
	}

	public HurtBox GetTrackingTarget()
	{
		return trackingTarget;
	}

	private void OnEnable()
	{
		indicator.active = true;
	}

	private void OnDisable()
	{
		indicator.active = false;
	}

	private void FixedUpdate()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		trackerUpdateStopwatch += Time.fixedDeltaTime;
		if (trackerUpdateStopwatch >= 1f / trackerUpdateFrequency)
		{
			trackerUpdateStopwatch -= 1f / trackerUpdateFrequency;
			_ = trackingTarget;
			Ray aimRay = default(Ray);
			aimRay._002Ector(inputBank.aimOrigin, inputBank.aimDirection);
			SearchForTarget(aimRay);
			indicator.targetTransform = (Object.op_Implicit((Object)(object)trackingTarget) ? ((Component)trackingTarget).transform : null);
		}
	}

	private void SearchForTarget(Ray aimRay)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		search.teamMaskFilter = TeamMask.GetUnprotectedTeams(teamComponent.teamIndex);
		search.filterByLoS = true;
		search.searchOrigin = aimRay.origin;
		search.searchDirection = aimRay.direction;
		search.sortMode = BullseyeSearch.SortMode.Distance;
		search.maxDistanceFilter = maxTrackingDistance;
		search.maxAngleFilter = maxTrackingAngle;
		search.RefreshCandidates();
		search.FilterOutGameObject(((Component)this).gameObject);
		trackingTarget = search.GetResults().FirstOrDefault();
	}
}
