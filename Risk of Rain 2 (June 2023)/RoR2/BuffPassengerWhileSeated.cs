using UnityEngine;

namespace RoR2;

public class BuffPassengerWhileSeated : MonoBehaviour
{
	[SerializeField]
	private BuffDef buff;

	[SerializeField]
	private VehicleSeat vehicleSeat;

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)vehicleSeat))
		{
			if (Object.op_Implicit((Object)(object)vehicleSeat.currentPassengerBody))
			{
				AddBuff(vehicleSeat.currentPassengerBody);
			}
			vehicleSeat.onPassengerEnter += OnPassengerEnter;
			vehicleSeat.onPassengerExit += OnPassengerExit;
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)vehicleSeat))
		{
			if (Object.op_Implicit((Object)(object)vehicleSeat.currentPassengerBody))
			{
				RemoveBuff(vehicleSeat.currentPassengerBody);
			}
			vehicleSeat.onPassengerEnter -= OnPassengerEnter;
			vehicleSeat.onPassengerExit -= OnPassengerExit;
		}
	}

	private void OnPassengerEnter(GameObject passengerObject)
	{
		CharacterBody component = passengerObject.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			AddBuff(component);
		}
	}

	private void OnPassengerExit(GameObject passengerObject)
	{
		CharacterBody component = passengerObject.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			RemoveBuff(component);
		}
	}

	private void AddBuff(CharacterBody passengerBody)
	{
		passengerBody.AddBuff(buff);
	}

	private void RemoveBuff(CharacterBody passengerBody)
	{
		passengerBody.RemoveBuff(buff);
	}
}
