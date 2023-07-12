using RoR2.Stats;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class CaptainDefenseMatrixController : MonoBehaviour
{
	public int defenseMatrixToGrantPlayer = 1;

	public int defenseMatrixToGrantMechanicalAllies = 1;

	private CharacterBody characterBody;

	private void Awake()
	{
		characterBody = ((Component)this).GetComponent<CharacterBody>();
	}

	private void Start()
	{
		if (NetworkServer.active)
		{
			TryGrantItem();
		}
	}

	private void OnEnable()
	{
		if (NetworkServer.active)
		{
			MasterSummon.onServerMasterSummonGlobal += OnServerMasterSummonGlobal;
		}
	}

	private void OnDisable()
	{
		if (NetworkServer.active)
		{
			MasterSummon.onServerMasterSummonGlobal -= OnServerMasterSummonGlobal;
		}
	}

	private void TryGrantItem()
	{
		if (Object.op_Implicit((Object)(object)characterBody.master))
		{
			bool flag = false;
			if (Object.op_Implicit((Object)(object)characterBody.master.playerStatsComponent))
			{
				flag = characterBody.master.playerStatsComponent.currentStats.GetStatValueDouble(PerBodyStatDef.totalTimeAlive, BodyCatalog.GetBodyName(characterBody.bodyIndex)) > 0.0;
			}
			if (!flag && characterBody.master.inventory.GetItemCount(RoR2Content.Items.CaptainDefenseMatrix) <= 0)
			{
				characterBody.master.inventory.GiveItem(RoR2Content.Items.CaptainDefenseMatrix, defenseMatrixToGrantPlayer);
			}
		}
	}

	private void OnServerMasterSummonGlobal(MasterSummon.MasterSummonReport summonReport)
	{
		if (!Object.op_Implicit((Object)(object)characterBody.master) || !((Object)(object)characterBody.master == (Object)(object)summonReport.leaderMasterInstance))
		{
			return;
		}
		CharacterMaster summonMasterInstance = summonReport.summonMasterInstance;
		if (Object.op_Implicit((Object)(object)summonMasterInstance))
		{
			CharacterBody body = summonMasterInstance.GetBody();
			if (Object.op_Implicit((Object)(object)body) && (body.bodyFlags & CharacterBody.BodyFlags.Mechanical) != 0)
			{
				summonMasterInstance.inventory.GiveItem(RoR2Content.Items.CaptainDefenseMatrix, defenseMatrixToGrantMechanicalAllies);
			}
		}
	}
}
