using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Bell.BellWeapon;

public class BuffBeam : BaseState
{
	public static float duration;

	public static GameObject buffBeamPrefab;

	public static AnimationCurve beamWidthCurve;

	public static string playBeamSoundString;

	public static string stopBeamSoundString;

	public HurtBox target;

	private float healTimer;

	private float healInterval;

	private float healChunk;

	private CharacterBody targetBody;

	private GameObject buffBeamInstance;

	private BezierCurveLine healBeamCurve;

	private Transform muzzleTransform;

	private Transform beamTipTransform;

	public override void OnEnter()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(playBeamSoundString, base.gameObject);
		Ray aimRay = GetAimRay();
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.teamMaskFilter = TeamMask.none;
		if (Object.op_Implicit((Object)(object)base.teamComponent))
		{
			bullseyeSearch.teamMaskFilter.AddTeam(base.teamComponent.teamIndex);
		}
		bullseyeSearch.filterByLoS = false;
		bullseyeSearch.maxDistanceFilter = 50f;
		bullseyeSearch.maxAngleFilter = 180f;
		bullseyeSearch.searchOrigin = ((Ray)(ref aimRay)).origin;
		bullseyeSearch.searchDirection = ((Ray)(ref aimRay)).direction;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
		bullseyeSearch.RefreshCandidates();
		bullseyeSearch.FilterOutGameObject(base.gameObject);
		target = bullseyeSearch.GetResults().FirstOrDefault();
		Debug.LogFormat("Buffing target {0}", new object[1] { target });
		if (Object.op_Implicit((Object)(object)target))
		{
			targetBody = target.healthComponent.body;
			targetBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex);
		}
		string childName = "Muzzle";
		Transform modelTransform = GetModelTransform();
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			muzzleTransform = component.FindChild(childName);
			buffBeamInstance = Object.Instantiate<GameObject>(buffBeamPrefab);
			ChildLocator component2 = buffBeamInstance.GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				beamTipTransform = component2.FindChild("BeamTip");
			}
			healBeamCurve = buffBeamInstance.GetComponentInChildren<BezierCurveLine>();
		}
	}

	private void UpdateHealBeamVisuals()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		float widthMultiplier = beamWidthCurve.Evaluate(base.age / duration);
		healBeamCurve.lineRenderer.widthMultiplier = widthMultiplier;
		healBeamCurve.v0 = muzzleTransform.forward * 3f;
		((Component)healBeamCurve).transform.position = muzzleTransform.position;
		if (Object.op_Implicit((Object)(object)target))
		{
			beamTipTransform.position = ((Component)targetBody.mainHurtBox).transform.position;
		}
	}

	public override void Update()
	{
		base.Update();
		UpdateHealBeamVisuals();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if ((base.fixedAge >= duration || (Object)(object)target == (Object)null) && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		Util.PlaySound(stopBeamSoundString, base.gameObject);
		EntityState.Destroy((Object)(object)buffBeamInstance);
		if (Object.op_Implicit((Object)(object)targetBody))
		{
			targetBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility.buffIndex);
		}
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Any;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		HurtBoxReference.FromHurtBox(target).Write(writer);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		HurtBoxReference hurtBoxReference = default(HurtBoxReference);
		hurtBoxReference.Read(reader);
		GameObject obj = hurtBoxReference.ResolveGameObject();
		target = ((obj != null) ? obj.GetComponent<HurtBox>() : null);
	}
}
