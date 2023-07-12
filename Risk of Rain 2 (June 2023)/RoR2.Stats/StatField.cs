using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine.Networking;

namespace RoR2.Stats;

public struct StatField : IComparable<StatField>
{
	[StructLayout(LayoutKind.Explicit)]
	private struct ValueUnion
	{
		[FieldOffset(0)]
		public readonly ulong ulongValue;

		[FieldOffset(0)]
		public readonly double doubleValue;

		public static explicit operator ulong(ValueUnion v)
		{
			return v.ulongValue;
		}

		public static explicit operator double(ValueUnion v)
		{
			return v.doubleValue;
		}

		public static implicit operator ValueUnion(ulong ulongValue)
		{
			return new ValueUnion(ulongValue);
		}

		public static implicit operator ValueUnion(double doubleValue)
		{
			return new ValueUnion(doubleValue);
		}

		private ValueUnion(ulong ulongValue)
		{
			this = default(ValueUnion);
			this.ulongValue = ulongValue;
		}

		private ValueUnion(double doubleValue)
		{
			this = default(ValueUnion);
			this.doubleValue = doubleValue;
		}
	}

	public StatDef statDef;

	private ValueUnion value;

	public string name => statDef.name;

	public StatRecordType recordType => statDef.recordType;

	public StatDataType dataType => statDef.dataType;

	private ulong ulongValue
	{
		get
		{
			return value.ulongValue;
		}
		set
		{
			this.value = value;
		}
	}

	private double doubleValue
	{
		get
		{
			return value.doubleValue;
		}
		set
		{
			this.value = value;
		}
	}

	public override string ToString()
	{
		return dataType switch
		{
			StatDataType.ULong => TextSerialization.ToStringInvariant(value.ulongValue), 
			StatDataType.Double => TextSerialization.ToStringInvariant(value.doubleValue), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public string ToLocalNumeric()
	{
		return dataType switch
		{
			StatDataType.ULong => TextSerialization.ToStringNumeric(value.ulongValue), 
			StatDataType.Double => TextSerialization.ToStringNumeric(value.doubleValue), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public ulong CalculatePointValue()
	{
		throw new NotImplementedException();
	}

	[Pure]
	public static StatField GetDelta(ref StatField newerValue, ref StatField olderValue)
	{
		StatField statField = default(StatField);
		statField.statDef = newerValue.statDef;
		StatField result = statField;
		switch (newerValue.dataType)
		{
		case StatDataType.ULong:
			switch (newerValue.recordType)
			{
			case StatRecordType.Sum:
				result.ulongValue = newerValue.ulongValue - olderValue.ulongValue;
				break;
			case StatRecordType.Max:
				result.ulongValue = Math.Max(newerValue.ulongValue, olderValue.ulongValue);
				break;
			case StatRecordType.Newest:
				result.ulongValue = newerValue.ulongValue;
				break;
			}
			break;
		case StatDataType.Double:
			switch (newerValue.recordType)
			{
			case StatRecordType.Sum:
				result.doubleValue = newerValue.doubleValue - olderValue.doubleValue;
				break;
			case StatRecordType.Max:
				result.doubleValue = Math.Max(newerValue.doubleValue, olderValue.doubleValue);
				break;
			case StatRecordType.Newest:
				result.doubleValue = newerValue.doubleValue;
				break;
			}
			break;
		}
		return result;
	}

	public void PushDelta(ref StatField deltaField)
	{
		switch (dataType)
		{
		case StatDataType.ULong:
			PushStatValue(deltaField.ulongValue);
			break;
		case StatDataType.Double:
			PushStatValue(deltaField.doubleValue);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void Write(NetworkWriter writer)
	{
		switch (dataType)
		{
		case StatDataType.ULong:
			writer.WritePackedUInt64(ulongValue);
			break;
		case StatDataType.Double:
			writer.Write(doubleValue);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void Read(NetworkReader reader)
	{
		switch (dataType)
		{
		case StatDataType.ULong:
			ulongValue = reader.ReadPackedUInt64();
			break;
		case StatDataType.Double:
			doubleValue = reader.ReadDouble();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void EnforceDataType(StatDataType otherDataType)
	{
		if (dataType != otherDataType)
		{
			throw new InvalidOperationException($"Expected data type {dataType}, got data type {otherDataType}.");
		}
	}

	public void PushStatValue(ulong incomingValue)
	{
		EnforceDataType(StatDataType.ULong);
		switch (recordType)
		{
		case StatRecordType.Sum:
			ulongValue += incomingValue;
			break;
		case StatRecordType.Max:
			ulongValue = Math.Max(incomingValue, ulongValue);
			break;
		case StatRecordType.Newest:
			ulongValue = incomingValue;
			break;
		}
	}

	public void PushStatValue(double incomingValue)
	{
		EnforceDataType(StatDataType.Double);
		switch (recordType)
		{
		case StatRecordType.Sum:
			doubleValue += incomingValue;
			break;
		case StatRecordType.Max:
			doubleValue = Math.Max(incomingValue, doubleValue);
			break;
		case StatRecordType.Newest:
			doubleValue = incomingValue;
			break;
		}
	}

	public void SetFromString(string valueString)
	{
		switch (dataType)
		{
		case StatDataType.ULong:
		{
			TextSerialization.TryParseInvariant(valueString, out ulong result2);
			value = result2;
			break;
		}
		case StatDataType.Double:
		{
			TextSerialization.TryParseInvariant(valueString, out double result);
			value = result;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public ulong GetULongValue()
	{
		EnforceDataType(StatDataType.ULong);
		return ulongValue;
	}

	public double GetDoubleValue()
	{
		EnforceDataType(StatDataType.Double);
		return doubleValue;
	}

	public double GetValueAsDouble()
	{
		return dataType switch
		{
			StatDataType.ULong => ulongValue, 
			StatDataType.Double => doubleValue, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	[Obsolete]
	public decimal GetDecimalValue()
	{
		return dataType switch
		{
			StatDataType.ULong => ulongValue, 
			StatDataType.Double => (decimal)doubleValue, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public bool IsDefault()
	{
		return dataType switch
		{
			StatDataType.ULong => ulongValue == 0, 
			StatDataType.Double => doubleValue == 0.0, 
			_ => true, 
		};
	}

	public void SetDefault()
	{
		switch (dataType)
		{
		case StatDataType.ULong:
			ulongValue = 0uL;
			break;
		case StatDataType.Double:
			doubleValue = 0.0;
			break;
		default:
			throw new NotImplementedException();
		}
	}

	public ulong GetPointValue(double pointValue)
	{
		return dataType switch
		{
			StatDataType.ULong => (ulong)((double)ulongValue * pointValue), 
			StatDataType.Double => (ulong)(doubleValue * pointValue), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public int CompareTo(StatField other)
	{
		return CompareTo(in other);
	}

	public int CompareTo(in StatField other)
	{
		EnforceDataType(other.dataType);
		return dataType switch
		{
			StatDataType.ULong => ulongValue.CompareTo(other.ulongValue), 
			StatDataType.Double => doubleValue.CompareTo(other.doubleValue), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public void SetToMaxValue()
	{
		switch (dataType)
		{
		case StatDataType.ULong:
			ulongValue = ulong.MaxValue;
			break;
		case StatDataType.Double:
			doubleValue = double.MaxValue;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
