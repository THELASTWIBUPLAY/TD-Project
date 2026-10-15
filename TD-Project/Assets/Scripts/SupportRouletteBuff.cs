using System.Collections;
using UnityEngine;
using TMPro;

public class SupportRouletteBuff : MonoBehaviour
{
    private enum BuffType { Aspd, CritRate, CritDmg, Attack, Range }

    private TextMeshPro rollDisplay;
    private bool isRunning = false;

    void Awake()
    {
        GameObject textObj = new GameObject("RouletteTextDisplay");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);

        rollDisplay = textObj.AddComponent<TextMeshPro>();
        rollDisplay.fontSize = 4f;
        rollDisplay.alignment = TextAlignmentOptions.Center;
        rollDisplay.sortingOrder = 10;
        rollDisplay.text = "";
    }

    void OnEnable()
    {
        if (!isRunning)
        {
            StartCoroutine(RouletteLoop());
        }
    }

    void OnDisable()
    {
        ResetAllBuffs();
        StopAllCoroutines();
        isRunning = false;
    }

    private IEnumerator RouletteLoop()
    {
        isRunning = true;

        while (true)
        {
            float rollTimer = 0f;
            BuffType chosenBuff = (BuffType)Random.Range(0, 5);

            string[] buffLabels = { "ASPD!", "CRIT RATE!", "CRIT DMG!", "ATK DMG!", "RANGE!" };
            Color[] buffColors = { Color.green, Color.yellow, new Color(1f, 0.5f, 0f), Color.red, Color.cyan };

            while (rollTimer < 1.5f)
            {
                int randIdx = Random.Range(0, 5);
                rollDisplay.text = buffLabels[randIdx];
                rollDisplay.color = buffColors[randIdx];
                rollTimer += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }

            ApplyBuff(chosenBuff);
            rollDisplay.text = $"★ {buffLabels[(int)chosenBuff]} ★";
            rollDisplay.color = buffColors[(int)chosenBuff];

            float buffDuration = 20f;
            while (buffDuration > 0f)
            {
                rollDisplay.text = $"{buffLabels[(int)chosenBuff]} ({Mathf.CeilToInt(buffDuration)}s)";
                yield return new WaitForSeconds(1f);
                buffDuration -= 1f;
            }

            ResetAllBuffs();
            rollDisplay.text = "";

            yield return new WaitForSeconds(38.5f); 
        }
    }

    private void ApplyBuff(BuffType type)
    {
        ResetAllBuffs();

        switch (type)
        {
            case BuffType.Aspd:
                Character.GlobalAtkSpeedMultiplier += 0.35f;
                break;
            case BuffType.CritRate:
                Character.GlobalCritRateBonus = 0.30f;
                break;
            case BuffType.CritDmg:
                Character.GlobalCritDmgBonus = 0.50f;
                break;
            case BuffType.Attack:
                Character.GlobalDamageBonusPercent += 30f;
                break;
            case BuffType.Range:
                Character.GlobalRangeMultiplier = 1.5f;
                break;
        }
    }

    private void ResetAllBuffs()
    {
        Character.GlobalAtkSpeedMultiplier = 1f;
        Character.GlobalCritRateBonus = 0f;
        Character.GlobalCritDmgBonus = 0f;
        Character.GlobalDamageBonusPercent = 0f;
        Character.GlobalRangeMultiplier = 1f;
    }
}