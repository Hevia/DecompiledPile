using RoR2;
using UnityEngine.Networking;

namespace EntityStates.LunarTeleporter;

public class Idle : LunarTeleporterBaseState
{
	public override void OnEnter()
	{
		base.OnEnter();
		preferredInteractability = Interactability.Available;
		PlayAnimation("Base", "Idle");
		outer.mainStateType = new SerializableEntityStateType(typeof(IdleToActive));
		if (NetworkServer.active)
		{
			teleporterInteraction.sceneExitController.useRunNextStageScene = true;
		}
	}
}
