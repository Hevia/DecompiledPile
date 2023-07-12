using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(Interactor))]
[RequireComponent(typeof(InputBankTest))]
public class InteractionDriver : MonoBehaviour, ILifeBehavior
{
	public bool highlightInteractor;

	private bool inputReceived;

	private NetworkIdentity networkIdentity;

	private InputBankTest inputBank;

	private CharacterBody characterBody;

	private EquipmentSlot equipmentSlot;

	[NonSerialized]
	public GameObject interactableOverride;

	private const float interactableCooldownDuration = 0.25f;

	private float interactableCooldown;

	public Interactor interactor { get; private set; }

	private void Awake()
	{
		networkIdentity = ((Component)this).GetComponent<NetworkIdentity>();
		interactor = ((Component)this).GetComponent<Interactor>();
		inputBank = ((Component)this).GetComponent<InputBankTest>();
		characterBody = ((Component)this).GetComponent<CharacterBody>();
		equipmentSlot = (Object.op_Implicit((Object)(object)characterBody) ? characterBody.equipmentSlot : null);
	}

	private void FixedUpdate()
	{
		if (networkIdentity.hasAuthority)
		{
			interactableCooldown -= Time.fixedDeltaTime;
			inputReceived = inputBank.interact.justPressed || (inputBank.interact.down && interactableCooldown <= 0f);
			if (inputBank.interact.justReleased)
			{
				inputReceived = false;
				interactableCooldown = 0f;
			}
		}
		if (inputReceived)
		{
			GameObject val = FindBestInteractableObject();
			if (Object.op_Implicit((Object)(object)val))
			{
				interactor.AttemptInteraction(val);
				interactableCooldown = 0.25f;
			}
		}
	}

	public GameObject FindBestInteractableObject()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)interactableOverride))
		{
			return interactableOverride;
		}
		float extraRaycastDistance = 0f;
		Ray originalAimRay = default(Ray);
		((Ray)(ref originalAimRay))._002Ector(inputBank.aimOrigin, inputBank.aimDirection);
		Ray raycastRay = CameraRigController.ModifyAimRayIfApplicable(originalAimRay, ((Component)this).gameObject, out extraRaycastDistance);
		float num = interactor.maxInteractionDistance;
		if (Object.op_Implicit((Object)(object)equipmentSlot) && equipmentSlot.equipmentIndex == RoR2Content.Equipment.Recycle.equipmentIndex)
		{
			num *= 2f;
		}
		return interactor.FindBestInteractableObject(raycastRay, num + extraRaycastDistance, ((Ray)(ref originalAimRay)).origin, num);
	}

	static InteractionDriver()
	{
		OutlineHighlight.onPreRenderOutlineHighlight = (Action<OutlineHighlight>)Delegate.Combine(OutlineHighlight.onPreRenderOutlineHighlight, new Action<OutlineHighlight>(OnPreRenderOutlineHighlight));
	}

	private static void OnPreRenderOutlineHighlight(OutlineHighlight outlineHighlight)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)outlineHighlight.sceneCamera) || !Object.op_Implicit((Object)(object)outlineHighlight.sceneCamera.cameraRigController))
		{
			return;
		}
		GameObject target = outlineHighlight.sceneCamera.cameraRigController.target;
		if (!Object.op_Implicit((Object)(object)target))
		{
			return;
		}
		InteractionDriver component = target.GetComponent<InteractionDriver>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		GameObject val = component.FindBestInteractableObject();
		if (!Object.op_Implicit((Object)(object)val))
		{
			return;
		}
		IInteractable component2 = val.GetComponent<IInteractable>();
		Highlight component3 = val.GetComponent<Highlight>();
		if (Object.op_Implicit((Object)(object)component3))
		{
			Color val2 = component3.GetColor();
			if (component2 != null && ((Behaviour)(MonoBehaviour)component2).isActiveAndEnabled && component2.GetInteractability(component.interactor) == Interactability.ConditionsNotMet)
			{
				val2 = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unaffordable));
			}
			outlineHighlight.highlightQueue.Enqueue(new OutlineHighlight.HighlightInfo
			{
				renderer = component3.targetRenderer,
				color = val2 * component3.strength
			});
		}
	}

	public void OnDeathStart()
	{
		((Behaviour)this).enabled = false;
	}
}
