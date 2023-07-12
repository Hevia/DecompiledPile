using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RoR2;

public class Highlight : MonoBehaviour
{
	public enum HighlightColor
	{
		interactive,
		teleporter,
		pickup,
		unavailable
	}

	private static List<Highlight> highlightList = new List<Highlight>();

	private static ReadOnlyCollection<Highlight> _readonlyHighlightList = new ReadOnlyCollection<Highlight>(highlightList);

	private IDisplayNameProvider displayNameProvider;

	[HideInInspector]
	public PickupIndex pickupIndex;

	public Renderer targetRenderer;

	public float strength = 1f;

	public HighlightColor highlightColor;

	public bool isOn;

	public static ReadOnlyCollection<Highlight> readonlyHighlightList => _readonlyHighlightList;

	private void Awake()
	{
		displayNameProvider = ((Component)this).GetComponent<IDisplayNameProvider>();
	}

	public void OnEnable()
	{
		highlightList.Add(this);
	}

	public void OnDisable()
	{
		highlightList.Remove(this);
	}

	public Color GetColor()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		return (Color)(highlightColor switch
		{
			HighlightColor.interactive => Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.Interactable)), 
			HighlightColor.teleporter => Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.Teleporter)), 
			HighlightColor.pickup => PickupCatalog.GetPickupDef(pickupIndex)?.baseColor ?? PickupCatalog.invalidPickupColor, 
			_ => Color.magenta, 
		});
	}
}
