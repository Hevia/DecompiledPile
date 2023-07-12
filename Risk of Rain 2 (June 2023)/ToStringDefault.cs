using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ToStringDefault<T> : IToStringImplementation<T>
{
	public string DoToString(in T input)
	{
		T val = input;
		return val.ToString();
	}

	string IToStringImplementation<T>.DoToString(in T input)
	{
		return DoToString(in input);
	}
}
