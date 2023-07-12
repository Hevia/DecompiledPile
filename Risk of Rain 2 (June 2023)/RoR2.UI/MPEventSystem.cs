using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RewiredStandaloneInputModule))]
public class MPEventSystem : EventSystem
{
	public enum InputSource
	{
		MouseAndKeyboard,
		Gamepad
	}

	private struct PushInfo
	{
		public int index;

		public Vector2 position;

		public Vector2 pushVector;
	}

	private static readonly List<MPEventSystem> instancesList;

	public static ReadOnlyCollection<MPEventSystem> readOnlyInstancesList;

	public int playerSlot = -1;

	[NonSerialized]
	public bool allowCursorPush = true;

	[NonSerialized]
	public bool isCombinedEventSystem;

	private CursorIndicatorController cursorIndicatorController;

	[NotNull]
	public Player player;

	[CanBeNull]
	public LocalUser localUser;

	public TooltipProvider currentTooltipProvider;

	public TooltipController currentTooltip;

	private static PushInfo[] pushInfos;

	private const float radius = 24f;

	private const float invRadius = 1f / 24f;

	private const float radiusSqr = 576f;

	private const float pushFactor = 10f;

	public static int activeCount { get; private set; }

	public int cursorOpenerCount { get; set; }

	public int cursorOpenerForGamepadCount { get; set; }

	public bool isHovering
	{
		get
		{
			if (Object.op_Implicit((Object)(object)((EventSystem)this).currentInputModule))
			{
				return ((MPInputModule)(object)((EventSystem)this).currentInputModule).isHovering;
			}
			return false;
		}
	}

	public bool isCursorVisible => ((Component)cursorIndicatorController).gameObject.activeInHierarchy;

	public InputSource currentInputSource { get; private set; }

	public InputMapperHelper inputMapperHelper { get; private set; }

	public static MPEventSystem FindByPlayer(Player player)
	{
		foreach (MPEventSystem instances in instancesList)
		{
			if (instances.player == player)
			{
				return instances;
			}
		}
		return null;
	}

	protected override void Update()
	{
		EventSystem current = EventSystem.current;
		EventSystem.current = (EventSystem)(object)this;
		((EventSystem)this).Update();
		EventSystem.current = current;
		ValidateCurrentSelectedGameobject();
		if (player.GetButtonDown(25) && (PauseScreenController.instancesList.Count == 0 || SimpleDialogBox.instancesList.Count == 0))
		{
			Console.instance.SubmitCmd(null, "pause");
		}
	}

	protected override void Awake()
	{
		((UIBehaviour)this).Awake();
		instancesList.Add(this);
		cursorIndicatorController = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/CursorIndicator"), ((Component)this).transform).GetComponent<CursorIndicatorController>();
		inputMapperHelper = new InputMapperHelper(this);
	}

