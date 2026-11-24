using System;
using UnityEngine;

public enum PickupKind { Shard, Heart, Shield, Boost }

public class OrbPickup : MonoBehaviour
{
    public static event Action<PickupKind, Vector3> Collected;

    PickupKind kind;
    SpriteRenderer icon;
    float born;
    float life;
    bool collected;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Collected = null;

    public static OrbPickup Create(PickupKind kind, Vector2 position, Transform parent)
    {
        GameObject go = new GameObject(kind + "Pickup");
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        OrbPickup pickup = go.AddComponent<OrbPickup>();
        pickup.Setup(kind);
        return pickup;
    }

    void Setup(PickupKind pickupKind)
    {
        kind = pickupKind;
        born = Time.time;
        life = kind == PickupKind.Shard ? float.MaxValue : 10f;

        string shape;
        Color color;
        switch (kind)
        {
            case PickupKind.Heart: shape = "heart"; color = OrbPalette.HeartPink; break;
            case PickupKind.Shield: shape = "shield"; color = OrbPalette.ShieldIce; break;
            case PickupKind.Boost: shape = "bolt"; color = OrbPalette.BoostAmber; break;
            default: shape = "gem"; color = OrbPalette.ShardCyan; break;
        }
        ArenaDraw.Shape(transform, "pickupGlow", "glow", new Color(color.r, color.g, color.b, 0.45f), 1.8f, 1, Vector2.zero);
        icon = ArenaDraw.Shape(transform, "pickupIcon", shape, color, 0.8f, 2, Vector2.zero);

        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;
    }

    void Update()
    {
        float age = Time.time - born;
        float pop = Ease.Evaluate(EaseType.OutBack, age / 0.35f);
        float bob = 1f + 0.08f * Mathf.Sin(Time.time * 5f + born);
        transform.localScale = Vector3.one * pop * bob;
        icon.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 3f + born) * 8f);

        if (kind != PickupKind.Shard)
        {
            float left = life - age;
            if (left < 3f) icon.enabled = Mathf.FloorToInt(Time.time * 8f) % 2 == 0;
            if (left <= 0f) Destroy(gameObject);
        }

        GameSession session = GameSession.Instance;
        PlayerOrb player = PlayerOrb.Instance;
        if (kind == PickupKind.Shard && session != null && session.Running && player != null)
        {
            Vector2 to = player.transform.position - transform.position;
            float distance = to.magnitude;
            if (distance < 2.4f) transform.position += (Vector3)(to.normalized * (7f * (1f - distance / 2.4f) + 1.5f) * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        PlayerOrb player = other.GetComponent<PlayerOrb>();
        GameSession session = GameSession.Instance;
        if (player == null || session == null || !session.Running) return;
        collected = true;

        Vector3 at = transform.position;
        switch (kind)
        {
            case PickupKind.Heart: player.Vitals.Heal(); break;
            case PickupKind.Shield: player.Vitals.RaiseShield(); break;
            case PickupKind.Boost: player.Boost(5f); break;
        }
        Collected?.Invoke(kind, at);
        Destroy(gameObject);
    }
}
