using System.Runtime.InteropServices;
using RoR2;
using RoR2.Networking;
using UnityEngine.Networking;

namespace Unity;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public class GeneratedNetworkCode
{
	public static void _ReadStructSyncListPickupIndex_VoidSuppressorBehavior(NetworkReader reader, VoidSuppressorBehavior.SyncListPickupIndex instance)
	{
		ushort num = reader.ReadUInt16();
		((SyncList<PickupIndex>)(object)instance).Clear();
		for (ushort num2 = 0; num2 < num; num2 = (ushort)(num2 + 1))
		{
			((SyncListStruct<PickupIndex>)instance).AddInternal(instance.DeserializeItem(reader));
		}
	}

	public static void _WriteStructSyncListPickupIndex_VoidSuppressorBehavior(NetworkWriter writer, VoidSuppressorBehavior.SyncListPickupIndex value)
	{
		ushort count = ((SyncListStruct<PickupIndex>)value).Count;
		writer.Write(count);
		for (ushort num = 0; num < count; num = (ushort)(num + 1))
		{
			value.SerializeItem(writer, ((SyncListStruct<PickupIndex>)value).GetItem((int)num));
		}
	}

	public static void _ReadStructSyncListUserVote_None(NetworkReader reader, SyncListUserVote instance)
	{
		ushort num = reader.ReadUInt16();
		((SyncList<UserVote>)(object)instance).Clear();
		for (ushort num2 = 0; num2 < num; num2 = (ushort)(num2 + 1))
		{
			((SyncListStruct<UserVote>)instance).AddInternal(instance.DeserializeItem(reader));
		}
	}

	public static void _WriteStructSyncListUserVote_None(NetworkWriter writer, SyncListUserVote value)
	{
		ushort count = ((SyncListStruct<UserVote>)value).Count;
		writer.Write(count);
		for (ushort num = 0; num < count; num = (ushort)(num + 1))
		{
			value.SerializeItem(writer, ((SyncListStruct<UserVote>)value).GetItem((int)num));
		}
	}

	public static void _WriteArrayString_None(NetworkWriter writer, string[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort num = (ushort)value.Length;
		writer.Write(num);
		for (ushort num2 = 0; num2 < value.Length; num2 = (ushort)(num2 + 1))
		{
			writer.Write(value[num2]);
		}
	}

	public static string[] _ReadArrayString_None(NetworkReader reader)
	{
		int num = reader.ReadUInt16();
		if (num == 0)
		{
			return new string[0];
		}
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			ref string reference = ref array[i];
			reference = reader.ReadString();
		}
		return array;
	}

	public static void _WriteArrayString_None(NetworkWriter writer, string[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort num = (ushort)value.Length;
		writer.Write(num);
		for (ushort num2 = 0; num2 < value.Length; num2 = (ushort)(num2 + 1))
		{
			writer.Write(value[num2]);
		}
	}

	public static void _WriteNetworkGuid_None(NetworkWriter writer, NetworkGuid value)
	{
		writer.WritePackedUInt64(value._a);
		writer.WritePackedUInt64(value._b);
	}

	public static void _WriteNetworkDateTime_None(NetworkWriter writer, NetworkDateTime value)
	{
		writer.WritePackedUInt64((ulong)value._binaryValue);
	}

	public static void _WriteRunStopwatch_Run(NetworkWriter writer, Run.RunStopwatch value)
	{
		writer.Write(value.offsetFromFixedTime);
		writer.Write(value.isPaused);
	}

	public static NetworkGuid _ReadNetworkGuid_None(NetworkReader reader)
	{
		NetworkGuid result = default(NetworkGuid);
		result._a = reader.ReadPackedUInt64();
		result._b = reader.ReadPackedUInt64();
		return result;
	}

	public static NetworkDateTime _ReadNetworkDateTime_None(NetworkReader reader)
	{
		NetworkDateTime result = default(NetworkDateTime);
		result._binaryValue = (long)reader.ReadPackedUInt64();
		return result;
	}

	public static Run.RunStopwatch _ReadRunStopwatch_Run(NetworkReader reader)
	{
		Run.RunStopwatch result = default(Run.RunStopwatch);
		result.offsetFromFixedTime = reader.ReadSingle();
		result.isPaused = reader.ReadBoolean();
		return result;
	}

	public static void _WriteNetworkMasterIndex_MasterCatalog(NetworkWriter writer, MasterCatalog.NetworkMasterIndex value)
	{
		writer.WritePackedUInt32(value.i);
	}

	public static MasterCatalog.NetworkMasterIndex _ReadNetworkMasterIndex_MasterCatalog(NetworkReader reader)
	{
		MasterCatalog.NetworkMasterIndex result = default(MasterCatalog.NetworkMasterIndex);
		result.i = reader.ReadPackedUInt32();
		return result;
	}

	public static void _WritePickupIndex_None(NetworkWriter writer, PickupIndex value)
	{
		writer.WritePackedUInt32((uint)value.value);
	}

	public static PickupIndex _ReadPickupIndex_None(NetworkReader reader)
	{
		PickupIndex result = default(PickupIndex);
		result.value = (int)reader.ReadPackedUInt32();
		return result;
	}

	public static PhysForceInfo _ReadPhysForceInfo_None(NetworkReader reader)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		PhysForceInfo result = default(PhysForceInfo);
		result.force = reader.ReadVector3();
		return result;
	}

