using System;
using System.Collections.Generic;
using HG;
using RoR2.ExpansionManagement;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class BazaarController : MonoBehaviour
{
	[Serializable]
	public struct SeerSceneOverride
	{
		[SerializeField]
		public SceneDef sceneDef;

		[SerializeField]
		public ExpansionDef requiredExpasion;

		[Range(0f, 1f)]
		[SerializeField]
		public float overrideChance;

		[SerializeField]
		public int minStagesCleared;

		[SerializeField]
		public string bannedEventFlag;
	}

	public GameObject shopkeeper;

	public TextMeshPro shopkeeperChat;

	public float shopkeeperTrackDistance = 250f;

	public float shopkeeperTrackAngle = 120f;

	[Tooltip("Any PurchaseInteraction objects here will have their activation state set based on whether or not the specified unlockable is available.")]
	public PurchaseInteraction[] unlockableTerminals;

	public SeerStationController[] seerStations;

	public SeerSceneOverride[] seerSceneOverrides;

	private InputBankTest shopkeeperInputBank;

	private CharacterBody shopkeeperTargetBody;

	private Xoroshiro128Plus rng;

	public static BazaarController instance { get; private set; }

	private void Awake()
	{
		instance = SingletonHelper.Assign<BazaarController>(instance, this);
	}

	private void Start()
	{
		if (NetworkServer.active)
		{
			OnStartServer();
		}
	}

	private void OnStartServer()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
		SetUpSeerStations();
	}

	private void SetUpSeerStations()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		SceneDef nextStageScene = Run.instance.nextStageScene;
		List<SceneDef> list = new List<SceneDef>();
		if ((Object)(object)nextStageScene != (Object)null)
		{
			int stageOrder = nextStageScene.stageOrder;
			Enumerator<SceneDef> enumerator = SceneCatalog.allSceneDefs.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SceneDef current = enumerator.Current;
					if (current.stageOrder == stageOrder && ((Object)(object)current.requiredExpansion == (Object)null || Run.instance.IsExpansionEnabled(current.requiredExpansion)))
					{
						list.Add(current);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		}
		WeightedSelection<SceneDef> weightedSelection = new WeightedSelection<SceneDef>();
		float num = 0f;
		SeerSceneOverride[] array = seerSceneOverrides;
		for (int i = 0; i < array.Length; i++)
		{
			SeerSceneOverride seerSceneOverride = array[i];
			bool num2 = Run.instance.stageClearCount >= seerSceneOverride.minStagesCleared;
			bool flag = (Object)(object)seerSceneOverride.requiredExpasion == (Object)null || Run.instance.IsExpansionEnabled(seerSceneOverride.requiredExpasion);
			bool flag2 = string.IsNullOrEmpty(seerSceneOverride.bannedEventFlag) || !Run.instance.GetEventFlag(seerSceneOverride.bannedEventFlag);
			if (num2 && flag && flag2)
			{
				weightedSelection.AddChoice(seerSceneOverride.sceneDef, seerSceneOverride.overrideChance);
				num += seerSceneOverride.overrideChance;
			}
		}
		SeerStationController[] array2 = seerStations;
		foreach (SeerStationController seerStationController in array2)
		{
			if (list.Count == 0)
			{
				((Component)seerStationController).GetComponent<PurchaseInteraction>().SetAvailable(newAvailable: false);
				continue;
			}
			Util.ShuffleList(list, rng);
			int index = list.Count - 1;
			SceneDef targetScene = list[index];
			list.RemoveAt(index);
			if (rng.nextNormalizedFloat < num)
			{
				targetScene = weightedSelection.Evaluate(rng.nextNormalizedFloat);
			}
			seerStationController.SetTargetScene(targetScene);
		}
	}

	private void OnDestroy()
	{
		instance = SingletonHelper.Unassign<BazaarController>(instance, this);
	}

	public void CommentOnAnnoy()
	{
		int num = 6;
		if (Util.CheckRoll(20f))
		{
			Chat.SendBroadcastChat(new Chat.NpcChatMessage
			{
				sender = shopkeeper,
				baseToken = "NEWT_ANNOY_" + Random.Range(1, num)
			});
		}
	}

	public void CommentOnEnter()
	{
	}

	public void CommentOnLeaving()
	{
	}

	public void CommentOnLunarPurchase()
	{
		int num = 8;
		if (Util.CheckRoll(20f))
		{
			Chat.SendBroadcastChat(new Chat.NpcChatMessage
			{
				sender = shopkeeper,
				baseToken = "NEWT_LUNAR_PURCHASE_" + Random.Range(1, num)
			});
		}
	}

	public void CommentOnBlueprintPurchase()
	{
	}

	public void CommentOnDronePurchase()
	{
	}

	public void CommentOnUpgrade()
	{
		int num = 3;
		if (Util.CheckRoll(100f))
		{
			Chat.SendBroadcastChat(new Chat.NpcChatMessage
			{
				sender = shopkeeper,
				baseToken = "NEWT_UPGRADE_" + Random.Range(1, num)
			});
		}
	}

	private void Update()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)shopkeeper))
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)shopkeeperInputBank))
		{
			shopkeeperInputBank = shopkeeper.GetComponent<InputBankTest>();
			return;
		}
		Ray aimRay = default(Ray);
		aimRay._002Ector(shopkeeperInputBank.aimOrigin, shopkeeper.transform.forward);
		shopkeeperTargetBody = Util.GetEnemyEasyTarget(shopkeeper.GetComponent<CharacterBody>(), aimRay, shopkeeperTrackDistance, shopkeeperTrackAngle);
		if (Object.op_Implicit((Object)(object)shopkeeperTargetBody))
		{
			Vector3 direction = ((Component)shopkeeperTargetBody.mainHurtBox).transform.position - aimRay.origin;
			aimRay.direction = direction;
		}
		shopkeeperInputBank.aimDirection = aimRay.direction;
	}
}
