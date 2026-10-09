using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum WheelRewardType
{
    Character,
    Zonk
}

[System.Serializable]
public class WheelSlotData
{
    public string slotName;
    public WheelRewardType rewardType;
    public CharacterClassType characterClass;
    public Sprite slotIcon;
    public RuntimeAnimatorController idleAnimator;
}

public class RouletteWheelGenerator : MonoBehaviour
{
    [Header("Wheel Configuration")]
    public Transform wheelContainer;
    public GameObject slotPrefab;
    public int totalSlots = 8;
    public float radius = 200f;

    [Header("Spin Settings")]
    public float spinDuration = 3.5f;
    public int fullRounds = 5;
    public Button spinButton;
    public Button skipButton;

    [Header("Rewards Setup (8 Items)")]
    public List<WheelSlotData> slotRewards = new List<WheelSlotData>();

    private float currentAngle = 0f;
    private bool isSpinning = false;
    private List<GameObject> spawnedSlots = new List<GameObject>();

    void Start()
    {
        SetupDefaultRewardsIfEmpty();
        GenerateWheelSlots();

        if (spinButton != null)
        {
            spinButton.onClick.RemoveAllListeners();
            spinButton.onClick.AddListener(StartSpin);
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipSpin);
        }
    }

    void SetupDefaultRewardsIfEmpty()
    {
        if (slotRewards == null || slotRewards.Count == 0)
        {
            slotRewards = new List<WheelSlotData>()
            {
                new WheelSlotData { slotName = "Fighter", rewardType = WheelRewardType.Character, characterClass = CharacterClassType.Fighter },
                new WheelSlotData { slotName = "Zonk 1", rewardType = WheelRewardType.Zonk },
                new WheelSlotData { slotName = "Mage", rewardType = WheelRewardType.Character, characterClass = CharacterClassType.Mage },
                new WheelSlotData { slotName = "Ranged", rewardType = WheelRewardType.Character, characterClass = CharacterClassType.Ranged },
                new WheelSlotData { slotName = "Zonk 2", rewardType = WheelRewardType.Zonk },
                new WheelSlotData { slotName = "Support", rewardType = WheelRewardType.Character, characterClass = CharacterClassType.Support },
                new WheelSlotData { slotName = "Tank", rewardType = WheelRewardType.Character, characterClass = CharacterClassType.Tank },
                new WheelSlotData { slotName = "Zonk 3", rewardType = WheelRewardType.Zonk }
            };
        }
    }

    public void GenerateWheelSlots()
    {
        foreach (GameObject slot in spawnedSlots)
        {
            if (slot != null) Destroy(slot);
        }
        spawnedSlots.Clear();

        if (wheelContainer == null || slotPrefab == null) return;

        float angleStep = 360f / totalSlots;

        for (int i = 0; i < totalSlots; i++)
        {
            float angleDeg = i * angleStep;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(Mathf.Sin(angleRad) * radius, Mathf.Cos(angleRad) * radius, 0f);

            GameObject slotGO = Instantiate(slotPrefab, wheelContainer);
            slotGO.transform.localPosition = pos;
            slotGO.transform.localRotation = Quaternion.Euler(0, 0, -angleDeg);
            spawnedSlots.Add(slotGO);

            WheelSlotUI slotUI = slotGO.GetComponent<WheelSlotUI>();
            if (slotUI != null && i < slotRewards.Count)
            {
                slotUI.SetupSlot(slotRewards[i].slotIcon, slotRewards[i].idleAnimator);
            }
        }
    }

    public void StartSpin()
    {
        if (isSpinning) return;

        int winningIndex = Random.Range(0, slotRewards.Count);
        StartCoroutine(SpinRoutine(winningIndex));
    }

    public void SkipSpin()
    {
        StopAllCoroutines();
        isSpinning = false;

        int winningIndex = Random.Range(0, slotRewards.Count);

        float angleStep = 360f / totalSlots;
        float targetAngle = winningIndex * angleStep;
        if (wheelContainer != null)
        {
            wheelContainer.eulerAngles = new Vector3(0, 0, targetAngle);
        }

        OnSpinCompleted(winningIndex);
}

    private IEnumerator SpinRoutine(int targetIndex)
    {
        isSpinning = true;
        if (spinButton != null) spinButton.interactable = false;

        float elapsed = 0f;
        float angleStep = 360f / totalSlots;

        float targetAngle = (fullRounds * 360f) + (targetIndex * angleStep);
        float startAngle = currentAngle % 360f;
        float totalToRotate = targetAngle - startAngle;

        while (elapsed < spinDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / spinDuration;
            float easeOut = 1f - (1f - t) * (1f - t);

            currentAngle = startAngle + (totalToRotate * easeOut);
            wheelContainer.eulerAngles = new Vector3(0, 0, currentAngle);

            yield return null;
        }

        currentAngle = targetAngle;
        wheelContainer.eulerAngles = new Vector3(0, 0, targetAngle);

        isSpinning = false;
        OnSpinCompleted(targetIndex);
    }

    private void OnSpinCompleted(int winningIndex)
    {
        WheelSlotData reward = slotRewards[winningIndex];

        if (reward.rewardType == WheelRewardType.Character)
        {
            Debug.Log($"<color=green>[SpinWheel] SELAMAT! Mendapatkan Karakter Kelas: {reward.characterClass}</color>");
            if (SlotGridManager.Instance != null)
            {
                SlotGridManager.Instance.SpawnCharacterFromWheel(reward.characterClass);
            }
        }
        else
        {
            Debug.Log("<color=red>[SpinWheel] ZONK! Kamu tidak mendapatkan karakter pada wave ini.</color>");
        }

        if (spinButton != null) spinButton.interactable = true;

        StartCoroutine(ClosePanelRoutine());
    }

    private IEnumerator ClosePanelRoutine()
    {
        yield return new WaitForSecondsRealtime(1.2f);

        gameObject.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestoreSpeedAfterModal();
        }
    }
}