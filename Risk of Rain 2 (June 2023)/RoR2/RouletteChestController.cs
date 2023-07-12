using System;
using EntityStates;
using JetBrains.Annotations;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(EntityStateMachine))]
[RequireComponent(typeof(PurchaseInteraction))]
public class RouletteChestController : NetworkBehaviour
{
	public struct Entry
	{
		public PickupIndex pickupIndex;

		public Run.FixedTimeStamp endTime;
	}

	private class RouletteChestControllerBaseState : EntityState
	{
		protected RouletteChestController rouletteChestController { get; private set; }

		public override void OnEnter()
		{
			base.OnEnter();
			rouletteChestController = GetComponent<RouletteChestController>();
		}

		public virtual void HandleInteractionServer(Interactor activator)
		{
		}
	}

	private class Idle : RouletteChestControllerBaseState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			PlayAnimation("Body", "Idle");
			base.rouletteChestController.purchaseInteraction.Networkavailable = true;
		}

		public override void HandleInteractionServer(Interactor activator)
		{
			base.HandleInteractionServer(activator);
			outer.SetNextState(new Startup());
		}
	}

	private class Startup : RouletteChestControllerBaseState
	{
		public static float baseDuration;

		public static string soundEntryEvent;

		public override void OnEnter()
		{
			base.OnEnter();
			PlayAnimation("Body", "IdleToActive");
			base.rouletteChestController.purchaseInteraction.Networkavailable = false;
			base.rouletteChestController.purchaseInteraction.costType = CostTypeIndex.None;
			Util.PlaySound(soundEntryEvent, base.gameObject);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && base.fixedAge > baseDuration)
			{
				outer.SetNextState(new Cycling());
			}
		}
	}

	private class Cycling : RouletteChestControllerBaseState
	{
		public static string soundCycleEvent;

		public static string soundCycleSpeedRtpc;

		public static float soundCycleSpeedRtpcScale;

		public override void OnEnter()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Expected O, but got Unknown
			base.OnEnter();
			base.rouletteChestController.onChangedEntryClient.AddListener(new UnityAction(OnChangedEntryClient));
			if (NetworkServer.active)
			{
				base.rouletteChestController.BeginCycleServer();
				base.rouletteChestController.onCycleCompletedServer.AddListener(new UnityAction(OnCycleCompleted));
			}
			base.rouletteChestController.purchaseInteraction.Networkavailable = true;
			base.rouletteChestController.purchaseInteraction.costType = CostTypeIndex.None;
		}

		private void OnCycleCompleted()
		{
			outer.SetNextState(new Opening());
		}

		public override void OnExit()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			base.rouletteChestController.onCycleCompletedServer.RemoveListener(new UnityAction(OnCycleCompleted));
			base.rouletteChestController.onChangedEntryClient.RemoveListener(new UnityAction(OnChangedEntryClient));
			base.OnExit();
		}

		private void OnChangedEntryClient()
		{
			int entryIndexForTime = base.rouletteChestController.GetEntryIndexForTime(Run.FixedTimeStamp.now);
			float num = base.rouletteChestController.CalcEntryDuration(entryIndexForTime);
			PlayAnimation("Body", "ActiveLoop", "ActiveLoop.playbackRate", num);
			float num2 = Util.Remap(num, minTime, minTime + base.rouletteChestController.bonusTime, 1f, 0f);
			Util.PlaySound(soundCycleEvent, base.gameObject, soundCycleSpeedRtpc, num2 * soundCycleSpeedRtpcScale);
		}

		public override void HandleInteractionServer(Interactor activator)
		{
			base.HandleInteractionServer(activator);
			base.rouletteChestController.EndCycleServer(activator);
		}
	}

	private class Opening : RouletteChestControllerBaseState
	{
		public static float baseDuration;

		public static string soundEntryEvent;

		public override void OnEnter()
		{
			base.OnEnter();
			PlayAnimation("Body", "ActiveToOpening");
			base.rouletteChestController.purchaseInteraction.Networkavailable = false;
			base.rouletteChestController.purchaseInteraction.costType = CostTypeIndex.None;
			Util.PlaySound(soundEntryEvent, base.gameObject);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && base.fixedAge > baseDuration)
			{
				outer.SetNextState(new Opened());
			}
		}
	}

	private class Opened : RouletteChestControllerBaseState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			PlayAnimation("Body", "Opened");
			base.rouletteChestController.purchaseInteraction.Networkavailable = false;
			base.rouletteChestController.purchaseInteraction.costType = CostTypeIndex.None;
		}
	}

	public int maxEntries = 1;

	public float bonusTime;

	public AnimationCurve bonusTimeDecay;

	public PickupDropTable dropTable;

	public Transform ejectionTransform;

	public Vector3 localEjectionVelocity;

	public Animator modelAnimator;

	public PickupDisplay pickupDisplay;

	private EntityStateMachine stateMachine;

	private PurchaseInteraction purchaseInteraction;

	private static readonly float averageHgReactionTime = 0.20366667f;

	private static readonly float lowestHgReactionTime = 0.15f;

	private static readonly float recognitionWindow = 0.15f;

	private static readonly float minTime = lowestHgReactionTime + recognitionWindow;

	private static readonly float rewindTime = 0.05f;

	private Run.FixedTimeStamp activationTime = Run.FixedTimeStamp.positiveInfinity;

	private Entry[] entries = Array.Empty<Entry>();

	private Xoroshiro128Plus rng;

	private static readonly uint activationTimeDirtyBit = 1u;

	private static readonly uint entriesDirtyBit = 2u;

	private static readonly uint enabledDirtyBit = 4u;

	private static readonly uint allDirtyBitsMask = activationTimeDirtyBit | entriesDirtyBit;

	private int previousEntryIndexClient = -1;

	public UnityEvent onCycleBeginServer;

	public UnityEvent onCycleCompletedServer;

	public UnityEvent onChangedEntryClient;

	private bool isCycling => !activationTime.isPositiveInfinity;

	private float CalcEntryDuration(int i)
	{
		float num = (float)i / (float)maxEntries;
		return bonusTimeDecay.Evaluate(num) * bonusTime + minTime;
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		uint syncVarDirtyBits = ((NetworkBehaviour)this).syncVarDirtyBits;
		if (initialState)
		{
			syncVarDirtyBits = allDirtyBitsMask;
		}
		writer.WritePackedUInt32(syncVarDirtyBits);
		if ((syncVarDirtyBits & activationTimeDirtyBit) != 0)
		{
			writer.Write(activationTime);
		}
		if ((syncVarDirtyBits & entriesDirtyBit) != 0)
		{
			writer.WritePackedUInt32((uint)entries.Length);
			for (int i = 0; i < entries.Length; i++)
			{
				writer.Write(entries[i].pickupIndex);
			}
		}
		return syncVarDirtyBits != 0;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		uint num = reader.ReadPackedUInt32();
		if ((num & activationTimeDirtyBit) != 0)
		{
			activationTime = reader.ReadFixedTimeStamp();
		}
		if ((num & entriesDirtyBit) != 0)
		{
			Array.Resize(ref entries, (int)reader.ReadPackedUInt32());
			Run.FixedTimeStamp endTime = activationTime;
			for (int i = 0; i < entries.Length; i++)
			{
				ref Entry reference = ref entries[i];
				reference.pickupIndex = reader.ReadPickupIndex();
				reference.endTime = endTime + CalcEntryDuration(i);
				endTime = reference.endTime;
			}
		}
	}

	private void Awake()
	{
		stateMachine = ((Component)this).GetComponent<EntityStateMachine>();
		purchaseInteraction = ((Component)this).GetComponent<PurchaseInteraction>();
	}

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (NetworkServer.active)
		{
			rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
		}
	}

	private void OnEnable()
	{
		((NetworkBehaviour)this).SetDirtyBit(enabledDirtyBit);
		if (Object.op_Implicit((Object)(object)pickupDisplay))
		{
			((Behaviour)pickupDisplay).enabled = true;
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)pickupDisplay))
		{
			pickupDisplay.SetPickupIndex(PickupIndex.none);
			((Behaviour)pickupDisplay).enabled = false;
		}
		((NetworkBehaviour)this).SetDirtyBit(enabledDirtyBit);
	}

	[Server]
	private void GenerateEntriesServer(Run.FixedTimeStamp startTime)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.RouletteChestController::GenerateEntriesServer(RoR2.Run/FixedTimeStamp)' called on client");
			return;
		}
		Array.Resize(ref entries, maxEntries);
		for (int i = 0; i < entries.Length; i++)
		{
			ref Entry reference = ref entries[i];
			reference.endTime = startTime + CalcEntryDuration(i);
			startTime = reference.endTime;
		}
		PickupIndex pickupIndex = PickupIndex.none;
		for (int j = 0; j < entries.Length; j++)
		{
			ref Entry reference2 = ref entries[j];
			PickupIndex pickupIndex2 = dropTable.GenerateDrop(rng);
			if (pickupIndex2 == pickupIndex)
			{
				pickupIndex2 = dropTable.GenerateDrop(rng);
			}
			reference2.pickupIndex = pickupIndex2;
			pickupIndex = pickupIndex2;
		}
		((NetworkBehaviour)this).SetDirtyBit(entriesDirtyBit);
	}

	[Server]
	public void HandleInteractionServer(Interactor activator)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.RouletteChestController::HandleInteractionServer(RoR2.Interactor)' called on client");
		}
		else
		{
			((RouletteChestControllerBaseState)stateMachine.state).HandleInteractionServer(activator);
		}
	}

	[Server]
	private void BeginCycleServer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.RouletteChestController::BeginCycleServer()' called on client");
			return;
		}
		activationTime = Run.FixedTimeStamp.now;
		((NetworkBehaviour)this).SetDirtyBit(activationTimeDirtyBit);
		GenerateEntriesServer(activationTime);
		UnityEvent obj = onCycleBeginServer;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	[Server]
	private void EndCycleServer([CanBeNull] Interactor activator)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.RouletteChestController::EndCycleServer(RoR2.Interactor)' called on client");
			return;
		}
		float num = 0f;
		NetworkUser networkUser;
		if (Object.op_Implicit((Object)(object)activator) && (networkUser = Util.LookUpBodyNetworkUser(((Component)activator).gameObject)) != null)
		{
			num = RttManager.GetConnectionRTT(((NetworkBehaviour)networkUser).connectionToClient);
		}
		Run.FixedTimeStamp time = Run.FixedTimeStamp.now - num - rewindTime;
		PickupIndex pickupIndexForTime = GetPickupIndexForTime(time);
		EjectPickupServer(pickupIndexForTime);
		activationTime = Run.FixedTimeStamp.positiveInfinity;
		onCycleCompletedServer.Invoke();
	}

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)pickupDisplay))
		{
			pickupDisplay.SetPickupIndex(isCycling ? GetPickupIndexForTime(Run.FixedTimeStamp.now) : PickupIndex.none);
		}
		if (NetworkClient.active)
		{
			int entryIndexForTime = GetEntryIndexForTime(Run.FixedTimeStamp.now);
			if (entryIndexForTime != previousEntryIndexClient)
			{
				previousEntryIndexClient = entryIndexForTime;
				onChangedEntryClient.Invoke();
			}
		}
		if (NetworkServer.active && isCycling && entries.Length != 0)
		{
			Run.FixedTimeStamp endTime = entries[entries.Length - 1].endTime;
			if (endTime.hasPassed)
			{
				EndCycleServer(null);
			}
		}
	}

	private int GetEntryIndexForTime(Run.FixedTimeStamp time)
	{
		for (int i = 0; i < entries.Length; i++)
		{
			if (time < entries[i].endTime)
			{
				return i;
			}
		}
		if (entries.Length != 0)
		{
			return entries.Length - 1;
		}
		return -1;
	}

	private PickupIndex GetPickupIndexForTime(Run.FixedTimeStamp time)
	{
		int entryIndexForTime = GetEntryIndexForTime(time);
		if (entryIndexForTime != -1)
		{
			return entries[entryIndexForTime].pickupIndex;
		}
		return PickupIndex.none;
	}

	private void EjectPickupServer(PickupIndex pickupIndex)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!(pickupIndex == PickupIndex.none))
		{
			PickupDropletController.CreatePickupDroplet(pickupIndex, ejectionTransform.position, ejectionTransform.rotation * localEjectionVelocity);
		}
	}

	private void UNetVersion()
	{
	}

	public override void PreStartClient()
	{
	}
}
