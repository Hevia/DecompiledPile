using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace IndiesSkills.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = new ResourceManager("IndiesSkills.Properties.Resources", typeof(Resources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static byte[] Feed_The_Tar_Icon
	{
		get
		{
			object @object = ResourceManager.GetObject("Feed_The_Tar_Icon", resourceCulture);
			return (byte[])@object;
		}
	}

	internal static byte[] Grasping_Tar_Icon
	{
		get
		{
			object @object = ResourceManager.GetObject("Grasping_Tar_Icon", resourceCulture);
			return (byte[])@object;
		}
	}

	internal static byte[] Stalwart_Icon
	{
		get
		{
			object @object = ResourceManager.GetObject("Stalwart_Icon", resourceCulture);
			return (byte[])@object;
		}
	}

	internal Resources()
	{
	}
}
