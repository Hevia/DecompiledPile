using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class ServerBrowserStripController : MonoBehaviour
{
	private RectTransform rectTransform;

	public HGTextMeshProUGUI nameLabel;

	public HGTextMeshProUGUI addressLabel;

	public HGTextMeshProUGUI playerCountLabel;

	public HGTextMeshProUGUI latencyLabel;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		rectTransform = (RectTransform)((Component)this).transform;
	}
}
