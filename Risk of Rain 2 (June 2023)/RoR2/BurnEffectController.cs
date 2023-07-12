using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class BurnEffectController : MonoBehaviour
{
	public class EffectParams
	{
		public string startSound;

		public string stopSound;

		public Material overlayMaterial;

		public GameObject fireEffectPrefab;
	}

	private List<GameObject> burnEffectInstances;

	public GameObject target;

	private TemporaryOverlay temporaryOverlay;

	private int soundID;

	public EffectParams effectType = normalEffect;

	public static EffectParams normalEffect;

	public static EffectParams helfireEffect;

	public static EffectParams poisonEffect;

	public static EffectParams blightEffect;

	public static EffectParams strongerBurnEffect;

	public float fireParticleSize = 5f;

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void Init()
	{
		normalEffect = new EffectParams
		{
			startSound = "Play_item_proc_igniteOnKill_Loop",
			stopSound = "Stop_item_proc_igniteOnKill_Loop",
			overlayMaterial = LegacyResourcesAPI.Load<Material>("Materials/matOnFire"),
			fireEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/FireEffect")
		};
		helfireEffect = new EffectParams
		{
			startSound = "Play_item_proc_igniteOnKill_Loop",
			stopSound = "Stop_item_proc_igniteOnKill_Loop",
			overlayMaterial = LegacyResourcesAPI.Load<Material>("Materials/matOnHelfire"),
			fireEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/HelfireEffect")
		};
		poisonEffect = new EffectParams
		{
			overlayMaterial = LegacyResourcesAPI.Load<Material>("Materials/matPoisoned"),
			fireEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/PoisonEffect")
		};
		blightEffect = new EffectParams
		{
			overlayMaterial = LegacyResourcesAPI.Load<Material>("Materials/matBlighted"),
			fireEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/BlightEffect")
		};
		strongerBurnEffect = new EffectParams
		{
			overlayMaterial = LegacyResourcesAPI.Load<Material>("Materials/matStrongerBurn"),
			fireEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/StrongerBurnEffect")
		};
	}

	private void Start()
	{
		if (effectType == null)
		{
			Debug.LogError((object)("BurnEffectController on " + ((Object)((Component)this).gameObject).name + " has no effect type!"));
			return;
		}
		Util.PlaySound(effectType.startSound, ((Component)this).gameObject);
		burnEffectInstances = new List<GameObject>();
		if ((Object)(object)effectType.overlayMaterial != (Object)null)
		{
			temporaryOverlay = ((Component)this).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay.originalMaterial = effectType.overlayMaterial;
		}
		if (!Object.op_Implicit((Object)(object)target))
		{
			return;
		}
		CharacterModel component = target.GetComponent<CharacterModel>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)temporaryOverlay))
		{
			temporaryOverlay.AddToCharacerModel(component);
		}
		CharacterBody body = component.body;
		CharacterModel.RendererInfo[] baseRendererInfos = component.baseRendererInfos;
		if (!Object.op_Implicit((Object)(object)body))
		{
			return;
		}
		for (int i = 0; i < baseRendererInfos.Length; i++)
		{
			if (!baseRendererInfos[i].ignoreOverlays)
			{
				GameObject val = AddFireParticles(baseRendererInfos[i].renderer, body.coreTransform);
				if (Object.op_Implicit((Object)(object)val))
				{
					burnEffectInstances.Add(val);
				}
			}
		}
	}

	private void OnDestroy()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound(effectType.stopSound, ((Component)this).gameObject);
		if (Object.op_Implicit((Object)(object)temporaryOverlay))
		{
			Object.Destroy((Object)(object)temporaryOverlay);
		}
		for (int i = 0; i < burnEffectInstances.Count; i++)
		{
			if (Object.op_Implicit((Object)(object)burnEffectInstances[i]))
			{
				EmissionModule emission = burnEffectInstances[i].GetComponent<ParticleSystem>().emission;
				((EmissionModule)(ref emission)).enabled = false;
				DestroyOnTimer component = burnEffectInstances[i].GetComponent<DestroyOnTimer>();
				LightIntensityCurve component2 = burnEffectInstances[i].GetComponent<LightIntensityCurve>();
				if (Object.op_Implicit((Object)(object)component))
				{
					((Behaviour)component).enabled = true;
				}
				if (Object.op_Implicit((Object)(object)component2))
				{
					((Behaviour)component2).enabled = true;
				}
			}
		}
	}

	private GameObject AddFireParticles(Renderer modelRenderer, Transform targetParentTransform)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		if (modelRenderer is MeshRenderer || modelRenderer is SkinnedMeshRenderer)
		{
			GameObject obj = Object.Instantiate<GameObject>(effectType.fireEffectPrefab, targetParentTransform);
			ParticleSystem component = obj.GetComponent<ParticleSystem>();
			ShapeModule shape = component.shape;
			if (Object.op_Implicit((Object)(object)modelRenderer))
			{
				if (modelRenderer is MeshRenderer)
				{
					((ShapeModule)(ref shape)).shapeType = (ParticleSystemShapeType)13;
					((ShapeModule)(ref shape)).meshRenderer = (MeshRenderer)modelRenderer;
				}
				else if (modelRenderer is SkinnedMeshRenderer)
				{
					((ShapeModule)(ref shape)).shapeType = (ParticleSystemShapeType)14;
					((ShapeModule)(ref shape)).skinnedMeshRenderer = (SkinnedMeshRenderer)modelRenderer;
				}
			}
			_ = component.main;
			((Component)component).gameObject.SetActive(true);
			BoneParticleController component2 = obj.GetComponent<BoneParticleController>();
			if (Object.op_Implicit((Object)(object)component2) && modelRenderer is SkinnedMeshRenderer)
			{
				component2.skinnedMeshRenderer = (SkinnedMeshRenderer)modelRenderer;
			}
			return obj;
		}
		return null;
	}
}
