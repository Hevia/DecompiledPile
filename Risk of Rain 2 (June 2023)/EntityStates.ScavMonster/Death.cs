using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ScavMonster;

public class Death : GenericCharacterDeath
{
	[SerializeField]
	public SpawnCard spawnCard;

	public static float duration;

	private bool shouldDropPack;

	protected override bool shouldAutoDestroy => false;

	public override void OnEnter()
	{
		base.OnEnter();
		if (!NetworkServer.active)
		{
			return;
		}
		CharacterMaster characterMaster = (Object.op_Implicit((Object)(object)base.characterBody) ? base.characterBody.master : null);
		if (Object.op_Implicit((Object)(object)characterMaster))
		{
			bool flag = characterMaster.IsExtraLifePendingServer();
			bool flag2 = characterMaster.inventory.GetItemCount(RoR2Content.Items.Ghost) > 0;
			bool flag3 = !Object.op_Implicit((Object)(object)Stage.instance) || Stage.instance.scavPackDroppedServer;
			shouldDropPack = !flag && !flag2 && !flag3;
			if (shouldDropPack)
			{
				Stage.instance.scavPackDroppedServer = true;
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && base.fixedAge >= duration)
		{
			DestroyBodyAsapServer();
		}
	}

	public override void OnExit()
	{
		DestroyModel();
		base.OnExit();
	}

	protected override void OnPreDestroyBodyServer()
	{
		base.OnPreDestroyBodyServer();
		if (shouldDropPack)
		{
			AttemptDropPack();
		}
	}

	private void AttemptDropPack()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		DirectorCore instance = DirectorCore.instance;
		if (Object.op_Implicit((Object)(object)instance))
		{
			Xoroshiro128Plus rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
			DirectorPlacementRule placementRule = new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
				position = base.characterBody.footPosition,
				minDistance = 0f,
				maxDistance = float.PositiveInfinity
			};
			Object.op_Implicit((Object)(object)instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, rng)));
		}
	}
}
