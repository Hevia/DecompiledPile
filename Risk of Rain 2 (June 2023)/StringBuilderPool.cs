using System;
using System.Runtime.CompilerServices;
using System.Text;
using HG;
using JetBrains.Annotations;

[Obsolete("Use HG.StringBuilderPool instead.", false)]
public static class StringBuilderPool
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[NotNull]
	[Obsolete("Use HG.StringBuilderPool instead.", false)]
	public static StringBuilder RentStringBuilder()
	{
		return StringBuilderPool.RentStringBuilder();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("Use HG.StringBuilderPool instead.", false)]
	[CanBeNull]
	public static StringBuilder ReturnStringBuilder([NotNull] StringBuilder stringBuilder)
	{
		return StringBuilderPool.ReturnStringBuilder(stringBuilder);
	}
}
