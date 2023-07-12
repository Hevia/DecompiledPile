using System;
using System.Runtime.CompilerServices;
using HG;
using JetBrains.Annotations;

namespace RoR2;

public class FixedSizeArrayPool<T>
{
	private int _lengthOfArrays;

	[NotNull]
	private T[][] pooledArrays;

	private int count;

	public int lengthOfArrays
	{
		get
		{
			return _lengthOfArrays;
		}
		set
		{
			if (_lengthOfArrays != value)
			{
				ArrayUtils.Clear<T[]>(pooledArrays, ref count);
				_lengthOfArrays = value;
			}
		}
	}

	public FixedSizeArrayPool(int lengthOfArrays)
	{
		_lengthOfArrays = lengthOfArrays;
		pooledArrays = Array.Empty<T[]>();
	}

	[NotNull]
	public T[] Request()
	{
		if (count <= 0)
		{
			return new T[_lengthOfArrays];
		}
		T[] result = pooledArrays[--count];
		pooledArrays[count] = null;
		return result;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[NotNull]
	private static ArgumentException CreateArraySizeMismatchException(int incomingArrayLength, int arrayPoolSizeRequirement)
	{
		return new ArgumentException($"Array of length {incomingArrayLength} may not be returned to pool for arrays of length {arrayPoolSizeRequirement}", "array");
	}

	public void Return([NotNull] T[] array)
	{
		if (array.Length != _lengthOfArrays)
		{
			throw CreateArraySizeMismatchException(array.Length, _lengthOfArrays);
		}
		T[] array2 = array;
		T val = default(T);
		ArrayUtils.SetAll<T>(array2, ref val);
		ArrayUtils.ArrayAppend<T[]>(ref pooledArrays, ref count, ref array);
	}
}
