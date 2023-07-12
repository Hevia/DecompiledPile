using System;
using System.Collections.Generic;
using System.Reflection;
using HG.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace RoR2;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
[MeansImplicitUse]
public class SystemInitializerAttribute : SearchableAttribute
{
	private class SystemInitializationLogHandler : ILogHandler
	{
		public ILogHandler underlyingLogHandler;

		private SystemInitializerAttribute _currentInitializer;

		private string logPrefix = string.Empty;

		public SystemInitializerAttribute currentInitializer
		{
			get
			{
				return _currentInitializer;
			}
			set
			{
				_currentInitializer = value;
				logPrefix = "[" + currentInitializer.associatedType.FullName + "] ";
			}
		}

		public void LogException(Exception exception, Object context)
		{
			LogFormat((LogType)4, context, exception.Message, null);
		}

		public void LogFormat(LogType logType, Object context, string format, params object[] args)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			underlyingLogHandler.LogFormat(logType, context, logPrefix + format, args);
		}
	}

	public static bool hasExecuted;

	public Type[] dependencies = Array.Empty<Type>();

	private MethodInfo methodInfo;

	private Type associatedType;

	public SystemInitializerAttribute(params Type[] dependencies)
	{
		if (dependencies != null)
		{
			this.dependencies = dependencies;
		}
	}

	public static void Execute()
	{
		Queue<SystemInitializerAttribute> queue = new Queue<SystemInitializerAttribute>();
		foreach (SystemInitializerAttribute instance in SearchableAttribute.GetInstances<SystemInitializerAttribute>())
		{
			MethodInfo methodInfo = ((SearchableAttribute)instance).target as MethodInfo;
			if (methodInfo != null && methodInfo.IsStatic)
			{
				queue.Enqueue(instance);
				instance.methodInfo = methodInfo;
				instance.associatedType = methodInfo.DeclaringType;
			}
		}
		HashSet<Type> initializedTypes = new HashSet<Type>();
		SystemInitializationLogHandler systemInitializationLogHandler = new SystemInitializationLogHandler();
		ILogHandler logHandler = (systemInitializationLogHandler.underlyingLogHandler = Debug.unityLogger.logHandler);
		int num = 0;
		while (queue.Count > 0)
		{
			SystemInitializerAttribute systemInitializerAttribute2 = queue.Dequeue();
			if (!InitializerDependenciesMet(systemInitializerAttribute2))
			{
				queue.Enqueue(systemInitializerAttribute2);
				num++;
				if (num >= queue.Count)
				{
					Debug.LogFormat("SystemInitializerAttribute infinite loop detected. currentMethod={0}", new object[1] { systemInitializerAttribute2.associatedType.FullName + systemInitializerAttribute2.methodInfo.Name });
					Debug.LogFormat("initializer dependencies = " + string.Join(",\n", (IEnumerable<Type>)systemInitializerAttribute2.dependencies), Array.Empty<object>());
					Debug.LogFormat("initialized types = " + string.Join(",\n", initializedTypes), Array.Empty<object>());
					break;
				}
				continue;
			}
			try
			{
				Debug.unityLogger.logHandler = (ILogHandler)(object)systemInitializationLogHandler;
				systemInitializationLogHandler.currentInitializer = systemInitializerAttribute2;
				systemInitializerAttribute2.methodInfo.Invoke(null, Array.Empty<object>());
				initializedTypes.Add(systemInitializerAttribute2.associatedType);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
			finally
			{
				Debug.unityLogger.logHandler = logHandler;
			}
			num = 0;
		}
		hasExecuted = true;
		bool InitializerDependenciesMet(SystemInitializerAttribute initializerAttribute)
		{
			Type[] array = initializerAttribute.dependencies;
			foreach (Type item in array)
			{
				if (!initializedTypes.Contains(item))
				{
					return false;
				}
			}
			return true;
		}
	}
}
