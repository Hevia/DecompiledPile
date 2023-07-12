using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class ConvertPlayerMoneyToExperience : MonoBehaviour
{
	private Dictionary<GameObject, uint> burstSizes = new Dictionary<GameObject, uint>();

	private float burstTimer;

	public float burstInterval = 0.25f;

	public int burstCount = 8;

	private void Start()
	{
		if (!NetworkServer.active)
		{
			Debug.LogErrorFormat("Component {0} can only be added on the server!", new object[1] { ((object)this).GetType().Name });
			Object.Destroy((Object)(object)this);
		}
		else
		{
			burstTimer = 0f;
		}
	}

	private void FixedUpdate()
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		burstTimer -= Time.fixedDeltaTime;
		if (!(burstTimer <= 0f))
		{
			return;
		}
		bool flag = false;
		ReadOnlyCollection<PlayerCharacterMasterController> instances = PlayerCharacterMasterController.instances;
		for (int i = 0; i < instances.Count; i++)
		{
			GameObject gameObject = ((Component)instances[i]).gameObject;
			CharacterMaster component = gameObject.GetComponent<CharacterMaster>();
			if (!burstSizes.TryGetValue(gameObject, out var value))
			{
				value = (uint)Mathf.CeilToInt((float)component.money / (float)burstCount);
				burstSizes[gameObject] = value;
			}
			if (value > component.money)
			{
				value = component.money;
			}
			component.money -= value;
			GameObject bodyObject = component.GetBodyObject();
			ulong num = (ulong)((float)value / 2f / (float)instances.Count);
			if (value != 0)
			{
				flag = true;
			}
			if (Object.op_Implicit((Object)(object)bodyObject))
			{
				ExperienceManager.instance.AwardExperience(((Component)this).transform.position, bodyObject.GetComponent<CharacterBody>(), num);
			}
			else
			{
				TeamManager.instance.GiveTeamExperience(component.teamIndex, num);
			}
		}
		if (flag)
		{
			burstTimer = burstInterval;
		}
		else if (burstTimer < -2.5f)
		{
			Object.Destroy((Object)(object)this);
		}
	}
}
