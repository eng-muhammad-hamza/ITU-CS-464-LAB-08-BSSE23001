using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HitFlashOverlay : MonoBehaviour
{
    Image flashImage;
    Color flashColor;
    float flashStart = -10f;
    float flashPeak;

    void Awake()
    {
        flashImage = GetComponent<Image>();
        flashImage.raycastTarget = false;
        flashImage.color = new Color(1f, 1f, 1f, 0f);
    }

    void OnEnable()
    {
        OrbVitals.Damaged += onOrbDamaged;
        OrbVitals.ShieldBlocked += onShieldBlocked;
        OrbVitals.Healed += onOrbHealed;
        flashImage.color = new Color(1f, 1f, 1f, 0f);
        flashStart = -10f;
    }

    void OnDisable()
    {
        OrbVitals.Damaged -= onOrbDamaged;
        OrbVitals.ShieldBlocked -= onShieldBlocked;
        OrbVitals.Healed -= onOrbHealed;
    }

    void onOrbDamaged(OrbHitReport report) => Flash(OrbPalette.DamageCoral, 0.5f);

    void onShieldBlocked(Vector3 at) => Flash(OrbPalette.ShieldIce, 0.28f);

    void onOrbHealed(int hearts, Vector3 at) => Flash(OrbPalette.HeartPink, 0.2f);

    void Flash(Color color, float peak)
    {
        flashColor = color;
        flashPeak = peak;
        flashStart = Time.unscaledTime;
    }

    void Update()
    {
        float since = Time.unscaledTime - flashStart;
        float alpha = since < 0.4f ? Mathf.Pow(1f - since / 0.4f, 2f) * flashPeak : 0f;
        flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
    }
}
