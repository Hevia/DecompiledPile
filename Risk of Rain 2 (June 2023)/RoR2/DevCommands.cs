using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using RoR2.ConVar;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace RoR2;

public static class DevCommands
{
	private class CvSvNetLogObjectIds : ToggleVirtualConVar
	{
		private static readonly CvSvNetLogObjectIds instance = new CvSvNetLogObjectIds("sv_net_log_object_ids", ConVarFlags.None, "0", "Logs objects associated with each network id to net_id_log.txt as encountered by the server.");

		private uint highestObservedId;

		private FieldInfo monitoredField;

		private TextWriter writer;

		public CvSvNetLogObjectIds(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
			monitoredField = typeof(NetworkIdentity).GetField("s_NextNetworkId", BindingFlags.Static | BindingFlags.NonPublic);
		}

		protected override void OnEnable()
		{
			RoR2Application.onFixedUpdate += Update;
			RoR2Application.onUpdate += Update;
			writer = new StreamWriter("net_id_log.txt", append: false);
			highestObservedId = GetCurrentHighestID();
		}

		protected override void OnDisable()
		{
			writer.Dispose();
			writer = null;
			RoR2Application.onUpdate -= Update;
			RoR2Application.onFixedUpdate -= Update;
		}

		private uint GetCurrentHighestID()
		{
			return (uint)monitoredField.GetValue(null);
		}

