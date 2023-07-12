using RoR2;
using UnityEngine;

namespace EntityStates.ParentMonster;

public class DeathState : GenericCharacterDeath
{
	[SerializeField]
	public float timeBeforeDestealth = 2.5f;

	[SerializeField]
	public float destealthDuration;

	[SerializeField]
	public Material destealthMaterial;

	[SerializeField]
	public GameObject effectPrefab;

	[SerializeField]
	public string effectMuzzleString;

	private bool destealth;

	protected override bool shouldAutoDestroy
	{
		get
		{
			if (destealth)
			{
				return base.fixedAge > timeBeforeDestealth + destealthDuration;
			}
			return false;
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}

	public override void OnExit()
	{
		DestroyModel();
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > timeBeforeDestealth && !destealth)
		{
			DoDestealth();
		}
		if (destealth && base.fixedAge > timeBeforeDestealth + destealthDuration)
		{
			DestroyModel();
		}
	}

	private void DoDestealth()
	{
		destealth = true;
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, effectMuzzleString, transmit: false);
		}
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			CharacterModel component = ((Component)modelTransform).gameObject.GetComponent<CharacterModel>();
			if (Object.op_Implicit((Object)(object)destealthMaterial))
			{
				TemporaryOverlay temporaryOverlay = ((Component)modelTransform).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = destealthDuration;
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = destealthMaterial;
				temporaryOverlay.inspectorCharacterModel = component;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.animateShaderAlpha = true;
				PrintController component2 = ((Component)base.modelLocator.modelTransform).gameObject.GetComponent<PrintController>();
				((Behaviour)component2).enabled = false;
				component2.printTime = destealthDuration;
				component2.startingPrintHeight = 0f;
				component2.maxPrintHeight = 20f;
				component2.startingPrintBias = 0f;
				component2.maxPrintBias = 2f;
				component2.disableWhenFinished = false;
				component2.printCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
				((Behaviour)component2).enabled = true;
			}
			Transform val = FindModelChild("CoreLight");
			if (Object.op_Implicit((Object)(object)val))
			{
				((Component)val).gameObject.SetActive(false);
			}
		}
	}
}
