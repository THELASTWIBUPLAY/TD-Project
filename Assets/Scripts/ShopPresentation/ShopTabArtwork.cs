using UnityEngine;

// Presentation only. Existing tab buttons still decide which page is open.
public class ShopTabArtwork : MonoBehaviour
{
    public GameObject[] pages;
    public UnityEngine.UI.Image[] tabs;
    public Sprite[] selectedSprites;
    private int previous = -1;

    private void OnEnable() => previous = -1;

    private void LateUpdate()
    {
        int selected = 0;
        for (int i = 0; i < pages.Length; i++)
            if (pages[i] && pages[i].activeSelf) { selected = i; break; }
        if (selected == previous) return;
        previous = selected;
        for (int i = 0; i < tabs.Length; i++)
        {
            // Keep identical geometry between states; the supplied dark sprites have different padding.
            tabs[i].sprite = selectedSprites[i];
            tabs[i].color = i == selected ? Color.white : new Color(.35f, .45f, .55f, 1f);
            if (i != selected) tabs[i].transform.SetAsFirstSibling();
            var label = tabs[i].GetComponentInChildren<TMPro.TMP_Text>();
            if (label) label.color = i == selected ? new Color(.03f, .13f, .22f) : Color.white;
        }
        tabs[selected].transform.SetAsLastSibling();
    }
}
