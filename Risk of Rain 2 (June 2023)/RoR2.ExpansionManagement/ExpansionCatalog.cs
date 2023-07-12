using System;
using HG;
using RoR2.ContentManagement;

namespace RoR2.ExpansionManagement;

public static class ExpansionCatalog
{
	private static ExpansionDef[] _expansionDefs = Array.Empty<ExpansionDef>();

	public static ReadOnlyArray<ExpansionDef> expansionDefs => ReadOnlyArray<ExpansionDef>.op_Implicit(_expansionDefs);

	private static void SetExpansions(ExpansionDef[] newExpansionsDefs)
	{
		ArrayUtils.CloneTo<ExpansionDef>(newExpansionsDefs, ref _expansionDefs);
		for (int i = 0; i < _expansionDefs.Length; i++)
		{
			_expansionDefs[i].expansionIndex = (ExpansionIndex)i;
		}
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		SetExpansions(ContentManager.expansionDefs);
	}
}
