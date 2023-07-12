using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class InputSourceFilter : MonoBehaviour
{
	public MPEventSystem.InputSource requiredInputSource;

	public GameObject[] objectsToFilter;

	private MPEventSystemLocator eventSystemLocator;

	private bool wasOn;

	protected MPEventSystem eventSystem => eventSystemLocator?.eventSystem;

	private void Start()
	{
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
		Refresh(forceRefresh: true);
	}

	private void Update()
	{
		Refresh();
	}

	private void Refresh(bool forceRefresh = false)
	{
		bool flag = eventSystem?.currentInputSource == requiredInputSource;
		if (flag != wasOn || forceRefresh)
		{
			for (int i = 0; i < objectsToFilter.Length; i++)
			{
				objectsToFilter[i].SetActive(flag);
			}
		}
		wasOn = flag;
	}
}
