using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance { get; private set; }

    public GameObject damageTextPrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SpawnDamageText(Vector3 position, float damage)
    {
        if (damageTextPrefab == null) return;

        // Beri sedikit offset acak agar angka tidak bertumpuk kaku
        Vector3 spawnPos = position + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(0.1f, 0.3f), 0f);
        GameObject obj = Instantiate(damageTextPrefab, spawnPos, Quaternion.identity);

        DamageText dt = obj.GetComponent<DamageText>();
        if (dt != null)
        {
            dt.Setup(damage);
        }
    }
}