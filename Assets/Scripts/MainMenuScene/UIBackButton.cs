using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBackButton : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();

        // Matikan navigasi otomatis agar tidak memicu bug salah tekan tombol via spasi/enter
        Navigation nav = _button.navigation;
        nav.mode = Navigation.Mode.None;
        _button.navigation = nav;
    }

    private void OnEnable()
    {
        if (_button != null)
        {
            _button.onClick.AddListener(HandleBackClick);
        }
    }

    private void OnDisable()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(HandleBackClick);
        }
    }

    private void HandleBackClick()
    {
        if (MainMenuManager.Instance != null)
        {
            MainMenuManager.Instance.Back();
        }
        else
        {
            Debug.LogWarning("[UIBackButton] MainMenuManager.Instance tidak ditemukan di Scene!");
        }
    }
}