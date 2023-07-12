using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class EventFunctions : MonoBehaviour
{
	public void DestroySelf()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void DestroyGameObject(GameObject obj)
	{
		Object.Destroy((Object)(object)obj);
	}

	public void UnparentTransform(Transform transform)
	{
		if (Object.op_Implicit((Object)(object)transform))
		{
			transform.SetParent((Transform)null);
		}
	}

	public void ToggleGameObjectActive(GameObject obj)
	{
		obj.SetActive(!obj.activeSelf);
	}

	public void CreateLocalEffect(GameObject effectObj)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData();
		effectData.origin = ((Component)this).transform.position;
		EffectManager.SpawnEffect(effectObj, effectData, transmit: false);
	}

	public void CreateNetworkedEffect(GameObject effectObj)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData();
		effectData.origin = ((Component)this).transform.position;
		EffectManager.SpawnEffect(effectObj, effectData, transmit: true);
	}

	public void OpenURL(string url)
	{
		Application.OpenURL(url);
	}

	public void PlaySound(string soundString)
	{
		Util.PlaySound(soundString, ((Component)this).gameObject);
	}

	public void PlayUISound(string soundString)
	{
		Util.PlaySound(soundString, ((Component)RoR2Application.instance).gameObject);
	}

	public void PlayNetworkedUISound(NetworkSoundEventDef nseDef)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)nseDef))
		{
			EffectManager.SimpleSoundEffect(nseDef.index, Vector3.zero, transmit: true);
		}
	}

	public void RunSetFlag(string flagName)
	{
		if (NetworkServer.active)
		{
			Run.instance?.SetEventFlag(flagName);
		}
	}

	public void RunResetFlag(string flagName)
	{
		if (NetworkServer.active)
		{
			Run.instance?.ResetEventFlag(flagName);
		}
	}

	public void DisableAllChildren()
	{
		for (int num = ((Component)this).transform.childCount - 1; num >= 0; num--)
		{
			((Component)((Component)this).transform.GetChild(num)).gameObject.SetActive(false);
		}
	}

	public void DisableAllChildrenExcept(GameObject objectToEnable)
	{
		for (int num = ((Component)this).transform.childCount - 1; num >= 0; num--)
		{
			GameObject gameObject = ((Component)((Component)this).transform.GetChild(num)).gameObject;
			if (!((Object)(object)gameObject == (Object)(object)objectToEnable))
			{
				gameObject.SetActive(false);
			}
		}
		objectToEnable.SetActive(true);
	}

	public void BeginEnding(GameEndingDef gameEndingDef)
	{
		if (NetworkServer.active)
		{
			Run.instance.BeginGameOver(gameEndingDef);
		}
	}
}
