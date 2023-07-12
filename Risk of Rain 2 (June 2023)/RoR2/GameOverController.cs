using System.Collections.Generic;
using HG;
using JetBrains.Annotations;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(VoteController))]
public class GameOverController : NetworkBehaviour
{
	[Tooltip("The prefab to use for the end-of-game report panel.")]
	public GameObject gameEndReportPanelPrefab;

	public float appearanceDelay = 1f;

	private VoteController voteController;

	private Dictionary<HUD, GameEndReportPanelController> reportPanels = new Dictionary<HUD, GameEndReportPanelController>();

	private const uint runReportDirtyBit = 1u;

	private const uint allDirtyBits = 1u;

	private static int kRpcRpcClientGameOver;

	public static GameOverController instance { get; private set; }

	public bool shouldDisplayGameEndReportPanels { get; set; }

	public RunReport runReport { get; private set; }

	[Server]
	public void SetRunReport([NotNull] RunReport newRunReport)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.GameOverController::SetRunReport(RoR2.RunReport)' called on client");
			return;
		}
		((NetworkBehaviour)this).SetDirtyBit(1u);
		runReport = newRunReport;
		if (Object.op_Implicit((Object)(object)runReport.gameEnding))
		{
			EntityStateMachine.FindByCustomName(((Component)this).gameObject, "Main").initialStateType = runReport.gameEnding.gameOverControllerState;
		}
	}

	private int FindPlayerIndex(LocalUser localUser)
	{
		int i = 0;
		for (int playerInfoCount = runReport.playerInfoCount; i < playerInfoCount; i++)
		{
			if (runReport.GetPlayerInfo(i).localUser == localUser)
			{
				return i;
			}
		}
		Debug.Log((object)"Found no valid player index. Falling back to 0.");
		return 0;
	}

	private void UpdateReportScreens()
	{
		if (shouldDisplayGameEndReportPanels)
		{
			int i = 0;
			for (int count = HUD.readOnlyInstanceList.Count; i < count; i++)
			{
				HUD hUD = HUD.readOnlyInstanceList[i];
				if (!reportPanels.ContainsKey(hUD))
				{
					reportPanels[hUD] = GenerateReportScreen(hUD);
				}
			}
		}
		List<HUD> list = CollectionPool<HUD, List<HUD>>.RentCollection();
		foreach (HUD key in reportPanels.Keys)
		{
			if (!Object.op_Implicit((Object)(object)key) || !shouldDisplayGameEndReportPanels)
			{
				list.Add(key);
			}
		}
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			reportPanels.Remove(list[j]);
		}
		CollectionPool<HUD, List<HUD>>.ReturnCollection(list);
	}

	private GameEndReportPanelController GenerateReportScreen(HUD hud)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		LocalUser localUser = hud.localUserViewer;
		GameObject obj = Object.Instantiate<GameObject>(gameEndReportPanelPrefab, ((Component)hud).transform);
		obj.transform.parent = ((Component)hud).transform;
		obj.GetComponent<MPEventSystemProvider>().eventSystem = localUser.eventSystem;
		GameEndReportPanelController component = obj.GetComponent<GameEndReportPanelController>();
		GameEndReportPanelController.DisplayData displayData = default(GameEndReportPanelController.DisplayData);
		displayData.runReport = runReport;
		displayData.playerIndex = FindPlayerIndex(localUser);
		GameEndReportPanelController.DisplayData displayData2 = displayData;
		component.SetDisplayData(displayData2);
		component.SetContinueButtonAction((UnityAction)delegate
		{
			if (Object.op_Implicit((Object)(object)localUser.currentNetworkUser))
			{
				localUser.currentNetworkUser.CallCmdSubmitVote(((Component)voteController).gameObject, 0);
			}
		});
		GameObject obj2 = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/VoteInfoPanel"), (Transform)(RectTransform)((Component)component.continueButton).transform.parent);
		obj2.transform.SetAsFirstSibling();
		obj2.GetComponent<VoteInfoPanelController>().voteController = voteController;
		return component;
	}

	private void Awake()
	{
		runReport = new RunReport();
		voteController = ((Component)this).GetComponent<VoteController>();
	}

	private void OnEnable()
	{
		instance = SingletonHelper.Assign<GameOverController>(instance, this);
	}

	private void OnDisable()
	{
		instance = SingletonHelper.Unassign<GameOverController>(instance, this);
	}

	private void Update()
	{
		UpdateReportScreens();
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		uint num = ((NetworkBehaviour)this).syncVarDirtyBits;
		if (initialState)
		{
			num = 1u;
		}
		bool num2 = (num & 1) != 0;
		if (!initialState)
		{
			writer.Write((byte)num);
		}
		if (num2)
		{
			runReport.Write(writer);
		}
		if (!initialState)
		{
			return num != 0;
		}
		return false;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (((uint)(initialState ? 1 : reader.ReadByte()) & (true ? 1u : 0u)) != 0)
		{
			runReport.Read(reader);
		}
	}

	[ClientRpc]
	public void RpcClientGameOver()
	{
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			Run.instance.OnClientGameOver(runReport);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcClientGameOver(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcClientGameOver called on server.");
		}
		else
		{
			((GameOverController)(object)obj).RpcClientGameOver();
		}
	}

	public void CallRpcClientGameOver()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcClientGameOver called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcClientGameOver);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcClientGameOver");
	}

	static GameOverController()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kRpcRpcClientGameOver = 1518660169;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameOverController), kRpcRpcClientGameOver, new CmdDelegate(InvokeRpcRpcClientGameOver));
		NetworkCRC.RegisterBehaviour("GameOverController", 0);
	}

	public override void PreStartClient()
	{
	}
}
