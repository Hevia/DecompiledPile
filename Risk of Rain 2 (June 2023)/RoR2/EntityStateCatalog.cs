using System;
using System.Collections.Generic;
using System.Linq;
using EntityStates;
using HG;
using JetBrains.Annotations;
using RoR2.ContentManagement;
using UnityEngine;

namespace RoR2;

public static class EntityStateCatalog
{
	private static readonly Dictionary<Type, EntityStateIndex> stateTypeToIndex = new Dictionary<Type, EntityStateIndex>();

	private static Type[] stateIndexToType = Array.Empty<Type>();

	private static readonly Dictionary<Type, Action<object>> instanceFieldInitializers = new Dictionary<Type, Action<object>>();

	private static string[] stateIndexToTypeName = Array.Empty<string>();

	[Obsolete("This is only for use in legacy editors only until they're fully phased out.")]
	public static string[] baseGameStateTypeNames = (from type in typeof(EntityState).Assembly.GetTypes()
		where typeof(EntityState).IsAssignableFrom(type)
		select type.FullName into typeName
		orderby typeName
		select typeName).ToArray();

	private static void SetStateInstanceInitializer([NotNull] Type stateType, [NotNull] Action<object> initializer)
	{
		if (typeof(EntityState).IsAssignableFrom(stateType))
		{
			instanceFieldInitializers[stateType] = initializer;
		}
	}

	public static void InitializeStateFields([NotNull] EntityState entityState)
	{
		if (instanceFieldInitializers.TryGetValue(entityState.GetType(), out var value))
		{
			value(entityState);
		}
	}

	private static void SetElements(Type[] newEntityStateTypes, EntityStateConfiguration[] newEntityStateConfigurations)
	{
		ArrayUtils.CloneTo<Type>(newEntityStateTypes, ref stateIndexToType);
		string[] array = new string[stateIndexToType.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = stateIndexToType[i].AssemblyQualifiedName;
		}
		Array.Sort(array, stateIndexToType, (IComparer<string>?)StringComparer.Ordinal);
		stateTypeToIndex.Clear();
		for (int j = 0; j < stateIndexToType.Length; j++)
		{
			Type key = stateIndexToType[j];
			stateTypeToIndex[key] = (EntityStateIndex)j;
		}
		Array.Resize(ref stateIndexToTypeName, stateIndexToType.Length);
		for (int k = 0; k < stateIndexToType.Length; k++)
		{
			stateIndexToTypeName[k] = stateIndexToType[k].Name;
		}
		instanceFieldInitializers.Clear();
		for (int l = 0; l < newEntityStateConfigurations.Length; l++)
		{
			ApplyEntityStateConfiguration(newEntityStateConfigurations[l]);
		}
	}

	private static void ApplyEntityStateConfiguration(EntityStateConfiguration entityStateConfiguration)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Type type = (Type)entityStateConfiguration.targetType;
		if (type == null)
		{
			Debug.LogFormat("ApplyEntityStateConfiguration({0}) failed: state type is null.", new object[1] { entityStateConfiguration });
			return;
		}
		if (!stateTypeToIndex.ContainsKey(type))
		{
			Debug.LogFormat("ApplyEntityStateConfiguration({0}) failed: state type {1} is not registered.", new object[2] { entityStateConfiguration, type.FullName });
			return;
		}
		entityStateConfiguration.ApplyStatic();
		Action<object> action = entityStateConfiguration.BuildInstanceInitializer();
		if (action == null)
		{
			instanceFieldInitializers.Remove(type);
		}
		else
		{
			instanceFieldInitializers[type] = action;
		}
	}

	private static void OnStateConfigurationUpdatedByEditor(EntityStateConfiguration entityStateConfiguration)
	{
		ApplyEntityStateConfiguration(entityStateConfiguration);
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		SetElements(ContentManager.entityStateTypes, ContentManager.entityStateConfigurations);
		EntityStateConfiguration.onEditorUpdatedConfigurationGlobal += OnStateConfigurationUpdatedByEditor;
	}

	public static EntityStateIndex GetStateIndex(Type entityStateType)
	{
		if (stateTypeToIndex.TryGetValue(entityStateType, out var value))
		{
			return value;
		}
		return EntityStateIndex.Invalid;
	}

	public static Type GetStateType(EntityStateIndex entityStateIndex)
	{
		Type[] array = stateIndexToType;
		Type type = null;
		return ArrayUtils.GetSafe<Type>(array, (int)entityStateIndex, ref type);
	}

	[CanBeNull]
	public static string GetStateTypeName(Type entityStateType)
	{
		return GetStateTypeName(GetStateIndex(entityStateType));
	}

	[CanBeNull]
	public static string GetStateTypeName(EntityStateIndex entityStateIndex)
	{
		return ArrayUtils.GetSafe<string>(stateIndexToTypeName, (int)entityStateIndex);
	}

	public static EntityState InstantiateState(EntityStateIndex entityStateIndex)
	{
		Type stateType = GetStateType(entityStateIndex);
		if (stateType != null)
		{
			return Activator.CreateInstance(stateType) as EntityState;
		}
		Debug.LogFormat("Bad stateTypeIndex {0}", new object[1] { entityStateIndex });
		return null;
	}

	public static EntityState InstantiateState(Type stateType)
	{
		if (stateType != null && stateType.IsSubclassOf(typeof(EntityState)))
		{
			return Activator.CreateInstance(stateType) as EntityState;
		}
		Debug.LogFormat("Bad stateType {0}", new object[1] { (stateType == null) ? "null" : stateType.FullName });
		return null;
	}

	public static EntityState InstantiateState(SerializableEntityStateType serializableStateType)
	{
		return InstantiateState(serializableStateType.stateType);
	}
}
