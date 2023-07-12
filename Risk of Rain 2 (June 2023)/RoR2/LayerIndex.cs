using UnityEngine;

namespace RoR2;

public struct LayerIndex
{
	public static class CommonMasks
	{
		public static readonly LayerMask bullet = LayerMask.op_Implicit(LayerMask.op_Implicit(world.mask) | LayerMask.op_Implicit(entityPrecise.mask));

		public static readonly LayerMask interactable = LayerMask.op_Implicit(LayerMask.op_Implicit(defaultLayer.mask) | LayerMask.op_Implicit(world.mask) | LayerMask.op_Implicit(pickups.mask));
	}

	public int intVal;

	private static uint assignedLayerMask;

	public static readonly LayerIndex invalidLayer;

	public static readonly LayerIndex defaultLayer;

	public static readonly LayerIndex transparentFX;

	public static readonly LayerIndex ignoreRaycast;

	public static readonly LayerIndex water;

	public static readonly LayerIndex ui;

	public static readonly LayerIndex fakeActor;

	public static readonly LayerIndex noCollision;

	public static readonly LayerIndex pickups;

	public static readonly LayerIndex world;

	public static readonly LayerIndex entityPrecise;

	public static readonly LayerIndex debris;

	public static readonly LayerIndex projectile;

	public static readonly LayerIndex manualRender;

	public static readonly LayerIndex collideWithCharacterHullOnly;

	public static readonly LayerIndex ragdoll;

	public static readonly LayerIndex noDraw;

	public static readonly LayerIndex prefabBrush;

	public static readonly LayerIndex postProcess;

	public static readonly LayerIndex uiWorldSpace;

	public static readonly string modderMessage;

	public static readonly LayerIndex playerBody;

	public static readonly LayerIndex enemyBody;

	public static readonly LayerIndex triggerZone;

	private static readonly LayerMask[] collisionMasks;

	public LayerMask mask => LayerMask.op_Implicit((intVal >= 0) ? (1 << intVal) : intVal);

	public LayerMask collisionMask => collisionMasks[intVal];

	static LayerIndex()
	{
		assignedLayerMask = 0u;
		invalidLayer = new LayerIndex
		{
			intVal = -1
		};
		defaultLayer = GetLayerIndex("Default");
		transparentFX = GetLayerIndex("TransparentFX");
		ignoreRaycast = GetLayerIndex("Ignore Raycast");
		water = GetLayerIndex("Water");
		ui = GetLayerIndex("UI");
		fakeActor = GetLayerIndex("FakeActor");
		noCollision = GetLayerIndex("NoCollision");
		pickups = GetLayerIndex("Pickups");
		world = GetLayerIndex("World");
		entityPrecise = GetLayerIndex("EntityPrecise");
		debris = GetLayerIndex("Debris");
		projectile = GetLayerIndex("Projectile");
		manualRender = GetLayerIndex("ManualRender");
		collideWithCharacterHullOnly = GetLayerIndex("CollideWithCharacterHullOnly");
		ragdoll = GetLayerIndex("Ragdoll");
		noDraw = GetLayerIndex("NoDraw");
		prefabBrush = GetLayerIndex("PrefabBrush");
		postProcess = GetLayerIndex("PostProcess");
		uiWorldSpace = GetLayerIndex("UI, WorldSpace");
		modderMessage = "Layers below are used only by console versions.";
		playerBody = GetLayerIndex("PlayerBody");
		enemyBody = GetLayerIndex("EnemyBody");
		triggerZone = GetLayerIndex("TriggerZone");
		collisionMasks = CalcCollisionMasks();
		for (int i = 0; i < 32; i++)
		{
			string text = LayerMask.LayerToName(i);
			if (text != "" && (assignedLayerMask & (uint)(1 << i)) == 0)
			{
				Debug.LogWarningFormat("Layer \"{0}\" is defined in this project's \"Tags and Layers\" settings but is not defined in LayerIndex!", new object[1] { text });
			}
		}
	}

	private static LayerIndex GetLayerIndex(string layerName)
	{
		LayerIndex layerIndex = default(LayerIndex);
		layerIndex.intVal = LayerMask.NameToLayer(layerName);
		LayerIndex result = layerIndex;
		if (result.intVal == invalidLayer.intVal)
		{
			Debug.LogErrorFormat("Layer \"{0}\" is not defined in this project's \"Tags and Layers\" settings.", new object[1] { layerName });
		}
		else
		{
			assignedLayerMask |= (uint)(1 << result.intVal);
		}
		return result;
	}

	private static LayerMask[] CalcCollisionMasks()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		LayerMask[] array = (LayerMask[])(object)new LayerMask[32];
		for (int i = 0; i < 32; i++)
		{
			LayerMask val = default(LayerMask);
			for (int j = 0; j < 32; j++)
			{
				if (!Physics.GetIgnoreLayerCollision(i, j))
				{
					val = LayerMask.op_Implicit(LayerMask.op_Implicit(val) | (1 << j));
				}
			}
			array[i] = val;
		}
		return array;
	}
}
