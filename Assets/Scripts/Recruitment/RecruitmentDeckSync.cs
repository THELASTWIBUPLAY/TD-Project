using System;
using System.Threading.Tasks;
using UnityEngine;

// Put on the always-active HeroManager so Deck does not depend on opening Shop first.
public class RecruitmentDeckSync : MonoBehaviour
{
    [SerializeField] private RecruitmentService service;
    private async void Start()
    {
        try
        {
            while (this && SaveManager.Instance != null && !SaveManager.Instance.IsLoaded) await Task.Delay(100);
            while (this && service && service.Busy) await Task.Delay(100);
            if (!this || !service) return;
            await service.RefreshAsync();
        }
        catch (Exception e) { Debug.LogWarning("[Recruitment] Initial Deck sync failed; use Shop Refresh to retry. " + e.Message); }
    }
}
