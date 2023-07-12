using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using HG;
using RoR2.HudOverlay;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(CharacterBody))]
[RequireComponent(typeof(InputBankTest))]
[RequireComponent(typeof(TeamComponent))]
public class VoidSurvivorController : NetworkBehaviour, IOnTakeDamageServerReceiver, IOnDamageDealtServerReceiver
{
	[Header("Cached Components")]
	public CharacterBody characterBody;

	public Animator characterAnimator;

	public EntityStateMachine corruptionModeStateMachine;

	public EntityStateMachine bodyStateMachine;

	public EntityStateMachine weaponStateMachine;

	[Header("Corruption Values")]
	public float maxCorruption;

	public float minimumCorruptionPerVoidItem;

	public float corruptionPerSecondInCombat;

	public float corruptionPerSecondOutOfCombat;

	public float corruptionForFullDamage;

	public float corruptionForFullHeal;

	public float corruptionPerCrit;

	public float corruptionDeltaThresholdToAnimate;

	[Header("Corruption Mode")]
	public BuffDef corruptedBuffDef;

	public float corruptionFractionPerSecondWhileCorrupted;

	[Header("UI")]
	[SerializeField]
	public GameObject overlayPrefab;

	[SerializeField]
	public string overlayChildLocatorEntry;

	private ChildLocator overlayInstanceChildLocator;

	private Animator overlayInstanceAnimator;

	private OverlayController overlayController;

	private List<ImageFillController> fillUiList = new List<ImageFillController>();

	private TextMeshProUGUI uiCorruptionText;

	private int voidItemCount;

	[SyncVar(hook = "OnCorruptionModified")]
	private float _corruption;

	public float corruption => _corruption;

	public float corruptionFraction => corruption / maxCorruption;

	public float corruptionPercentage => corruptionFraction * 100f;

	public float minimumCorruption => minimumCorruptionPerVoidItem * (float)voidItemCount;

	public bool isFullCorruption => corruption >= maxCorruption;

	public bool isCorrupted
	{
		get
		{
			if (Object.op_Implicit((Object)(object)characterBody))
			{
				return characterBody.HasBuff(corruptedBuffDef);
			}
			return false;
		}
	}

	public bool isPermanentlyCorrupted => minimumCorruption >= maxCorruption;

	private HealthComponent bodyHealthComponent => characterBody.healthComponent;

	public float Network_corruption
	{
		get
		{
			return _corruption;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnCorruptionModified(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref _corruption, 1u);
		}
	}

	private void OnEnable()
	{
		OverlayCreationParams overlayCreationParams = default(OverlayCreationParams);
		overlayCreationParams.prefab = overlayPrefab;
		overlayCreationParams.childLocatorEntry = overlayChildLocatorEntry;
		OverlayCreationParams overlayCreationParams2 = overlayCreationParams;
		overlayController = HudOverlayManager.AddOverlay(((Component)this).gameObject, overlayCreationParams2);
		overlayController.onInstanceAdded += OnOverlayInstanceAdded;
		overlayController.onInstanceRemove += OnOverlayInstanceRemoved;
		if (Object.op_Implicit((Object)(object)characterBody))
		{
			characterBody.onInventoryChanged += OnInventoryChanged;
			if (NetworkServer.active)
			{
				HealthComponent.onCharacterHealServer += OnCharacterHealServer;
			}
		}
	}

	private void OnDisable()
	{
		if (overlayController != null)
		{
			overlayController.onInstanceAdded -= OnOverlayInstanceAdded;
			overlayController.onInstanceRemove -= OnOverlayInstanceRemoved;
			fillUiList.Clear();
			HudOverlayManager.RemoveOverlay(overlayController);
		}
		if (Object.op_Implicit((Object)(object)characterBody))
		{
			characterBody.onInventoryChanged -= OnInventoryChanged;
			if (NetworkServer.active)
			{
				HealthComponent.onCharacterHealServer -= OnCharacterHealServer;
			}
		}
	}

	private void FixedUpdate()
	{
		float num = 0f;
		num = ((!characterBody.HasBuff(corruptedBuffDef)) ? (characterBody.outOfCombat ? corruptionPerSecondOutOfCombat : corruptionPerSecondInCombat) : (num + corruptionFractionPerSecondWhileCorrupted * (maxCorruption - minimumCorruption)));
		if (NetworkServer.active && !characterBody.HasBuff(RoR2Content.Buffs.HiddenInvincibility))
		{
			AddCorruption(num * Time.fixedDeltaTime);
		}
		UpdateUI();
		if (Object.op_Implicit((Object)(object)characterAnimator))
		{
			characterAnimator.SetFloat("corruptionFraction", corruptionFraction);
		}
	}

