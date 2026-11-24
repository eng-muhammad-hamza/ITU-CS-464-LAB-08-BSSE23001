using System;
using UnityEngine;

public struct OrbHitReport
{
    public int heartsLeft;
    public int maxHearts;
    public Vector3 position;
    public Vector3 source;
}

public class OrbVitals : MonoBehaviour
{
    public static event Action<OrbHitReport> Damaged;
    public static event Action<int, Vector3> Healed;
    public static event Action<Vector3> ShieldBlocked;
    public static event Action<Vector3> ShieldRaised;
    public static event Action<Vector3> Died;

    float invulnerableUntil;

    public int Hearts { get; private set; }
    public int MaxHearts { get; private set; }
    public bool HasShield { get; private set; }
    public bool Alive { get; private set; } = true;
    public bool IsInvulnerable => Time.time < invulnerableUntil;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Damaged = null;
        Healed = null;
        ShieldBlocked = null;
        ShieldRaised = null;
        Died = null;
    }

    public void Configure(int hearts, bool startsWithShield)
    {
        MaxHearts = hearts;
        Hearts = hearts;
        HasShield = startsWithShield;
        Alive = true;
    }

    public void GrantInvulnerability(float seconds)
    {
        invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds);
    }

    public bool TryHurt(Vector3 source)
    {
        GameSession session = GameSession.Instance;
        if (!Alive || session == null || !session.Running || IsInvulnerable) return false;

        if (HasShield)
        {
            HasShield = false;
            invulnerableUntil = Time.time + 0.7f;
            ShieldBlocked?.Invoke(transform.position);
            return true;
        }

        Hearts--;
        invulnerableUntil = Time.time + 1.2f;
        Damaged?.Invoke(new OrbHitReport
        {
            heartsLeft = Hearts,
            maxHearts = MaxHearts,
            position = transform.position,
            source = source
        });

        if (Hearts <= 0)
        {
            Alive = false;
            Died?.Invoke(transform.position);
        }
        return true;
    }

    public void Heal()
    {
        if (!Alive || Hearts >= MaxHearts) return;
        Hearts++;
        Healed?.Invoke(Hearts, transform.position);
    }

    public void RaiseShield()
    {
        if (!Alive) return;
        HasShield = true;
        ShieldRaised?.Invoke(transform.position);
    }
}
