using RoR2;
using UnityEngine;

namespace EntityStates.GolemMonster;

public class SpawnState : GenericCharacterSpawnState
{
	private GameObject eyeGameObject;

	public override void OnEnter()
	{
		base.OnEnter();
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			((Behaviour)((Component)modelTransform).GetComponent<PrintController>()).enabled = true;
		}
		Transform obj = FindModelChild("Eye");
		eyeGameObject = ((obj != null) ? ((Component)obj).gameObject : null);
		if (Object.op_Implicit((Object)(object)eyeGameObject))
		{
			eyeGameObject.SetActive(false);
		}
	}

	public override void OnExit()
	{
		if (!outer.destroying && Object.op_Implicit((Object)(object)eyeGameObject))
		{
			eyeGameObject.SetActive(true);
		}
	}
}
