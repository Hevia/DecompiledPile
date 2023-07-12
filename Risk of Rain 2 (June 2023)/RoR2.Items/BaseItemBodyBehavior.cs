using System;
using System.Collections.Generic;
using System.Reflection;
using HG;
using HG.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Items;

public abstract class BaseItemBodyBehavior : MonoBehaviour
{
	private struct ItemTypePair
	{
		public ItemIndex itemIndex;

		public Type behaviorType;
	}

	private struct NetworkContextSet
	{
		public ItemTypePair[] itemTypePairs;

		public FixedSizeArrayPool<BaseItemBodyBehavior> behaviorArraysPool;

		public void SetItemTypePairs(List<ItemTypePair> itemTypePairs)
		{
			this.itemTypePairs = itemTypePairs.ToArray();
			behaviorArraysPool = new FixedSizeArrayPool<BaseItemBodyBehavior>(this.itemTypePairs.Length);
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	[MeansImplicitUse]
	public class ItemDefAssociationAttribute : SearchableAttribute
	{
		public Type behaviorTypeOverride;

		public bool useOnServer = true;

		public bool useOnClient = true;
	}

	public int stack;

	private static NetworkContextSet server;

	private static NetworkContextSet client;

	private static NetworkContextSet shared;

	private static CharacterBody earlyAssignmentBody = null;

	private static Dictionary<UnityObjectWrapperKey<CharacterBody>, BaseItemBodyBehavior[]> bodyToItemBehaviors = new Dictionary<UnityObjectWrapperKey<CharacterBody>, BaseItemBodyBehavior[]>();

	public CharacterBody body { get; private set; }

	protected void Awake()
	{
		body = earlyAssignmentBody;
		earlyAssignmentBody = null;
	}

	[SystemInitializer(new Type[] { typeof(ItemCatalog) })]
	private static void Init()
	{
		List<ItemTypePair> list = new List<ItemTypePair>();
		List<ItemTypePair> list2 = new List<ItemTypePair>();
		List<ItemTypePair> list3 = new List<ItemTypePair>();
		List<ItemDefAssociationAttribute> list4 = new List<ItemDefAssociationAttribute>();
		SearchableAttribute.GetInstances<ItemDefAssociationAttribute>(list4);
		Type typeFromHandle = typeof(BaseItemBodyBehavior);
		Type typeFromHandle2 = typeof(ItemDef);
		foreach (ItemDefAssociationAttribute item in list4)
		{
			if (!(((SearchableAttribute)item).target is MethodInfo methodInfo))
			{
				Debug.LogError((object)("ItemDefAssociationAttribute cannot be applied to object of type '" + ((object)item)?.GetType().FullName + "'"));
				continue;
			}
			if (!methodInfo.IsStatic)
			{
				Debug.LogError((object)("ItemDefAssociationAttribute cannot be applied to method " + methodInfo.DeclaringType.FullName + "." + methodInfo.Name + ": Method is not static."));
				continue;
			}
			Type type = item.behaviorTypeOverride ?? methodInfo.DeclaringType;
			if (!typeFromHandle.IsAssignableFrom(type))
			{
				Debug.LogError((object)("ItemDefAssociationAttribute cannot be applied to method " + methodInfo.DeclaringType.FullName + "." + methodInfo.Name + ": " + methodInfo.DeclaringType.FullName + " does not derive from " + typeFromHandle.FullName + "."));
				continue;
			}
			if (type.IsAbstract)
			{
				Debug.LogError((object)("ItemDefAssociationAttribute cannot be applied to method " + methodInfo.DeclaringType.FullName + "." + methodInfo.Name + ": " + methodInfo.DeclaringType.FullName + " is an abstract type."));
				continue;
			}
			if (!typeFromHandle2.IsAssignableFrom(methodInfo.ReturnType))
			{
				Debug.LogError((object)string.Format("{0} cannot be applied to method {1}.{2}: {3}.{4} returns type '{5}' instead of {6}.", "ItemDefAssociationAttribute", methodInfo.DeclaringType.FullName, methodInfo.Name, methodInfo.DeclaringType.FullName, methodInfo, methodInfo.ReturnType?.FullName ?? "void", typeFromHandle2.FullName));
				continue;
			}
			if (methodInfo.GetGenericArguments().Length != 0)
			{
				Debug.LogError((object)string.Format("{0} cannot be applied to method {1}.{2}: {3}.{4} must take no arguments.", "ItemDefAssociationAttribute", methodInfo.DeclaringType.FullName, methodInfo.Name, methodInfo.DeclaringType.FullName, methodInfo));
				continue;
			}
			ItemDef itemDef = (ItemDef)methodInfo.Invoke(null, Array.Empty<object>());
			if (!Object.op_Implicit((Object)(object)itemDef))
			{
				Debug.LogError((object)(methodInfo.DeclaringType.FullName + "." + methodInfo.Name + " returned null."));
				continue;
			}
			if (itemDef.itemIndex < (ItemIndex)0)
			{
				Debug.LogError((object)$"{methodInfo.DeclaringType.FullName}.{methodInfo.Name} returned an ItemDef that's not registered in the ItemCatalog. result={itemDef}");
				continue;
			}
			if (item.useOnServer)
			{
				list.Add(new ItemTypePair
				{
					itemIndex = itemDef.itemIndex,
					behaviorType = type
				});
			}
			if (item.useOnClient)
			{
				list2.Add(new ItemTypePair
				{
					itemIndex = itemDef.itemIndex,
					behaviorType = type
				});
			}
			if (item.useOnServer || item.useOnClient)
			{
				list3.Add(new ItemTypePair
				{
					itemIndex = itemDef.itemIndex,
					behaviorType = type
				});
			}
		}
		server.SetItemTypePairs(list);
		client.SetItemTypePairs(list2);
		shared.SetItemTypePairs(list3);
		CharacterBody.onBodyAwakeGlobal += OnBodyAwakeGlobal;
		CharacterBody.onBodyDestroyGlobal += OnBodyDestroyGlobal;
		CharacterBody.onBodyInventoryChangedGlobal += OnBodyInventoryChangedGlobal;
	}

	private static ref NetworkContextSet GetNetworkContext()
	{
		bool active = NetworkServer.active;
		bool active2 = NetworkClient.active;
		if (active)
		{
			if (active2)
			{
				return ref shared;
			}
			return ref server;
		}
		if (active2)
		{
			return ref client;
		}
		throw new InvalidOperationException("Neither server nor client is running.");
	}

	private static void OnBodyAwakeGlobal(CharacterBody body)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		BaseItemBodyBehavior[] value = GetNetworkContext().behaviorArraysPool.Request();
		bodyToItemBehaviors.Add(UnityObjectWrapperKey<CharacterBody>.op_Implicit(body), value);
	}

