using UnityEngine;
using UnityEngine.UI;

public class WheelSlotUI : MonoBehaviour
{
    public Image iconImage;
    public Animator slotAnimator;

    public void SetupSlot(Sprite sprite, RuntimeAnimatorController animController)
    {
        if (iconImage == null) return;

        iconImage.gameObject.SetActive(true);

        if (sprite != null)
        {
            iconImage.sprite = sprite;
        }

        if (animController != null && slotAnimator != null)
        {
            slotAnimator.enabled = true;
            slotAnimator.runtimeAnimatorController = animController;
        }
        else if (slotAnimator != null)
        {
            slotAnimator.enabled = false;
        }
    }
}