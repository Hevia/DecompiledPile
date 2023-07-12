using System;
using UnityEngine;

namespace EntityStates;

[Serializable]
public struct SerializableEntityStateType
{
	[SerializeField]
	private string _typeName;

	public string typeName
	{
		get
		{
			return _typeName;
		}
		private set
		{
			stateType = Type.GetType(value);
		}
	}

	public Type stateType
	{
		get
		{
			if (_typeName == null)
			{
				return null;
			}
			Type type = Type.GetType(_typeName);
			if (!(type != null) || !type.IsSubclassOf(typeof(EntityState)))
			{
				return null;
			}
			return type;
		}
		set
		{
			_typeName = ((value != null && value.IsSubclassOf(typeof(EntityState))) ? value.AssemblyQualifiedName : "");
		}
	}

	public SerializableEntityStateType(string typeName)
	{
		_typeName = "";
		this.typeName = typeName;
	}

	public SerializableEntityStateType(Type stateType)
	{
		_typeName = "";
		this.stateType = stateType;
	}
}
