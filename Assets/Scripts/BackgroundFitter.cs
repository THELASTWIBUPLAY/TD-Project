using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    private void Start()
    {
        FitToScreenKeepAspect();
    }

    public void FitToScreenKeepAspect()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

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