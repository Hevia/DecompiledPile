using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidRaidCrab;

public class EscapeDeath : GenericCharacterDeath
{
	[SerializeField]
	public float duration;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	[SerializeField]
	public bool addPrintController;

	[SerializeField]
	public float printDuration;

	[SerializeField]
	public float startingPrintHeight;

	[SerializeField]
	public float maxPrintHeight;

	[SerializeField]
	public string gauntletEntranceChildName;

	[SerializeField]
	public string enterSoundString;

	private Vector3 gauntletEntrancePosition;

	private Transform gauntletEntranceTransform;

	private NetworkInstanceId netId;

	protected override bool shouldAutoDestroy => false;

	protected override void PlayDeathAnimation(float crossfadeDuration)
	{
		PlayCrossfade(animationLayerName, animationStateName, animationPlaybackRateParam, duration, crossfadeDuration);
	}

	public override void OnEnter()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		netId = NetworkInstanceId.Invalid;
		if (Object.op_Implicit((Object)(object)base.characterBody) && Object.op_Implicit((Object)(object)base.characterBody.master))
		{
			netId = ((NetworkBehaviour)base.characterBody.master).netId;
		}
		Util.PlaySound(enterSoundString, base.gameObject);
		if (addPrintController)
		{
			Transform modelTransform = GetModelTransform();
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				PrintController printController = ((Component)modelTransform).gameObject.AddComponent<PrintController>();
				printController.printTime = printDuration;
				((Behaviour)printController).enabled = true;
				printController.startingPrintHeight = startingPrintHeight;
				printController.maxPrintHeight = maxPrintHeight;
				printController.startingPrintBias = 0f;
				printController.maxPrintBias = 0f;
				printController.disableWhenFinished = false;
				printController.printCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			}
		}
		if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
		{
			base.rigidbodyMotor.moveVector = Vector3.zero;
		}
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			gauntletEntranceTransform = modelChildLocator.FindChild(gauntletEntranceChildName);
			RefreshGauntletEntrancePosition();
		}
	}

	public override void OnExit()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			RefreshGauntletEntrancePosition();
			VoidRaidGauntletController.instance.TryOpenGauntlet(gauntletEntrancePosition, netId);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active)
		{
			RefreshGauntletEntrancePosition();
		}
		if (base.isAuthority && base.fixedAge >= duration)
		{
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}

	private void RefreshGauntletEntrancePosition()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)gauntletEntranceTransform))
		{
			gauntletEntrancePosition = gauntletEntranceTransform.position;
		}
	}
}
