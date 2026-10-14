using System.Collections;
using UnityEngine;

public class ProjectileOnImpact : MonoBehaviour
{
    [Header("Visual & Impact References")]
    public GameObject visual;
    public GameObject impact;

    [Header("Settings")]
    public float impactWait = 0.5f;
    public bool impactInheritRotation = false;

    private GameObject impactInstance;

    public void Impact()
    {
        transform.SetParent(null);

        if (visual != null)
        {
            visual.SetActive(false);
        }

        if (impact != null)
        {
            impactInstance = Instantiate(impact, transform.position, impactInheritRotation ? transform.rotation : Quaternion.identity);
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