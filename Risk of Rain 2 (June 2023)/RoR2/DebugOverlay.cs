using System;
using UnityEngine;

namespace RoR2;

public class DebugOverlay : MonoBehaviour
{
	public class MeshDrawer : IDisposable
	{
		private GameObject gameObject;

		private MeshFilter meshFilter;

		private MeshRenderer meshRenderer;

		public bool hasMeshOwnership;

		public Transform transform { get; private set; }

		public bool enabled
		{
			get
			{
				if (Object.op_Implicit((Object)(object)gameObject))
				{
					return gameObject.activeSelf;
				}
				return false;
			}
			set
			{
				if (Object.op_Implicit((Object)(object)gameObject))
				{
					gameObject.SetActive(value);
				}
			}
		}

		public Mesh mesh
		{
			get
			{
				return meshFilter.sharedMesh;
			}
			set
			{
				if (!((Object)(object)meshFilter.sharedMesh == (Object)(object)value))
				{
					if (Object.op_Implicit((Object)(object)meshFilter.sharedMesh) && hasMeshOwnership)
					{
						Object.Destroy((Object)(object)meshFilter.sharedMesh);
					}
					meshFilter.sharedMesh = value;
				}
			}
		}

		public Material material
		{
			get
			{
				return ((Renderer)meshRenderer).sharedMaterial;
			}
			set
			{
				((Renderer)meshRenderer).sharedMaterial = value;
			}
		}

		public MeshDrawer(Transform parent)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			gameObject = new GameObject("MeshDrawer");
			transform = gameObject.transform;
			transform.SetParent(parent);
			meshFilter = gameObject.AddComponent<MeshFilter>();
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
			material = defaultWireMaterial;
		}

		public void Dispose()
		{
			mesh = null;
			Object.Destroy((Object)(object)gameObject);
			gameObject = null;
			transform = null;
			meshFilter = null;
			meshRenderer = null;
		}
	}

	private static DebugOverlay instance;

	private Transform transform;

	public static Material defaultWireMaterial;

	private void Awake()
	{
		transform = ((Component)this).transform;
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		defaultWireMaterial = LegacyResourcesAPI.Load<Material>("Materials/UI/matDebugUI");
		GameObject val = new GameObject("DebugOverlay");
		instance = val.AddComponent<DebugOverlay>();
		Object.DontDestroyOnLoad((Object)val);
	}

	public static MeshDrawer GetMeshDrawer()
	{
		return new MeshDrawer(instance.transform);
	}
}