	private void ValidateCurrentSelectedGameobject()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)((EventSystem)this).currentSelectedGameObject))
		{
			return;
		}
		MPButton component = ((EventSystem)this).currentSelectedGameObject.GetComponent<MPButton>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		if (component.CanBeSelected())
		{
			if (currentInputSource != InputSource.Gamepad)
			{
				return;
			}
			Navigation navigation = ((Selectable)component).navigation;
			if ((int)((Navigation)(ref navigation)).mode != 0)
			{
				return;
			}
		}
		((EventSystem)this).SetSelectedGameObject((GameObject)null);
	}

	public void SetSelectedObject(GameObject o)
	{
		((EventSystem)this).SetSelectedGameObject(o);
	}

	private static void OnActiveSceneChanged(Scene scene1, Scene scene2)
	{
		RecenterCursors();
	}

	private static void RecenterCursors()
	{
		foreach (MPEventSystem instances in instancesList)
		{
			if (instances.currentInputSource == InputSource.Gamepad && Object.op_Implicit((Object)(object)((EventSystem)instances).currentInputModule))
			{
				((MPInput)(object)((EventSystem)instances).currentInputModule.input).CenterCursor();
			}
		}
	}

	protected override void OnDestroy()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		player.controllers.RemoveLastActiveControllerChangedDelegate(new PlayerActiveControllerChangedDelegate(OnLastActiveControllerChanged));
		instancesList.Remove(this);
		inputMapperHelper.Dispose();
		((UIBehaviour)this).OnDestroy();
	}

	protected override void Start()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		((UIBehaviour)this).Start();
		SetCursorIndicatorEnabled(cursorIndicatorEnabled: false);
		if (Object.op_Implicit((Object)(object)((EventSystem)this).currentInputModule) && Object.op_Implicit((Object)(object)((EventSystem)this).currentInputModule.input))
		{
			((MPInput)(object)((EventSystem)this).currentInputModule.input).CenterCursor();
		}
		player.controllers.AddLastActiveControllerChangedDelegate(new PlayerActiveControllerChangedDelegate(OnLastActiveControllerChanged));
		OnLastActiveControllerChanged(player, player.controllers.GetLastActiveController());
	}

	protected override void OnEnable()
	{
		((EventSystem)this).OnEnable();
		activeCount++;
	}

	protected override void OnDisable()
	{
		SetCursorIndicatorEnabled(cursorIndicatorEnabled: false);
		((EventSystem)this).OnDisable();
		activeCount--;
	}

	protected void LateUpdate()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (Object.op_Implicit((Object)(object)((EventSystem)this).currentInputModule) && Object.op_Implicit((Object)(object)((EventSystem)this).currentInputModule.input))
		{
			flag = ((currentInputSource != InputSource.Gamepad) ? (cursorOpenerCount > 0) : (cursorOpenerForGamepadCount > 0));
		}
		SetCursorIndicatorEnabled(flag);
		MPInputModule mPInputModule = ((EventSystem)this).currentInputModule as MPInputModule;
		if (flag)
		{
			CursorIndicatorController.CursorSet cursorSet = cursorIndicatorController.noneCursorSet;
			switch (currentInputSource)
			{
			case InputSource.MouseAndKeyboard:
				cursorSet = cursorIndicatorController.mouseCursorSet;
				break;
			case InputSource.Gamepad:
				cursorSet = cursorIndicatorController.gamepadCursorSet;
				break;
			}
			cursorIndicatorController.SetCursor(cursorSet, (!isHovering) ? CursorIndicatorController.CursorImage.Pointer : CursorIndicatorController.CursorImage.Hover, GetColor());
			cursorIndicatorController.SetPosition(((BaseInputModule)mPInputModule).input.mousePosition);
		}
	}

	private void OnLastActiveControllerChanged(Player player, Controller controller)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected I4, but got Unknown
		if (controller != null)
		{
			ControllerType type = controller.type;
			switch ((int)type)
			{
			case 0:
				currentInputSource = InputSource.MouseAndKeyboard;
				break;
			case 1:
				currentInputSource = InputSource.MouseAndKeyboard;
				break;
			case 2:
				currentInputSource = InputSource.Gamepad;
				break;
			}
		}
	}

	private void SetCursorIndicatorEnabled(bool cursorIndicatorEnabled)
	{
		if (((Component)cursorIndicatorController).gameObject.activeSelf != cursorIndicatorEnabled)
		{
			((Component)cursorIndicatorController).gameObject.SetActive(cursorIndicatorEnabled);
			if (cursorIndicatorEnabled)
			{
				((MPInput)(object)((BaseInputModule)(MPInputModule)(object)((EventSystem)this).currentInputModule).input).CenterCursor();
			}
		}
	}

	public Color GetColor()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (activeCount <= 1)
		{
			return Color.white;
		}
		return ColorCatalog.GetMultiplayerColor(playerSlot);
	}

	public bool GetCursorPosition(out Vector2 position)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)((EventSystem)this).currentInputModule))
		{
			position = ((EventSystem)this).currentInputModule.input.mousePosition;
			return true;
		}
		position = Vector2.zero;
		return false;
	}

	public Rect GetScreenRect()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		CameraRigController cameraRigController = localUser?.cameraRigController;
		if (!Object.op_Implicit((Object)(object)cameraRigController))
		{
			return new Rect(0f, 0f, (float)Screen.width, (float)Screen.height);
		}
		return cameraRigController.viewport;
	}

	private static Vector2 RandomOnCircle()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		float value = Random.value;
		return new Vector2(Mathf.Cos(value * MathF.PI * 2f), Mathf.Sin(value * MathF.PI * 2f));
	}

	private static Vector2 CalculateCursorPushVector(Vector2 positionA, Vector2 positionB)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = positionA - positionB;
		if (val == Vector2.zero)
		{
			val = RandomOnCircle();
		}
		float sqrMagnitude = ((Vector2)(ref val)).sqrMagnitude;
		if (sqrMagnitude >= 576f)
		{
			return Vector2.zero;
		}
		float num = Mathf.Sqrt(sqrMagnitude);
		float num2 = num * (1f / 24f);
		float num3 = 1f - num2;
		return val / num * num3 * 10f * 0.5f;
	}

	private static void PushCursorsApart()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		if (activeCount <= 1)
		{
			return;
		}
		int count = instancesList.Count;
		if (pushInfos.Length < activeCount)
		{
			pushInfos = new PushInfo[activeCount];
		}
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			if (((Behaviour)instancesList[i]).enabled)
			{
				instancesList[i].GetCursorPosition(out var position);
				pushInfos[num++] = new PushInfo
				{
					index = i,
					position = position
				};
			}
		}
		for (int j = 0; j < activeCount; j++)
		{
			PushInfo pushInfo = pushInfos[j];
			for (int k = j + 1; k < activeCount; k++)
			{
				PushInfo pushInfo2 = pushInfos[k];
				Vector2 val = CalculateCursorPushVector(pushInfo.position, pushInfo2.position);
				ref Vector2 pushVector = ref pushInfo.pushVector;
				pushVector += val;
				ref Vector2 pushVector2 = ref pushInfo2.pushVector;
				pushVector2 -= val;
				pushInfos[k] = pushInfo2;
			}
			pushInfos[j] = pushInfo;
		}
		for (int l = 0; l < activeCount; l++)
		{
			PushInfo pushInfo3 = pushInfos[l];
			MPEventSystem mPEventSystem = instancesList[pushInfo3.index];
			if (mPEventSystem.allowCursorPush && Object.op_Implicit((Object)(object)((EventSystem)mPEventSystem).currentInputModule))
			{
				MPInput obj = (MPInput)(object)((EventSystem)mPEventSystem).currentInputModule.input;
				obj.internalMousePosition += pushInfo3.pushVector;
			}
		}
	}

	static MPEventSystem()
	{
		instancesList = new List<MPEventSystem>();
		readOnlyInstancesList = new ReadOnlyCollection<MPEventSystem>(instancesList);
		pushInfos = Array.Empty<PushInfo>();
		RoR2Application.onUpdate += PushCursorsApart;
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
	}
}
