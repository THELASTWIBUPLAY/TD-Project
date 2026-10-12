using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleStateButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [System.Serializable]
    public struct StateVisual
    {
        public string stateName;     
        public Sprite normalSprite;   
        public Sprite pressedSprite;  
    }

    [Header("UI Reference")]
    public Image targetImage;

    [Header("Daftar State (Normal & Pressed per State)")]
    public StateVisual[] buttonStates; 
    
    private int currentStateIndex = 0;
    private bool isPressed = false;

    public void OnToggleClick()
    {
        if (buttonStates == null || buttonStates.Length == 0) return;

        currentStateIndex = (currentStateIndex + 1) % buttonStates.Length;
        UpdateVisual();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateVisual();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (targetImage == null || buttonStates == null || buttonStates.Length == 0) return;

        StateVisual current = buttonStates[currentStateIndex];

        if (isPressed && current.pressedSprite != null)
        {
            targetImage.sprite = current.pressedSprite;
        }
        else if (current.normalSprite != null)
        {
            targetImage.sprite = current.normalSprite;
        }
    }
}