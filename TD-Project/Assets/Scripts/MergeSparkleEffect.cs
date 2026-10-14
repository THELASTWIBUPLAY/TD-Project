using UnityEngine;

public class MergeSparkleEffect : MonoBehaviour
{
    public static void Create(Vector3 position, int starLevel)
    {
        GameObject sparkleGO = new GameObject("MergeSparkle");
        sparkleGO.transform.position = position;

        ParticleSystem ps = sparkleGO.AddComponent<ParticleSystem>();
        sparkleGO.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Sprites/Default"));

        var main = ps.main;
        main.loop = false;
        main.startLifetime = 0.4f;
        main.startSpeed = 3.5f;
        main.startSize = 0.15f;

        main.startColor = (starLevel >= 3) ? new Color(1f, 0.5f, 0f) : new Color(1f, 0.9f, 0.2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        int count = (starLevel >= 3) ? 20 : 12;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 1.2f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        ps.Play();
    }
}