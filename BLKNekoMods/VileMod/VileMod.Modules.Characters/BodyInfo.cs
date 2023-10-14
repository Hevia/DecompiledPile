using HG.BlendableTypes;
using RoR2;
using UnityEngine;

namespace VileMod.Modules.Characters;

internal class BodyInfo
{
	public string bodyName = "";

	public string bodyNameToken = "";

	public string subtitleNameToken = "";

	public string bodyNameToClone = "Commando";

	public Color bodyColor = Color.white;

	public Texture characterPortrait = null;

	public float sortPosition = 100f;

	public GameObject crosshair = null;

	public GameObject podPrefab = null;

	public float maxHealth = 100f;

	public float healthRegen = 1f;

	public float armor = 0f;

	public float shield = 0f;

	public int jumpCount = 1;

	public float damage = 12f;

	public float attackSpeed = 1f;

	public float crit = 1f;

	public float moveSpeed = 7f;

	public float acceleration = 80f;

	public float jumpPower = 15f;

	public bool autoCalculateLevelStats = true;

	public float healthGrowth = 30.000002f;

	public float regenGrowth = 0.2f;

	public float armorGrowth = 0f;

	public float shieldGrowth = 0f;

	public float damageGrowth = 2.4f;

	public float attackSpeedGrowth = 0f;

	public float critGrowth = 0f;

	public float moveSpeedGrowth = 0f;

	public float jumpPowerGrowth = 0f;

	public Vector3 aimOriginPosition = new Vector3(0f, 1.6f, 0f);

	public Vector3 modelBasePosition = new Vector3(0f, -0.92f, 0f);

	public Vector3 cameraPivotPosition = new Vector3(0f, 0.8f, 0f);

	public float cameraParamsVerticalOffset = 1.37f;

	public float cameraParamsDepth = -10f;

	private CharacterCameraParams _cameraParams;

	public CharacterCameraParams cameraParams
	{
		get
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_cameraParams == (Object)null)
			{
				_cameraParams = ScriptableObject.CreateInstance<CharacterCameraParams>();
				_cameraParams.data.minPitch = BlendableFloat.op_Implicit(-70f);
				_cameraParams.data.maxPitch = BlendableFloat.op_Implicit(70f);
				_cameraParams.data.wallCushion = BlendableFloat.op_Implicit(0.1f);
				_cameraParams.data.pivotVerticalOffset = BlendableFloat.op_Implicit(cameraParamsVerticalOffset);
				_cameraParams.data.idealLocalCameraPos = BlendableVector3.op_Implicit(new Vector3(0f, 0f, cameraParamsDepth));
			}
			return _cameraParams;
		}
		set
		{
			_cameraParams = value;
		}
	}
}
