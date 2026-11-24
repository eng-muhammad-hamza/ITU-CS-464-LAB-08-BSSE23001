using TMPro;
using UnityEngine;

public static class OrbBurstFx
{
    static Material cachedMaterial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => cachedMaterial = null;

    public static Material ParticleMaterial
    {
        get
        {
            if (cachedMaterial == null) cachedMaterial = new Material(Shader.Find("Sprites/Default"));
            return cachedMaterial;
        }
    }

    public static void Burst(Vector3 position, Color color, int count, float speed, float size, float lifetime)
    {
        GameObject go = new GameObject("orbBurst");
        go.transform.position = position;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.duration = 0.25f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.6f, lifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
        main.startColor = color;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = true;
        main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        ParticleSystem.SizeOverLifetimeModule sizeModule = ps.sizeOverLifetime;
        sizeModule.enabled = true;
        sizeModule.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = ParticleMaterial;
        renderer.sortingOrder = 20;
        ps.Play();
    }

    public static void Ring(Vector3 position, Color color, float maxRadius, float seconds)
    {
        GameObject go = new GameObject("pulseRing");
        go.transform.position = position;
        go.AddComponent<PulseRing>().Begin(color, maxRadius, seconds);
    }

    public static void FloatingText(Vector3 position, string message, Color color, float fontSize = 6f)
    {
        GameObject go = new GameObject("risingText");
        go.transform.position = position;
        TextMeshPro text = go.AddComponent<TextMeshPro>();
        text.rectTransform.sizeDelta = new Vector2(8f, 2f);
        text.text = message;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.sortingOrder = 30;
        go.AddComponent<RisingText>();
    }
}
