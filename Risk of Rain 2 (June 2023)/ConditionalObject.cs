using UnityEngine;

[DefaultExecutionOrder(-5)]
public class ConditionalObject : MonoBehaviour
{
	public bool enabledOnSwitch = true;

	public bool enabledOnXbox = true;

	public bool enabledOnPS4 = true;

	public bool enabledOnSteam = true;

	public bool enabledOnEGS = true;

	public bool disableInProduction;

	public bool disableIfNoActiveRun;

	private void CheckConditions()
	{
		((Component)this).gameObject.SetActive(enabledOnSteam);
	}

	private void Awake()
	{
		if (!disableInProduction)
		{
			CheckConditions();
		}
	}
}
