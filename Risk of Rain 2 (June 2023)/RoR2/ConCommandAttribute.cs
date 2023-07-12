using System;
using HG.Reflection;

namespace RoR2;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class ConCommandAttribute : SearchableAttribute
{
	public string commandName;

	public ConVarFlags flags;

	public string helpText = "";
}
