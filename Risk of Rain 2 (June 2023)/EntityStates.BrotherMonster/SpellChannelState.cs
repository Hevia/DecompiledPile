using RoR2;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace EntityStates.BrotherMonster;

public class SpellChannelState : SpellBaseState
{
	public static float stealInterval;

	public static float delayBeforeBeginningSteal;

	public static float maxDuration;

	public static GameObject channelEffectPrefab;

	private bool hasBegunSteal;

	private GameObject channelEffectInstance;

	private Transform spellChannelChildTransform;

	private bool hasSubscribedToStealFinish;

	protected override bool DisplayWeapon => false;

	public override void OnEnter()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayAnimation("Body", "SpellChannel");
		Util.PlaySound("Play_moonBrother_phase4_itemSuck_start", base.gameObject);
		spellChannelChildTransform = FindModelChild("SpellChannel");
		if (Object.op_Implicit((Object)(object)spellChannelChildTransform))
		{
			channelEffectInstance = Object.Instantiate<GameObject>(channelEffectPrefab, spellChannelChildTransform.position, Quaternion.identity, spellChannelChildTransform);
		}
		base.characterBody.AddBuff(RoR2Content.Buffs.Immune);
	}

	public override void FixedUpdate()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!Object.op_Implicit((Object)(object)itemStealController))
		{
			return;
		}
		if (!hasSubscribedToStealFinish && base.isAuthority)
		{
			hasSubscribedToStealFinish = true;
			if (NetworkServer.active)
			{
				itemStealController.onStealFinishServer.AddListener(new UnityAction(OnStealEndAuthority));
			}
			else
			{
				itemStealController.onStealFinishClient += OnStealEndAuthority;
			}
		}
		if (NetworkServer.active && base.fixedAge > delayBeforeBeginningSteal && !hasBegunSteal)
		{
			hasBegunSteal = true;
			itemStealController.stealInterval = stealInterval;
			TeamIndex teamIndex = GetTeam();
			itemStealController.StartSteal((CharacterMaster characterMaster) => characterMaster.teamIndex != teamIndex && characterMaster.hasBody);
		}
		if (base.isAuthority && base.fixedAge > delayBeforeBeginningSteal + maxDuration)
		{
			outer.SetNextState(new SpellChannelExitState());
		}
		if (Object.op_Implicit((Object)(object)spellChannelChildTransform))
		{
			((Component)itemStealController).transform.position = spellChannelChildTransform.position;
		}
	}

	public override void OnExit()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)itemStealController) && hasSubscribedToStealFinish)
		{
			itemStealController.onStealFinishServer.RemoveListener(new UnityAction(OnStealEndAuthority));
			itemStealController.onStealFinishClient -= OnStealEndAuthority;
		}
		if (Object.op_Implicit((Object)(object)channelEffectInstance))
		{
			EntityState.Destroy((Object)(object)channelEffectInstance);
		}
		Util.PlaySound("Play_moonBrother_phase4_itemSuck_end", base.gameObject);
		base.characterBody.RemoveBuff(RoR2Content.Buffs.Immune);
		base.OnExit();
	}

	private void OnStealEndAuthority()
	{
		outer.SetNextState(new SpellChannelExitState());
	}
}
