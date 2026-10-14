using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    [Header("Sprite Variants")]
    [Tooltip("Sprite untuk layar standar (misal 9:16)")]
    public Sprite sprite916;

    [Tooltip("Sprite untuk layar jangkung/panjang (misal 9:19, 9:20)")]
    public Sprite sprite919;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        ApplyBackground();
    }

    public void ApplyBackground()
    {
        if (sr == null) return;

        SelectSpriteByAspectRatio();

        FitToScreenKeepAspect();
    }

    private void SelectSpriteByAspectRatio()
    {
        float screenAspect = (float)Screen.height / Screen.width;

        if (screenAspect >= 1.95f && sprite919 != null)
        {
            sr.sprite = sprite919;
        }
        else if (sprite916 != null)
        {
            sr.sprite = sprite916;
        }
    }

    public void FitToScreenKeepAspect()
    {
        if (sr.sprite == null) return;

        transform.localScale = Vector3.one;

        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        float worldScreenHeight = mainCam.orthographicSize * 2.0f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        float scaleX = worldScreenWidth / spriteWidth;
        float scaleY = worldScreenHeight / spriteHeight;

        float maxScale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(maxScale, maxScale, 1f);
    }
}