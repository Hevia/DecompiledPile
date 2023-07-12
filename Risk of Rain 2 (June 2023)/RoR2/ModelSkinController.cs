using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(CharacterModel))]
[DisallowMultipleComponent]
public class ModelSkinController : MonoBehaviour
{
	public SkinDef[] skins;

	private CharacterModel characterModel;

	public int currentSkinIndex { get; private set; } = -1;


	private void Awake()
	{
		characterModel = ((Component)this).GetComponent<CharacterModel>();
	}

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)characterModel.body))
		{
			ApplySkin((int)characterModel.body.skinIndex);
		}
	}

	public void ApplySkin(int skinIndex)
	{
		if (skinIndex != currentSkinIndex && (uint)skinIndex < skins.Length)
		{
			skins[skinIndex].Apply(((Component)this).gameObject);
			currentSkinIndex = skinIndex;
		}
	}
}
