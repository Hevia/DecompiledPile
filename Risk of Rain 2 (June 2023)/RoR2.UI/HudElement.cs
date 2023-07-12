using UnityEngine;

namespace RoR2.UI;

[DisallowMultipleComponent]
public class HudElement : MonoBehaviour
{
	private HUD _hud;

	private GameObject _targetBodyObject;

	private CharacterBody _targetCharacterBody;

	public HUD hud
	{
		get
		{
			return _hud;
		}
		set
		{
			_hud = value;
			if (Object.op_Implicit((Object)(object)_hud))
			{
				targetBodyObject = _hud.targetBodyObject;
			}
			else
			{
				targetBodyObject = null;
			}
		}
	}

	public GameObject targetBodyObject
	{
		get
		{
			return _targetBodyObject;
		}
		set
		{
			_targetBodyObject = value;
			if (Object.op_Implicit((Object)(object)_targetBodyObject))
			{
				_targetCharacterBody = _targetBodyObject.GetComponent<CharacterBody>();
			}
		}
	}

	public CharacterBody targetCharacterBody
	{
		get
		{
			return _targetCharacterBody;
		}
		set
		{
			_targetCharacterBody = value;
			if (Object.op_Implicit((Object)(object)targetCharacterBody))
			{
				_targetBodyObject = ((Component)targetCharacterBody).gameObject;
			}
		}
	}
}
