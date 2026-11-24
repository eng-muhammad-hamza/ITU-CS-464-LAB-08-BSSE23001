using TMPro;
using UnityEngine;

public class RisingText : MonoBehaviour
{
    const float Lifetime = 0.9f;
    const float Rise = 1.6f;
    TextMeshPro text;
    Vector3 start;
    Color startColor;
    float elapsed;

    void Start()
    {
        text = GetComponent<TextMeshPro>();
        start = transform.position;
        startColor = text.color;
    }

    void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        float p = Mathf.Clamp01(elapsed / Lifetime);
        transform.position = start + Vector3.up * (Ease.Evaluate(EaseType.OutCubic, p) * Rise);

        float pop = p < 0.2f ? Mathf.Lerp(0.4f, 1.25f, Ease.Evaluate(EaseType.OutQuad, p / 0.2f)) : Mathf.Lerp(1.25f, 1f, Mathf.Clamp01((p - 0.2f) / 0.2f));
        transform.localScale = Vector3.one * pop;

        Color c = startColor;
        c.a = p < 0.6f ? 1f : 1f - (p - 0.6f) / 0.4f;
        text.color = c;
        if (p >= 1f) Destroy(gameObject);
    }
}
