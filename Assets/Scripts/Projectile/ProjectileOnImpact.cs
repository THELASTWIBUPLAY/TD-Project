using System.Collections;
using UnityEngine;

public class ProjectileOnImpact : MonoBehaviour
{
    [Header("Visual & Impact References")]
    public GameObject visual;
    public GameObject impact;

    [Header("Settings")]
    public float impactWait = 0.5f;

    private GameObject impactInstance;
    private Vector3 ogScale;

    private void Awake()
    {
        ogScale = transform.localScale;
    }

    public void Impact()
    {
        transform.SetParent(null);
        transform.localScale = ogScale;

        if (visual != null)
        {
            visual.SetActive(false);
        }

        if (impact != null)
        {
            impactInstance = Instantiate(impact, transform.position, transform.rotation);
        }

        StartCoroutine(CleanupRoutine());
    }

    private IEnumerator CleanupRoutine()
    {
        yield return new WaitForSeconds(impactWait);
        Destroy(impactInstance);
        Destroy(gameObject);
    }
}