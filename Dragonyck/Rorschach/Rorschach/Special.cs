using EntityStates;
using EntityStates.Mage.Weapon;
using RoR2;
using UnityEngine;

namespace Rorschach;

internal class Special : BaseSkillState
{
	private float duration = 3f;

	private ChildLocator childLocator;

	private Transform muzzleTransform;

	private GameObject fireInstance;

	private float stopwatch;

	private float attackDelay;

	private uint ID;

	private Ray aimRay;

	private RorschachRageBarBehaviour behaviour;

	public override void OnEnter()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		((BaseState)this).OnEnter();
		((BaseState)this).StartAimMode(2f, false);
		behaviour = ((EntityState)this).GetComponent<RorschachRageBarBehaviour>();
		if (Object.op_Implicit((Object)(object)behaviour))
		{
			behaviour.canExecute = false;
		}
		attackDelay = 0.2f / ((BaseState)this).attackSpeedStat;
		childLocator = ((EntityState)this).GetModelChildLocator();
		muzzleTransform = childLocator.FindChild("sprayMuzzle");
		fireInstance = Object.Instantiate<GameObject>(Prefabs.sprayFire, muzzleTransform);
		fireInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
		ID = Util.PlaySound(Flamethrower.startAttackSoundString, ((EntityState)this).gameObject);
		aimRay = ((BaseState)this).GetAimRay();
		((EntityState)this).GetModelAnimator().SetBool("skillOver", false);
		((EntityState)this).PlayAnimation("Gesture, Override", "spray");
		((Component)childLocator.FindChild("spraycan")).gameObject.SetActive(true);
	}

	public override void FixedUpdate()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		((EntityState)this).FixedUpdate();
		if (Object.op_Implicit((Object)(object)((EntityState)this).characterDirection))
		{
			Ray val = ((BaseState)this).GetAimRay();
			Vector2 val2 = Util.Vector3XZToVector2XY(((Ray)(ref val)).direction);
			if (val2 != Vector2.zero)
			{
				((Vector2)(ref val2)).Normalize();
				Vector3 val3 = new Vector3(val2.x, 0f, val2.y);
				Vector3 normalized = ((Vector3)(ref val3)).normalized;
				((EntityState)this).characterDirection.moveVector = normalized;
			}
		}
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= attackDelay)
		{
			stopwatch = 0f;
			Fire("handR");
			Fire("handL");
		}
		if (((EntityState)this).fixedAge >= duration && ((EntityState)this).isAuthority)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
		UpdateFlamethrowerEffect();
	}

	private void UpdateFlamethrowerEffect()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Ray val = ((BaseState)this).GetAimRay();
		Vector3 direction = ((Ray)(ref val)).direction;
		if (Object.op_Implicit((Object)(object)fireInstance))
		{
			fireInstance.transform.forward = direction;
		}
	}

	private void Fire(string muzzleString)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		Ray val = ((BaseState)this).GetAimRay();
		if (!((EntityState)this).isAuthority)
		{
			return;
		}
		new BulletAttack
		{
			owner = ((EntityState)this).gameObject,
			weapon = ((EntityState)this).gameObject,
			origin = ((Ray)(ref val)).origin,
			aimVector = ((Ray)(ref val)).direction,
			minSpread = 0f,
			damage = ((BaseState)this).damageStat,
			force = 40f,
			muzzleName = muzzleString,
			hitEffectPrefab = null,
			isCrit = ((BaseState)this).RollCrit(),
			radius = Flamethrower.radius,
			falloffModel = (FalloffModel)0,
			stopperMask = ((LayerIndex)(ref LayerIndex.world)).mask,
			procCoefficient = Flamethrower.procCoefficientPerTick,
			maxDistance = 20f,
			smartCollision = true,
			damageType = (DamageType)(Util.CheckRoll(Flamethrower.ignitePercentChance, ((EntityState)this).characterBody.master) ? 128 : 0),
			hitCallback = (HitCallback)delegate(BulletAttack _bulletAttack, ref BulletHit info)
			{
				//IL_0040: Unknown result type (might be due to invalid IL or missing references)
				//IL_0050: Unknown result type (might be due to invalid IL or missing references)
				bool result = BulletAttack.defaultHitCallback.Invoke(_bulletAttack, ref info);
				HealthComponent val2 = (Object.op_Implicit((Object)(object)info.hitHurtBox) ? info.hitHurtBox.healthComponent : null);
				if (Object.op_Implicit((Object)(object)val2) && val2.alive && info.hitHurtBox.teamIndex != ((EntityState)this).characterBody.teamComponent.teamIndex)
				{
					CharacterMain.AddRage(0.005f);
				}
				return result;
			}
		}.Fire();
		if (Object.op_Implicit((Object)(object)((EntityState)this).characterMotor))
		{
			((EntityState)this).characterMotor.ApplyForce(((Ray)(ref val)).direction * (0f - Flamethrower.recoilForce), false, false);
		}
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
		((Component)childLocator.FindChild("spraycan")).gameObject.SetActive(false);
		if (Object.op_Implicit((Object)(object)behaviour))
		{
			behaviour.canExecute = true;
		}
		if (Object.op_Implicit((Object)(object)fireInstance))
		{
			EntityState.Destroy((Object)(object)fireInstance);
		}
		((EntityState)this).GetModelAnimator().SetBool("skillOver", true);
		AkSoundEngine.StopPlayingID(ID);
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)1;
	}
}
