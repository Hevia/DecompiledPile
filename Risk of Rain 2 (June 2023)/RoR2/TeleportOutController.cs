using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class TeleportOutController : NetworkBehaviour
{
	[NonSerialized]
	[SyncVar]
	public GameObject target;

	public ParticleSystem bodyGlowParticles;

	public static string tpOutSoundString = "Play_UI_teleport_off_map";

	private float fixedAge;

	private const float warmupDuration = 2f;

	public float delayBeforePlayingSFX = 1f;

	private bool hasPlayedSFX;

	private NetworkInstanceId ___targetNetId;

	public GameObject Networktarget
	{
		get
		{
			return target;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref target, 1u, ref ___targetNetId);
		}
	}

	public static void AddTPOutEffect(CharacterModel characterModel, float beginAlpha, float endAlpha, float duration)
	{
		if (Object.op_Implicit((Object)(object)characterModel))
		{
			TemporaryOverlay temporaryOverlay = ((Component)characterModel).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay.duration = duration;
			temporaryOverlay.animateShaderAlpha = true;
			temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, beginAlpha, 1f, endAlpha);
			temporaryOverlay.destroyComponentOnEnd = true;
			temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matTPInOut");
			temporaryOverlay.AddToCharacerModel(characterModel);
		}
	}

	public override void OnStartClient()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		((NetworkBehaviour)this).OnStartClient();
		if (Object.op_Implicit((Object)(object)target))
		{
			ModelLocator component = target.GetComponent<ModelLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform modelTransform = component.modelTransform;
				if (Object.op_Implicit((Object)(object)modelTransform))
				{
					CharacterModel component2 = ((Component)modelTransform).GetComponent<CharacterModel>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						AddTPOutEffect(component2, 0f, 1f, 2f);
						if (component2.baseRendererInfos.Length != 0)
						{
							Renderer renderer = component2.baseRendererInfos[component2.baseRendererInfos.Length - 1].renderer;
							if (Object.op_Implicit((Object)(object)renderer))
							{
								ShapeModule shape = bodyGlowParticles.shape;
								if (renderer is MeshRenderer)
								{
									((ShapeModule)(ref shape)).shapeType = (ParticleSystemShapeType)13;
									((ShapeModule)(ref shape)).meshRenderer = (MeshRenderer)(object)((renderer is MeshRenderer) ? renderer : null);
								}
								else if (renderer is SkinnedMeshRenderer)
								{
									((ShapeModule)(ref shape)).shapeType = (ParticleSystemShapeType)14;
									((ShapeModule)(ref shape)).skinnedMeshRenderer = (SkinnedMeshRenderer)(object)((renderer is SkinnedMeshRenderer) ? renderer : null);
								}
							}
						}
					}
				}
			}
		}
		bodyGlowParticles.Play();
	}

	public void FixedUpdate()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		fixedAge += Time.fixedDeltaTime;
		if (fixedAge >= delayBeforePlayingSFX && !hasPlayedSFX)
		{
			hasPlayedSFX = true;
			Util.PlaySound(tpOutSoundString, target);
		}
		if (NetworkServer.active && fixedAge >= 2f && Object.op_Implicit((Object)(object)target))
		{
			GameObject teleportEffectPrefab = Run.instance.GetTeleportEffectPrefab(target);
			if (Object.op_Implicit((Object)(object)teleportEffectPrefab))
			{
				EffectManager.SpawnEffect(teleportEffectPrefab, new EffectData
				{
					origin = target.transform.position
				}, transmit: true);
			}
			Object.Destroy((Object)(object)target);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(target);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(target);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			___targetNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			target = reader.ReadGameObject();
		}
	}

	public override void PreStartClient()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkInstanceId)(ref ___targetNetId)).IsEmpty())
		{
			Networktarget = ClientScene.FindLocalObject(___targetNetId);
		}
	}
}
