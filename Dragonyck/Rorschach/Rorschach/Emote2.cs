namespace Rorschach;

internal class Emote2 : BaseEmote
{
	public override void OnEnter()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		animName = "Emote2";
		key = MainPlugin.emote2Key.Value;
		base.OnEnter();
	}
}
