using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class RandomizeSplatBias : MonoBehaviour
{
	public float minRedBias;

	public float maxRedBias;

	public float minGreenBias;

	public float maxGreenBias;

	public float minBlueBias;

	public float maxBlueBias;

	private MaterialPropertyBlock _propBlock;

	private CharacterModel characterModel;

	private List<Material> materialsList;

	private List<Renderer> rendererList;

	private Shader printShader;

	private void Start()
	{
		materialsList = new List<Material>();
		rendererList = new List<Renderer>();
		printShader = LegacyResourcesAPI.Load<Shader>("Shaders/Deferred/HGStandard");
		Setup();
	}

	private void Setup()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		characterModel = ((Component)this).GetComponent<CharacterModel>();
		if (Object.op_Implicit((Object)(object)characterModel))
		{
			for (int i = 0; i < characterModel.baseRendererInfos.Length; i++)
			{
				CharacterModel.RendererInfo rendererInfo = characterModel.baseRendererInfos[i];
				Material val = Object.Instantiate<Material>(rendererInfo.defaultMaterial);
				if ((Object)(object)val.shader == (Object)(object)printShader)
				{
					materialsList.Add(val);
					rendererList.Add(rendererInfo.renderer);
					rendererInfo.defaultMaterial = val;
					characterModel.baseRendererInfos[i] = rendererInfo;
				}
				Renderer obj = rendererList[i];
				_propBlock = new MaterialPropertyBlock();
				obj.GetPropertyBlock(_propBlock);
				_propBlock.SetFloat("_RedChannelBias", Random.Range(minRedBias, maxRedBias));
				_propBlock.SetFloat("_BlueChannelBias", Random.Range(minBlueBias, maxBlueBias));
				_propBlock.SetFloat("_GreenChannelBias", Random.Range(minGreenBias, maxGreenBias));
				obj.SetPropertyBlock(_propBlock);
			}
		}
		else
		{
			Renderer componentInChildren = ((Component)this).GetComponentInChildren<Renderer>();
			Material val2 = Object.Instantiate<Material>(componentInChildren.material);
			materialsList.Add(val2);
			componentInChildren.material = val2;
			_propBlock = new MaterialPropertyBlock();
			componentInChildren.GetPropertyBlock(_propBlock);
			_propBlock.SetFloat("_RedChannelBias", Random.Range(minRedBias, maxRedBias));
			_propBlock.SetFloat("_BlueChannelBias", Random.Range(minBlueBias, maxBlueBias));
			_propBlock.SetFloat("_GreenChannelBias", Random.Range(minGreenBias, maxGreenBias));
			componentInChildren.SetPropertyBlock(_propBlock);
		}
	}

	private void OnDestroy()
	{
		if (materialsList != null)
		{
			for (int i = 0; i < materialsList.Count; i++)
			{
				Object.Destroy((Object)(object)materialsList[i]);
			}
		}
	}
}
