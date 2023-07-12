using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class SummonMasterBehavior : NetworkBehaviour
{
	[Tooltip("The master to spawn")]
	public GameObject masterPrefab;

	public bool callOnEquipmentSpentOnPurchase;

	public bool destroyAfterSummoning = true;

	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	[Server]
	public void OpenSummon(Interactor activator)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.SummonMasterBehavior::OpenSummon(RoR2.Interactor)' called on client");
		}
		else
		{
			OpenSummonReturnMaster(activator);
		}
	}

	[Server]
	public CharacterMaster OpenSummonReturnMaster(Interactor activator)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'RoR2.CharacterMaster RoR2.SummonMasterBehavior::OpenSummonReturnMaster(RoR2.Interactor)' called on client");
			return null;
		}
		float num = 0f;
		CharacterMaster characterMaster = new MasterSummon
		{
			masterPrefab = masterPrefab,
			position = ((Component)this).transform.position + Vector3.up * num,
			rotation = ((Component)this).transform.rotation,
			summonerBodyObject = ((activator != null) ? ((Component)activator).gameObject : null),
			ignoreTeamMemberLimit = true,
			useAmbientLevel = true
		}.Perform();
		if (Object.op_Implicit((Object)(object)characterMaster))
		{
			GameObject bodyObject = characterMaster.GetBodyObject();
			if (Object.op_Implicit((Object)(object)bodyObject))
			{
				ModelLocator component = bodyObject.GetComponent<ModelLocator>();
				if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.modelTransform))
				{
					TemporaryOverlay temporaryOverlay = ((Component)component.modelTransform).gameObject.AddComponent<TemporaryOverlay>();
					temporaryOverlay.duration = 0.5f;
					temporaryOverlay.animateShaderAlpha = true;
					temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					temporaryOverlay.destroyComponentOnEnd = true;
					temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matSummonDrone");
					temporaryOverlay.AddToCharacerModel(((Component)component.modelTransform).GetComponent<CharacterModel>());
				}
			}
		}
		if (destroyAfterSummoning)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		return characterMaster;
	}

	public void OnEnable()
	{
		if (callOnEquipmentSpentOnPurchase)
		{
			PurchaseInteraction.onEquipmentSpentOnPurchase += OnEquipmentSpentOnPurchase;
		}
	}

	public void OnDisable()
	{
		if (callOnEquipmentSpentOnPurchase)
		{
			PurchaseInteraction.onEquipmentSpentOnPurchase -= OnEquipmentSpentOnPurchase;
		}
	}

	[Server]
	private void OnEquipmentSpentOnPurchase(PurchaseInteraction purchaseInteraction, Interactor interactor, EquipmentIndex equipmentIndex)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.SummonMasterBehavior::OnEquipmentSpentOnPurchase(RoR2.PurchaseInteraction,RoR2.Interactor,RoR2.EquipmentIndex)' called on client");
		}
		else if ((Object)(object)purchaseInteraction == (Object)(object)((Component)this).GetComponent<PurchaseInteraction>())
		{
			CharacterMaster characterMaster = OpenSummonReturnMaster(interactor);
			if (Object.op_Implicit((Object)(object)characterMaster))
			{
				characterMaster.inventory.SetEquipmentIndex(equipmentIndex);
			}
		}
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
