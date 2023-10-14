using System;
using System.IO;
using System.Reflection;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

namespace VileMod.Modules;

internal static class Assets
{
	internal static GameObject swordSwingEffect;

	internal static GameObject swordHitImpactEffect;

	internal static GameObject bombExplosionEffect;

	internal static NetworkSoundEventDef swordHitSoundEvent;

	internal static GameObject BurningDriveVFX;

	internal static GameObject RedEyeVFX;

	internal static Texture VIcon;

	internal static Sprite VilePassiveIcon;

	internal static Sprite VBB;

	internal static Sprite VES;

	internal static Sprite VFR;

	internal static Sprite VSI;

	internal static Sprite VBD;

	internal static Sprite VCP;

	internal static Sprite VCB;

	internal static Sprite VT7;

	internal static Sprite VNB;

	internal static Sprite VSkin;

	public static Sprite BarSprite;

	internal static AssetBundle mainAssetBundle;

	private const string assetbundleName = "vilev3bundle";

	private const string csProjName = "VileModV3";

	internal static void Initialize()
	{
		bool flag = false;
		LoadAssetBundle();
		LoadSoundbank();
		PopulateAssets();
	}

	internal static void LoadAssetBundle()
	{
		try
		{
			if ((Object)(object)mainAssetBundle == (Object)null)
			{
				using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VileModV3.vilev3bundle"))
				{
					mainAssetBundle = AssetBundle.LoadFromStream(stream);
					return;
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Failed to load assetbundle. Make sure your assetbundle name is setup correctly\n" + ex);
		}
	}

	internal static void LoadSoundbank()
	{
		using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VileModV3.VileModV3SB.bnk");
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, array.Length);
		SoundBanks.Add(array);
	}

	internal static void PopulateAssets()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)mainAssetBundle))
		{
			Log.Error("There is no AssetBundle to load assets from.");
			return;
		}
		if (Object.op_Implicit((Object)(object)bombExplosionEffect))
		{
			ShakeEmitter val = bombExplosionEffect.AddComponent<ShakeEmitter>();
			val.amplitudeTimeDecay = true;
			val.duration = 0.5f;
			val.radius = 200f;
			val.scaleShakeRadiusWithLocalScale = false;
			val.wave = new Wave
			{
				amplitude = 1f,
				frequency = 40f,
				cycleOffset = 0f
			};
		}
		BurningDriveVFX = LoadEffect("MagicFireBig", "");
		RedEyeVFX = LoadEffect("RedEye", "");
		VIcon = mainAssetBundle.LoadAsset<Texture>("VileIcon");
		VilePassiveIcon = mainAssetBundle.LoadAsset<Sprite>("VilePassiveIcon");
		VBB = mainAssetBundle.LoadAsset<Sprite>("VileBumpityBoom");
		VSI = mainAssetBundle.LoadAsset<Sprite>("VileShotgunIce");
		VCP = mainAssetBundle.LoadAsset<Sprite>("VileCerberusPhantom");
		VES = mainAssetBundle.LoadAsset<Sprite>("VileEletricSpark");
		VBD = mainAssetBundle.LoadAsset<Sprite>("VileBurningDrive");
		VCB = mainAssetBundle.LoadAsset<Sprite>("VileCherryBlast");
		VFR = mainAssetBundle.LoadAsset<Sprite>("VileFrontRunner");
		VT7 = mainAssetBundle.LoadAsset<Sprite>("VileTriple7");
		VNB = mainAssetBundle.LoadAsset<Sprite>("VileNapalmBomb");
		VSkin = mainAssetBundle.LoadAsset<Sprite>("VileSkinBase");
		BarSprite = mainAssetBundle.LoadAsset<Sprite>("HeatBar");
	}

	private static GameObject CreateTracer(string originalTracerName, string newTracerName)
	{
		if ((Object)(object)LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/" + originalTracerName) == (Object)null)
		{
			return null;
		}
		GameObject val = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/" + originalTracerName), newTracerName, true);
		if (!Object.op_Implicit((Object)(object)val.GetComponent<EffectComponent>()))
		{
			val.AddComponent<EffectComponent>();
		}
		if (!Object.op_Implicit((Object)(object)val.GetComponent<VFXAttributes>()))
		{
			val.AddComponent<VFXAttributes>();
		}
		if (!Object.op_Implicit((Object)(object)val.GetComponent<NetworkIdentity>()))
		{
			val.AddComponent<NetworkIdentity>();
		}
		val.GetComponent<Tracer>().speed = 250f;
		val.GetComponent<Tracer>().length = 50f;
		AddNewEffectDef(val);
		return val;
	}

	internal static NetworkSoundEventDef CreateNetworkSoundEventDef(string eventName)
	{
		NetworkSoundEventDef val = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
		val.akId = AkSoundEngine.GetIDFromString(eventName);
		val.eventName = eventName;
		Content.AddNetworkSoundEventDef(val);
		return val;
	}

	internal static void ConvertAllRenderersToHopooShader(GameObject objectToConvert)
	{
		if (!Object.op_Implicit((Object)(object)objectToConvert))
		{
			return;
		}
		Renderer[] componentsInChildren = objectToConvert.GetComponentsInChildren<Renderer>();
		foreach (Renderer val in componentsInChildren)
		{
			if (val != null)
			{
				val.material?.SetHopooMaterial();
			}
		}
	}

	internal static RendererInfo[] SetupRendererInfos(GameObject obj)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		MeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<MeshRenderer>();
		RendererInfo[] array = (RendererInfo[])(object)new RendererInfo[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			array[i] = new RendererInfo
			{
				defaultMaterial = ((Renderer)componentsInChildren[i]).material,
				renderer = (Renderer)(object)componentsInChildren[i],
				defaultShadowCastingMode = (ShadowCastingMode)1,
				ignoreOverlays = false
			};
		}
		return array;
	}

	public static GameObject LoadSurvivorModel(string modelName)
	{
		GameObject val = mainAssetBundle.LoadAsset<GameObject>(modelName);
		if ((Object)(object)val == (Object)null)
		{
			Log.Error("Trying to load a null model- check to see if the BodyName in your code matches the prefab name of the object in Unity\nFor Example, if your prefab in unity is 'mdlHenry', then your BodyName must be 'Henry'");
			return null;
		}
		return PrefabAPI.InstantiateClone(val, ((Object)val).name, false);
	}

	internal static GameObject LoadCrosshair(string crosshairName)
	{
		if ((Object)(object)LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/" + crosshairName + "Crosshair") == (Object)null)
		{
			return LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
		}
		return LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/" + crosshairName + "Crosshair");
	}

	private static GameObject LoadEffect(string resourceName)
	{
		return LoadEffect(resourceName, "", parentToTransform: false);
	}

	private static GameObject LoadEffect(string resourceName, string soundName)
	{
		return LoadEffect(resourceName, soundName, parentToTransform: false);
	}

	private static GameObject LoadEffect(string resourceName, bool parentToTransform)
	{
		return LoadEffect(resourceName, "", parentToTransform);
	}

	private static GameObject LoadEffect(string resourceName, string soundName, bool parentToTransform)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = mainAssetBundle.LoadAsset<GameObject>(resourceName);
		if (!Object.op_Implicit((Object)(object)val))
		{
			Log.Error("Failed to load effect: " + resourceName + " because it does not exist in the AssetBundle");
			return null;
		}
		val.AddComponent<DestroyOnTimer>().duration = 12f;
		val.AddComponent<NetworkIdentity>();
		val.AddComponent<VFXAttributes>().vfxPriority = (VFXPriority)2;
		EffectComponent val2 = val.AddComponent<EffectComponent>();
		val2.applyScale = false;
		val2.effectIndex = (EffectIndex)(-1);
		val2.parentToReferencedTransform = parentToTransform;
		val2.positionAtReferencedTransform = true;
		val2.soundName = soundName;
		AddNewEffectDef(val, soundName);
		return val;
	}

	private static void AddNewEffectDef(GameObject effectPrefab)
	{
		AddNewEffectDef(effectPrefab, "");
	}

	private static void AddNewEffectDef(GameObject effectPrefab, string soundName)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		EffectDef val = new EffectDef();
		val.prefab = effectPrefab;
		val.prefabEffectComponent = effectPrefab.GetComponent<EffectComponent>();
		val.prefabName = ((Object)effectPrefab).name;
		val.prefabVfxAttributes = effectPrefab.GetComponent<VFXAttributes>();
		val.spawnSoundEventName = soundName;
		Content.AddEffectDef(val);
	}
}
