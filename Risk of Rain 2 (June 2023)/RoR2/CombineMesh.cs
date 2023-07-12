using UnityEngine;

namespace RoR2;

public class CombineMesh : MonoBehaviour
{
	private void Start()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		Renderer component = ((Component)this).GetComponent<Renderer>();
		MeshFilter[] componentsInChildren = ((Component)this).GetComponentsInChildren<MeshFilter>();
		CombineInstance[] array = (CombineInstance[])(object)new CombineInstance[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((CombineInstance)(ref array[i])).mesh = componentsInChildren[i].sharedMesh;
			((CombineInstance)(ref array[i])).transform = ((Component)componentsInChildren[i]).transform.localToWorldMatrix;
			((Component)componentsInChildren[i]).gameObject.SetActive(false);
		}
		MeshFilter obj = ((Component)this).gameObject.AddComponent<MeshFilter>();
		obj.mesh = new Mesh();
		obj.mesh.CombineMeshes(array, true, true, true);
		component.material = ((Component)componentsInChildren[0]).GetComponent<Renderer>().sharedMaterial;
		((Component)this).gameObject.SetActive(true);
	}
}
