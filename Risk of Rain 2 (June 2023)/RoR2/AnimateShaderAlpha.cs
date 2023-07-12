using ThreeEyedGames;
using UnityEngine;

namespace RoR2;

public class AnimateShaderAlpha : MonoBehaviour
{
	public AnimationCurve alphaCurve;

	private Renderer targetRenderer;

	private MaterialPropertyBlock _propBlock;

	private Material[] materials;

	public float timeMax = 5f;

	[Tooltip("Optional field if you want to animate Decal 'Fade' rather than renderer _ExternalAlpha.")]
	public Decal decal;

	public bool pauseTime;

	public bool destroyOnEnd;

	public bool disableOnEnd;

	[HideInInspector]
	public float time;

	private float initialFade;

	private void Start()
	{
		targetRenderer = ((Component)this).GetComponent<Renderer>();
		if (Object.op_Implicit((Object)(object)targetRenderer))
		{
			materials = targetRenderer.materials;
		}
		if (Object.op_Implicit((Object)(object)decal))
		{
			initialFade = decal.Fade;
		}
	}

	private void Update()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		if (!pauseTime)
		{
			time = Mathf.Min(timeMax, time + Time.deltaTime);
		}
		float num = alphaCurve.Evaluate(time / timeMax);
		if (Object.op_Implicit((Object)(object)decal))
		{
			decal.Fade = num * initialFade;
		}
		else
		{
			Material[] array = materials;
			for (int i = 0; i < array.Length; i++)
			{
				_ = array[i];
				_propBlock = new MaterialPropertyBlock();
				targetRenderer.GetPropertyBlock(_propBlock);
				_propBlock.SetFloat("_ExternalAlpha", num);
				targetRenderer.SetPropertyBlock(_propBlock);
			}
		}
		if (time >= timeMax)
		{
			if (disableOnEnd)
			{
				((Behaviour)this).enabled = false;
			}
			if (destroyOnEnd)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
	}
}
