using EntityStates.LaserTurbine;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(GenericOwnership))]
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(EntityStateMachine))]
[RequireComponent(typeof(NetworkStateMachine))]
public class LaserTurbineController : NetworkBehaviour
{
	public float visualSpinRate = 7200f;

	public Transform chargeIndicator;

	public Transform spinIndicator;

	public Transform turbineDisplayRoot;

	public bool showTurbineDisplay;

	public string spinRtpc;

	public float spinRtpcScale;

	public float visualSpin;

	public float visualSpinDecayRate = 0.2f;

	private GenericOwnership genericOwnership;

	private CharacterBody cachedOwnerBody;

	public float charge { get; private set; }

	private void Awake()
	{
		genericOwnership = ((Component)this).GetComponent<GenericOwnership>();
		genericOwnership.onOwnerChanged += OnOwnerChanged;
	}

	private void Update()
	{
		if (NetworkClient.active)
		{
			UpdateClient();
		}
	}

	private void FixedUpdate()
	{
		int killChargeCount = 0;
		if (Object.op_Implicit((Object)(object)cachedOwnerBody))
		{
			killChargeCount = cachedOwnerBody.GetBuffCount(RoR2Content.Buffs.LaserTurbineKillCharge);
		}
		charge = CalcCurrentChargeValue(killChargeCount);
		if (Object.op_Implicit((Object)(object)turbineDisplayRoot))
		{
			((Component)turbineDisplayRoot).gameObject.SetActive(showTurbineDisplay);
		}
	}

	private void OnEnable()
	{
		if (NetworkServer.active)
		{
			GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobalServer;
		}
	}

	private void OnDisable()
	{
		GlobalEventManager.onCharacterDeathGlobal -= OnCharacterDeathGlobalServer;
	}

	[Client]
	private void UpdateClient()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			Debug.LogWarning((object)"[Client] function 'System.Void RoR2.LaserTurbineController::UpdateClient()' called on server");
			return;
		}
		if (visualSpin <= charge)
		{
			visualSpin = charge;
		}
		else
		{
			visualSpin -= visualSpinDecayRate * Time.deltaTime;
		}
		visualSpin = Mathf.Max(visualSpin, 0f);
		float num = HGMath.CircleAreaToRadius(visualSpin * HGMath.CircleRadiusToArea(1f));
		chargeIndicator.localScale = new Vector3(num, num, num);
		Vector3 localEulerAngles = spinIndicator.localEulerAngles;
		localEulerAngles.y += visualSpin * Time.deltaTime * visualSpinRate;
		spinIndicator.localEulerAngles = localEulerAngles;
		AkSoundEngine.SetRTPCValue(spinRtpc, visualSpin * spinRtpcScale, ((Component)this).gameObject);
	}

	[Server]
	public void ExpendCharge()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.LaserTurbineController::ExpendCharge()' called on client");
		}
		else if (Object.op_Implicit((Object)(object)cachedOwnerBody))
		{
			cachedOwnerBody.ClearTimedBuffs(RoR2Content.Buffs.LaserTurbineKillCharge);
		}
	}

	private void OnCharacterDeathGlobalServer(DamageReport damageReport)
	{
		if (damageReport.attacker == genericOwnership.ownerObject && damageReport.attacker != null)
		{
			OnOwnerKilledOtherServer();
		}
	}

	private void OnOwnerKilledOtherServer()
	{
		if (Object.op_Implicit((Object)(object)cachedOwnerBody))
		{
			cachedOwnerBody.AddTimedBuff(RoR2Content.Buffs.LaserTurbineKillCharge, RechargeState.killChargeDuration, RechargeState.killChargesRequired);
		}
	}

	private void OnOwnerChanged(GameObject newOwner)
	{
		cachedOwnerBody = (Object.op_Implicit((Object)(object)newOwner) ? newOwner.GetComponent<CharacterBody>() : null);
	}

	public float CalcCurrentChargeValue(int killChargeCount)
	{
		return Mathf.Clamp01((float)killChargeCount / (float)RechargeState.killChargesRequired);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
