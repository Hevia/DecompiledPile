using System;
using BepInEx;
using BepInEx.Bootstrap;
using IndiesSkills.MyEntityStates;
using IndiesSkills.Skills;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace IndiesSkills;

[BepInDependency(/*Could not decode attribute arguments.*/)]
[BepInDependency(/*Could not decode attribute arguments.*/)]
[BepInDependency(/*Could not decode attribute arguments.*/)]
[BepInPlugin("com.indieanajones.indiesskills", "IndiesSkills", "1.1.0")]
[R2APISubmoduleDependency(new string[] { "LoadoutAPI", "LanguageAPI" })]
public class Main : BaseUnityPlugin
{
	public class SyncTetherPosition : INetMessage, ISerializableObject
	{
		public NetworkInstanceId netIdToUpdate;

		public Vector3 positionSetter;

		public SyncTetherPosition()
		{
		}

		public SyncTetherPosition(NetworkInstanceId netID, Vector3 positionGiven)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			netIdToUpdate = netID;
			positionSetter = positionGiven;
		}

		public void Deserialize(NetworkReader reader)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			netIdToUpdate = reader.ReadNetworkId();
			positionSetter = reader.ReadVector3();
		}

		public void OnReceived()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (!NetworkServer.active)
			{
				ClientScene.FindLocalObject(netIdToUpdate).transform.position = positionSetter;
			}
		}

		public void Serialize(NetworkWriter writer)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			writer.Write(netIdToUpdate);
			writer.Write(positionSetter);
		}
	}

	public void Start()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (Chainloader.PluginInfos.ContainsKey("com.rob.Paladin"))
		{
			PaladinPassive.addPaladinPassives();
			LifeDrain.addLifeDrain();
			ViciousSlash.addViciousSlash();
			TarBomb.addTarBomb();
			bool flag = default(bool);
			ContentAddition.AddEntityState<OilSlash>(ref flag);
			ContentAddition.AddEntityState<ThrowTarBomb>(ref flag);
			ContentAddition.AddEntityState<TarTetherMove>(ref flag);
			ContentAddition.AddEntityState<TarTetherMoveScepter>(ref flag);
			NetworkingAPI.RegisterMessageType<SyncTetherPosition>();
		}
	}

	public static Sprite LoadTexture2D(byte[] resourceBytes)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (resourceBytes == null)
		{
			throw new ArgumentNullException("resourceBytes");
		}
		Texture2D val = new Texture2D(128, 128, (TextureFormat)4, false);
		ImageConversion.LoadImage(val, resourceBytes, false);
		return Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(1f, 1f));
	}
}
