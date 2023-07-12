using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidRaidCrab;

public class DeathState : GenericCharacterDeath
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
	public GameObject initialEffectPrefab;

	[SerializeField]
	public string initialEffectMuzzle;

	[SerializeField]
	public GameObject explosionEffectPrefab;

	[SerializeField]
	public string explosionEffectMuzzle;

	[SerializeField]
	public Vector3 ragdollForce;

	[SerializeField]
	public float explosionForce;

	[SerializeField]
	public bool addPrintController;

	[SerializeField]
	public float printDuration;

	[SerializeField]
	public float startingPrintBias;

	[SerializeField]
	public float maxPrintBias;

	private Transform modelTransform;

	protected override bool shouldAutoDestroy => false;

	protected override void PlayDeathAnimation(float crossfadeDuration)
	{
		PlayCrossfade(animationLayerName, animationStateName, animationPlaybackRateParam, duration, crossfadeDuration);
	}

	public override void OnEnter()
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)VoidRaidGauntletController.instance))
		{
			VoidRaidGauntletController.instance.SetCurrentDonutCombatDirectorEnabled(isEnabled: false);
		}
		modelTransform = GetModelTransform();
		Transform val = FindModelChild("StandableSurface");
		if (Object.op_Implicit((Object)(object)val))
		{
			((Component)val).gameObject.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)explosionEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(initialEffectPrefab, base.gameObject, initialEffectMuzzle, transmit: false);
		}
		if (addPrintController)
		{
			PrintController printController = ((Component)modelTransform).gameObject.AddComponent<PrintController>();
			printController.printTime = printDuration;
			((Behaviour)printController).enabled = true;
			printController.startingPrintHeight = 99f;
			printController.maxPrintHeight = 99f;
			printController.startingPrintBias = startingPrintBias;
			printController.maxPrintBias = maxPrintBias;
			printController.disableWhenFinished = false;
			printController.printCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		}
		if (Object.op_Implicit((Object)(object)base.rigidbodyMotor))
		{
			base.rigidbodyMotor.moveVector = Vector3.zero;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && base.fixedAge >= duration)
		{
			if (Object.op_Implicit((Object)(object)explosionEffectPrefab))
			{
				EffectManager.SimpleMuzzleFlash(explosionEffectPrefab, base.gameObject, explosionEffectMuzzle, transmit: true);
			}
			DestroyBodyAsapServer();
		}
	}

	public override void OnExit()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			RagdollController component = ((Component)modelTransform).GetComponent<RagdollController>();
			Rigidbody component2 = GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component2))
			{
				component.BeginRagdoll(ragdollForce);
			}
			ExplodeRigidbodiesOnStart component3 = ((Component)modelTransform).GetComponent<ExplodeRigidbodiesOnStart>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				component3.force = explosionForce;
				((Behaviour)component3).enabled = true;
			}
		}
	}
}
