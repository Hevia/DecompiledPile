using System;
using HG;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(RectTransform))]
public class HealthBar : MonoBehaviour
{
	private struct BarInfo
	{
		public bool enabled;

		public Color color;

		public Sprite sprite;

		public Type imageType;

		public float normalizedXMin;

		public float normalizedXMax;

		public float sizeDelta;
	}

	private struct BarInfoCollection
	{
		public BarInfo trailingUnderHealthbarInfo;

		public BarInfo instantHealthbarInfo;

		public BarInfo trailingOverHealthbarInfo;

		public BarInfo shieldBarInfo;

		public BarInfo curseBarInfo;

		public BarInfo barrierBarInfo;

		public BarInfo cullBarInfo;

		public BarInfo flashBarInfo;

		public BarInfo magneticBarInfo;

		public BarInfo ospBarInfo;

		public BarInfo lowHealthOverBarInfo;

		public BarInfo lowHealthUnderBarInfo;

		public int GetActiveCount()
		{
			int count = 0;
			Check(ref lowHealthUnderBarInfo);
			Check(ref trailingUnderHealthbarInfo);
			Check(ref instantHealthbarInfo);
			Check(ref trailingOverHealthbarInfo);
			Check(ref shieldBarInfo);
			Check(ref curseBarInfo);
			Check(ref barrierBarInfo);
			Check(ref flashBarInfo);
			Check(ref cullBarInfo);
			Check(ref magneticBarInfo);
			Check(ref ospBarInfo);
			Check(ref lowHealthOverBarInfo);
			return count;
			void Check(ref BarInfo field)
			{
				count += (field.enabled ? 1 : 0);
			}
		}
	}

	private HealthComponent _source;

	public HealthBarStyle style;

	[Tooltip("The container rect for the actual bars.")]
	public RectTransform barContainer;

	public RectTransform eliteBackdropRectTransform;

	public Image criticallyHurtImage;

	public Image deadImage;

	public float maxLastHitTimer = 1f;

	public bool scaleHealthbarWidth;

	public float minHealthbarWidth;

	public float maxHealthbarWidth;

	public float minHealthbarHealth;

	public float maxHealthbarHealth;

	private float displayStringCurrentHealth;

	private float displayStringFullHealth;

	private RectTransform rectTransform;

	private float cachedFractionalValue = 1f;

	private float healthFractionVelocity;

	private bool healthCritical;

	private bool isInventoryCheckDirty = true;

	private bool hasLowHealthItem;

	[NonSerialized]
	public CharacterBody viewerBody;

	private static readonly Color infusionPanelColor = Color32.op_Implicit(new Color32((byte)231, (byte)84, (byte)58, byte.MaxValue));

	private static readonly Color voidPanelColor = Color32.op_Implicit(new Color32((byte)217, (byte)123, byte.MaxValue, byte.MaxValue));

