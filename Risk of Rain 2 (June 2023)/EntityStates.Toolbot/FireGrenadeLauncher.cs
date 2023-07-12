using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Toolbot;

public class FireGrenadeLauncher : GenericProjectileBaseState, IToolbotPrimarySkillState, ISkillState
{
	Transform IToolbotPrimarySkillState.muzzleTransform { get; set; }

	string IToolbotPrimarySkillState.baseMuzzleName => targetMuzzle;

	bool IToolbotPrimarySkillState.isInDualWield { get; set; }

	int IToolbotPrimarySkillState.currentHand { get; set; }

	string IToolbotPrimarySkillState.muzzleName { get; set; }

	public SkillDef skillDef { get; set; }

	public GenericSkill activatorSkillSlot { get; set; }

	public override void OnEnter()
	{
		BaseToolbotPrimarySkillStateMethods.OnEnter(this, base.gameObject, base.skillLocator, GetModelChildLocator());
		base.OnEnter();
	}

	protected override void PlayAnimation(float duration)
	{
		if (!((IToolbotPrimarySkillState)this).isInDualWield)
		{
			PlayAnimation("Gesture, Additive", "FireGrenadeLauncher", "FireGrenadeLauncher.playbackRate", duration);
		}
		else
		{
			BaseToolbotPrimarySkillStateMethods.PlayGenericFireAnim(this, base.gameObject, base.skillLocator, duration);
		}
	}

	protected override Ray ModifyProjectileAimRay(Ray projectileRay)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (((IToolbotPrimarySkillState)this).isInDualWield)
		{
			Transform muzzleTransform = ((IToolbotPrimarySkillState)this).muzzleTransform;
			if (Object.op_Implicit((Object)(object)muzzleTransform))
			{
				((Ray)(ref projectileRay)).origin = muzzleTransform.position;
			}
		}
		return projectileRay;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		this.Serialize(base.skillLocator, writer);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		this.Deserialize(base.skillLocator, reader);
	}
}
