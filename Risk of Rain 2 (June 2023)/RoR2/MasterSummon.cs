using System;
using JetBrains.Annotations;
using RoR2.CharacterAI;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class MasterSummon
{
	public struct MasterSummonReport
	{
		public CharacterMaster summonMasterInstance;

		public CharacterMaster leaderMasterInstance;
	}

	public interface IInventorySetupCallback
	{
		void SetupSummonedInventory([NotNull] MasterSummon masterSummon, [NotNull] Inventory summonedInventory);
	}

	public GameObject masterPrefab;

	public Vector3 position;

	public Quaternion rotation;

	public GameObject summonerBodyObject;

	public TeamIndex? teamIndexOverride;

	public bool ignoreTeamMemberLimit;

	public Action<CharacterMaster> preSpawnSetupCallback;

	public Loadout loadout;

	public bool? useAmbientLevel;

	[CanBeNull]
	public Inventory inventoryToCopy;

	[CanBeNull]
	public Func<ItemIndex, bool> inventoryItemCopyFilter;

	public IInventorySetupCallback inventorySetupCallback;

	public static event Action<MasterSummonReport> onServerMasterSummonGlobal;

	public CharacterMaster Perform()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		TeamIndex teamIndex = TeamIndex.None;
		if (teamIndexOverride.HasValue)
		{
			teamIndex = teamIndexOverride.Value;
		}
		else
		{
			if (!Object.op_Implicit((Object)(object)summonerBodyObject))
			{
				Debug.LogErrorFormat("Cannot spawn master {0}: No team specified.", new object[1] { masterPrefab });
				return null;
			}
			teamIndex = TeamComponent.GetObjectTeam(summonerBodyObject);
		}
		if (!ignoreTeamMemberLimit)
		{
			TeamDef teamDef = TeamCatalog.GetTeamDef(teamIndex);
			if (teamDef == null)
			{
				Debug.LogErrorFormat("Attempting to spawn master {0} on TeamIndex.None. Is this intentional?", new object[1] { masterPrefab });
				return null;
			}
			if (teamDef != null && teamDef.softCharacterLimit <= TeamComponent.GetTeamMembers(teamIndex).Count)
			{
				return null;
			}
		}
		CharacterBody characterBody = null;
		CharacterMaster characterMaster = null;
		SkinDef skinDef = null;
		if (Object.op_Implicit((Object)(object)summonerBodyObject))
		{
			characterBody = summonerBodyObject.GetComponent<CharacterBody>();
			skinDef = SkinCatalog.FindCurrentSkinDefForBodyInstance(summonerBodyObject);
		}
		if (Object.op_Implicit((Object)(object)characterBody))
		{
			characterMaster = characterBody.master;
		}
		Inventory inventory = characterMaster?.inventory;
		GameObject val = Object.Instantiate<GameObject>(masterPrefab, position, rotation);
		CharacterMaster component = val.GetComponent<CharacterMaster>();
		component.teamIndex = teamIndex;
		Loadout loadout = Loadout.RequestInstance();
		this.loadout?.Copy(loadout);
		if (Object.op_Implicit((Object)(object)skinDef))
		{
			SkinDef.MinionSkinReplacement[] minionSkinReplacements = skinDef.minionSkinReplacements;
			if (minionSkinReplacements.Length != 0)
			{
				for (int i = 0; i < minionSkinReplacements.Length; i++)
				{
					BodyIndex bodyIndex = BodyCatalog.FindBodyIndex(minionSkinReplacements[i].minionBodyPrefab);
					int num = SkinCatalog.FindLocalSkinIndexForBody(bodyIndex, minionSkinReplacements[i].minionSkin);
					if (num != -1)
					{
						loadout.bodyLoadoutManager.SetSkinIndex(bodyIndex, (uint)num);
					}
				}
			}
		}
		component.SetLoadoutServer(loadout);
		Loadout.ReturnInstance(loadout);
		CharacterMaster characterMaster2 = characterMaster;
		if (Object.op_Implicit((Object)(object)characterMaster2) && Object.op_Implicit((Object)(object)characterMaster2.minionOwnership.ownerMaster))
		{
			characterMaster2 = characterMaster2.minionOwnership.ownerMaster;
		}
		component.minionOwnership.SetOwner(characterMaster2);
		if (Object.op_Implicit((Object)(object)summonerBodyObject))
		{
			AIOwnership component2 = val.GetComponent<AIOwnership>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				if (Object.op_Implicit((Object)(object)characterMaster))
				{
					component2.ownerMaster = characterMaster;
				}
				CharacterBody component3 = summonerBodyObject.GetComponent<CharacterBody>();
				if (Object.op_Implicit((Object)(object)component3))
				{
					CharacterMaster master = component3.master;
					if (Object.op_Implicit((Object)(object)master))
					{
						component2.ownerMaster = master;
					}
				}
			}
			BaseAI component4 = val.GetComponent<BaseAI>();
			if (Object.op_Implicit((Object)(object)component4))
			{
				component4.leader.gameObject = summonerBodyObject;
			}
		}
		if (Object.op_Implicit((Object)(object)inventoryToCopy))
		{
			component.inventory.CopyEquipmentFrom(inventoryToCopy);
			component.inventory.CopyItemsFrom(inventoryToCopy, inventoryItemCopyFilter ?? Inventory.defaultItemCopyFilterDelegate);
		}
		inventorySetupCallback?.SetupSummonedInventory(this, component.inventory);
		bool flag = false;
		if (!useAmbientLevel.HasValue)
		{
			if (Object.op_Implicit((Object)(object)inventory) && inventory.GetItemCount(RoR2Content.Items.UseAmbientLevel) > 0)
			{
				flag = true;
			}
		}
		else
		{
			flag = useAmbientLevel.Value;
		}
		if (flag)
		{
			component.inventory.GiveItem(RoR2Content.Items.UseAmbientLevel);
		}
		if (Object.op_Implicit((Object)(object)inventory) && useAmbientLevel != false)
		{
			component.inventory.GiveItem(RoR2Content.Items.UseAmbientLevel, inventory.GetItemCount(RoR2Content.Items.UseAmbientLevel));
		}
		preSpawnSetupCallback?.Invoke(component);
		NetworkServer.Spawn(val);
		component.Respawn(position, rotation);
		MasterSummon.onServerMasterSummonGlobal?.Invoke(new MasterSummonReport
		{
			leaderMasterInstance = characterMaster2,
			summonMasterInstance = component
		});
		return component;
	}
}