	private static readonly Color voidShieldsColor = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)57, (byte)199, byte.MaxValue));

	private float theta;

	private UIElementAllocator<Image> barAllocator;

	private BarInfoCollection barInfoCollection;

	public TextMeshProUGUI currentHealthText;

	public TextMeshProUGUI fullHealthText;

	public HealthComponent source
	{
		get
		{
			return _source;
		}
		set
		{
			if ((Object)(object)_source != (Object)(object)value)
			{
				RemoveInventoryChangedHandler();
				_source = value;
				healthFractionVelocity = 0f;
				cachedFractionalValue = (Object.op_Implicit((Object)(object)_source) ? (_source.health / _source.fullCombinedHealth) : 0f);
				AddInventoryChangedHandler();
				isInventoryCheckDirty = true;
			}
		}
	}

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		rectTransform = (RectTransform)((Component)this).transform;
		barAllocator = new UIElementAllocator<Image>(barContainer, style.barPrefab);
	}

	private void Start()
	{
		UpdateHealthbar(0f);
	}

	public void Update()
	{
		UpdateHealthbar(Time.deltaTime);
	}

	private void OnEnable()
	{
		AddInventoryChangedHandler();
		isInventoryCheckDirty = true;
	}

	private void OnDisable()
	{
		RemoveInventoryChangedHandler();
	}

	private void ApplyBars()
	{
		int i = 0;
		barAllocator.AllocateElements(barInfoCollection.GetActiveCount());
		HandleBar(ref barInfoCollection.lowHealthUnderBarInfo);
		HandleBar(ref barInfoCollection.trailingUnderHealthbarInfo);
		HandleBar(ref barInfoCollection.instantHealthbarInfo);
		HandleBar(ref barInfoCollection.trailingOverHealthbarInfo);
		HandleBar(ref barInfoCollection.shieldBarInfo);
		HandleBar(ref barInfoCollection.curseBarInfo);
		HandleBar(ref barInfoCollection.barrierBarInfo);
		HandleBar(ref barInfoCollection.flashBarInfo);
		HandleBar(ref barInfoCollection.cullBarInfo);
		HandleBar(ref barInfoCollection.magneticBarInfo);
		HandleBar(ref barInfoCollection.ospBarInfo);
		HandleBar(ref barInfoCollection.lowHealthOverBarInfo);
		void HandleBar(ref BarInfo barInfo)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Expected O, but got Unknown
			if (barInfo.enabled)
			{
				Image obj = barAllocator.elements[i];
				obj.type = barInfo.imageType;
				obj.sprite = barInfo.sprite;
				((Graphic)obj).color = barInfo.color;
				SetRectPosition((RectTransform)((Component)obj).transform, barInfo.normalizedXMin, barInfo.normalizedXMax, barInfo.sizeDelta);
				int num = i + 1;
				i = num;
			}
		}
		static void SetRectPosition(RectTransform rectTransform, float xMin, float xMax, float sizeDelta)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			rectTransform.anchorMin = new Vector2(xMin, 0f);
			rectTransform.anchorMax = new Vector2(xMax, 1f);
			rectTransform.anchoredPosition = Vector2.zero;
			rectTransform.sizeDelta = new Vector2(sizeDelta * 0.5f + 1f, sizeDelta + 1f);
		}
	}

	private void UpdateBarInfos()
	{
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		float currentBarEnd;
		if (Object.op_Implicit((Object)(object)source))
		{
			if (isInventoryCheckDirty)
			{
				CheckInventory();
			}
			HealthComponent.HealthBarValues healthBarValues = source.GetHealthBarValues();
			currentBarEnd = 0f;
			_ = source.fullCombinedHealth;
			ref BarInfo lowHealthUnderBarInfo = ref barInfoCollection.lowHealthUnderBarInfo;
			lowHealthUnderBarInfo.enabled = hasLowHealthItem && source.isHealthLow;
			lowHealthUnderBarInfo.normalizedXMin = 0f;
			lowHealthUnderBarInfo.normalizedXMax = HealthComponent.lowHealthFraction * (1f - healthBarValues.curseFraction);
			ApplyStyle(ref lowHealthUnderBarInfo, ref style.lowHealthUnderStyle);
			cachedFractionalValue = Mathf.SmoothDamp(cachedFractionalValue, healthBarValues.healthFraction, ref healthFractionVelocity, 0.2f, float.PositiveInfinity, Time.deltaTime);
			ref BarInfo trailingUnderHealthbarInfo = ref barInfoCollection.trailingUnderHealthbarInfo;
			trailingUnderHealthbarInfo.normalizedXMin = 0f;
			trailingUnderHealthbarInfo.normalizedXMax = Mathf.Max(cachedFractionalValue, healthBarValues.healthFraction);
			trailingUnderHealthbarInfo.enabled = !trailingUnderHealthbarInfo.normalizedXMax.Equals(trailingUnderHealthbarInfo.normalizedXMin);
			ApplyStyle(ref trailingUnderHealthbarInfo, ref style.trailingUnderHealthBarStyle);
			ref BarInfo instantHealthbarInfo = ref barInfoCollection.instantHealthbarInfo;
			instantHealthbarInfo.enabled = healthBarValues.healthFraction > 0f;
			ApplyStyle(ref instantHealthbarInfo, ref style.instantHealthBarStyle);
			AddBar(ref instantHealthbarInfo, healthBarValues.healthFraction);
			ref BarInfo trailingOverHealthbarInfo = ref barInfoCollection.trailingOverHealthbarInfo;
			trailingOverHealthbarInfo.normalizedXMin = 0f;
			trailingOverHealthbarInfo.normalizedXMax = Mathf.Min(cachedFractionalValue + 0.01f, healthBarValues.healthFraction);
			trailingOverHealthbarInfo.enabled = !trailingOverHealthbarInfo.normalizedXMax.Equals(trailingOverHealthbarInfo.normalizedXMin);
			ApplyStyle(ref trailingOverHealthbarInfo, ref style.trailingOverHealthBarStyle);
			if (healthBarValues.isVoid)
			{
				trailingOverHealthbarInfo.color = voidPanelColor;
			}
			if (healthBarValues.isBoss || healthBarValues.hasInfusion)
			{
				trailingOverHealthbarInfo.color = infusionPanelColor;
			}
			if (healthCritical && style.flashOnHealthCritical)
			{
				trailingOverHealthbarInfo.color = GetCriticallyHurtColor();
			}
			ref BarInfo shieldBarInfo = ref barInfoCollection.shieldBarInfo;
			shieldBarInfo.enabled = healthBarValues.shieldFraction > 0f;
			ApplyStyle(ref shieldBarInfo, ref style.shieldBarStyle);
			if (healthBarValues.hasVoidShields)
			{
				shieldBarInfo.color = voidShieldsColor;
			}
			AddBar(ref shieldBarInfo, healthBarValues.shieldFraction);
			ref BarInfo curseBarInfo = ref barInfoCollection.curseBarInfo;
			curseBarInfo.enabled = healthBarValues.curseFraction > 0f;
			ApplyStyle(ref curseBarInfo, ref style.curseBarStyle);
			curseBarInfo.normalizedXMin = 1f - healthBarValues.curseFraction;
			curseBarInfo.normalizedXMax = 1f;
			ref BarInfo barrierBarInfo = ref barInfoCollection.barrierBarInfo;
			barrierBarInfo.enabled = source.barrier > 0f;
			ApplyStyle(ref barrierBarInfo, ref style.barrierBarStyle);
			barrierBarInfo.normalizedXMin = 0f;
			barrierBarInfo.normalizedXMax = healthBarValues.barrierFraction;
			ref BarInfo magneticBarInfo = ref barInfoCollection.magneticBarInfo;
			magneticBarInfo.enabled = source.magnetiCharge > 0f;
			magneticBarInfo.normalizedXMin = 0f;
			magneticBarInfo.normalizedXMax = healthBarValues.magneticFraction;
			magneticBarInfo.color = new Color(75f, 0f, 130f);
			ApplyStyle(ref magneticBarInfo, ref style.magneticStyle);
			float num = healthBarValues.cullFraction;
			if (healthBarValues.isElite && Object.op_Implicit((Object)(object)viewerBody))
			{
				num = Mathf.Max(num, viewerBody.executeEliteHealthFraction);
			}
			ref BarInfo cullBarInfo = ref barInfoCollection.cullBarInfo;
			cullBarInfo.enabled = num > 0f;
			cullBarInfo.normalizedXMin = 0f;
			cullBarInfo.normalizedXMax = num;
			ApplyStyle(ref cullBarInfo, ref style.cullBarStyle);
			float ospFraction = healthBarValues.ospFraction;
			ref BarInfo ospBarInfo = ref barInfoCollection.ospBarInfo;
			ospBarInfo.enabled = ospFraction > 0f;
			ospBarInfo.normalizedXMin = 0f;
			ospBarInfo.normalizedXMax = ospFraction;
			ApplyStyle(ref ospBarInfo, ref style.ospStyle);
			ref BarInfo lowHealthOverBarInfo = ref barInfoCollection.lowHealthOverBarInfo;
			lowHealthOverBarInfo.enabled = hasLowHealthItem && !source.isHealthLow;
			lowHealthOverBarInfo.normalizedXMin = HealthComponent.lowHealthFraction * (1f - healthBarValues.curseFraction);
			lowHealthOverBarInfo.normalizedXMax = HealthComponent.lowHealthFraction * (1f - healthBarValues.curseFraction) + 0.005f;
			ApplyStyle(ref lowHealthOverBarInfo, ref style.lowHealthOverStyle);
		}
		void AddBar(ref BarInfo barInfo, float fraction)
		{
			if (barInfo.enabled)
			{
				barInfo.normalizedXMin = currentBarEnd;
				currentBarEnd = (barInfo.normalizedXMax = barInfo.normalizedXMin + fraction);
			}
		}
		static void ApplyStyle(ref BarInfo barInfo, ref HealthBarStyle.BarStyle barStyle)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			barInfo.enabled &= barStyle.enabled;
			barInfo.color = barStyle.baseColor;
			barInfo.sprite = barStyle.sprite;
			barInfo.imageType = barStyle.imageType;
			barInfo.sizeDelta = barStyle.sizeDelta;
		}
	}

	private void UpdateHealthbar(float deltaTime)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		if (Object.op_Implicit((Object)(object)source))
		{
			CharacterBody body = source.body;
			bool isElite = body.isElite;
			float fullHealth = source.fullHealth;
			if (Object.op_Implicit((Object)(object)eliteBackdropRectTransform))
			{
				if (isElite)
				{
					num += 1f;
				}
				((Component)eliteBackdropRectTransform).gameObject.SetActive(isElite);
			}
			if (scaleHealthbarWidth && Object.op_Implicit((Object)(object)body))
			{
				float num2 = Util.Remap(Mathf.Clamp((body.baseMaxHealth + body.baseMaxShield) * num, 0f, maxHealthbarHealth), minHealthbarHealth, maxHealthbarHealth, minHealthbarWidth, maxHealthbarWidth);
				rectTransform.sizeDelta = new Vector2(num2, rectTransform.sizeDelta.y);
			}
			if (Object.op_Implicit((Object)(object)currentHealthText))
			{
				float num3 = Mathf.Ceil(source.combinedHealth);
				if (num3 != displayStringCurrentHealth)
				{
					displayStringCurrentHealth = num3;
					((TMP_Text)currentHealthText).text = num3.ToString();
				}
			}
			if (Object.op_Implicit((Object)(object)fullHealthText))
			{
				float num4 = Mathf.Ceil(fullHealth);
				if (num4 != displayStringFullHealth)
				{
					displayStringFullHealth = num4;
					((TMP_Text)fullHealthText).text = num4.ToString();
				}
			}
			healthCritical = source.isHealthLow && source.alive;
			if (Object.op_Implicit((Object)(object)criticallyHurtImage))
			{
				if (healthCritical)
				{
					((Behaviour)criticallyHurtImage).enabled = true;
					((Graphic)criticallyHurtImage).color = GetCriticallyHurtColor();
				}
				else
				{
					((Behaviour)criticallyHurtImage).enabled = false;
				}
			}
			if (Object.op_Implicit((Object)(object)deadImage))
			{
				((Behaviour)deadImage).enabled = !source.alive;
			}
		}
		UpdateBarInfos();
		ApplyBars();
	}

	private void AddInventoryChangedHandler()
	{
		if (Object.op_Implicit((Object)(object)source?.body?.inventory))
		{
			source.body.inventory.onInventoryChanged += OnInventoryChanged;
		}
	}

	private void RemoveInventoryChangedHandler()
	{
		if (Object.op_Implicit((Object)(object)source?.body?.inventory))
		{
			source.body.inventory.onInventoryChanged -= OnInventoryChanged;
		}
	}

	private void OnInventoryChanged()
	{
		isInventoryCheckDirty = true;
	}

	private void CheckInventory()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		isInventoryCheckDirty = false;
		Inventory inventory = source?.body?.inventory;
		if (!Object.op_Implicit((Object)(object)inventory))
		{
			return;
		}
		hasLowHealthItem = false;
		Enumerator<ItemIndex> enumerator = ItemCatalog.GetItemsWithTag(ItemTag.LowHealth).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ItemIndex current = enumerator.Current;
				if (inventory.GetItemCount(current) > 0)
				{
					hasLowHealthItem = true;
					break;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
	}

	public static Color GetCriticallyHurtColor()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.FloorToInt(Time.fixedTime * 10f) % 2 != 0)
		{
			return Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.Teleporter));
		}
		return Color.white;
	}
}
