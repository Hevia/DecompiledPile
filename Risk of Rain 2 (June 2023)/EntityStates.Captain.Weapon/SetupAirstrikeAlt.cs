using RoR2;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Captain.Weapon;

public class SetupAirstrikeAlt : BaseState
{
	public static SkillDef primarySkillDef;

	public static GameObject crosshairOverridePrefab;

	public static string enterSoundString;

	public static string exitSoundString;

	public static GameObject effectMuzzlePrefab;

	public static string effectMuzzleString;

	public static float baseExitDuration;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	private GenericSkill primarySkillSlot;

	private GameObject effectMuzzleInstance;

	private Animator modelAnimator;

	private float timerSinceComplete;

	private bool beginExit;

	private float exitDuration => baseExitDuration / attackSpeedStat;

	public override void OnEnter()
	{
		base.OnEnter();
		primarySkillSlot = (Object.op_Implicit((Object)(object)base.skillLocator) ? base.skillLocator.primary : null);
		if (Object.op_Implicit((Object)(object)primarySkillSlot))
		{
			primarySkillSlot.SetSkillOverride(this, primarySkillDef, GenericSkill.SkillOverridePriority.Contextual);
		}
		modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.SetBool("PrepAirstrike", true);
		}
		PlayCrossfade("Gesture, Override", "PrepAirstrike", 0.1f);
		PlayCrossfade("Gesture, Additive", "PrepAirstrike", 0.1f);
		Transform val = FindModelChild(effectMuzzleString);
		if (Object.op_Implicit((Object)(object)val))
		{
			effectMuzzleInstance = Object.Instantiate<GameObject>(effectMuzzlePrefab, val);
		}
		if (Object.op_Implicit((Object)(object)crosshairOverridePrefab))
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
		}
		Util.PlaySound(enterSoundString, base.gameObject);
		Util.PlaySound("Play_captain_shift_active_loop", base.gameObject);
	}

	public override void FixedUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			CharacterDirection obj = base.characterDirection;
			Ray aimRay = GetAimRay();
			obj.moveVector = ((Ray)(ref aimRay)).direction;
		}
		if (!Object.op_Implicit((Object)(object)primarySkillSlot) || primarySkillSlot.stock == 0)
		{
			beginExit = true;
		}
		if (beginExit)
		{
			timerSinceComplete += Time.fixedDeltaTime;
			if (timerSinceComplete > exitDuration)
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)primarySkillSlot))
		{
			primarySkillSlot.UnsetSkillOverride(this, primarySkillDef, GenericSkill.SkillOverridePriority.Contextual);
		}
		Util.PlaySound(exitSoundString, base.gameObject);
		Util.PlaySound("Stop_captain_shift_active_loop", base.gameObject);
		if (Object.op_Implicit((Object)(object)effectMuzzleInstance))
		{
			EntityState.Destroy((Object)(object)effectMuzzleInstance);
		}
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.SetBool("PrepAirstrike", false);
		}
		crosshairOverrideRequest?.Dispose();
		base.OnExit();
	}
}
