using UnityEngine;

namespace RoR2;

public class SprintEffectController : MonoBehaviour
{
	public ParticleSystem[] loopSystems;

	public GameObject loopRootObject;

	public bool mustBeGrounded;

	public bool mustBeVisible;

	public CharacterBody characterBody;

	public CharacterModel characterModel;

	private void Awake()
	{
		if (Object.op_Implicit((Object)(object)characterBody) && Util.IsPrefab(((Component)characterBody).gameObject) && !Util.IsPrefab(((Component)this).gameObject))
		{
			characterBody = null;
		}
	}

	private void FixedUpdate()
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		if (Object.op_Implicit((Object)(object)characterModel))
		{
			flag = characterModel.visibility > VisibilityLevel.Cloaked;
		}
		if (!Object.op_Implicit((Object)(object)characterBody))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)characterBody.characterMotor) && characterBody.healthComponent.alive && characterBody.isSprinting && (!mustBeGrounded || characterBody.characterMotor.isGrounded) && (!mustBeVisible || flag))
		{
			if (Object.op_Implicit((Object)(object)loopRootObject))
			{
				loopRootObject.SetActive(true);
			}
			for (int i = 0; i < loopSystems.Length; i++)
			{
				ParticleSystem val = loopSystems[i];
				MainModule main = val.main;
				((MainModule)(ref main)).loop = true;
				if (!val.isPlaying)
				{
					val.Play();
				}
			}
		}
		else
		{
			if (Object.op_Implicit((Object)(object)loopRootObject))
			{
				loopRootObject.SetActive(false);
			}
			for (int j = 0; j < loopSystems.Length; j++)
			{
				MainModule main2 = loopSystems[j].main;
				((MainModule)(ref main2)).loop = false;
			}
		}
	}
}
