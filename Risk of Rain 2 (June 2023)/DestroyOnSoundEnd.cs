using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DestroyOnSoundEnd : MonoBehaviour
{
	private AudioSource audioSource;

	private void Awake()
	{
		audioSource = ((Component)this).GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (!audioSource.isPlaying)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
