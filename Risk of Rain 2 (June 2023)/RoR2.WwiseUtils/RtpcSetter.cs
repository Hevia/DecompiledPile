using UnityEngine;

namespace RoR2.WwiseUtils;

public struct RtpcSetter
{
	private readonly string name;

	private readonly uint id;

	private readonly GameObject gameObject;

	private float expectedEngineValue;

	public float value;

	public RtpcSetter(string name, GameObject gameObject = null)
	{
		this.name = name;
		id = AkSoundEngine.GetIDFromString(name);
		this.gameObject = gameObject;
		expectedEngineValue = float.NegativeInfinity;
		value = expectedEngineValue;
	}

	public void FlushIfChanged()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!expectedEngineValue.Equals(value))
		{
			expectedEngineValue = value;
			AkSoundEngine.SetRTPCValue(id, value, gameObject);
		}
	}
}
