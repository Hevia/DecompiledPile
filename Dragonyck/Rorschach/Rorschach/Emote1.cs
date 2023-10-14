namespace Rorschach;

internal class Emote1 : BaseEmote
{
	public override void OnEnter()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		animName = "Emote1";
		key = MainPlugin.emote1Key.Value;
		base.OnEnter();
	}
}
