using UnityEngine;

public class WallCannon : MonoBehaviour
{
    float fireInterval;
    float nextFire;
    float chargeStart = -1f;
    Transform barrel;
    SpriteRenderer chargeGlow;

    public void Setup(float interval, float startDelay)
    {
        fireInterval = interval;
        nextFire = Time.time + 1.5f + startDelay;
        Color c = OrbPalette.DroneRed;

        ArenaDraw.Shape(transform, "cannonBase", "circle", UiTheme.SurfaceHi, 1.2f, 2, Vector2.zero);
        ArenaDraw.Shape(transform, "cannonRim", "ring", c, 1.25f, 3, Vector2.zero);

        barrel = new GameObject("cannonBarrel").transform;
        barrel.SetParent(transform, false);
        ArenaDraw.Rect(barrel, "cannonTube", c, new Vector2(0.9f, 0.28f), 4, new Vector2(0.55f, 0f));
        chargeGlow = ArenaDraw.Shape(transform, "cannonCharge", "glow", new Color(c.r, c.g, c.b, 0f), 2f, 5, Vector2.zero);
    }

    void Update()
    {
        GameSession session = GameSession.Instance;
        PlayerOrb player = PlayerOrb.Instance;
        if (session == null || !session.Running || player == null) return;

        Vector2 to = (Vector2)player.transform.position - (Vector2)transform.position;
        float targetAngle = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg;
        float angle = Mathf.MoveTowardsAngle(barrel.eulerAngles.z, targetAngle, 140f * Time.deltaTime);
        barrel.rotation = Quaternion.Euler(0f, 0f, angle);

        float untilFire = nextFire - Time.time;
        if (untilFire < 0.5f)
        {
            if (chargeStart < 0f) chargeStart = Time.time;
            float charge = Mathf.Clamp01((Time.time - chargeStart) / 0.5f);
            chargeGlow.color = new Color(1f, 0.36f, 0.36f, charge * 0.9f);
            chargeGlow.transform.localScale = Vector3.one * Mathf.Lerp(1f, 2.6f, charge);
        }
        if (untilFire <= 0f)
        {
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            CannonBolt.Create(transform.position + (Vector3)(dir * 0.9f), dir, 5.5f, transform.parent);
            OrbSfxPlayer.Play(ArenaCue.CannonFire, 1f, 0.7f);
            nextFire = Time.time + fireInterval;
            chargeStart = -1f;
            chargeGlow.color = new Color(1f, 0.36f, 0.36f, 0f);
            StartCoroutine(UiAnim.Punch(transform, 0.2f, 0.3f));
        }
    }
}
