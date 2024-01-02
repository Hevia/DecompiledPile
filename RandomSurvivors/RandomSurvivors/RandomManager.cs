using System;
using System.Runtime.CompilerServices;
using EntityStates;
using EntityStates.ClayBoss;
using EntityStates.ClayBoss.ClayBossWeapon;
using EntityStates.ClayBruiser.Weapon;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.Croco;
using EntityStates.Drone.DroneWeapon;
using EntityStates.Engi.EngiWeapon;
using EntityStates.EngiTurret.EngiTurretWeapon;
using EntityStates.GlobalSkills.LunarNeedle;
using EntityStates.GoldGat;
using EntityStates.GolemMonster;
using EntityStates.HermitCrab;
using EntityStates.ImpMonster;
using EntityStates.Interactables.StoneGate;
using EntityStates.JellyfishMonster;
using EntityStates.Loader;
using EntityStates.Merc;
using EntityStates.ScavMonster;
using EntityStates.Sniper.SniperWeapon;
using EntityStates.Toolbot;
using EntityStates.Treebot;
using EntityStates.Treebot.Weapon;
using EntityStates.UrchinTurret.Weapon;
using EntityStates.Vulture.Weapon;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace RandomSurvivors;

public class RandomManager : MonoBehaviour
{
	public HudElement myHud;

	public CharacterBody myBody;

	public CharacterMotor myMotor;

	public int mySeed;

	public Color myColor;

	public int myPassive;

	public bool canSprint;

	public bool isFlier;

	public string myAnim;

	public string myAnim2;

	public string prefab0;

	public float damage0 = 0f;

	public float force0;

	public float speed0;

	public float rate0;

	public string prefab1;

	public float damage1 = 0f;

	public float force1;

	public float speed1;

	public float rate1;

	public int multi1;

	public int multi1Type;

	public float damage2 = 0f;

	public float rate2;

	public float force2;

	public int multi2;

	public string tracer2;

	public string impact2;

	public string sfx2;

	public string reload2;

	public DamageType damageType2;

	public DamageType damageType3;

	public float damage3;

	public float force3;

	public float rate3;

	public string swing3;

	public string sfx3;

	public int primaryType;

	private bool setDamage = false;

	private bool isDebug = false;

