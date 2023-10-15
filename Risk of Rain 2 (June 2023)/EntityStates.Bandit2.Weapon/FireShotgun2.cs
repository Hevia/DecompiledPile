using RoR2;
using UnityEngine;

namespace EntityStates.Bandit2.Weapon;

public class FireShotgun2 : Bandit2FirePrimaryBase
{
	[SerializeField]
	public float minFixedSpreadYaw;

	[SerializeField]
	public float maxFixedSpreadYaw;

	protected override void ModifyBullet(BulletAttack bulletAttack)
	{
		base.ModifyBullet(bulletAttack);
		bulletAttack.bulletCount = 1u;
	}

	protected override void FireBullet(Ray aimRay)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		StartAimMode(aimRay, 3f);
		DoFireEffects();
		PlayFireAnimation();
		AddRecoil(-1f * recoilAmplitudeY, -1.5f * recoilAmplitudeY, -1f * recoilAmplitudeX, 1f * recoilAmplitudeX);
		if (base.isAuthority)
		{
			Vector3 val = Vector3.Cross(Vector3.up, aimRay.direction);
			Vector3 val2 = Vector3.Cross(aimRay.direction, val);
			float num = 0f;
			if (Object.op_Implicit((Object)(object)base.characterBody))
			{
				num = base.characterBody.spreadBloomAngle;
			}
			float num2 = 0f;
			float num3 = 0f;
			if (bulletCount > 1)
			{
				num3 = Random.Range(minFixedSpreadYaw + num, maxFixedSpreadYaw + num) * 2f;
				num2 = num3 / (float)(bulletCount - 1);
			}
			Vector3 val3 = Quaternion.AngleAxis((0f - num3) * 0.5f, val2) * aimRay.direction;
			Quaternion val4 = Quaternion.AngleAxis(num2, val2);
			Ray aimRay2 = default(Ray);
			((Ray)(ref aimRay2))._002Ector(aimRay.origin, val3);
			for (int i = 0; i < bulletCount; i++)
			{
				BulletAttack bulletAttack = GenerateBulletAttack(aimRay2);
				ModifyBullet(bulletAttack);
				bulletAttack.Fire();
				((Ray)(ref aimRay2)).direction = val4 * ((Ray)(ref aimRay2)).direction;
			}
		}
	}
}
