using System.Linq;
using RoR2;
using UnityEngine;

namespace EntityStates.GrandParentBoss;

public class PortalFist : BaseState
{
	public static float baseDuration;

	public static GameObject portalInEffectPrefab;

	public static GameObject portalOutEffectPrefab;

	public static string portalMuzzleString;

	public static string fistMeshChildLocatorString;

	public static string fistBoneChildLocatorString;

	public static string mecanimFistVisibilityString;

	public static float fistOverlayDuration;

	public static Material fistOverlayMaterial;

	public static float targetSearchMaxDistance;

	private GameObject fistMeshGameObject;

	private GameObject fistBoneGameObject;

	private GameObject fistTargetGameObject;

	private string fistTargetMuzzleString;

	private Animator modelAnimator;

	private CharacterModel characterModel;

	private float duration;

	private bool fistWasOutOfPortal = true;

	public override void OnEnter()
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		fistMeshGameObject = ((Component)FindModelChild(fistMeshChildLocatorString)).gameObject;
		fistBoneGameObject = ((Component)FindModelChild(fistBoneChildLocatorString)).gameObject;
		modelAnimator = GetModelAnimator();
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			characterModel = ((Component)modelTransform).GetComponent<CharacterModel>();
		}
		duration = baseDuration / attackSpeedStat;
		PlayCrossfade("Body", "PortalFist", "PortalFist.playbackRate", duration, 0.3f);
		Transform val = FindModelChild("PortalFistTargetRig");
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.viewer = base.characterBody;
		bullseyeSearch.searchOrigin = base.characterBody.corePosition;
		bullseyeSearch.searchDirection = base.characterBody.corePosition;
		bullseyeSearch.maxDistanceFilter = targetSearchMaxDistance;
		bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(GetTeam());
		bullseyeSearch.teamMaskFilter.RemoveTeam(TeamIndex.Neutral);
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
		bullseyeSearch.RefreshCandidates();
		HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
		if (Object.op_Implicit((Object)(object)hurtBox))
		{
			val.position = ((Component)hurtBox).transform.position;
			fistTargetMuzzleString = $"PortalFistTargetPosition{Random.Range(1, 5).ToString()}";
			fistTargetGameObject = ((Component)FindModelChild(fistTargetMuzzleString)).gameObject;
		}
	}

	public override void Update()
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		bool flag = modelAnimator.GetFloat(mecanimFistVisibilityString) > 0.5f;
		if (flag != fistWasOutOfPortal)
		{
			fistWasOutOfPortal = flag;
			EffectManager.SimpleMuzzleFlash(flag ? portalOutEffectPrefab : portalInEffectPrefab, base.gameObject, portalMuzzleString, transmit: false);
			EffectManager.SimpleMuzzleFlash(flag ? portalOutEffectPrefab : portalInEffectPrefab, base.gameObject, fistTargetMuzzleString, transmit: false);
			if (Object.op_Implicit((Object)(object)characterModel) && Object.op_Implicit((Object)(object)fistOverlayMaterial))
			{
				TemporaryOverlay temporaryOverlay = ((Component)characterModel).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = fistOverlayDuration;
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = fistOverlayMaterial;
				temporaryOverlay.inspectorCharacterModel = characterModel;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.animateShaderAlpha = true;
			}
		}
		if (Object.op_Implicit((Object)(object)fistTargetGameObject) && !flag)
		{
			fistBoneGameObject.transform.SetPositionAndRotation(fistTargetGameObject.transform.position, fistTargetGameObject.transform.rotation);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextStateToMain();
		}
	}
}
