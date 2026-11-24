using UnityEngine;

public class ArenaCameraShake : MonoBehaviour
{
    Vector3 restPosition;
    float timeLeft;
    float total;
    float magnitude;

    void Awake() => restPosition = transform.localPosition;

    void OnEnable()
    {
        OrbVitals.Damaged += onOrbDamaged;
        OrbVitals.ShieldBlocked += onShieldBlocked;
        PlayerOrb.Dashed += onOrbDashed;
        GameSession.RunWon += onRunWon;
        GameSession.RunLost += onRunLost;
    }

    void OnDisable()
    {
        OrbVitals.Damaged -= onOrbDamaged;
        OrbVitals.ShieldBlocked -= onShieldBlocked;
        PlayerOrb.Dashed -= onOrbDashed;
        GameSession.RunWon -= onRunWon;
        GameSession.RunLost -= onRunLost;
    }

    void onOrbDamaged(OrbHitReport report) => Shake(0.25f, 0.32f);

    void onShieldBlocked(Vector3 at) => Shake(0.15f, 0.14f);

    void onOrbDashed(Vector3 at, Color color) => Shake(0.1f, 0.06f);

    void onRunWon(Vector3 at) => Shake(0.3f, 0.12f);

    void onRunLost(Vector3 at) => Shake(0.5f, 0.45f);

    void Shake(float seconds, float strength)
    {
        if (!GameData.ScreenShake || seconds <= 0f || strength <= 0f) return;
        if (strength < Current()) return;
        timeLeft = seconds;
        total = seconds;
        magnitude = strength;
    }

    float Current() => total <= 0f ? 0f : magnitude * Mathf.Max(0f, timeLeft / total);

    void LateUpdate()
    {
        Vector3 offset = Vector3.zero;
        if (timeLeft > 0f)
        {
            timeLeft -= Time.unscaledDeltaTime;
            Vector2 r = Random.insideUnitCircle * Current();
            offset = new Vector3(r.x, r.y, 0f);
        }
        transform.localPosition = restPosition + offset;
    }
}