	private static void OnBodyDestroyGlobal(CharacterBody body)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		BaseItemBodyBehavior[] array = bodyToItemBehaviors[UnityObjectWrapperKey<CharacterBody>.op_Implicit(body)];
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy((Object)(object)array[i]);
		}
		bodyToItemBehaviors.Remove(UnityObjectWrapperKey<CharacterBody>.op_Implicit(body));
		if (NetworkServer.active || NetworkClient.active)
		{
			GetNetworkContext().behaviorArraysPool.Return(array);
		}
	}

	private static void OnBodyInventoryChangedGlobal(CharacterBody body)
	{
		UpdateBodyItemBehaviorStacks(body);
	}

	private static void UpdateBodyItemBehaviorStacks(CharacterBody body)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		ref NetworkContextSet networkContext = ref GetNetworkContext();
		BaseItemBodyBehavior[] array = bodyToItemBehaviors[UnityObjectWrapperKey<CharacterBody>.op_Implicit(body)];
		ItemTypePair[] itemTypePairs = networkContext.itemTypePairs;
		Inventory inventory = body.inventory;
		if (Object.op_Implicit((Object)(object)inventory))
		{
			for (int i = 0; i < itemTypePairs.Length; i++)
			{
				ItemTypePair itemTypePair = itemTypePairs[i];
				SetItemStack(body, ref array[i], itemTypePair.behaviorType, inventory.GetItemCount(itemTypePair.itemIndex));
			}
			return;
		}
		for (int j = 0; j < itemTypePairs.Length; j++)
		{
			ref BaseItemBodyBehavior reference = ref array[j];
			if (reference != null)
			{
				Object.Destroy((Object)(object)reference);
				reference = null;
			}
		}
	}

	private static void SetItemStack(CharacterBody body, ref BaseItemBodyBehavior behavior, Type behaviorType, int stack)
	{
		if (behavior == null != stack <= 0)
		{
			if (stack <= 0)
			{
				Object.Destroy((Object)(object)behavior);
				behavior = null;
			}
			else
			{
				earlyAssignmentBody = body;
				behavior = (BaseItemBodyBehavior)(object)((Component)body).gameObject.AddComponent(behaviorType);
				earlyAssignmentBody = null;
			}
		}
		if (behavior != null)
		{
			behavior.stack = stack;
		}
	}
}
