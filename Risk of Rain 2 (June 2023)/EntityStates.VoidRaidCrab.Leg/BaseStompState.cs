using RoR2;
using UnityEngine;

namespace EntityStates.VoidRaidCrab.Leg;

public abstract class BaseStompState : BaseLegState
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public string animName;

	public static GameObject warningIndicatorPrefab;

	public GameObject target;

	protected float duration;

	private bool lifetimeExpiredAuthority;

	private RayAttackIndicator warningIndicatorInstance;

	protected virtual bool shouldUseWarningIndicator => false;

	protected virtual bool shouldUpdateLegStompTargetPosition => false;

	public override void OnEnter()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration;
		string stompPlaybackRateParam = base.legController.stompPlaybackRateParam;
		if (!string.IsNullOrEmpty(stompPlaybackRateParam))
		{
			EntityState.PlayAnimationOnAnimator(base.legController.animator, base.legController.primaryLayerName, animName, stompPlaybackRateParam, duration);
		}
		else
		{
			EntityState.PlayAnimationOnAnimator(base.legController.animator, base.legController.primaryLayerName, animName);
			int layerIndex = base.legController.animator.GetLayerIndex(base.legController.primaryLayerName);
			AnimatorStateInfo currentAnimatorStateInfo = base.legController.animator.GetCurrentAnimatorStateInfo(layerIndex);
			duration = ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).length;
		}
		SetWarningIndicatorActive(shouldUseWarningIndicator);
	}

	public override void OnExit()
	{
		SetWarningIndicatorActive(newWarningIndicatorActive: false);
		base.OnExit();
	}

	public override void ModifyNextState(EntityState nextState)
	{
		base.ModifyNextState(nextState);
		if (nextState is BaseStompState baseStompState)
		{
			baseStompState.warningIndicatorInstance = warningIndicatorInstance;
			warningIndicatorInstance = null;
		}
	}

	public override void FixedUpdate()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.legController.mainBody.hasEffectiveAuthority && base.fixedAge >= duration && !lifetimeExpiredAuthority)
		{
			lifetimeExpiredAuthority = true;
			OnLifetimeExpiredAuthority();
		}
		if (shouldUpdateLegStompTargetPosition && Object.op_Implicit((Object)(object)target))
		{
			base.legController.SetStompTargetWorldPosition(target.transform.position);
		}
		UpdateWarningIndicatorInstance();
	}

	protected abstract void OnLifetimeExpiredAuthority();

	protected void SetWarningIndicatorActive(bool newWarningIndicatorActive)
	{
		if (Object.op_Implicit((Object)(object)warningIndicatorInstance) == newWarningIndicatorActive)
		{
			return;
		}
		if (newWarningIndicatorActive)
		{
			GameObject val = Object.Instantiate<GameObject>(warningIndicatorPrefab);
			warningIndicatorInstance = val.GetComponent<RayAttackIndicator>();
			UpdateWarningIndicatorInstance();
			return;
		}
		if (Object.op_Implicit((Object)(object)warningIndicatorInstance))
		{
			EntityState.Destroy((Object)(object)((Component)warningIndicatorInstance).gameObject);
		}
		warningIndicatorInstance = null;
	}

	private void UpdateWarningIndicatorInstance()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)warningIndicatorInstance))
		{
			Vector3 position = base.legController.toeTipTransform.position;
			Vector3 val = (Object.op_Implicit((Object)(object)base.legController.mainBody) ? ((Component)base.legController.mainBody).transform.position : position);
			warningIndicatorInstance.attackRay = new Ray(position, Vector3.down);
			warningIndicatorInstance.attackRange = position.y - val.y;
			warningIndicatorInstance.attackRadius = Stomp.blastRadius;
			warningIndicatorInstance.layerMask = LayerIndex.world.mask;
		}
	}
}
