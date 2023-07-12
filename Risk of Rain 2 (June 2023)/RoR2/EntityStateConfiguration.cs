using System;
using System.Collections.Generic;
using System.Reflection;
using EntityStates;
using HG;
using HG.GeneralSerializer;
using JetBrains.Annotations;
using UnityEngine;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/EntityStateConfiguration")]
public class EntityStateConfiguration : ScriptableObject
{
	[RequiredBaseType(typeof(EntityState))]
	public SerializableSystemType targetType;

	public SerializedFieldCollection serializedFieldsCollection;

	public static event Action<EntityStateConfiguration> onEditorUpdatedConfigurationGlobal;

	[ContextMenu("Set Name from Target Type")]
	public void SetNameFromTargetType()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Type type = (Type)targetType;
		if (!(type == null))
		{
			((Object)this).name = type.FullName;
		}
	}

	public void ApplyStatic()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Type type = (Type)targetType;
		if (type == null)
		{
			return;
		}
		SerializedField[] serializedFields = serializedFieldsCollection.serializedFields;
		foreach (SerializedField val in serializedFields)
		{
			try
			{
				FieldInfo field = type.GetField(val.fieldName, BindingFlags.Static | BindingFlags.Public);
				if ((object)field != null)
				{
					SerializedValue fieldValue = val.fieldValue;
					field.SetValue(null, ((SerializedValue)(ref fieldValue)).GetValue(field));
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
		}
	}

	[CanBeNull]
	public Action<object> BuildInstanceInitializer()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Type type = (Type)targetType;
		if (type == null)
		{
			return null;
		}
		SerializedField[] serializedFields = serializedFieldsCollection.serializedFields;
		List<(FieldInfo, object)> list = CollectionPool<(FieldInfo, object), List<(FieldInfo, object)>>.RentCollection();
		for (int i = 0; i < serializedFields.Length; i++)
		{
			ref SerializedField reference = ref serializedFields[i];
			try
			{
				FieldInfo field = type.GetField(reference.fieldName);
				if (!(field == null) && FieldPassesFilter(field))
				{
					list.Add((field, ((SerializedValue)(ref reference.fieldValue)).GetValue(field)));
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
		}
		if (list.Count == 0)
		{
			list = CollectionPool<(FieldInfo, object), List<(FieldInfo, object)>>.ReturnCollection(list);
			return null;
		}
		(FieldInfo, object)[] fieldValuesArray = list.ToArray();
		list = CollectionPool<(FieldInfo, object), List<(FieldInfo, object)>>.ReturnCollection(list);
		return InitializeObject;
		static bool FieldPassesFilter(FieldInfo fieldInfo)
		{
			return ((MemberInfo)fieldInfo).GetCustomAttribute<SerializeField>() != null;
		}
		void InitializeObject(object obj)
		{
			for (int j = 0; j < fieldValuesArray.Length; j++)
			{
				var (fieldInfo2, value) = fieldValuesArray[j];
				fieldInfo2.SetValue(obj, value);
			}
		}
	}

	private void Awake()
	{
		((SerializedFieldCollection)(ref serializedFieldsCollection)).PurgeUnityPsuedoNullFields();
	}

	private void OnValidate()
	{
		if (Application.isPlaying)
		{
			RoR2Application.onNextUpdate += delegate
			{
				EntityStateConfiguration.onEditorUpdatedConfigurationGlobal?.Invoke(this);
			};
		}
	}
}
