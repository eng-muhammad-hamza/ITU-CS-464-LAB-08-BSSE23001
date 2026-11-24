using UnityEngine;

public class PulseRing : MonoBehaviour
{
    const int Segments = 40;
    LineRenderer line;
    Color ringColor;
    float maxRadius;
    float duration;
    float elapsed;

    public void Begin(Color color, float radius, float seconds)
    {
        ringColor = color;
        maxRadius = radius;
        duration = Mathf.Max(0.05f, seconds);
        line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = Segments;
        line.sharedMaterial = OrbBurstFx.ParticleMaterial;
        line.sortingOrder = 15;
        Apply(0f);
    }

    void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        float p = Mathf.Clamp01(elapsed / duration);
        Apply(p);
        if (p >= 1f) Destroy(gameObject);
    }

    void Apply(float p)
    {
        float radius = Mathf.Lerp(0.1f, maxRadius, Ease.Evaluate(EaseType.OutQuad, p));
        for (int i = 0; i < Segments; i++)
        {
            float a = i * Mathf.PI * 2f / Segments;
            line.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
        }
        Color faded = ringColor;
        faded.a = 1f - p;
        line.startColor = faded;
        line.endColor = faded;
        float width = Mathf.Lerp(0.18f, 0.02f, p);
        line.startWidth = width;
        line.endWidth = width;
    }
}