	private void UpdateUI()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		foreach (ImageFillController fillUi in fillUiList)
		{
			fillUi.SetTValue(corruption / maxCorruption);
		}
		if (Object.op_Implicit((Object)(object)overlayInstanceChildLocator))
		{
			overlayInstanceChildLocator.FindChild("CorruptionThreshold").rotation = Quaternion.Euler(0f, 0f, Mathf.InverseLerp(0f, maxCorruption, corruption) * -360f);
			overlayInstanceChildLocator.FindChild("MinCorruptionThreshold").rotation = Quaternion.Euler(0f, 0f, Mathf.InverseLerp(0f, maxCorruption, minimumCorruption) * -360f);
		}
		if (Object.op_Implicit((Object)(object)overlayInstanceAnimator))
		{
			overlayInstanceAnimator.SetFloat("corruption", corruption);
			overlayInstanceAnimator.SetBool("isCorrupted", isCorrupted);
		}
		if (Object.op_Implicit((Object)(object)uiCorruptionText))
		{
			StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
			stringBuilder.AppendInt(Mathf.FloorToInt(corruption), 1u, 3u).Append("%");
			((TMP_Text)uiCorruptionText).SetText(stringBuilder);
			StringBuilderPool.ReturnStringBuilder(stringBuilder);
		}
	}

	private void OnOverlayInstanceAdded(OverlayController controller, GameObject instance)
	{
		fillUiList.Add(instance.GetComponent<ImageFillController>());
		uiCorruptionText = instance.GetComponentInChildren<TextMeshProUGUI>();
		overlayInstanceChildLocator = instance.GetComponent<ChildLocator>();
		overlayInstanceAnimator = instance.GetComponent<Animator>();
	}

	private void OnOverlayInstanceRemoved(OverlayController controller, GameObject instance)
	{
		fillUiList.Remove(instance.GetComponent<ImageFillController>());
	}

	private void OnCharacterHealServer(HealthComponent healthComponent, float amount, ProcChainMask procChainMask)
	{
		if (healthComponent == bodyHealthComponent && !procChainMask.HasProc(ProcType.VoidSurvivorCrush))
		{
			float num = amount / bodyHealthComponent.fullCombinedHealth;
			AddCorruption(num * corruptionForFullHeal);
		}
	}

	public void OnDamageDealtServer(DamageReport damageReport)
	{
		if (damageReport.damageInfo.crit)
		{
			AddCorruption(damageReport.damageInfo.procCoefficient * corruptionPerCrit);
		}
	}

	public void OnTakeDamageServer(DamageReport damageReport)
	{
		float num = damageReport.damageDealt / bodyHealthComponent.fullCombinedHealth;
		if (!damageReport.damageInfo.procChainMask.HasProc(ProcType.VoidSurvivorCrush))
		{
			AddCorruption(num * corruptionForFullDamage);
		}
	}

	private void OnInventoryChanged()
	{
		voidItemCount = 0;
		Inventory inventory = characterBody.inventory;
		if (Object.op_Implicit((Object)(object)inventory))
		{
			voidItemCount = inventory.GetTotalItemCountOfTier(ItemTier.VoidTier1) + inventory.GetTotalItemCountOfTier(ItemTier.VoidTier2) + inventory.GetTotalItemCountOfTier(ItemTier.VoidTier3) + inventory.GetTotalItemCountOfTier(ItemTier.VoidBoss);
		}
	}

	[Server]
	public void AddCorruption(float amount)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.VoidSurvivorController::AddCorruption(System.Single)' called on client");
		}
		else
		{
			Network_corruption = Mathf.Clamp(corruption + amount, minimumCorruption, maxCorruption);
		}
	}

	private void OnCorruptionModified(float newCorruption)
	{
		if (Object.op_Implicit((Object)(object)overlayInstanceAnimator) && Mathf.Abs(newCorruption - corruption) > corruptionDeltaThresholdToAnimate)
		{
			overlayInstanceAnimator.SetTrigger("corruptionIncreased");
		}
		Network_corruption = newCorruption;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(_corruption);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(_corruption);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			_corruption = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			OnCorruptionModified(reader.ReadSingle());
		}
	}

	public override void PreStartClient()
	{
	}
}