	public void Awake()
	{
		//IL_0926: Unknown result type (might be due to invalid IL or missing references)
		//IL_090f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0965: Unknown result type (might be due to invalid IL or missing references)
		//IL_094e: Unknown result type (might be due to invalid IL or missing references)
		myBody = ((Component)this).GetComponent<CharacterBody>();
		myHud = ((Component)this).GetComponent<HudElement>();
		myMotor = ((Component)this).GetComponent<CharacterMotor>();
		string[] array = new string[22]
		{
			"ClayPotProjectile", "ElectricWormSeekerProjectile", "EngiGrenadeProjectile", "Fireball", "FireworkProjectile", "GravekeeperHookProjectileSimple", "HermitCrabBombProjectile", "ImpCrawler", "MageFireboltBasic", "MageLightningboltBasic",
			"MagmaOrbProjectile", "PaladinRocket", "RoboBallProjectile", "ScavEnergyCannonProjectile", "SyringeProjectile", "TarSeeker", "Thermite", "TitanRockProjectile", "ToolbotGrenadeLauncherProjectile", "UrchinSeekingProjectile",
			"WindbladeProjectile", "ArtifactShellSeekingSolarFlare"
		};
		string[] array2 = new string[13]
		{
			"tracerbanditpistol", "tracerbanditshotgun", "tracerbarrage", "tracerembers", "tracerengiturret", "tracergoldgat", "tracergolem", "tracernosmoke", "tracersmokechase", "tracersmokeline/tracersmokeline",
			"tracertoolbotnails", "tracertoolbotrebar", "tracerwisp"
		};
		string[] array3 = new string[35]
		{
			"beetleacidimpact", "beetleguardsunderpop", "beetlespitexplosion", "bellbodypartsimpact", "bulletimpactsoft", "characterlandimpact", "coinimpact", "daggerimpact", "dirtimpact", "engiconcussionexplosion",
			"engigrenadeexplosion", "explosionvfx", "fireworkexplosion", "fireworkexplosion2", "fireworkexplosion3", "fmjimpact", "frozenimpacteffect", "hermitcrabbombexplosion", "hitspark1", "hitsparkbanditpistol",
			"hitsparkbarrage", "hitsparkfmj", "impactengiturret", "impacthuntressarrowrain", "impactimpswipe", "impactmercswing", "impactnailgun", "impactpotmobilecannon", "impactspear", "impacttoolbotdash",
			"impactwispember", "missileexplosionvfx", "sawmerangimpact", "stoneimpact", "wispdeath"
		};
		string[] array4 = new string[5] { "MercSwordSlashWhirlwind", "HuntressGlaiveSwing", "ClaymanSwordSwing", "AssassinDaggerSwing", "LemurianBiteTrail" };
		string[] array5 = new string[9]
		{
			GroundLight.comboAttackSoundString,
			GroundLight.finisherAttackSoundString,
			Uppercut.attackSoundString,
			WhirlwindBase.attackSoundString,
			Reload.soundString,
			Slash.slash1Sound,
			Slash.slash3Sound,
			DoubleSlash.slashSoundString,
			FireBuzzsaw.spinDownSoundString
		};
		DamageType[] array6 = new DamageType[13];
		RuntimeHelpers.InitializeArray(array6, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		DamageType[] array7 = (DamageType[])(object)array6;
		DamageType[] array8 = new DamageType[10];
		RuntimeHelpers.InitializeArray(array8, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		DamageType[] array9 = (DamageType[])(object)array8;
		string[] array10 = new string[30]
		{
			FireBarrage.fireBarrageSoundString,
			FireLightsOut.attackSoundString,
			FirePistol2.firePistolSoundString,
			FireShotgun.attackSoundString,
			FireShrapnel.attackSoundString,
			FireSweepBarrage.fireSoundString,
			FireTarball.attackSoundString,
			FireBombardment.shootSoundString,
			MinigunFire.fireSound,
			Slash.slash1Sound,
			Slash.slash3Sound,
			FireGatling.fireGatlingSoundString,
			FireMegaTurret.attackSoundString,
			FireTurret.attackSoundString,
			FireGrenades.attackSoundString,
			FireMines.throwMineSoundString,
			FireGauss.attackSoundString,
			BaseNailgunState.fireSoundString,
			FireLunarNeedle.fireSound,
			GoldGatFire.attackSoundString,
			FireLaser.attackSoundString,
			FireMortar.mortarSoundString,
			JellyNova.novaSoundString,
			FireHook.fireSoundString,
			FireRifle.attackSoundString,
			FireBuzzsaw.impactSoundString,
			FireSyringe.attackSound,
			FireSyringe.finalAttackSound,
			MinigunFire.fireSound,
			FireWindblade.soundString
		};
		string[] array11 = new string[24]
		{
			CastSmokescreen.jumpSoundString,
			PrepBarrage.prepBarrageSoundString,
			PrepLightsOut.prepSoundString,
			Reload.soundString,
			PrepTarBall.prepTarBallSoundString,
			MinigunSpinDown.sound,
			ChargeGrenades.chargeStockSoundString,
			BurrowIn.burrowInSoundString,
			BurrowOut.burrowOutSoundString,
			DoubleSlash.slashSoundString,
			Opening.doorFinishedOpenSoundString,
			JellyNova.chargingSoundString,
			ThrowPylon.soundString,
			PrepFlower2.enterSoundString,
			PrepEnergyCannon.sound,
			PrepSack.sound,
			Reload.soundString,
			AimStunDrone.exitSoundString,
			RecoverAimStunDrone.fireSoundString,
			BurrowDash.impactSoundString,
			CreatePounder.prepSoundString,
			MinigunFire.endSound,
			MinigunSpinDown.sound,
			ChargeWindblade.soundString
		};
		string[] array12 = new string[42]
		{
			"ArchWispCannon", "BeetleQueenSpit", "BellBall", "ClayPotProjectile", "CommandoGrenadeProjectile", "CrocoSpit", "DaggerProjectile", "ElectricOrbProjectile", "EngiMine", "FireTornado",
			"FMJ", "GravekeeperTrackingFireball", "HermitCrabBombProjectile", "ImpCrawler", "ImpVoidspikeProjectile", "LemurianBigFireball", "LemurianBouncyFireball", "MageFirebolt", "MageFireBombProjectile", "MageFirewallPillarProjectile",
			"MageFirewallSeedProjectile", "MageIceBombProjectile", "MageIcewallWalkerProjectile", "MageLightningBombProjectile", "MagmaOrbProjectile", "PaladinRocket", "PoisonOrbProjectile", "ScavEnergyCannonProjectile", "SpiderMine", "SuperRoboBallProjectile",
			"TarBall", "TarSeeker", "Thermite", "TitanRockProjectile", "ToolbotGrenadeLauncherProjectile", "TreebotFlowerSeed", "VagrantCannon", "VagrantTrackingBomb", "WindbladeProjectile", "WispCannon",
			"ArtifactShellSeekingSolarFlare", "EngiHarpoon"
		};
		Random.InitState(mySeed);
		prefab0 = "prefabs/projectiles/" + array[Random.Range(0, array.Length)];
		prefab1 = "prefabs/projectiles/" + array12[Random.Range(0, array12.Length)];
		tracer2 = "prefabs/effects/tracers/" + array2[Random.Range(0, array2.Length)];
		impact2 = "prefabs/effects/impacteffects/" + array3[Random.Range(0, array3.Length)];
		swing3 = "prefabs/effects/" + array4[Random.Range(0, array4.Length)];
		sfx2 = array10[Random.Range(0, array10.Length)];
		sfx3 = array5[Random.Range(0, array5.Length)];
		reload2 = array11[Random.Range(0, array11.Length)];
		force0 = Random.Range(1f, 100f);
		force1 = Random.Range(10f, 1000f);
		force2 = Random.Range(5f, 500f);
		force3 = Random.Range(1f, 100f);
		speed0 = Random.Range(4f, 150f);
		speed1 = Mathf.Clamp(Random.Range(-10f, 110f), 0f, 100f);
		rate0 = Random.Range(0.05f, 0.25f);
		rate1 = Random.Range(0.25f, 1.5f);
		rate2 = Random.Range(0.05f, 1f);
		rate3 = Random.Range(0.1f, 1f);
		multi1 = Mathf.Clamp(Random.Range(-8, 6), 1, 5);
		multi2 = Mathf.Clamp(Random.Range(-14, 5), 1, 4);
		damage0 = Random.Range(3f, 6f);
		damage1 = Random.Range(6f / (float)multi1, 12f / (float)multi1);
		damage2 = Random.Range(2f / (float)multi2, 5f / (float)multi2);
		damage3 = Random.Range(6f, 10f);
		if (rate2 < 0.5f)
		{
			damageType2 = array7[Random.Range(0, array7.Length)];
		}
		else
		{
			damageType2 = array9[Random.Range(0, array9.Length)];
		}
		if (rate3 < 0.5f)
		{
			damageType3 = array7[Random.Range(0, array7.Length)];
		}
		else
		{
			damageType3 = array9[Random.Range(0, array9.Length)];
		}
		if (multi1 > 0)
		{
			multi1Type = Random.Range(0, 5);
		}
		else
		{
			multi1Type = 0;
		}
	}

	public void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)myBody.skillLocator) && !setDamage)
		{
			GenericSkill skill = myBody.skillLocator.GetSkill((SkillSlot)0);
			float num = rate0 + skill.baseRechargeInterval / (float)skill.maxStock * 40f;
			float num2 = rate2 + skill.baseRechargeInterval / (float)skill.maxStock * 40f;
			float num3 = rate0 * 40f;
			float num4 = myBody.skillLocator.GetSkill((SkillSlot)1).baseRechargeInterval * 8f / (1f + (float)myBody.skillLocator.GetSkill((SkillSlot)1).maxStock / 4f) / (float)multi1;
			MonoBehaviour.print((object)"");
			MonoBehaviour.print((object)("Survivor Name: " + ((Object)myBody).name));
			MonoBehaviour.print((object)"  Primary Skill:");
			if (primaryType == 0)
			{
				MonoBehaviour.print((object)("    " + prefab0));
				MonoBehaviour.print((object)("    Damage: " + damage0 + " + " + num));
				MonoBehaviour.print((object)("    ROF:    " + rate0));
				MonoBehaviour.print((object)("    Speed:  " + speed0));
				MonoBehaviour.print((object)("    Force:  " + force0));
			}
			else if (primaryType == 1)
			{
				MonoBehaviour.print((object)("    " + tracer2));
				MonoBehaviour.print((object)("    " + impact2));
				MonoBehaviour.print((object)("    Damage: " + damage2 + " + " + num2));
				MonoBehaviour.print((object)("    ROF:    " + rate2));
				MonoBehaviour.print((object)("    Force:  " + force2));
				MonoBehaviour.print((object)("    Multi:  " + multi2));
			}
			else
			{
				MonoBehaviour.print((object)("    " + swing3));
				MonoBehaviour.print((object)("    Damage: " + damage3 + " + " + num3));
				MonoBehaviour.print((object)("    ROF:    " + rate3));
				MonoBehaviour.print((object)("    Force:  " + force3));
			}
			MonoBehaviour.print((object)"  Secondary Skill:");
			MonoBehaviour.print((object)("    " + prefab1));
			MonoBehaviour.print((object)("    Damage: " + damage1 + " + " + num4));
			MonoBehaviour.print((object)("    ROF:    " + rate1));
			MonoBehaviour.print((object)("    Speed:  " + speed1));
			MonoBehaviour.print((object)("    Force:  " + force1));
			MonoBehaviour.print((object)("    Multi:  " + multi1));
			MonoBehaviour.print((object)("      Type:   " + multi1Type));
			MonoBehaviour.print((object)"  Utility Skill:");
			MonoBehaviour.print((object)("    " + myBody.skillLocator.GetSkill((SkillSlot)2).skillNameToken));
			MonoBehaviour.print((object)"  Special Skill:");
			MonoBehaviour.print((object)("    " + myBody.skillLocator.GetSkill((SkillSlot)3).skillNameToken));
			MonoBehaviour.print((object)"");
			damage0 = (damage0 + num) / 7f;
			damage1 = (damage1 + num4) / 7f;
			damage2 = (damage2 + num2) / 7f;
			damage3 = (damage3 + num3) / 5f;
			setDamage = true;
			if (Object.op_Implicit((Object)(object)myBody.master))
			{
				PlayerCharacterMasterController component = ((Component)myBody.master).GetComponent<PlayerCharacterMasterController>();
				if (Object.op_Implicit((Object)(object)component))
				{
					GameObject networkUserObject = component.networkUserObject;
					if (Object.op_Implicit((Object)(object)networkUserObject))
					{
						NetworkUser component2 = networkUserObject.GetComponent<NetworkUser>();
						if (Object.op_Implicit((Object)(object)component2))
						{
							component2.userName = myBody.subtitleNameToken;
						}
					}
				}
			}
		}
		if (myBody.isSprinting && !canSprint)
		{
			myBody.isSprinting = false;
		}
	}

	public void OnGUI()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		if (((NetworkBehaviour)myBody).hasAuthority && isDebug)
		{
			float num = (float)Screen.width * 0.5f;
			float num2 = (float)Screen.height * 0.9f;
			GUI.color = Color32.op_Implicit(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			GUI.contentColor = Color32.op_Implicit(new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			float num3 = 500f;
			GUI.Box(new Rect(num - num3 / 2f, num2 - 25f, num3 / 2f, 25f), "Primary: " + Math.Floor(damage0 * 100f));
			GUI.Box(new Rect(num, num2 - 25f, num3 / 2f, 25f), "Secondary: " + Math.Floor(damage1 * 100f));
			GUI.Box(new Rect(num - num3 / 2f, num2 + 0f, num3, 25f), myBody.skillLocator.GetSkill((SkillSlot)2).skillDescriptionToken ?? "");
			GUI.Box(new Rect(num - num3 / 2f, num2 + 25f, num3, 25f), myBody.skillLocator.GetSkill((SkillSlot)3).skillDescriptionToken ?? "");
		}
	}
}
