using RoR2;
using UnityEngine;

namespace EntityStates.GrandParentBoss;

public class SpawnState : GenericCharacterSpawnState
{
	public static GameObject preSpawnEffect;

	public static string preSpawnEffectMuzzle;

	public static float preSpawnDuration;

	public static GameObject spawnEffect;

	public static string spawnEffectMuzzle;

	public static Material spawnOverlayMaterial;

	public static float spawnOverlayDuration;

	public static float blastAttackRadius;

	public static float blastAttackForce;

	public static float blastAttackBonusForce;

	private bool hasSpawned;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	private bool preSpawnIsDone;

	private bool isInvisible;

	public override void OnEnter()
	{
		base.OnEnter();
		characterModel = ((Component)base.modelLocator.modelTransform).GetComponent<CharacterModel>();
		hurtboxGroup = ((Component)base.modelLocator.modelTransform).GetComponent<HurtBoxGroup>();
		ToggleInvisibility(newInvisible: true);
		if (Object.op_Implicit((Object)(object)preSpawnEffect))
		{
			EffectManager.SimpleMuzzleFlash(preSpawnEffect, base.gameObject, preSpawnEffectMuzzle, transmit: false);
		}
	}

	public override void FixedUpdate()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!preSpawnIsDone && base.fixedAge > preSpawnDuration)
		{
			preSpawnIsDone = true;
			ToggleInvisibility(newInvisible: false);
			PlayAnimation("Body", "Spawn1", "Spawn1.playbackRate", duration - preSpawnDuration);
			if (Object.op_Implicit((Object)(object)spawnEffect))
			{
				EffectManager.SimpleMuzzleFlash(spawnEffect, base.gameObject, spawnEffectMuzzle, transmit: false);
			}
			if (base.isAuthority)
			{
				BlastAttack obj = new BlastAttack
				{
					attacker = base.gameObject,
					inflictor = base.gameObject
				};
				obj.teamIndex = TeamComponent.GetObjectTeam(obj.attacker);
				obj.position = base.characterBody.corePosition;
				obj.procCoefficient = 1f;
				obj.radius = blastAttackRadius;
				obj.baseForce = blastAttackForce;
				obj.bonusForce = Vector3.up * blastAttackBonusForce;
				obj.baseDamage = 0f;
				obj.falloffModel = BlastAttack.FalloffModel.Linear;
				obj.damageColorIndex = DamageColorIndex.Item;
				obj.attackerFiltering = AttackerFiltering.NeverHitSelf;
				obj.Fire();
			}
			if (Object.op_Implicit((Object)(object)characterModel) && Object.op_Implicit((Object)(object)spawnOverlayMaterial))
			{
				TemporaryOverlay temporaryOverlay = ((Component)characterModel).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = spawnOverlayDuration;
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = spawnOverlayMaterial;
				temporaryOverlay.inspectorCharacterModel = characterModel;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.animateShaderAlpha = true;
			}
		}
	}

	private void ToggleInvisibility(bool newInvisible)
	{
		if (isInvisible != newInvisible)
		{
			isInvisible = newInvisible;
			if (Object.op_Implicit((Object)(object)characterModel))
			{
				characterModel.invisibilityCount += (isInvisible ? 1 : (-1));
			}
			if (Object.op_Implicit((Object)(object)hurtboxGroup))
			{
				hurtboxGroup.hurtBoxesDeactivatorCounter += (isInvisible ? 1 : (-1));
			}
		}
	}

	public override void OnExit()
	{
		ToggleInvisibility(newInvisible: false);
		base.OnExit();
	}
}
