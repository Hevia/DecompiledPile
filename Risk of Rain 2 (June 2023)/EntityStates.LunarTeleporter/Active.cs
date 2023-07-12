using RoR2;
using UnityEngine.Networking;

namespace EntityStates.LunarTeleporter;

public class Active : LunarTeleporterBaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		preferredInteractability = Interactability.Available;
		PlayAnimation("Base", "Active");
		outer.mainStateType = new SerializableEntityStateType(typeof(ActiveToIdle));
		if (NetworkServer.active)
		{
			teleporterInteraction.sceneExitController.useRunNextStageScene = false;
		}
	}
}
