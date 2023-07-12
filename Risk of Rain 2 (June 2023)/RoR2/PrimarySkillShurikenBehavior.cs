using RoR2.Projectile;
using UnityEngine;

namespace RoR2;

public class PrimarySkillShurikenBehavior : CharacterBody.ItemBehavior
{
	private const float minSpreadDegrees = 0f;

	private const float rangeSpreadDegrees = 1f;

	private const int numShurikensPerStack = 1;

	private const int numShurikensBase = 2;

	private const string projectilePrefabPath = "Prefabs/Projectiles/ShurikenProjectile";

	private const float totalReloadTime = 10f;

	private const float damageCoefficientBase = 3f;

	private const float damageCoefficientPerStack = 1f;

	private const float force = 0f;

	private SkillLocator skillLocator;

	private float reloadTimer;

	private GameObject projectilePrefab;

	private InputBankTest inputBank;

	private void Awake()
	{
		((Behaviour)this).enabled = false;
	}

	private void Start()
	{
		projectilePrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/ShurikenProjectile");
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)body))
		{
			body.onSkillActivatedServer += OnSkillActivated;
			skillLocator = ((Component)body).GetComponent<SkillLocator>();
			inputBank = ((Component)body).GetComponent<InputBankTest>();
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)body))
		{
			body.onSkillActivatedServer -= OnSkillActivated;
			while (body.HasBuff(DLC1Content.Buffs.PrimarySkillShurikenBuff))
			{
				body.RemoveBuff(DLC1Content.Buffs.PrimarySkillShurikenBuff);
			}
		}
		inputBank = null;
		skillLocator = null;
	}

	private void OnSkillActivated(GenericSkill skill)
	{
		if (skillLocator?.primary == skill && body.GetBuffCount(DLC1Content.Buffs.PrimarySkillShurikenBuff) > 0)
		{
			body.RemoveBuff(DLC1Content.Buffs.PrimarySkillShurikenBuff);
			FireShuriken();
		}
	}

	private void FixedUpdate()
	{
		int num = stack + 2;
		if (body.GetBuffCount(DLC1Content.Buffs.PrimarySkillShurikenBuff) < num)
		{
			float num2 = 10f / (float)num;
			reloadTimer += Time.fixedDeltaTime;
			while (reloadTimer > num2 && body.GetBuffCount(DLC1Content.Buffs.PrimarySkillShurikenBuff) < num)
			{
				body.AddBuff(DLC1Content.Buffs.PrimarySkillShurikenBuff);
				reloadTimer -= num2;
			}
		}
	}

	private void FireShuriken()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		ProjectileManager.instance.FireProjectile(projectilePrefab, ((Ray)(ref aimRay)).origin, Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction) * GetRandomRollPitch(), ((Component)this).gameObject, body.damage * (3f + 1f * (float)stack), 0f, Util.CheckRoll(body.crit, body.master), DamageColorIndex.Item);
	}

	private Ray GetAimRay()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)inputBank))
		{
			return new Ray(inputBank.aimOrigin, inputBank.aimDirection);
		}
		return new Ray(((Component)this).transform.position, ((Component)this).transform.forward);
	}

	protected Quaternion GetRandomRollPitch()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.AngleAxis((float)Random.Range(0, 360), Vector3.forward);
		Quaternion val2 = Quaternion.AngleAxis(0f + Random.Range(0f, 1f), Vector3.left);
		return val * val2;
	}
}
