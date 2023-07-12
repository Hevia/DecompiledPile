using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.CharacterAI;

[RequireComponent(typeof(BaseAI))]
public class EmergencyDroneCustomTarget : MonoBehaviour
{
	private BaseAI ai;

	private BullseyeSearch search;

	public float searchInterval;

	private float timer;

	private void Awake()
	{
		ai = ((Component)this).GetComponent<BaseAI>();
		if (NetworkServer.active)
		{
			search = new BullseyeSearch();
		}
	}

	private void FixedUpdate()
	{
		if (NetworkServer.active)
		{
			FixedUpdateServer();
		}
	}

	private void FixedUpdateServer()
	{
		timer -= Time.fixedDeltaTime;
		if (timer <= 0f)
		{
			timer = searchInterval;
			DoSearch();
		}
	}

	private void DoSearch()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ai.body))
		{
			Ray aimRay = ai.bodyInputBank.GetAimRay();
			search.viewer = ai.body;
			search.filterByDistinctEntity = true;
			search.filterByLoS = false;
			search.maxDistanceFilter = float.PositiveInfinity;
			search.minDistanceFilter = 0f;
			search.maxAngleFilter = 360f;
			search.searchDirection = ((Ray)(ref aimRay)).direction;
			search.searchOrigin = ((Ray)(ref aimRay)).origin;
			search.sortMode = BullseyeSearch.SortMode.Distance;
			search.queryTriggerInteraction = (QueryTriggerInteraction)0;
			TeamMask none = TeamMask.none;
			none.AddTeam(ai.master.teamIndex);
			search.teamMaskFilter = none;
			search.RefreshCandidates();
			search.FilterOutGameObject(((Component)ai.body).gameObject);
			BaseAI.Target customTarget = ai.customTarget;
			HurtBox hurtBox = search.GetResults().Where(TargetPassesFilters).FirstOrDefault();
			customTarget.gameObject = ((hurtBox != null) ? ((Component)hurtBox.healthComponent).gameObject : null);
		}
	}

	private bool TargetPassesFilters(HurtBox hurtBox)
	{
		if (IsHurt(hurtBox))
		{
			return !HealBeamController.HealBeamAlreadyExists(((Component)ai.body).gameObject, hurtBox.healthComponent);
		}
		return false;
	}

	private static bool IsHurt(HurtBox hurtBox)
	{
		if (hurtBox.healthComponent.alive)
		{
			return hurtBox.healthComponent.health < hurtBox.healthComponent.fullHealth;
		}
		return false;
	}
}
