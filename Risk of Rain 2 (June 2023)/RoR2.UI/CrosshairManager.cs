using System.Collections.Generic;
using RoR2.WwiseUtils;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class CrosshairManager : MonoBehaviour
{
	public delegate void ShouldShowCrosshairDelegate(CrosshairManager crosshairManager, ref bool shouldShow);

	public delegate void PickCrosshairDelegate(CrosshairManager crosshairManager, ref GameObject crosshairPrefab);

	[Tooltip("The transform which should act as the container for the crosshair.")]
	public RectTransform container;

	public CameraRigController cameraRigController;

	[Tooltip("The hitmarker image.")]
	public Image hitmarker;

	private float hitmarkerAlpha;

	private float hitmarkerTimer;

	private const float hitmarkerDuration = 0.2f;

	private GameObject currentCrosshairPrefab;

	public CrosshairController crosshairController;

	private HudElement crosshairHudElement;

	private RtpcSetter rtpcDamageDirection;

	private static readonly List<CrosshairManager> instancesList;

	public event ShouldShowCrosshairDelegate shouldShowCrosshairGlobal;

	public event PickCrosshairDelegate pickCrosshairGlobal;

	private void OnEnable()
	{
		instancesList.Add(this);
		rtpcDamageDirection = new RtpcSetter("damageDirection", ((Component)RoR2Application.instance).gameObject);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	private static void StaticLateUpdate()
	{
		for (int i = 0; i < instancesList.Count; i++)
		{
			instancesList[i].DoLateUpdate();
		}
	}

	private void DoLateUpdate()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)cameraRigController))
		{
			UpdateCrosshair(Object.op_Implicit((Object)(object)cameraRigController.target) ? cameraRigController.target.GetComponent<CharacterBody>() : null, cameraRigController.crosshairWorldPosition, cameraRigController.uiCam);
		}
		UpdateHitMarker();
	}

	private void UpdateCrosshair(CharacterBody targetBody, Vector3 crosshairWorldPosition, Camera uiCamera)
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		GameObject crosshairPrefab = null;
		bool shouldShow = Object.op_Implicit((Object)(object)targetBody) && !Object.op_Implicit((Object)(object)targetBody.currentVehicle);
		this.shouldShowCrosshairGlobal?.Invoke(this, ref shouldShow);
		if (shouldShow && Object.op_Implicit((Object)(object)targetBody))
		{
			if (!cameraRigController.hasOverride)
			{
				if (Object.op_Implicit((Object)(object)targetBody) && targetBody.healthComponent.alive)
				{
					crosshairPrefab = (targetBody.isSprinting ? LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/SprintingCrosshair") : (targetBody.hideCrosshair ? null : CrosshairUtils.GetCrosshairPrefabForBody(targetBody)));
				}
			}
			else
			{
				crosshairPrefab = CrosshairUtils.GetCrosshairPrefabForBody(targetBody);
			}
			this.pickCrosshairGlobal?.Invoke(this, ref crosshairPrefab);
		}
		if ((Object)(object)crosshairPrefab != (Object)(object)currentCrosshairPrefab)
		{
			if (Object.op_Implicit((Object)(object)crosshairController))
			{
				Object.Destroy((Object)(object)((Component)crosshairController).gameObject);
				crosshairController = null;
			}
			if (Object.op_Implicit((Object)(object)crosshairPrefab))
			{
				GameObject val = Object.Instantiate<GameObject>(crosshairPrefab, (Transform)(object)container);
				crosshairController = val.GetComponent<CrosshairController>();
				crosshairHudElement = val.GetComponent<HudElement>();
			}
			currentCrosshairPrefab = crosshairPrefab;
		}
		if (Object.op_Implicit((Object)(object)crosshairController))
		{
			((RectTransform)((Component)crosshairController).gameObject.transform).anchoredPosition = new Vector2(0.5f, 0.5f);
		}
		if (Object.op_Implicit((Object)(object)crosshairHudElement))
		{
			crosshairHudElement.targetCharacterBody = targetBody;
		}
	}

	public void RefreshHitmarker(bool crit)
	{
		hitmarkerTimer = 0.2f;
		((Component)hitmarker).gameObject.SetActive(false);
		((Component)hitmarker).gameObject.SetActive(true);
		Util.PlaySound("Play_UI_hit", ((Component)RoR2Application.instance).gameObject);
		if (crit)
		{
			Util.PlaySound("Play_UI_crit", ((Component)RoR2Application.instance).gameObject);
		}
	}

	private void UpdateHitMarker()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		hitmarkerAlpha = Mathf.Pow(hitmarkerTimer / 0.2f, 0.75f);
		hitmarkerTimer = Mathf.Max(0f, hitmarkerTimer - Time.deltaTime);
		if (Object.op_Implicit((Object)(object)hitmarker))
		{
			Color color = ((Graphic)hitmarker).color;
			color.a = hitmarkerAlpha;
			((Graphic)hitmarker).color = color;
		}
	}

	private static void HandleHitMarker(DamageDealtMessage damageDealtMessage)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < instancesList.Count; i++)
		{
			CrosshairManager crosshairManager = instancesList[i];
			if (!Object.op_Implicit((Object)(object)crosshairManager.cameraRigController))
			{
				continue;
			}
			GameObject target = crosshairManager.cameraRigController.target;
			if ((Object)(object)damageDealtMessage.attacker == (Object)(object)target)
			{
				crosshairManager.RefreshHitmarker(damageDealtMessage.crit);
			}
			else if ((Object)(object)damageDealtMessage.victim == (Object)(object)target)
			{
				Transform transform = ((Component)crosshairManager.cameraRigController).transform;
				Vector3 position = transform.position;
				Vector3 forward = transform.forward;
				_ = transform.position;
				Vector3 val = damageDealtMessage.position - position;
				float num = Vector2.SignedAngle(new Vector2(val.x, val.z), new Vector2(forward.x, forward.z));
				if (num < 0f)
				{
					num += 360f;
				}
				crosshairManager.rtpcDamageDirection.value = num;
				Util.PlaySound("Play_UI_takeDamage", ((Component)RoR2Application.instance).gameObject);
			}
		}
	}

	static CrosshairManager()
	{
		instancesList = new List<CrosshairManager>();
		GlobalEventManager.onClientDamageNotified += HandleHitMarker;
		RoR2Application.onLateUpdate += StaticLateUpdate;
	}
}
