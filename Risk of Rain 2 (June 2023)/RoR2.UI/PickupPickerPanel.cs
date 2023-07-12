using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class PickupPickerPanel : MonoBehaviour
{
	public GridLayoutGroup gridlayoutGroup;

	public RectTransform buttonContainer;

	public GameObject buttonPrefab;

	public Image[] coloredImages;

	public Image[] darkColoredImages;

	public int maxColumnCount;

	private UIElementAllocator<MPButton> buttonAllocator;

	public PickupPickerController pickerController { get; set; }

	private void Awake()
	{
		buttonAllocator = new UIElementAllocator<MPButton>(buttonContainer, buttonPrefab);
		buttonAllocator.onCreateElement = OnCreateButton;
		gridlayoutGroup.constraint = (Constraint)1;
		gridlayoutGroup.constraintCount = maxColumnCount;
	}

	private void OnCreateButton(int index, MPButton button)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		((UnityEvent)((Button)button).onClick).AddListener((UnityAction)delegate
		{
			pickerController.SubmitChoice(index);
		});
	}

	public void SetPickupOptions(PickupPickerController.Option[] options)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		buttonAllocator.AllocateElements(options.Length);
		ReadOnlyCollection<MPButton> elements = buttonAllocator.elements;
		Sprite sprite = LegacyResourcesAPI.Load<Sprite>("Textures/MiscIcons/texUnlockIcon");
		if (options.Length != 0)
		{
			PickupDef pickupDef = PickupCatalog.GetPickupDef(options[0].pickupIndex);
			Color baseColor = pickupDef.baseColor;
			Color darkColor = pickupDef.darkColor;
			Image[] array = coloredImages;
			foreach (Image obj in array)
			{
				((Graphic)obj).color = ((Graphic)obj).color * baseColor;
			}
			array = darkColoredImages;
			foreach (Image obj2 in array)
			{
				((Graphic)obj2).color = ((Graphic)obj2).color * darkColor;
			}
		}
		for (int j = 0; j < options.Length; j++)
		{
			MPButton mPButton = elements[j];
			int num = j - j % maxColumnCount;
			int num2 = j % maxColumnCount;
			int num3 = num2 - maxColumnCount;
			int num4 = num2 - 1;
			int num5 = num2 + 1;
			int num6 = num2 + maxColumnCount;
			Navigation navigation = ((Selectable)mPButton).navigation;
			((Navigation)(ref navigation)).mode = (Mode)4;
			if (num4 >= 0)
			{
				MPButton selectOnLeft = elements[num + num4];
				((Navigation)(ref navigation)).selectOnLeft = (Selectable)(object)selectOnLeft;
			}
			if (num5 < maxColumnCount && num + num5 < options.Length)
			{
				MPButton selectOnRight = elements[num + num5];
				((Navigation)(ref navigation)).selectOnRight = (Selectable)(object)selectOnRight;
			}
			if (num + num3 >= 0)
			{
				MPButton selectOnUp = elements[num + num3];
				((Navigation)(ref navigation)).selectOnUp = (Selectable)(object)selectOnUp;
			}
			if (num + num6 < options.Length)
			{
				MPButton selectOnDown = elements[num + num6];
				((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)selectOnDown;
			}
			((Selectable)mPButton).navigation = navigation;
			ref PickupPickerController.Option reference = ref options[j];
			PickupDef pickupDef2 = PickupCatalog.GetPickupDef(reference.pickupIndex);
			Image component = ((Component)((Component)mPButton).GetComponent<ChildLocator>().FindChild("Icon")).GetComponent<Image>();
			if (reference.available)
			{
				((Graphic)component).color = Color.white;
				component.sprite = pickupDef2?.iconSprite;
				((Selectable)mPButton).interactable = true;
			}
			else
			{
				((Graphic)component).color = Color.gray;
				component.sprite = sprite;
				((Selectable)mPButton).interactable = false;
			}
		}
	}
}
