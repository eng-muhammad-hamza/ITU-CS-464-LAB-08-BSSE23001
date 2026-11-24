using UnityEngine;

public static class ArenaDraw
{
    public const float SquareFill = 0.88f;

    public static SpriteRenderer Shape(Transform parent, string name, string shape, Color color, float diameter, int order, Vector2 localPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = Vector3.one * diameter;
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteFactory.Get(shape, 64);
        renderer.color = color;
        renderer.sortingOrder = order;
        return renderer;
    }

    public static SpriteRenderer Rect(Transform parent, string name, Color color, Vector2 size, int order, Vector2 localPos)
    {
        SpriteRenderer renderer = Shape(parent, name, "square", color, 1f, order, localPos);
        renderer.transform.localScale = new Vector3(size.x / SquareFill, size.y / SquareFill, 1f);
        return renderer;
    }
}
