using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveTester : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "game_save.json");

    private void Update()
    {
        // F5: Simpan Data Lokal Secara Paksa
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ForceSave();
        }

        // F6: Muat Ulang Data dari File Save Lokal
        if (Input.GetKeyDown(KeyCode.F6))
        {
            ForceLoad();
        }

        // F7: Cetak Isi File JSON ke Console
        if (Input.GetKeyDown(KeyCode.F7))
        {
            PrintSaveFileContent();
        }

        // F8: Buka Folder Direktori Save File di Windows Explorer / Mac Finder
        if (Input.GetKeyDown(KeyCode.F8))
        {
            OpenSaveDirectory();
        }

        // F9: Hapus File Save (Simulasi Pemain Baru / Fresh Install)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            DeleteSaveFile();
        }
    }

    [ContextMenu("Save: Force Save Local")]
    public void ForceSave()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveLocal();
            Debug.Log("<color=green>[SaveTester] Data berhasil disimpan secara manual ke disk!</color>");
        }
        else
        {
            Debug.LogError("[SaveTester] SaveManager.Instance belum terpasang di Scene!");
        }
    }

    [ContextMenu("Save: Force Load Local")]
    public void ForceLoad()
    {
        if (SaveManager.Instance != null)
        {
            bool success = SaveManager.Instance.LoadLocal();
            if (success)
            {
                Debug.Log("<color=cyan>[SaveTester] Data save lokal berhasil dimuat kembali ke seluruh manager!</color>");
            }
            else
            {
                Debug.LogWarning("[SaveTester] Gagal memuat save atau file save belum ada.");
            }
        }
    }

    [ContextMenu("Save: Print JSON Content to Console")]
    public void PrintSaveFileContent()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.LogWarning($"[SaveTester] File save belum dibuat di: {SaveFilePath}");
            return;
        }

        try
        {
            string content = File.ReadAllText(SaveFilePath);
            Debug.Log($"<color=yellow>=== ISI FILE SAVE JSON ===</color>\n{content}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveTester] Gagal membaca isi save file: {ex.Message}");
        }
    }

    [ContextMenu("Save: Open Save Directory in Explorer")]
    public void OpenSaveDirectory()
    {
        string dirPath = Application.persistentDataPath;

#if UNITY_EDITOR
        // Buka folder dan langsung sorot filenya di File Explorer OS
        if (File.Exists(SaveFilePath))
        {
            EditorUtility.RevealInFinder(SaveFilePath);
        }
        else
        {
            EditorUtility.RevealInFinder(dirPath);
        }
#else
        // Untuk Development Build Standalone / PC
        System.Diagnostics.Process.Start(dirPath);
#endif
        Debug.Log($"[SaveTester] Membuka folder save: {dirPath}");
    }

    [ContextMenu("Save: Delete Save File (Reset All Data)")]
    public void DeleteSaveFile()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            Debug.Log("<color=red>[SaveTester] File 'game_save.json' BERHASIL DIHAPUS!</color>");
            Debug.Log("[SaveTester] Restart Play Mode untuk menguji alur pemain baru (First-Time User).");
        }
        else
        {
            Debug.LogWarning("[SaveTester] File save tidak ditemukan untuk dihapus.");
        }
    }
#endif
}