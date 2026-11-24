using System.Collections.Generic;
using UnityEngine;

public static class SpriteFactory
{
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => cache.Clear();

    public static readonly string[] IconShapes =
    {
        "circle", "ring", "square", "glow", "heart", "star", "gem", "coin", "lock", "play", "pause",
        "back", "gear", "home", "retry", "check", "close", "bolt", "shield", "hex", "drop"
    };

    public static Sprite Get(string shape, int size = 64)
    {
        string key = shape + size;
        if (cache.TryGetValue(key, out Sprite found) && found != null) return found;
        Texture2D tex = BuildTexture(shape, size, 2);
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        sprite.name = "gen_" + shape;
        cache[key] = sprite;
        return sprite;
    }

    public static Texture2D BuildTexture(string shape, int size, int samples)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Color32[] pixels = new Color32[size * size];
        float inv = 1f / (samples * samples);
        for (int py = 0; py < size; py++)
        {
            for (int px = 0; px < size; px++)
            {
                float sum = 0f;
                for (int sy = 0; sy < samples; sy++)
                    for (int sx = 0; sx < samples; sx++)
                    {
                        float x = ((px + (sx + 0.5f) / samples) / size) * 2f - 1f;
                        float y = ((py + (sy + 0.5f) / samples) / size) * 2f - 1f;
                        sum += Sample(shape, x, y);
                    }
                byte a = (byte)Mathf.RoundToInt(Mathf.Clamp01(sum * inv) * 255f);
                pixels[py * size + px] = new Color32(255, 255, 255, a);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply(false, false);
        return tex;
    }

    public static Texture2D BuildRoundRect(int size, int radius, int outline)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Color32[] pixels = new Color32[size * size];
        float half = size * 0.5f;
        for (int py = 0; py < size; py++)
        {
            for (int px = 0; px < size; px++)
            {
                float outer = RoundBoxDistance(px + 0.5f - half, py + 0.5f - half, half, radius);
                float coverage = Mathf.Clamp01(0.5f - outer);
                if (outline > 0)
                {
                    float inner = RoundBoxDistance(px + 0.5f - half, py + 0.5f - half, half - outline, Mathf.Max(0, radius - outline));
                    coverage *= Mathf.Clamp01(inner + 0.5f);
                }
                pixels[py * size + px] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(coverage * 255f));
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply(false, false);
        return tex;
    }

    static float RoundBoxDistance(float x, float y, float halfExtent, float radius)
    {
        float qx = Mathf.Abs(x) - (halfExtent - radius);
        float qy = Mathf.Abs(y) - (halfExtent - radius);
        float ox = Mathf.Max(qx, 0f), oy = Mathf.Max(qy, 0f);
        return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
    }

    static readonly float[] GemPoly = { -0.9f, 0.25f, -0.5f, 0.8f, 0.5f, 0.8f, 0.9f, 0.25f, 0f, -0.95f };
    static readonly float[] PlayPoly = { -0.55f, 0.8f, -0.55f, -0.8f, 0.85f, 0f };
    static readonly float[] BoltPoly = { 0.15f, 0.95f, -0.55f, -0.05f, -0.05f, -0.05f, -0.2f, -0.95f, 0.55f, 0.1f, 0.05f, 0.1f };
    static readonly float[] ShieldPoly = { -0.75f, 0.85f, 0.75f, 0.85f, 0.75f, 0.05f, 0f, -0.95f, -0.75f, 0.05f };
    static readonly float[] HexPoly = { 0.9f, 0f, 0.45f, 0.78f, -0.45f, 0.78f, -0.9f, 0f, -0.45f, -0.78f, 0.45f, -0.78f };
    static readonly float[] RoofPoly = { -0.97f, 0.05f, 0f, 0.92f, 0.97f, 0.05f };
    static readonly float[] DropPoly = { 0f, 0.95f, 0.6f, -0.1f, -0.6f, -0.1f };
    static readonly float[] ArrowPoly = { 0.12f, 0.33f, 0.30f, 0.83f, 0.56f, 0.26f };

    static readonly float[] StarPoly = { 0.0f, 0.97f, -0.235f, 0.324f, -0.923f, 0.3f, -0.38f, -0.124f, -0.57f, -0.785f, -0.0f, -0.4f, 0.57f, -0.785f, 0.38f, -0.124f, 0.923f, 0.3f, 0.235f, 0.324f };

    static float B(bool value) => value ? 1f : 0f;

    static float Sample(string shape, float x, float y)
    {
        float r = Mathf.Sqrt(x * x + y * y);
        switch (shape)
        {
            case "circle": return B(r <= 0.92f);
            case "ring": return B(r <= 0.92f && r >= 0.7f);
            case "square": return B(Mathf.Abs(x) <= 0.88f && Mathf.Abs(y) <= 0.88f);
            case "glow": { float g = Mathf.Clamp01(1f - r); return g * g; }
            case "coin": return B(r <= 0.92f && !(r >= 0.52f && r <= 0.64f));
            case "heart":
                {
                    float hx = x * 1.22f, hy = y * 1.22f + 0.12f;
                    float a = hx * hx + hy * hy - 1f;
                    return B(a * a * a - hx * hx * hy * hy * hy <= 0f);
                }
            case "star": return B(Poly(StarPoly, x, y));
            case "gem": return B(Poly(GemPoly, x, y));
            case "play": return B(Poly(PlayPoly, x, y));
            case "pause": return B(Mathf.Abs(y) <= 0.72f && ((x >= -0.58f && x <= -0.18f) || (x >= 0.18f && x <= 0.58f)));
            case "bolt": return B(Poly(BoltPoly, x, y));
            case "shield": return B(Poly(ShieldPoly, x, y));
            case "hex": return B(Poly(HexPoly, x, y));
            case "drop": return B(Poly(DropPoly, x, y) || Mathf.Sqrt(x * x + (y + 0.1f) * (y + 0.1f)) <= 0.6f);
            case "lock":
                {
                    bool body = x >= -0.62f && x <= 0.62f && y >= -0.85f && y <= 0.1f;
                    float sr = Mathf.Sqrt(x * x + (y - 0.1f) * (y - 0.1f));
                    bool shackle = y >= 0.1f && sr <= 0.46f && sr >= 0.26f;
                    return B(body || shackle);
                }
            case "back": return B(Seg(x, y, 0.35f, 0.78f, -0.4f, 0f) <= 0.15f || Seg(x, y, -0.4f, 0f, 0.35f, -0.78f) <= 0.15f);
            case "check": return B(Seg(x, y, -0.72f, 0f, -0.22f, -0.55f) <= 0.15f || Seg(x, y, -0.22f, -0.55f, 0.76f, 0.6f) <= 0.15f);
            case "close": return B(Seg(x, y, -0.65f, -0.65f, 0.65f, 0.65f) <= 0.15f || Seg(x, y, -0.65f, 0.65f, 0.65f, -0.65f) <= 0.15f);
            case "gear":
                {
                    float a = Mathf.Atan2(y, x);
                    float outer = Mathf.Cos(8f * a) > 0.15f ? 0.92f : 0.7f;
                    return B(r <= outer && r >= 0.3f);
                }
            case "home":
                {
                    bool roof = Poly(RoofPoly, x, y);
                    bool body = x >= -0.68f && x <= 0.68f && y >= -0.85f && y <= 0.1f;
                    bool door = x >= -0.2f && x <= 0.2f && y <= -0.2f;
                    return B((roof || body) && !door);
                }
            case "retry":
                {
                    float angle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
                    bool gap = angle > 20f && angle < 70f;
                    bool ringPart = r >= 0.4f && r <= 0.7f && !gap;
                    return B(ringPart || Poly(ArrowPoly, x, y));
                }
            default: return 0f;
        }
    }

    static bool Poly(float[] p, float x, float y)
    {
        bool inside = false;
        int n = p.Length / 2;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            float xi = p[2 * i], yi = p[2 * i + 1], xj = p[2 * j], yj = p[2 * j + 1];
            if (((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi)) inside = !inside;
        }
        return inside;
    }

    static float Seg(float px, float py, float ax, float ay, float bx, float by)
    {
        float vx = bx - ax, vy = by - ay, wx = px - ax, wy = py - ay;
        float t = Mathf.Clamp01((wx * vx + wy * vy) / (vx * vx + vy * vy));
        float dx = wx - t * vx, dy = wy - t * vy;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
