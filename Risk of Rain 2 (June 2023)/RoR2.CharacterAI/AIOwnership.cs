using UnityEngine;

namespace RoR2.CharacterAI;

[RequireComponent(typeof(BaseAI))]
[DisallowMultipleComponent]
public class AIOwnership : MonoBehaviour
{
	public CharacterMaster ownerMaster;

	private BaseAI baseAI;

	private void Awake()
	{
		baseAI = ((Component)this).GetComponent<BaseAI>();
	}

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)ownerMaster))
		{
			baseAI.leader.gameObject = ownerMaster.GetBodyObject();
		}
	}
}
