using EntityStates;
using RoR2.CharacterAI;
using UnityEngine;

namespace RoR2.VoidRaidCrab;

public class VoidRaidCrabAISkillDriverController : MonoBehaviour
{
	[SerializeField]
	private AISkillDriver[] weaponSkillDrivers;

	[SerializeField]
	private AISkillDriver[] bodySkillDrivers;

	[SerializeField]
	private AISkillDriver[] gauntletSkillDrivers;

	[SerializeField]
	private AISkillDriver finalStandSkillDriver;

	[SerializeField]
	private CharacterMaster master;

	[SerializeField]
	private string bodyStateMachineName = "Body";

	[SerializeField]
	private AISkillDriver debugForceSkillDriver;

	private EntityStateMachine bodyStateMachine;

	private HealthComponent healthComponent;

	private bool isGauntletSkillAvailable;

	public bool CanUseWeaponSkills()
	{
		if (bodyStateMachine.state is GenericCharacterMain && !isGauntletSkillAvailable)
		{
			return !ShouldUseFinalStandSkill();
		}
		return false;
	}

	public bool CanUseBodySkills()
	{
		if (bodyStateMachine.IsInMainState() && !isGauntletSkillAvailable)
		{
			return !ShouldUseFinalStandSkill();
		}
		return false;
	}

	public bool ShouldUseGauntletSkills()
	{
		if (isGauntletSkillAvailable && bodyStateMachine.IsInMainState())
		{
			return !ShouldUseFinalStandSkill();
		}
		return false;
	}

	public bool ShouldUseFinalStandSkill()
	{
		return CanUsePhaseSkillDriver(finalStandSkillDriver);
	}

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)master) && (!Object.op_Implicit((Object)(object)bodyStateMachine) || !Object.op_Implicit((Object)(object)healthComponent)))
		{
			GameObject bodyObject = master.GetBodyObject();
			if (Object.op_Implicit((Object)(object)bodyObject))
			{
				if (!Object.op_Implicit((Object)(object)bodyStateMachine))
				{
					bodyStateMachine = EntityStateMachine.FindByCustomName(bodyObject, bodyStateMachineName);
				}
				if (!Object.op_Implicit((Object)(object)healthComponent))
				{
					healthComponent = bodyObject.GetComponent<HealthComponent>();
				}
			}
		}
		if (!Object.op_Implicit((Object)(object)bodyStateMachine))
		{
			return;
		}
		isGauntletSkillAvailable = false;
		AISkillDriver[] array = gauntletSkillDrivers;
		foreach (AISkillDriver driver in array)
		{
			isGauntletSkillAvailable = isGauntletSkillAvailable || CanUsePhaseSkillDriver(driver);
		}
		if (Object.op_Implicit((Object)(object)debugForceSkillDriver))
		{
			SetSkillDriversEnabled(weaponSkillDrivers, driverEnabled: false);
			SetSkillDriversEnabled(bodySkillDrivers, driverEnabled: false);
			SetSkillDriversEnabled(gauntletSkillDrivers, driverEnabled: false);
			if (Object.op_Implicit((Object)(object)finalStandSkillDriver))
			{
				((Behaviour)finalStandSkillDriver).enabled = false;
			}
			bool flag = false;
			array = weaponSkillDrivers;
			foreach (AISkillDriver aISkillDriver in array)
			{
				if (aISkillDriver == debugForceSkillDriver)
				{
					((Behaviour)aISkillDriver).enabled = CanUseWeaponSkills();
					flag = true;
				}
			}
			if (!flag)
			{
				array = bodySkillDrivers;
				foreach (AISkillDriver aISkillDriver2 in array)
				{
					if (aISkillDriver2 == debugForceSkillDriver)
					{
						((Behaviour)aISkillDriver2).enabled = CanUseBodySkills();
						flag = true;
					}
				}
			}
			if (!flag)
			{
				array = gauntletSkillDrivers;
				foreach (AISkillDriver aISkillDriver3 in array)
				{
					if (aISkillDriver3 == debugForceSkillDriver)
					{
						((Behaviour)aISkillDriver3).enabled = ShouldUseGauntletSkills();
						flag = true;
					}
				}
			}
			if (!flag && debugForceSkillDriver == finalStandSkillDriver)
			{
				((Behaviour)debugForceSkillDriver).enabled = ShouldUseFinalStandSkill();
				flag = true;
			}
			if (!flag)
			{
				((Behaviour)debugForceSkillDriver).enabled = true;
			}
		}
		else
		{
			SetSkillDriversEnabled(weaponSkillDrivers, CanUseWeaponSkills());
			SetSkillDriversEnabled(bodySkillDrivers, CanUseBodySkills());
			SetSkillDriversEnabled(gauntletSkillDrivers, ShouldUseGauntletSkills());
			if (Object.op_Implicit((Object)(object)finalStandSkillDriver))
			{
				((Behaviour)finalStandSkillDriver).enabled = ShouldUseFinalStandSkill();
			}
		}
	}

	private void SetSkillDriversEnabled(AISkillDriver[] skillDrivers, bool driverEnabled)
	{
		for (int i = 0; i < skillDrivers.Length; i++)
		{
			((Behaviour)skillDrivers[i]).enabled = driverEnabled;
		}
	}

	private bool CanUsePhaseSkillDriver(AISkillDriver driver)
	{
		if (Object.op_Implicit((Object)(object)driver))
		{
			float num = 1f;
			if (Object.op_Implicit((Object)(object)healthComponent))
			{
				num = healthComponent.combinedHealthFraction;
			}
			if (num < driver.maxUserHealthFraction)
			{
				if (driver.timesSelected >= driver.maxTimesSelected)
				{
					return driver.maxTimesSelected < 0;
				}
				return true;
			}
			return false;
		}
		return false;
	}
}
