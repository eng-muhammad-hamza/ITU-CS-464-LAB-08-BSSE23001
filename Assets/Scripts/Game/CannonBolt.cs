using UnityEngine;

public class CannonBolt : MonoBehaviour
{
    Vector2 direction;
    float speed;
    float born;

    public static void Create(Vector2 position, Vector2 direction, float speed, Transform parent)
    {
        GameObject go = new GameObject("cannonBolt");
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        CannonBolt bolt = go.AddComponent<CannonBolt>();
        bolt.direction = direction.normalized;
        bolt.speed = speed;
        bolt.born = Time.time;

        Color c = OrbPalette.CannonBoltEmber;
        ArenaDraw.Shape(go.transform, "boltGlow", "glow", new Color(c.r, c.g, c.b, 0.5f), 1.4f, 1, Vector2.zero);
        ArenaDraw.Shape(go.transform, "boltCore", "circle", c, 0.45f, 3, Vector2.zero);
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.22f;
    }

    void Update()
    {
        GameSession session = GameSession.Instance;
        if (session == null || !session.Running) return;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        Vector2 pos = transform.position;
        Rect arena = session.Arena;
        bool outside = pos.x < arena.xMin - 0.5f || pos.x > arena.xMax + 0.5f || pos.y < arena.yMin - 0.5f || pos.y > arena.yMax + 0.5f;
        if (outside || Time.time - born > 8f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerOrb player = other.GetComponent<PlayerOrb>();
        if (player == null) return;
        if (player.TakeDamage(transform.position))
        {
            OrbBurstFx.Burst(transform.position, OrbPalette.CannonBoltEmber, 8, 4f, 0.2f, 0.35f);
            Destroy(gameObject);
        }
    }
}
