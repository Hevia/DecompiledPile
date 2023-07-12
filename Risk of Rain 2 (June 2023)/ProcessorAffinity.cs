using System;
using System.Runtime.InteropServices;
using RoR2;
using RoR2.ConVar;
using UnityEngine;

public static class ProcessorAffinity
{
	private class ProcessorAffinityConVar : BaseConVar
	{
		public static ProcessorAffinityConVar instance = new ProcessorAffinityConVar("processor_affinity", ConVarFlags.Engine, null, "The processor affinity mask.");

		private ProcessorAffinityConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out ulong result) && result != 0L)
			{
				SetProcessAffinityMask(GetCurrentProcess(), (IntPtr)(long)result);
				return;
			}
			Debug.LogFormat("Could not accept value \"{0}\"", new object[1] { newValue });
		}

		public override string GetString()
		{
			IntPtr lpProcessAffinityMask = IntPtr.Zero;
			IntPtr lpSystemAffinityMask = IntPtr.Zero;
			GetProcessAffinityMask(GetCurrentProcess(), ref lpProcessAffinityMask, ref lpSystemAffinityMask);
			return lpProcessAffinityMask.ToString();
		}
	}

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SetProcessAffinityMask(IntPtr hProcess, IntPtr dwProcessAffinityMask);

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetProcessAffinityMask(IntPtr hProcess, ref IntPtr lpProcessAffinityMask, ref IntPtr lpSystemAffinityMask);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetCurrentProcess();
}