		private void Update()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			if (NetworkServer.active)
			{
				uint currentHighestID = GetCurrentHighestID();
				while (highestObservedId < currentHighestID)
				{
					GameObject val = NetworkServer.FindLocalObject(new NetworkInstanceId(highestObservedId));
					writer.WriteLine(string.Format("[{0, 0:D10}]={1}", highestObservedId, Object.op_Implicit((Object)(object)val) ? ((Object)val).name : "null"));
					highestObservedId++;
				}
			}
		}
	}

	private static void AddTokenIfDefault(List<string> lines, string token)
	{
		if (!string.IsNullOrEmpty(token) && (object)Language.GetString(token) == token)
		{
			lines.Add(string.Format("\t\t\"{0}\": \"{0}\",", token));
		}
	}

	[ConCommand(commandName = "language_generate_tokens", flags = ConVarFlags.None, helpText = "Generates default token definitions to be inserted into a JSON language file.")]
	private static void CCLanguageGenerateTokens(ConCommandArgs args)
	{
		List<string> list = new List<string>();
		foreach (ItemDef item in ItemCatalog.allItems.Select(ItemCatalog.GetItemDef))
		{
			AddTokenIfDefault(list, item.nameToken);
			AddTokenIfDefault(list, item.pickupToken);
			AddTokenIfDefault(list, item.descriptionToken);
		}
		list.Add("\r\n");
		foreach (EquipmentDef item2 in EquipmentCatalog.allEquipment.Select(EquipmentCatalog.GetEquipmentDef))
		{
			AddTokenIfDefault(list, item2.nameToken);
			AddTokenIfDefault(list, item2.pickupToken);
			AddTokenIfDefault(list, item2.descriptionToken);
		}
		Debug.Log((object)string.Join("\r\n", list));
	}

	[ConCommand(commandName = "rng_test_roll", flags = ConVarFlags.None, helpText = "Tests the RNG. First argument is a percent chance, second argument is a number of rolls to perform. Result is the average number of rolls that passed.")]
	private static void CCTestRng(ConCommandArgs args)
	{
		float argFloat = args.GetArgFloat(0);
		ulong argULong = args.GetArgULong(1);
		ulong num = 0uL;
		for (ulong num2 = 0uL; num2 < argULong; num2++)
		{
			if (RoR2Application.rng.RangeFloat(0f, 100f) < argFloat)
			{
				num++;
			}
		}
		Debug.Log((object)((double)num / (double)argULong * 100.0));
	}

	[ConCommand(commandName = "getpos", flags = ConVarFlags.None, helpText = "Prints the current position of the sender's body.")]
	private static void CCGetPos(ConCommandArgs args)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)args.GetSenderBody()).transform.position;
		Debug.LogFormat("{0} {1} {2}", new object[3] { position.x, position.y, position.z });
	}

	[ConCommand(commandName = "setpos", flags = ConVarFlags.Cheat, helpText = "Teleports the sender's body to the specified position.")]
	private static void CCSetPos(ConCommandArgs args)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		CharacterBody senderBody = args.GetSenderBody();
		Vector3 newPosition = default(Vector3);
		((Vector3)(ref newPosition))._002Ector(args.GetArgFloat(0), args.GetArgFloat(1), args.GetArgFloat(2));
		TeleportHelper.TeleportGameObject(((Component)senderBody).gameObject, newPosition);
	}

	[ConCommand(commandName = "create_object_from_resources", flags = (ConVarFlags.ExecuteOnServer | ConVarFlags.Cheat), helpText = "Instantiates an object from the Resources folder where the sender is looking.")]
	private static void CreateObjectFromResources(ConCommandArgs args)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		CharacterBody senderBody = args.GetSenderBody();
		GameObject val = LegacyResourcesAPI.Load<GameObject>(args.GetArgString(0));
		if (!Object.op_Implicit((Object)(object)val))
		{
			throw new ConCommandException("Prefab could not be found at the specified path. Argument must be a Resources/-relative path to a prefab.");
		}
		if (((Component)senderBody).GetComponent<InputBankTest>().GetAimRaycast(float.PositiveInfinity, out var hitInfo))
		{
			Vector3 point = ((RaycastHit)(ref hitInfo)).point;
			Quaternion identity = Quaternion.identity;
			NetworkServer.Spawn(Object.Instantiate<GameObject>(val, point, identity));
		}
	}

	[ConCommand(commandName = "resources_load_async_test", flags = ConVarFlags.None, helpText = "Tests Resources.LoadAsync. Loads the asset at the specified path and prints out the results of the operation.")]
	private static void CCResourcesLoadAsyncTest(ConCommandArgs args)
	{
		string path = args.GetArgString(0);
		ResourceRequest request = Resources.LoadAsync(path);
		((AsyncOperation)request).completed += Check;
		void Check(AsyncOperation asyncOperation)
		{
			Debug.Log((object)$"resources_load_async_test \"{path}\" results:\n  request.progress={0}\n  request.isDone={((AsyncOperation)request).isDone}\n  request.asset?.GetType()={((object)request.asset)?.GetType()}\n  request.asset={request.asset}");
		}
	}

	private static Object FindObjectFromInstanceID(int instanceId)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		return (Object)typeof(Object).GetMethod("FindObjectFromInstanceID", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1] { instanceId });
	}

	[ConCommand(commandName = "dump_object_info", flags = ConVarFlags.None, helpText = "Prints debug info about the object with the provided instance ID.")]
	private static void CCDumpObjectInfo(ConCommandArgs args)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		int argInt = args.GetArgInt(0);
		Object val = FindObjectFromInstanceID(argInt);
		if (!Object.op_Implicit(val))
		{
			throw new Exception($"Object is not valid. objectInstanceId={argInt}");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(val.name);
		stringBuilder.AppendLine($"  instanceId={val.GetInstanceID()}");
		stringBuilder.AppendLine("  type=" + ((object)val).GetType().FullName);
		GameObject val2 = null;
		GameObject val3;
		Component val4;
		if ((val3 = (GameObject)(object)((val is GameObject) ? val : null)) != null)
		{
			val2 = val3;
		}
		else if ((val4 = (Component)(object)((val is Component) ? val : null)) != null)
		{
			val2 = val4.gameObject;
		}
		if (Object.op_Implicit((Object)(object)val2))
		{
			Scene scene = val2.scene;
			stringBuilder.Append("  scene=\"" + ((Scene)(ref scene)).name + "\"");
			stringBuilder.Append("  transformPath=" + Util.BuildPrefabTransformPath(val2.transform.root, val2.transform, appendCloneSuffix: false, includeRoot: true));
		}
		args.Log(stringBuilder.ToString());
	}
}
