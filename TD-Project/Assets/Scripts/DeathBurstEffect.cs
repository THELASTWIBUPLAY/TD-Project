using UnityEngine;

public class DeathBurstEffect : MonoBehaviour
{
    public static void Create(Vector3 position, Color color, float baseScale = 0.5f)
    {
        GameObject burstGO = new GameObject("DeathBurst");
        burstGO.transform.position = position;

        ParticleSystem ps = burstGO.AddComponent<ParticleSystem>();
        burstGO.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Sprites/Default"));

        var main = ps.main;
        main.loop = false; 
        main.startLifetime = 0.35f;
        main.startSpeed = 4f * Mathf.Clamp(baseScale, 0.8f, 2f);
        main.startSize = 0.12f * baseScale;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.Destroy; 

        var emission = ps.emission;
        emission.rateOverTime = 0;
        int burstCount = Mathf.RoundToInt(10 * Mathf.Clamp(baseScale, 1f, 2.5f));
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, burstCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.15f * baseScale;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 1f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        ps.Play();
    }
}