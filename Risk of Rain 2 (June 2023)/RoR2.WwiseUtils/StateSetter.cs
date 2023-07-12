namespace RoR2.WwiseUtils;

public struct StateSetter
{
	private readonly string name;

	private readonly uint id;

	private uint expectedEngineValueId;

	public uint valueId;

	public StateSetter(string name)
	{
		this.name = name;
		id = AkSoundEngine.GetIDFromString(name);
		expectedEngineValueId = 0u;
		valueId = expectedEngineValueId;
	}

	public void FlushIfChanged()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!expectedEngineValueId.Equals(valueId))
		{
			expectedEngineValueId = valueId;
			AkSoundEngine.SetState(id, valueId);
		}
	}
}
