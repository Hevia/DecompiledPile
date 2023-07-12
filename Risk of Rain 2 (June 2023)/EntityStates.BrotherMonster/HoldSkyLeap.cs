using KinematicCharacterController;
using RoR2;
using UnityEngine;

namespace EntityStates.BrotherMonster;

public class HoldSkyLeap : BaseState
{
	public static float duration;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	public override void OnEnter()
	{
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			characterModel = ((Component)modelTransform).GetComponent<CharacterModel>();
			hurtboxGroup = ((Component)modelTransform).GetComponent<HurtBoxGroup>();
		}
		if (Object.op_Implicit((Object)(object)characterModel))
		{
			characterModel.invisibilityCount++;
		}
		if (Object.op_Implicit((Object)(object)hurtboxGroup))
		{
			HurtBoxGroup hurtBoxGroup = hurtboxGroup;
			int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
			hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
		}
		base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
		Util.PlaySound("Play_moonBrother_phaseJump_land_preWhoosh", base.gameObject);
		base.gameObject.layer = LayerIndex.fakeActor.intVal;
		((BaseCharacterController)base.characterMotor).Motor.RebuildCollidableLayers();
		if (!Object.op_Implicit((Object)(object)SceneInfo.instance))
		{
			return;
		}
		ChildLocator component = ((Component)SceneInfo.instance).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild("CenterOfArena");
			if (Object.op_Implicit((Object)(object)val))
			{
				((BaseCharacterController)base.characterMotor).Motor.SetPositionAndRotation(val.position, Quaternion.identity, true);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge > duration)
		{
			outer.SetNextState(new ExitSkyLeap());
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)characterModel))
		{
			characterModel.invisibilityCount--;
		}
		if (Object.op_Implicit((Object)(object)hurtboxGroup))
		{
			HurtBoxGroup hurtBoxGroup = hurtboxGroup;
			int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
			hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
		}
		base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
		base.gameObject.layer = LayerIndex.defaultLayer.intVal;
		((BaseCharacterController)base.characterMotor).Motor.RebuildCollidableLayers();
		base.OnExit();
	}
}
