using Rewired;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2;

[RequireComponent(typeof(MPEventSystemLocator))]
public class InputStickVisualizer : MonoBehaviour
{
	[Header("Move")]
	public Scrollbar moveXBar;

	public Scrollbar moveYBar;

	public TextMeshProUGUI moveXLabel;

	public TextMeshProUGUI moveYLabel;

	[Header("Aim")]
	public Scrollbar aimXBar;

	public Scrollbar aimYBar;

	public TextMeshProUGUI aimXLabel;

	public TextMeshProUGUI aimYLabel;

	public Scrollbar aimStickPostSmoothingXBar;

	public Scrollbar aimStickPostSmoothingYBar;

	public Scrollbar aimStickPostDualZoneXBar;

	public Scrollbar aimStickPostDualZoneYBar;

	public Scrollbar aimStickPostExponentXBar;

	public Scrollbar aimStickPostExponentYBar;

	private MPEventSystemLocator eventSystemLocator;

	private void Awake()
	{
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
	}

	private Player GetPlayer()
	{
		return eventSystemLocator.eventSystem?.player;
	}

	private CameraRigController GetCameraRigController()
	{
		if (CameraRigController.readOnlyInstancesList.Count <= 0)
		{
			return null;
		}
		return CameraRigController.readOnlyInstancesList[0];
	}

	private void SetBarValues(Vector2 vector, Scrollbar scrollbarX, Scrollbar scrollbarY)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)scrollbarX))
		{
			scrollbarX.value = Util.Remap(vector.x, -1f, 1f, 0f, 1f);
		}
		if (Object.op_Implicit((Object)(object)scrollbarY))
		{
			scrollbarY.value = Util.Remap(vector.y, -1f, 1f, 0f, 1f);
		}
	}

	private void Update()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		Player player = GetPlayer();
		if (Object.op_Implicit((Object)(object)GetCameraRigController()) && player != null)
		{
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(player.GetAxis(0), player.GetAxis(1));
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(player.GetAxis(16), player.GetAxis(17));
			SetBarValues(val, moveXBar, moveYBar);
			SetBarValues(val2, aimXBar, aimYBar);
			((TMP_Text)moveXLabel).text = $"move.x={val.x:0.0000}";
			((TMP_Text)moveYLabel).text = $"move.y={val.y:0.0000}";
			((TMP_Text)aimXLabel).text = $"aim.x={val2.x:0.0000}";
			((TMP_Text)aimYLabel).text = $"aim.y={val2.y:0.0000}";
		}
	}
}
