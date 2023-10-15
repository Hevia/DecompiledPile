using System;
using EntityStates.Railgunner.Reload;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Railgunner.Weapon;

public abstract class BaseFireSnipe : GenericBulletBaseState, IBaseWeaponState
{
	private const string reloadStateMachineName = "Reload";

	private const string backpackStateMachineName = "Backpack";

	[SerializeField]
	public GameObject crosshairOverridePrefab;

	[SerializeField]
	public bool useSecondaryStocks;

	[SerializeField]
	public bool queueReload;

	[Header("Projectile")]
	[SerializeField]
	public float critDamageMultiplier;

	[SerializeField]
	public float selfKnockbackForce;

	[SerializeField]
	public bool isPiercing;

	[SerializeField]
	public float piercingDamageCoefficientPerTarget;

	[Header("Animation")]
	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	private bool wasMiss;

	private bool wasAtLeastOneWeakpoint;

	public static event Action<DamageInfo> onWeakPointHit;

	public static event Action onWeakPointMissed;

	public static event Action<BaseFireSnipe> onFireSnipe;

	public override void OnEnter()
	{
		wasMiss = true;
		BaseFireSnipe.onFireSnipe?.Invoke(this);
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)crosshairOverridePrefab))
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
		}
		if (base.isAuthority && useSecondaryStocks && Object.op_Implicit((Object)(object)base.skillLocator) && Object.op_Implicit((Object)(object)base.skillLocator.secondary))
		{
			base.skillLocator.secondary.DeductStock(1);
		}
	}

	public override void OnExit()
	{
		if (base.isAuthority && (wasMiss || (!wasAtLeastOneWeakpoint && !wasMiss)))
		{
			BaseFireSnipe.onWeakPointMissed?.Invoke();
		}
		crosshairOverrideRequest?.Dispose();
		base.OnExit();
	}

	protected override void ModifyBullet(BulletAttack bulletAttack)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		bulletAttack.sniper = true;
		bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
		EntityStateMachine entityStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Reload");
		if (Object.op_Implicit((Object)(object)entityStateMachine))
		{
			if (entityStateMachine.state is Boosted boosted)
			{
				bulletAttack.damage += boosted.GetBonusDamage();
				boosted.ConsumeBoost(queueReload);
			}
			else if (queueReload && entityStateMachine.state is Waiting waiting)
			{
				waiting.QueueReload();
			}
		}
		if (isPiercing)
		{
			bulletAttack.stopperMask = LayerIndex.world.mask;
		}
		bulletAttack.modifyOutgoingDamageCallback = delegate(BulletAttack _bulletAttack, ref BulletAttack.BulletHit hitInfo, DamageInfo damageInfo)
		{
			_bulletAttack.damage *= piercingDamageCoefficientPerTarget;
			wasMiss = false;
			if (damageInfo.crit)
			{
				damageInfo.damage *= critDamageMultiplier;
				BaseFireSnipe.onWeakPointHit?.Invoke(damageInfo);
				wasAtLeastOneWeakpoint = true;
			}
		};
		EntityStateMachine entityStateMachine2 = EntityStateMachine.FindByCustomName(base.gameObject, "Backpack");
		EntityState entityState = InstantiateBackpackState();
		if (Object.op_Implicit((Object)(object)entityStateMachine2) && entityState != null)
		{
			entityStateMachine2.SetNextState(entityState);
		}
	}

	protected override void OnFireBulletAuthority(Ray aimRay)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		base.characterBody.characterMotor.ApplyForce((0f - selfKnockbackForce) * aimRay.direction);
	}

	protected override void PlayFireAnimation()
	{
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
	}

	public bool CanScope()
	{
		return true;
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Frozen;
	}

	protected virtual EntityState InstantiateBackpackState()
	{
		return null;
	}
}
