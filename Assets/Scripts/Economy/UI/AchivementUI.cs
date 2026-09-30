using Assets.Scripts.Economy.Managers;
using UnityEngine;

public class AchievementUI : MonoBehaviour
{
	[SerializeField] private AchievementSlotUI[] _slots;
	private CanvasGroup _myCanvas;
	private bool _isSubscribed;

	private void Awake()
	{
		_myCanvas = GetComponent<CanvasGroup>();
	}

	private void Start()
	{
		TrySubscribe();
		RefreshUI();
	}

	private void OnEnable()
	{
		TrySubscribe();
		RefreshUI();
	}

	private void OnDisable()
	{
		if (AchievementManager.Instance != null && _isSubscribed)
		{
			AchievementManager.Instance.OnAchievementUpdated -= RefreshUI;
			_isSubscribed = false;
		}
	}

	private void TrySubscribe()
	{
		if (!_isSubscribed && AchievementManager.Instance != null)
		{
			AchievementManager.Instance.OnAchievementUpdated += RefreshUI;
			_isSubscribed = true;
		}
	}

	public void RefreshUI()
	{
		if (AchievementManager.Instance == null || AchievementManager.Instance.Database == null)
			return;

		var list = AchievementManager.Instance.Database.Achievements;

		for (int i = 0; i < _slots.Length; i++)
		{
			if (_slots[i] == null) continue;

			if (i < list.Count)
			{
				_slots[i].gameObject.SetActive(true);
				var data = list[i];
				int progress = AchievementManager.Instance.GetProgress(data.achievementID);
				var state = AchievementManager.Instance.GetAchievementState(data);
				_slots[i].Setup(data, progress, state);
			}
			else
			{
				_slots[i].gameObject.SetActive(false);
			}
		}
	}

	public void OpenPanel()
	{
		RefreshUI();
		if (MainMenuManager.Instance != null)
		{
			MainMenuManager.Instance.TogglePanel(_myCanvas, true);
			MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, false);
		}
	}

	public void ClosePanel()
	{
		if (MainMenuManager.Instance != null)
		{
			MainMenuManager.Instance.TogglePanel(_myCanvas, false);
			MainMenuManager.Instance.TogglePanel(MainMenuManager.Instance.MainPanel, true);
		}
	}
}