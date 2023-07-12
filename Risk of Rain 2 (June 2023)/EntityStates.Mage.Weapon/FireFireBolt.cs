using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Mage.Weapon;

public class FireFireBolt : BaseState, SteppedSkillDef.IStepSetter
{
	public enum Gauntlet
	{
		Left,
		Right
	}

	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public GameObject muzzleflashEffectPrefab;

	[SerializeField]
	public float procCoefficient;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float force = 20f;

	public static float attackSpeedAltAnimationThreshold;

	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public string attackSoundString;

	[SerializeField]
	public float attackSoundPitch;

	public static float bloom;

	private float duration;

	private bool hasFiredGauntlet;

	private string muzzleString;

	private Transform muzzleTransform;

	private Animator animator;

	private ChildLocator childLocator;

	private Gauntlet gauntlet;

	public void SetStep(int i)
	{
		gauntlet = (Gauntlet)i;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSoundPitch);
		base.characterBody.SetAimTimer(2f);
		animator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)animator))
		{
			childLocator = ((Component)animator).GetComponent<ChildLocator>();
		}
		switch (gauntlet)
		{
		case Gauntlet.Left:
			muzzleString = "MuzzleLeft";
			if (attackSpeedStat < attackSpeedAltAnimationThreshold)
			{
				PlayCrossfade("Gesture, Additive", "Cast1Left", "FireGauntlet.playbackRate", duration, 0.1f);
				PlayAnimation("Gesture Left, Additive", "Empty");
				PlayAnimation("Gesture Right, Additive", "Empty");
			}
			else
			{
				PlayAnimation("Gesture Left, Additive", "FireGauntletLeft", "FireGauntlet.playbackRate", duration);
				PlayAnimation("Gesture, Additive", "HoldGauntletsUp", "FireGauntlet.playbackRate", duration);
				FireGauntlet();
			}
			break;
		case Gauntlet.Right:
			muzzleString = "MuzzleRight";
			if (attackSpeedStat < attackSpeedAltAnimationThreshold)
			{
				PlayCrossfade("Gesture, Additive", "Cast1Right", "FireGauntlet.playbackRate", duration, 0.1f);
				PlayAnimation("Gesture Left, Additive", "Empty");
				PlayAnimation("Gesture Right, Additive", "Empty");
			}
			else
			{
				PlayAnimation("Gesture Right, Additive", "FireGauntletRight", "FireGauntlet.playbackRate", duration);
				PlayAnimation("Gesture, Additive", "HoldGauntletsUp", "FireGauntlet.playbackRate", duration);
				FireGauntlet();
			}
			break;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	private void FireGauntlet()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (!hasFiredGauntlet)
		{
			base.characterBody.AddSpreadBloom(bloom);
			hasFiredGauntlet = true;
			Ray aimRay = GetAimRay();
			if (Object.op_Implicit((Object)(object)childLocator))
			{
				muzzleTransform = childLocator.FindChild(muzzleString);
			}
			if (Object.op_Implicit((Object)(object)muzzleflashEffectPrefab))
			{
				EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleString, transmit: false);
			}
			if (base.isAuthority)
			{
				ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction), base.gameObject, damageCoefficient * damageStat, 0f, Util.CheckRoll(critStat, base.characterBody.master));
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (animator.GetFloat("FireGauntlet.fire") > 0f && !hasFiredGauntlet)
		{
			FireGauntlet();
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write((byte)gauntlet);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		gauntlet = (Gauntlet)reader.ReadByte();
	}
}
