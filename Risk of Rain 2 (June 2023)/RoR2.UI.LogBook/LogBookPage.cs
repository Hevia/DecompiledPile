using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI.LogBook;

public class LogBookPage : MonoBehaviour
{
	public RawImage iconImage;

	public ModelPanel modelPanel;

	public TextMeshProUGUI titleText;

	public TextMeshProUGUI categoryText;

	public TextMeshProUGUI pageNumberText;

	public RectTransform contentContainer;

	private PageBuilder pageBuilder;

	public void SetEntry(UserProfile userProfile, Entry entry)
	{
		pageBuilder?.Destroy();
		pageBuilder = new PageBuilder();
		pageBuilder.container = contentContainer;
		pageBuilder.entry = entry;
		pageBuilder.userProfile = userProfile;
		entry.pageBuilderMethod?.Invoke(pageBuilder);
		iconImage.texture = entry.iconTexture;
		((TMP_Text)titleText).text = entry.GetDisplayName(userProfile);
		((TMP_Text)categoryText).text = entry.GetCategoryDisplayName(userProfile);
		modelPanel.modelPrefab = entry.modelPrefab;
		((Component)((Component)modelPanel).transform.parent.parent).gameObject.SetActive(Object.op_Implicit((Object)(object)entry.modelPrefab));
	}
}
