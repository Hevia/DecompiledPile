using UnityEngine;

public static class SingletonHelper
{
	public static void Assign<T>(ref T field, T instance) where T : Object
	{
		if (!Object.op_Implicit((Object)(object)field))
		{
			field = instance;
			return;
		}
		Debug.LogErrorFormat((Object)(object)instance, "Duplicate instance of singleton class {0}. Only one should exist at a time.", new object[1] { typeof(T).Name });
	}

	public static void Unassign<T>(ref T field, T instance) where T : Object
	{
		if ((Object)(object)field == (Object)(object)instance)
		{
			field = default(T);
		}
	}

	public static T Assign<T>(T existingInstance, T instance) where T : Object
	{
		if (!Object.op_Implicit((Object)(object)existingInstance))
		{
			return instance;
		}
		Debug.LogErrorFormat((Object)(object)instance, "Duplicate instance of singleton class {0}. Only one should exist at a time.", new object[1] { typeof(T).Name });
		return existingInstance;
	}

	public static T Unassign<T>(T existingInstance, T instance) where T : Object
	{
		if ((Object)(object)instance == (Object)(object)existingInstance)
		{
			return default(T);
		}
		return existingInstance;
	}
}
