using System.Collections.Generic;
using UnityEngine;

public class SlotGridManager : MonoBehaviour
{
    public static SlotGridManager Instance { get; private set; }

    [Header("Grid Layout (5 Kolom x 3 Baris)")]
    public int columns = 5;
    public int rows = 3;
    public float spacingX = 1.0f; // Jarak antar slot horizontal
    public float spacingY = 1.0f; // Jarak antar slot vertikal
    public Vector2 gridCenterPosition = new Vector2(0f, -2.5f); // Titik pusat formasi

    [Header("Prefabs")]
    public GameObject slotPrefab;
    public GameObject characterPrefab;

    [HideInInspector]
    public List<CharacterSlot> allSlots = new List<CharacterSlot>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GenerateGrid();
    }

    void Start()
    {
        SpawnDefaultCenterCharacter();
    }

    void GenerateGrid()
    {
        allSlots.Clear();

        // Hitung batas awal kiri-bawah agar formasi presisi di tengah titik gridCenterPosition
        float startX = gridCenterPosition.x - ((columns - 1) * spacingX / 2f);
        float startY = gridCenterPosition.y - ((rows - 1) * spacingY / 2f);

        int index = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector2 pos = new Vector2(startX + (c * spacingX), startY + (r * spacingY));
                GameObject newSlot = Instantiate(slotPrefab, pos, Quaternion.identity, transform);
                newSlot.name = $"Slot_{index}";

                CharacterSlot slotComponent = newSlot.GetComponent<CharacterSlot>();
                allSlots.Add(slotComponent);
                index++;
            }
        }
    }

    void SpawnDefaultCenterCharacter()
    {
        // 15 slot index-nya 0 sampai 14, titik tengah pastinya adalah indeks 7
        int centerIndex = allSlots.Count / 2;

        if (allSlots.Count > centerIndex && allSlots[centerIndex] != null)
        {
            CharacterSlot centerSlot = allSlots[centerIndex];
            GameObject defaultChar = Instantiate(characterPrefab, centerSlot.transform.position, Quaternion.identity);
            centerSlot.AssignCharacter(defaultChar);
        }
    }

    // Mencari slot kosong secara acak
    public CharacterSlot GetRandomEmptySlot()
    {
        List<CharacterSlot> emptySlots = new List<CharacterSlot>();

        // 1. Kumpulkan semua slot yang saat ini belum ada karakternya
        foreach (CharacterSlot slot in allSlots)
        {
            if (slot != null && !slot.isOccupied)
            {
                emptySlots.Add(slot);
            }
        }

        // 2. Jika ada slot kosong, pilih salah satu secara acak
        if (emptySlots.Count > 0)
        {
            int randIndex = Random.Range(0, emptySlots.Count);
            return emptySlots[randIndex];
        }

        return null; // Semua 15 slot sudah terisi penuh
    }
    // Fungsi untuk memicu merge 3 karakter
    public void CheckAndExecuteMerge()
    {
        // Cek dari bintang 1 dulu, baru bintang 2
        for (int checkStar = 1; checkStar <= 2; checkStar++)
        {
            List<CharacterSlot> matchingSlots = new List<CharacterSlot>();

            // Cari semua slot yang berisi karakter dengan bintang yang cocok
            foreach (CharacterSlot slot in allSlots)
            {
                if (slot != null && slot.isOccupied && slot.currentCharacter != null)
                {
                    if (slot.currentCharacter.starLevel == checkStar)
                    {
                        matchingSlots.Add(slot);
                    }
                }
            }

            // Jika ketemu minimal 3 karakter dengan bintang sama:
            if (matchingSlots.Count >= 3)
            {
                CharacterSlot targetSlot = matchingSlots[0];
                CharacterSlot sacrificed1 = matchingSlots[1];
                CharacterSlot sacrificed2 = matchingSlots[2];

                // Hapus 2 karakter kurban dan bersihkan petaknya
                if (sacrificed1.currentCharacter != null)
                {
                    Destroy(sacrificed1.currentCharacter.gameObject);
                    sacrificed1.ClearSlot();
                }

                if (sacrificed2.currentCharacter != null)
                {
                    Destroy(sacrificed2.currentCharacter.gameObject);
                    sacrificed2.ClearSlot();
                }

                // Update karakter utama menjadi bintang berikutnya
                if (targetSlot.currentCharacter != null)
                {
                    targetSlot.currentCharacter.SetStarLevel(checkStar + 1);
                    Debug.Log($"MERGE SUKSES: Menjadi Bintang {checkStar + 1} di {targetSlot.gameObject.name}");
                }

                // Cek apakah merge lanjutan (misal 3 buah bintang 2 -> bintang 3) bisa langsung terjadi
                CheckAndExecuteMerge();
                break;
            }
        }
    }
}