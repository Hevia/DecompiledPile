using System.IO;
using System.Reflection;
using R2API;
using UnityEngine;

namespace Rorschach;

internal class Assets
{
	public static AssetBundle MainAssetBundle;

	public static void PopulateAssets()
	{
		if ((Object)(object)MainAssetBundle == (Object)null)
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Rorschach.rorschachassets");
			MainAssetBundle = AssetBundle.LoadFromStream(stream);
		}
		using (Stream stream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("Rorschach.Rorschach.bnk"))
		{
			byte[] array = new byte[stream2.Length];
			stream2.Read(array, 0, array.Length);
			SoundBanks.Add(array);
		}
		using Stream stream3 = Assembly.GetExecutingAssembly().GetManifestResourceStream("Rorschach.Rorschach2.bnk");
		byte[] array2 = new byte[stream3.Length];
		stream3.Read(array2, 0, array2.Length);
		SoundBanks.Add(array2);
	}
}
