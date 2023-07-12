using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class TreebotSunBuffGranter : MonoBehaviour
{
	public Transform referenceTransform;

	public float raycastFrequency;

	public BuffDef buffDef;

	private const float raycastLength = 1000f;

	private float timer;

	private bool hadSunlight;

	private Light sun;

	private CharacterBody characterBody;

	private void Start()
	{
		FindSun();
		characterBody = ((Component)this).GetComponent<CharacterBody>();
	}

	private void FixedUpdate()
	{
		timer -= Time.fixedDeltaTime;
		if (timer <= 0f)
		{
			timer = 1f / raycastFrequency;
			CheckSunlight();
		}
	}

	private void FindSun()
	{
		sun = RenderSettings.sun;
	}

	private void CheckSunlight()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)sun))
		{
			FindSun();
			if (!Object.op_Implicit((Object)(object)sun))
			{
				return;
			}
		}
		bool flag = !Physics.Raycast(((Component)this).transform.position, -((Component)sun).transform.forward, 1000f, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1);
		if (Object.op_Implicit((Object)(object)buffDef))
		{
			if (hadSunlight && !flag)
			{
				characterBody.RemoveBuff(buffDef.buffIndex);
			}
			else if (flag && !hadSunlight)
			{
				characterBody.AddBuff(buffDef.buffIndex);
			}
		}
		hadSunlight = flag;
	}
}
