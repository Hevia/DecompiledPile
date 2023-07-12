using UnityEngine;
using UnityEngine.Networking;

public class Spinner : MonoBehaviour
{
	private float randRotationSpeed;

	private int randNumX;

	private int randNumY;

	private int randNumZ;

	private float randZeroOrOneX;

	private float randZeroOrOneY;

	private float randZeroOrOneZ;

	private void Start()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		Random.Range(0f, 360f);
		Random.Range(0f, 360f);
		Random.Range(0f, 360f);
		Random.Range(0f, 360f);
		((Component)this).gameObject.transform.rotation = Random.rotationUniform;
		randRotationSpeed = Random.Range(0.2f, 1f);
		randNumX = Random.Range(0, 2);
		randNumY = Random.Range(0, 2);
		randNumZ = Random.Range(0, 2);
		randZeroOrOneX = randNumX;
		randZeroOrOneY = randNumY;
		randZeroOrOneZ = randNumZ;
		if (randZeroOrOneX == 0f && randZeroOrOneY == 0f && randZeroOrOneZ == 0f)
		{
			randZeroOrOneX = 1f;
			randZeroOrOneY = 1f;
			randZeroOrOneZ = 1f;
		}
	}

	private void FixedUpdate()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.transform.Rotate(new Vector3(randZeroOrOneX, randZeroOrOneY, randZeroOrOneZ), randRotationSpeed * Time.timeScale);
	}
}