	public static void _WritePhysForceInfo_None(NetworkWriter writer, PhysForceInfo value)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(value.force);
	}

	public static CharacterMotor.HitGroundInfo _ReadHitGroundInfo_CharacterMotor(NetworkReader reader)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		CharacterMotor.HitGroundInfo result = default(CharacterMotor.HitGroundInfo);
		result.velocity = reader.ReadVector3();
		result.position = reader.ReadVector3();
		return result;
	}

	public static void _WriteHitGroundInfo_CharacterMotor(NetworkWriter writer, CharacterMotor.HitGroundInfo value)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(value.velocity);
		writer.Write(value.position);
	}

	public static void _WriteFixedTimeStamp_Run(NetworkWriter writer, Run.FixedTimeStamp value)
	{
		writer.Write(value.t);
	}

	public static Run.FixedTimeStamp _ReadFixedTimeStamp_Run(NetworkReader reader)
	{
		Run.FixedTimeStamp result = default(Run.FixedTimeStamp);
		result.t = reader.ReadSingle();
		return result;
	}

	public static void _WriteHurtBoxReference_None(NetworkWriter writer, HurtBoxReference value)
	{
		writer.Write(value.rootObject);
		writer.WritePackedUInt32((uint)value.hurtBoxIndexPlusOne);
	}

	public static HurtBoxReference _ReadHurtBoxReference_None(NetworkReader reader)
	{
		HurtBoxReference result = default(HurtBoxReference);
		result.rootObject = reader.ReadGameObject();
		result.hurtBoxIndexPlusOne = (byte)reader.ReadPackedUInt32();
		return result;
	}

	public static void _WriteParentIdentifier_NetworkParent(NetworkWriter writer, NetworkParent.ParentIdentifier value)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		writer.WritePackedUInt32((uint)value.indexInParentChildLocatorPlusOne);
		writer.Write(value.parentNetworkInstanceId);
	}

	public static NetworkParent.ParentIdentifier _ReadParentIdentifier_NetworkParent(NetworkReader reader)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		NetworkParent.ParentIdentifier result = default(NetworkParent.ParentIdentifier);
		result.indexInParentChildLocatorPlusOne = (byte)reader.ReadPackedUInt32();
		result.parentNetworkInstanceId = reader.ReadNetworkId();
		return result;
	}

	public static void _WriteArrayString_None(NetworkWriter writer, string[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort num = (ushort)value.Length;
		writer.Write(num);
		for (ushort num2 = 0; num2 < value.Length; num2 = (ushort)(num2 + 1))
		{
			writer.Write(value[num2]);
		}
	}

	public static UnlockableIndex[] _ReadArrayUnlockableIndex_None(NetworkReader reader)
	{
		int num = reader.ReadUInt16();
		if (num == 0)
		{
			return new UnlockableIndex[0];
		}
		UnlockableIndex[] array = new UnlockableIndex[num];
		for (int i = 0; i < num; i++)
		{
			ref UnlockableIndex reference = ref array[i];
			reference = (UnlockableIndex)reader.ReadInt32();
		}
		return array;
	}

	public static void _WriteArrayUnlockableIndex_None(NetworkWriter writer, UnlockableIndex[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort num = (ushort)value.Length;
		writer.Write(num);
		for (ushort num2 = 0; num2 < value.Length; num2 = (ushort)(num2 + 1))
		{
			writer.Write((int)value[num2]);
		}
	}

	public static void _WriteNetworkUserId_None(NetworkWriter writer, NetworkUserId value)
	{
		writer.WritePackedUInt64(value.value);
		writer.Write(value.strValue);
		writer.WritePackedUInt32((uint)value.subId);
	}

	public static NetworkUserId _ReadNetworkUserId_None(NetworkReader reader)
	{
		NetworkUserId result = default(NetworkUserId);
		result.value = reader.ReadPackedUInt64();
		result.strValue = reader.ReadString();
		result.subId = (byte)reader.ReadPackedUInt32();
		return result;
	}

	public static PingerController.PingInfo _ReadPingInfo_PingerController(NetworkReader reader)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		PingerController.PingInfo result = default(PingerController.PingInfo);
		result.active = reader.ReadBoolean();
		result.origin = reader.ReadVector3();
		result.normal = reader.ReadVector3();
		result.targetNetworkIdentity = reader.ReadNetworkIdentity();
		return result;
	}

	public static void _WritePingInfo_PingerController(NetworkWriter writer, PingerController.PingInfo value)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(value.active);
		writer.Write(value.origin);
		writer.Write(value.normal);
		writer.Write(value.targetNetworkIdentity);
	}

	public static CubicBezier3 _ReadCubicBezier3_None(NetworkReader reader)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		CubicBezier3 result = default(CubicBezier3);
		result.a = reader.ReadVector3();
		result.b = reader.ReadVector3();
		result.c = reader.ReadVector3();
		result.d = reader.ReadVector3();
		return result;
	}

	public static WormBodyPositions2.KeyFrame _ReadKeyFrame_WormBodyPositions2(NetworkReader reader)
	{
		WormBodyPositions2.KeyFrame result = default(WormBodyPositions2.KeyFrame);
		result.curve = _ReadCubicBezier3_None(reader);
		result.length = reader.ReadSingle();
		result.time = reader.ReadSingle();
		return result;
	}

	public static void _WriteCubicBezier3_None(NetworkWriter writer, CubicBezier3 value)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		writer.Write(value.a);
		writer.Write(value.b);
		writer.Write(value.c);
		writer.Write(value.d);
	}

	public static void _WriteKeyFrame_WormBodyPositions2(NetworkWriter writer, WormBodyPositions2.KeyFrame value)
	{
		_WriteCubicBezier3_None(writer, value.curve);
		writer.Write(value.length);
		writer.Write(value.time);
	}

	public static float[] _ReadArraySingle_None(NetworkReader reader)
	{
		int num = reader.ReadUInt16();
		if (num == 0)
		{
			return new float[0];
		}
		float[] array = new float[num];
		for (int i = 0; i < num; i++)
		{
			ref float reference = ref array[i];
			reference = reader.ReadSingle();
		}
		return array;
	}

	public static void _WriteArraySingle_None(NetworkWriter writer, float[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort num = (ushort)value.Length;
		writer.Write(num);
		for (ushort num2 = 0; num2 < value.Length; num2 = (ushort)(num2 + 1))
		{
			writer.Write(value[num2]);
		}
	}

	public static void _WriteCSteamID_None(NetworkWriter writer, CSteamID value)
	{
		writer.Write(value.stringValue);
		writer.WritePackedUInt64(value.steamValue);
	}

	public static void _WriteArrayString_None(NetworkWriter writer, string[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort num = (ushort)value.Length;
		writer.Write(num);
		for (ushort num2 = 0; num2 < value.Length; num2 = (ushort)(num2 + 1))
		{
			writer.Write(value[num2]);
		}
	}

	public static CSteamID _ReadCSteamID_None(NetworkReader reader)
	{
		CSteamID result = default(CSteamID);
		result.stringValue = reader.ReadString();
		result.steamValue = reader.ReadPackedUInt64();
		return result;
	}

	public static void _WriteArrayString_None(NetworkWriter writer, string[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort num = (ushort)value.Length;
		writer.Write(num);
		for (ushort num2 = 0; num2 < value.Length; num2 = (ushort)(num2 + 1))
		{
			writer.Write(value[num2]);
		}
	}

	public static void _WriteArrayString_None(NetworkWriter writer, string[] value)
	{
		if (value == null)
		{
			writer.Write((ushort)0);
			return;
		}
		ushort num = (ushort)value.Length;
		writer.Write(num);
		for (ushort num2 = 0; num2 < value.Length; num2 = (ushort)(num2 + 1))
		{
			writer.Write(value[num2]);
		}
	}

	public static void _WriteUserID_None(NetworkWriter writer, UserID value)
	{
		_WriteCSteamID_None(writer, value.CID);
	}

	public static UserID _ReadUserID_None(NetworkReader reader)
	{
		UserID result = default(UserID);
		result.CID = _ReadCSteamID_None(reader);
		return result;
	}

	public static ServerAchievementIndex _ReadServerAchievementIndex_None(NetworkReader reader)
	{
		ServerAchievementIndex result = default(ServerAchievementIndex);
		result.intValue = (int)reader.ReadPackedUInt32();
		return result;
	}

	public static void _WriteServerAchievementIndex_None(NetworkWriter writer, ServerAchievementIndex value)
	{
		writer.WritePackedUInt32((uint)value.intValue);
	}
}
