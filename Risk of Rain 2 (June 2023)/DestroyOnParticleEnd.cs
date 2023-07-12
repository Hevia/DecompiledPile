using UnityEngine;

public class DestroyOnParticleEnd : MonoBehaviour
{
	private ParticleSystem ps;

	public void Awake()
	{
		ps = ((Component)this).GetComponentInChildren<ParticleSystem>();
	}

	public void Update()
	{
		if (Object.op_Implicit((Object)(object)ps) && !ps.IsAlive())
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
