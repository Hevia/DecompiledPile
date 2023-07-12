using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using HG;
using JetBrains.Annotations;
using RoR2.Networking;
using RoR2.Stats;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class RunReport
{
	public class PlayerInfo
	{
		[CanBeNull]
		public NetworkUser networkUser;

		[CanBeNull]
		public CharacterMaster master;

		public int localPlayerIndex = -1;

		public string name = string.Empty;

		public BodyIndex bodyIndex = BodyIndex.None;

		public BodyIndex killerBodyIndex = BodyIndex.None;

		public bool isDead;

		public StatSheet statSheet = StatSheet.New();

		public ItemIndex[] itemAcquisitionOrder = Array.Empty<ItemIndex>();

		public int[] itemStacks = ItemCatalog.RequestItemStackArray();

		public EquipmentIndex[] equipment = Array.Empty<EquipmentIndex>();

		public string finalMessageToken = string.Empty;

		public string userProfileFileName = string.Empty;

		private static readonly HGXml.SerializationRules<int[]> itemStacksRules = new HGXml.SerializationRules<int[]>
		{
			serializer = delegate(XElement element, int[] value)
			{
				element.RemoveAll();
				element.Add(from itemIndex in ItemCatalog.allItems
					where value[(int)itemIndex] > 0
					select new XElement(((Object)ItemCatalog.GetItemDef(itemIndex)).name, value[(int)itemIndex]));
			},
			deserializer = delegate(XElement element, ref int[] value)
			{
				Array.Resize(ref value, ItemCatalog.itemCount);
				for (ItemIndex itemIndex2 = (ItemIndex)0; (int)itemIndex2 < ItemCatalog.itemCount; itemIndex2++)
				{
					value[(int)itemIndex2] = 0;
				}
				foreach (XElement item in element.Elements())
				{
					ItemIndex itemIndex3 = ItemCatalog.FindItemIndex(item.Name.LocalName);
					if (ItemCatalog.IsIndexValid(in itemIndex3))
					{
						HGXml.FromXml(item, ref value[(int)itemIndex3]);
					}
				}
				return true;
			}
		};

		private static readonly HGXml.SerializationRules<EquipmentIndex[]> equipmentRules = new HGXml.SerializationRules<EquipmentIndex[]>
		{
			serializer = delegate(XElement element, EquipmentIndex[] value)
			{
				element.Value = string.Join(" ", value.Select(delegate(EquipmentIndex equipmentIndex)
				{
					EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipmentIndex);
					return ((equipmentDef != null) ? ((Object)equipmentDef).name : null) ?? "None";
				}));
			},
			deserializer = delegate(XElement element, ref EquipmentIndex[] value)
			{
				value = element.Value.Split(new char[1] { ' ' }).Select(EquipmentCatalog.FindEquipmentIndex).ToArray();
				return true;
			}
		};

		[CanBeNull]
		public LocalUser localUser
		{
			get
			{
				if (!Object.op_Implicit((Object)(object)networkUser))
				{
					return null;
				}
				return networkUser.localUser;
			}
		}

		public bool isLocalPlayer => localPlayerIndex >= 0;

		public string bodyName
		{
			get
			{
				GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(bodyIndex);
				return ((bodyPrefab != null) ? ((Object)bodyPrefab.gameObject).name : null) ?? "InvalidBody";
			}
			set
			{
				bodyIndex = BodyCatalog.FindBodyIndex(value);
			}
		}

		public string killerBodyName
		{
			get
			{
				GameObject bodyPrefab = BodyCatalog.GetBodyPrefab(killerBodyIndex);
				return ((bodyPrefab != null) ? ((Object)bodyPrefab.gameObject).name : null) ?? "InvalidBody";
			}
			set
			{
				killerBodyIndex = BodyCatalog.FindBodyIndex(value);
			}
		}

		public void Write(NetworkWriter writer)
		{
			writer.WriteBodyIndex(bodyIndex);
			writer.WriteBodyIndex(killerBodyIndex);
			writer.Write(isDead);
			writer.Write(Object.op_Implicit((Object)(object)master) ? ((Component)master).gameObject : null);
			statSheet.Write(writer);
			writer.WritePackedUInt32((uint)itemAcquisitionOrder.Length);
			for (int i = 0; i < itemAcquisitionOrder.Length; i++)
			{
				writer.Write(itemAcquisitionOrder[i]);
			}
			writer.WriteItemStacks(itemStacks);
			writer.WritePackedUInt32((uint)equipment.Length);
			for (int j = 0; j < equipment.Length; j++)
			{
				writer.Write(equipment[j]);
			}
			writer.Write(finalMessageToken);
		}

		public void Read(NetworkReader reader)
		{
			bodyIndex = reader.ReadBodyIndex();
			killerBodyIndex = reader.ReadBodyIndex();
			isDead = reader.ReadBoolean();
			GameObject val = reader.ReadGameObject();
			master = (Object.op_Implicit((Object)(object)val) ? val.GetComponent<CharacterMaster>() : null);
			statSheet.Read(reader);
			int newSize = (int)reader.ReadPackedUInt32();
			Array.Resize(ref itemAcquisitionOrder, newSize);
			for (int i = 0; i < itemAcquisitionOrder.Length; i++)
			{
				ItemIndex itemIndex = reader.ReadItemIndex();
				itemAcquisitionOrder[i] = itemIndex;
			}
			reader.ReadItemStacks(itemStacks);
			int newSize2 = (int)reader.ReadPackedUInt32();
			Array.Resize(ref equipment, newSize2);
			for (int j = 0; j < equipment.Length; j++)
			{
				EquipmentIndex equipmentIndex = reader.ReadEquipmentIndex();
				equipment[j] = equipmentIndex;
			}
			finalMessageToken = reader.ReadString();
			ResolveLocalInformation();
		}

		public void ResolveLocalInformation()
		{
			name = Util.GetBestMasterName(master);
			PlayerCharacterMasterController playerCharacterMasterController = null;
			if (Object.op_Implicit((Object)(object)master))
			{
				playerCharacterMasterController = ((Component)master).GetComponent<PlayerCharacterMasterController>();
			}
			networkUser = null;
			if (Object.op_Implicit((Object)(object)playerCharacterMasterController))
			{
				networkUser = playerCharacterMasterController.networkUser;
			}
			localPlayerIndex = -1;
			userProfileFileName = string.Empty;
			if (Object.op_Implicit((Object)(object)networkUser) && networkUser.localUser != null)
			{
				localPlayerIndex = networkUser.localUser.id;
				userProfileFileName = networkUser.localUser.userProfile.fileName;
			}
		}

		public static PlayerInfo Generate(PlayerCharacterMasterController playerCharacterMasterController, GameEndingDef gameEnding)
		{
			CharacterMaster characterMaster = playerCharacterMasterController.master;
			Inventory inventory = characterMaster.inventory;
			PlayerStatsComponent component = ((Component)playerCharacterMasterController).GetComponent<PlayerStatsComponent>();
			PlayerInfo playerInfo = new PlayerInfo
			{
				networkUser = playerCharacterMasterController.networkUser,
				master = characterMaster,
				bodyIndex = BodyCatalog.FindBodyIndex(characterMaster.bodyPrefab),
				killerBodyIndex = characterMaster.GetKillerBodyIndex(),
				isDead = characterMaster.lostBodyToDeath
			};
			if (playerInfo.killerBodyIndex == BodyIndex.None && Object.op_Implicit((Object)(object)gameEnding) && Object.op_Implicit((Object)(object)gameEnding.defaultKillerOverride))
			{
				playerInfo.killerBodyIndex = BodyCatalog.FindBodyIndex(gameEnding.defaultKillerOverride);
			}
			StatSheet.Copy(component.currentStats, playerInfo.statSheet);
			playerInfo.itemAcquisitionOrder = inventory.itemAcquisitionOrder.ToArray();
			ItemIndex itemIndex = (ItemIndex)0;
			for (ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount; itemIndex < itemCount; itemIndex++)
			{
				playerInfo.itemStacks[(int)itemIndex] = inventory.GetItemCount(itemIndex);
			}
			playerInfo.equipment = new EquipmentIndex[inventory.GetEquipmentSlotCount()];
			for (uint num = 0u; num < playerInfo.equipment.Length; num++)
			{
				playerInfo.equipment[num] = inventory.GetEquipment(num).equipmentIndex;
			}
			playerInfo.finalMessageToken = playerCharacterMasterController.finalMessageTokenServer;
			return playerInfo;
		}

		public static void ToXml(XElement element, PlayerInfo playerInfo)
		{
			element.RemoveAll();
			element.Add(HGXml.ToXml("name", playerInfo.name));
			element.Add(HGXml.ToXml("bodyName", playerInfo.bodyName));
			element.Add(HGXml.ToXml("killerBodyName", playerInfo.killerBodyName));
			element.Add(HGXml.ToXml("isDead", playerInfo.isDead));
			element.Add(HGXml.ToXml("statSheet", playerInfo.statSheet));
			element.Add(HGXml.ToXml("itemAcquisitionOrder", playerInfo.itemAcquisitionOrder));
			element.Add(HGXml.ToXml("itemStacks", playerInfo.itemStacks, itemStacksRules));
			element.Add(HGXml.ToXml("equipment", playerInfo.equipment, equipmentRules));
			element.Add(HGXml.ToXml("finalMessageToken", playerInfo.finalMessageToken));
			element.Add(HGXml.ToXml("localPlayerIndex", playerInfo.localPlayerIndex));
			element.Add(HGXml.ToXml("userProfileFileName", playerInfo.userProfileFileName));
		}

		public static bool FromXml(XElement element, ref PlayerInfo playerInfo)
		{
			playerInfo = new PlayerInfo();
			element.Element("name")?.Deserialize(ref playerInfo.name);
			string dest = playerInfo.bodyName;
			element.Element("bodyName")?.Deserialize(ref dest);
			playerInfo.bodyName = dest;
			string dest2 = playerInfo.killerBodyName;
			element.Element("killerBodyName")?.Deserialize(ref dest2);
			playerInfo.killerBodyName = dest2;
			element.Element("isDead")?.Deserialize(ref playerInfo.isDead);
			element.Element("statSheet")?.Deserialize(ref playerInfo.statSheet);
			element.Element("itemAcquisitionOrder")?.Deserialize(ref playerInfo.itemAcquisitionOrder);
			element.Element("itemStacks")?.Deserialize(ref playerInfo.itemStacks, itemStacksRules);
			element.Element("equipment")?.Deserialize(ref playerInfo.equipment, equipmentRules);
			element.Element("finalMessageToken")?.Deserialize(ref playerInfo.finalMessageToken);
			element.Element("localPlayerIndex")?.Deserialize(ref playerInfo.localPlayerIndex);
			element.Element("userProfileFileName")?.Deserialize(ref playerInfo.userProfileFileName);
			return true;
		}

		public static void ArrayToXml(XElement element, PlayerInfo[] playerInfos)
		{
			element.RemoveAll();
			for (int i = 0; i < playerInfos.Length; i++)
			{
				element.Add(HGXml.ToXml("PlayerInfo", playerInfos[i]));
			}
		}

		public static bool ArrayFromXml(XElement element, ref PlayerInfo[] playerInfos)
		{
			playerInfos = (from e in element.Elements()
				where e.Name == "PlayerInfo"
				select e).Select(delegate(XElement e)
			{
				PlayerInfo value = null;
				HGXml.FromXml(e, ref value);
				return value;
			}).ToArray();
			return true;
		}
	}

	private const string currentXmlVersion = "2";

	public Guid runGuid;

	private GameModeIndex gameModeIndex = GameModeIndex.Invalid;

	public GameEndingDef gameEnding;

	public ulong seed;

	public DateTime runStartTimeUtc;

	public DateTime snapshotTimeUtc;

	public Run.FixedTimeStamp snapshotRunTime;

	public float runStopwatchValue;

	public RuleBook ruleBook = new RuleBook();

	private PlayerInfo[] playerInfos = Array.Empty<PlayerInfo>();

	private static string runReportsFolder;

	public string gameModeName
	{
		get
		{
			Run run = gameMode;
			return ((run != null) ? ((Object)run).name : null) ?? "InvalidGameMode";
		}
		set
		{
			gameMode = GameModeCatalog.FindGameModePrefabComponent(value);
		}
	}

	public Run gameMode
	{
		get
		{
			return GameModeCatalog.GetGameModePrefabComponent(gameModeIndex);
		}
		set
		{
			gameModeIndex = value?.gameModeIndex ?? GameModeIndex.Invalid;
		}
	}

	public int playerInfoCount => playerInfos.Length;

	[NotNull]
	public PlayerInfo GetPlayerInfo(int i)
	{
		return playerInfos[i];
	}

	[CanBeNull]
	public PlayerInfo GetPlayerInfoSafe(int i)
	{
		return ArrayUtils.GetSafe<PlayerInfo>(playerInfos, i);
	}

	public int FindPlayerIndex(LocalUser localUser)
	{
		if (localUser != null)
		{
			for (int i = 0; i < playerInfos.Length; i++)
			{
				if (playerInfos[i].localUser == localUser)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public int FindPlayerIndex([CanBeNull] UserProfile userProfile)
	{
		if (userProfile != null)
		{
			for (int i = 0; i < playerInfos.Length; i++)
			{
				if (string.Equals(userProfile.fileName, playerInfos[i].userProfileFileName, StringComparison.OrdinalIgnoreCase))
				{
					return i;
				}
			}
		}
		return -1;
	}

	[CanBeNull]
	public PlayerInfo FindPlayerInfo(LocalUser localUser)
	{
		return ArrayUtils.GetSafe<PlayerInfo>(playerInfos, FindPlayerIndex(localUser));
	}

	[CanBeNull]
	public PlayerInfo FindPlayerInfo([CanBeNull] UserProfile userProfile)
	{
		return ArrayUtils.GetSafe<PlayerInfo>(playerInfos, FindPlayerIndex(userProfile));
	}

	[CanBeNull]
	public PlayerInfo FindFirstPlayerInfo()
	{
		if (playerInfoCount <= 0)
		{
			return null;
		}
		return playerInfos[0];
	}

	public static RunReport Generate([NotNull] Run run, GameEndingDef gameEnding)
	{
		RunReport runReport = new RunReport();
		runReport.runGuid = run.GetUniqueId();
		runReport.gameModeIndex = GameModeCatalog.FindGameModeIndex(((Object)((Component)run).gameObject).name);
		runReport.seed = run.seed;
		runReport.runStartTimeUtc = run.GetStartTimeUtc();
		runReport.snapshotTimeUtc = DateTime.UtcNow;
		runReport.snapshotRunTime = Run.FixedTimeStamp.now;
		runReport.runStopwatchValue = run.GetRunStopwatch();
		runReport.gameEnding = gameEnding;
		runReport.ruleBook.Copy(run.ruleBook);
		runReport.playerInfos = new PlayerInfo[PlayerCharacterMasterController.instances.Count];
		for (int i = 0; i < runReport.playerInfos.Length; i++)
		{
			runReport.playerInfos[i] = PlayerInfo.Generate(PlayerCharacterMasterController.instances[i], gameEnding);
		}
		runReport.ResolveLocalInformation();
		return runReport;
	}

	private void ResolveLocalInformation()
	{
		PlayerInfo[] array = playerInfos;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ResolveLocalInformation();
		}
	}

	public void Write(NetworkWriter writer)
	{
		writer.WriteGuid(in runGuid);
		writer.WritePackedIndex32((int)GameEndingCatalog.GetGameEndingIndex(gameEnding));
		writer.WritePackedIndex32((int)gameModeIndex);
		writer.Write(seed);
		NetworkDateTime networkDateTime = (NetworkDateTime)runStartTimeUtc;
		writer.Write(in networkDateTime);
		networkDateTime = (NetworkDateTime)snapshotTimeUtc;
		writer.Write(in networkDateTime);
		writer.Write(snapshotRunTime);
		writer.Write(runStopwatchValue);
		writer.Write(ruleBook);
		writer.Write((byte)playerInfos.Length);
		for (int i = 0; i < playerInfos.Length; i++)
		{
			playerInfos[i].Write(writer);
		}
	}

	public void Read(NetworkReader reader)
	{
		runGuid = reader.ReadGuid();
		gameEnding = GameEndingCatalog.GetGameEndingDef((GameEndingIndex)reader.ReadPackedIndex32());
		gameModeIndex = (GameModeIndex)reader.ReadPackedIndex32();
		seed = reader.ReadUInt64();
		runStartTimeUtc = (DateTime)reader.ReadNetworkDateTime();
		snapshotTimeUtc = (DateTime)reader.ReadNetworkDateTime();
		snapshotRunTime = reader.ReadFixedTimeStamp();
		runStopwatchValue = reader.ReadSingle();
		reader.ReadRuleBook(ruleBook);
		int newSize = reader.ReadByte();
		Array.Resize(ref playerInfos, newSize);
		for (int i = 0; i < playerInfos.Length; i++)
		{
			if (playerInfos[i] == null)
			{
				playerInfos[i] = new PlayerInfo();
			}
			playerInfos[i].Read(reader);
		}
		Array.Sort(playerInfos, delegate(PlayerInfo a, PlayerInfo b)
		{
			if (a.isLocalPlayer == b.isLocalPlayer)
			{
				if (a.isLocalPlayer)
				{
					return b.localPlayerIndex - a.localPlayerIndex;
				}
				return 0;
			}
			return (!a.isLocalPlayer) ? 1 : (-1);
		});
	}

	public static void ToXml(XElement element, RunReport runReport)
	{
		element.RemoveAll();
		element.Add(HGXml.ToXml("version", "2"));
		element.Add(HGXml.ToXml("runGuid", runReport.runGuid));
		element.Add(HGXml.ToXml("gameModeName", runReport.gameModeName));
		element.Add(HGXml.ToXml("gameEnding", runReport.gameEnding));
		element.Add(HGXml.ToXml("seed", runReport.seed));
		element.Add(HGXml.ToXml("runStartTimeUtc", runReport.runStartTimeUtc));
		element.Add(HGXml.ToXml("snapshotTimeUtc", runReport.snapshotTimeUtc));
		element.Add(HGXml.ToXml("snapshotRunTime", runReport.snapshotRunTime));
		element.Add(HGXml.ToXml("runStopwatchValue", runReport.runStopwatchValue));
		element.Add(HGXml.ToXml("ruleBook", runReport.ruleBook));
		element.Add(HGXml.ToXml("playerInfos", runReport.playerInfos));
	}

	public static bool FromXml(XElement element, ref RunReport runReport)
	{
		string dest = "NO_VERSION";
		element.Element("version")?.Deserialize(ref dest);
		if (dest != "2" && !(dest == "1"))
		{
			Debug.LogFormat("Could not load RunReport with non-upgradeable version \"{0}\".", new object[1] { dest });
			runReport = null;
			return false;
		}
		element.Element("runGuid")?.Deserialize(ref runReport.runGuid);
		string dest2 = runReport.gameModeName;
		element.Element("gameModeName")?.Deserialize(ref dest2);
		runReport.gameModeName = dest2;
		element.Element("gameEnding")?.Deserialize(ref runReport.gameEnding);
		element.Element("seed")?.Deserialize(ref runReport.seed);
		element.Element("runStartTimeUtc")?.Deserialize(ref runReport.runStartTimeUtc);
		element.Element("snapshotTimeUtc")?.Deserialize(ref runReport.snapshotTimeUtc);
		element.Element("snapshotRunTime")?.Deserialize(ref runReport.snapshotRunTime);
		element.Element("runStopwatchValue")?.Deserialize(ref runReport.runStopwatchValue);
		element.Element("ruleBook")?.Deserialize(ref runReport.ruleBook);
		element.Element("playerInfos")?.Deserialize(ref runReport.playerInfos);
		return true;
	}

	[SystemInitializer(new Type[]
	{
		typeof(AchievementManager),
		typeof(BodyCatalog),
		typeof(EquipmentCatalog),
		typeof(GameModeCatalog),
		typeof(ItemCatalog),
		typeof(SceneCatalog),
		typeof(StatDef),
		typeof(SurvivorCatalog),
		typeof(UnlockableCatalog)
	})]
	private static void Init()
	{
		runReportsFolder = Application.dataPath + "/RunReports/";
		HGXml.Register<RunReport>(ToXml, FromXml);
		HGXml.Register<PlayerInfo>(PlayerInfo.ToXml, PlayerInfo.FromXml);
		HGXml.Register<PlayerInfo[]>(PlayerInfo.ArrayToXml, PlayerInfo.ArrayFromXml);
	}

	[NotNull]
	private static string FileNameToPath([NotNull] string fileName)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}{1}.xml", runReportsFolder, fileName);
	}

	[CanBeNull]
	public static RunReport Load([NotNull] string fileName)
	{
		string text = FileNameToPath(fileName);
		try
		{
			XElement xElement = XDocument.Load(text).Element("RunReport");
			if (xElement == null)
			{
				Debug.LogFormat("Could not load RunReport {0}: {1}", new object[2] { text, "File is malformed." });
				return null;
			}
			RunReport runReport = new RunReport();
			FromXml(xElement, ref runReport);
			return runReport;
		}
		catch (Exception ex)
		{
			Debug.LogFormat("Could not load RunReport {0}: {1}", new object[2] { text, ex.Message });
			return null;
		}
	}

	public static bool Save([NotNull] RunReport runReport, [NotNull] string fileName)
	{
		string text = FileNameToPath(fileName);
		try
		{
			if (!Directory.Exists(runReportsFolder))
			{
				Directory.CreateDirectory(runReportsFolder);
			}
			XDocument xDocument = new XDocument();
			xDocument.Add(HGXml.ToXml("RunReport", runReport));
			xDocument.Save(text);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogFormat("Could not save RunReport {0}: {1}", new object[2] { text, ex.Message });
			return false;
		}
	}

	public static void TestSerialization(RunReport runReport)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		NetworkWriter val = new NetworkWriter();
		runReport.Write(val);
		NetworkReader reader = new NetworkReader(val.AsArray());
		new RunReport().Read(reader);
	}
}
