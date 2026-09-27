using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private ResourcesItemSO _resources;
    public ResourcesItemSO Resources => _resources;

    [Header("Panels")]
    [SerializeField] private CanvasGroup _recruitPanel;
    [SerializeField] private CanvasGroup _specialPanel;
    [SerializeField] private CanvasGroup _resourcesPanel;
    private CanvasGroup myPanel;

    private void Awake()
    {
        myPanel = GetComponent<CanvasGroup>();
    }

    public void OpenRecruitShop()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_recruitPanel, true);
            MainMenuManager.Instance.TogglePanel(_specialPanel, false);
            MainMenuManager.Instance.TogglePanel(_resourcesPanel, false);
        }
    }

    public void OpenSpecialShop()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_recruitPanel, false);
            MainMenuManager.Instance.TogglePanel(_specialPanel, true);
            MainMenuManager.Instance.TogglePanel(_resourcesPanel, false);
        }
    }

    public void OpenResourcesShop()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.TogglePanel(_recruitPanel, false);
            MainMenuManager.Instance.TogglePanel(_specialPanel, false);
            MainMenuManager.Instance.TogglePanel(_resourcesPanel, true);
        }
    }
}