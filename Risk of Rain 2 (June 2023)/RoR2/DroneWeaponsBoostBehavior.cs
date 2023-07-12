using System.Collections.Generic;
using RoR2.Orbs;
using UnityEngine;

namespace RoR2;

public class DroneWeaponsBoostBehavior : CharacterBody.ItemBehavior
{
	private const string controllerPrefabPath = "Prefabs/NetworkedObjects/DroneWeaponsChainGunController";

	private const string missileMuzzleChildName = "MissileMuzzle";

	private const float microMissileDamageCoefficient = 3f;

	private const float microMissileProcCoefficient = 1f;

	private Transform missileMuzzleTransform;

	private GameObject chainGunController;

	public void Start()
	{
		((Component)this).GetComponent<InputBankTest>();
		ModelLocator component = ((Component)this).GetComponent<ModelLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform modelTransform = component.modelTransform;
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				CharacterModel component2 = ((Component)modelTransform).GetComponent<CharacterModel>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					List<GameObject> itemDisplayObjects = component2.GetItemDisplayObjects(DLC1Content.Items.DroneWeaponsDisplay1.itemIndex);
					itemDisplayObjects.AddRange(component2.GetItemDisplayObjects(DLC1Content.Items.DroneWeaponsDisplay2.itemIndex));
					foreach (GameObject item in itemDisplayObjects)
					{
						ChildLocator component3 = item.GetComponent<ChildLocator>();
						if (Object.op_Implicit((Object)(object)component3))
						{
							Transform val = component3.FindChild("MissileMuzzle");
							if (Object.op_Implicit((Object)(object)val))
							{
								missileMuzzleTransform = val;
								break;
							}
						}
					}
				}
			}
		}
		chainGunController = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/DroneWeaponsChainGunController"));
		chainGunController.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(((Component)this).gameObject);
	}

	public void OnDestroy()
	{
		Object.Destroy((Object)(object)chainGunController);
	}

	public void OnEnemyHit(DamageInfo damageInfo, CharacterBody victimBody)
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		CharacterBody component = damageInfo.attacker.GetComponent<CharacterBody>();
		CharacterMaster characterMaster = component?.master;
		if (Object.op_Implicit((Object)(object)characterMaster) && !damageInfo.procChainMask.HasProc(ProcType.MicroMissile) && Util.CheckRoll(10f, characterMaster))
		{
			ProcChainMask procChainMask = damageInfo.procChainMask;
			procChainMask.AddProc(ProcType.MicroMissile);
			HurtBox hurtBox = victimBody.mainHurtBox;
			if (Object.op_Implicit((Object)(object)victimBody.hurtBoxGroup))
			{
				hurtBox = victimBody.hurtBoxGroup.hurtBoxes[Random.Range(0, victimBody.hurtBoxGroup.hurtBoxes.Length)];
			}
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				MicroMissileOrb microMissileOrb = new MicroMissileOrb();
				microMissileOrb.damageValue = component.damage * 3f;
				microMissileOrb.isCrit = Util.CheckRoll(component.crit, characterMaster);
				microMissileOrb.teamIndex = TeamComponent.GetObjectTeam(((Component)component).gameObject);
				microMissileOrb.attacker = ((Component)component).gameObject;
				microMissileOrb.procCoefficient = 1f;
				microMissileOrb.procChainMask = procChainMask;
				microMissileOrb.origin = (Object.op_Implicit((Object)(object)missileMuzzleTransform) ? missileMuzzleTransform.position : component.corePosition);
				microMissileOrb.target = hurtBox;
				microMissileOrb.damageColorIndex = DamageColorIndex.Item;
				OrbManager.instance.AddOrb(microMissileOrb);
			}
		}
	}
}
