using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TextureGenerator — Creates solid, vibrant, colorful sprites.
/// No blur — clean edges with subtle shading for depth.
/// </summary>
public static class TextureGenerator
{
    static Dictionary<string, Sprite> _cache = new();

    // ── Solid Direction Color Palette ────────────────────────────
    // Fixed vibrant colors per direction — no randomness
    static readonly Color[] DirColors = new Color[]
    {
        new Color(0.20f, 0.80f, 0.40f),  // Up    = Emerald Green
        new Color(0.95f, 0.30f, 0.30f),  // Down  = Vivid Red
        new Color(0.25f, 0.55f, 1.00f),  // Left  = Royal Blue
        new Color(1.00f, 0.65f, 0.10f),  // Right = Amber Orange
    };

    public static Color GetColorForDirection(Direction dir)
    {
        return DirColors[(int)dir];
    }

    public static Sprite CreateArrowSprite(Direction dir)
    {
        string key = "ArrowSolid_" + dir;
        if (_cache.TryGetValue(key, out Sprite cached)) return cached;

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        Color baseColor = DirColors[(int)dir];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (IsInsideArrow(x, y, size, dir, out float dist))
                {
                    float aa = Mathf.Clamp01(-dist / 1.2f);
                    if (aa > 0)
                    {
                        // Solid base with subtle top-to-bottom gradient for depth
                        float gradientFactor = (float)y / size;
                        Color col = Color.Lerp(baseColor * 0.85f, baseColor * 1.1f, gradientFactor);

                        // Crisp white inner highlight near the top
                        float highlightDist = Mathf.Clamp01((dist + 8f) / 6f);
                        col = Color.Lerp(col, Color.white, highlightDist * 0.20f * gradientFactor);

                        // Dark edge outline — thin and clean
                        float outline = Mathf.Clamp01((dist + 2.0f) / 1.5f);
                        Color darkEdge = baseColor * 0.4f;
                        darkEdge.a = 1f;
                        col = Color.Lerp(col, darkEdge, outline * 0.7f);

                        col.a = aa;
                        tex.SetPixel(x, y, col);
                        continue;
                    }
                }
                tex.SetPixel(x, y, Color.clear);
            }
        }

        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), (float)size);
        _cache[key] = sprite;
        return sprite;
    }

    static bool IsInsideArrow(float x, float y, int size, Direction dir, out float dist)
    {
        // Normalize to -1..1
        float nx = (x / (size / 2f)) - 1f;
        float ny = (y / (size / 2f)) - 1f;

        // Rotate coordinates so we always draw an "UP" arrow in (tx, ty) space
        float tx = 0, ty = 0;
        switch (dir)
        {
            case Direction.Up:    tx = nx;  ty = ny;  break;
            case Direction.Down:  tx = -nx; ty = -ny; break;
            case Direction.Left:  tx = ny;  ty = -nx; break;
            case Direction.Right: tx = -ny; ty = nx;  break;
        }

        // --- SDF for a "Stylish Arrow" ---
        // 1. Triangle Head
        float dHead = SdTriangle(new Vector2(tx, ty - 0.25f),
                                 new Vector2(-0.65f, 0),
                                 new Vector2(0.65f, 0),
                                 new Vector2(0, 0.65f));

        // 2. Rounded Shaft
        float dShaft = SdBox(new Vector2(tx, ty + 0.2f), new Vector2(0.22f, 0.45f)) - 0.05f;

        dist = Mathf.Min(dHead, dShaft);
        return dist < 0.05f;
    }

    static float SdTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        Vector2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
        Vector2 v0 = p - p0, v1 = p - p1, v2 = p - p2;
        Vector2 pq0 = v0 - e0 * Mathf.Clamp01(Vector2.Dot(v0, e0) / Vector2.Dot(e0, e0));
        Vector2 pq1 = v1 - e1 * Mathf.Clamp01(Vector2.Dot(v1, e1) / Vector2.Dot(e1, e1));
        Vector2 pq2 = v2 - e2 * Mathf.Clamp01(Vector2.Dot(v2, e2) / Vector2.Dot(e2, e2));
        float s = Mathf.Sign(e0.x * e2.y - e0.y * e2.x);
        Vector2 d = Vector2.Min(Vector2.Min(new Vector2(Vector2.Dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
                                            new Vector2(Vector2.Dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
                                            new Vector2(Vector2.Dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)));
        return -Mathf.Sqrt(d.x) * Mathf.Sign(d.y);
    }

    static float SdBox(Vector2 p, Vector2 b)
    {
        Vector2 d = new Vector2(Mathf.Abs(p.x) - b.x, Mathf.Abs(p.y) - b.y);
        return Vector2.Max(d, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0.0f);
    }

    // ── Grid Cell Sprite — Solid with rounded corners ───────────
    public static Sprite CreateSquareSprite(Color color)
    {
        int size = 128;
        int radius = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = GetDistanceToRoundedRect(x, y, size, size, radius);
                float alpha = Mathf.Clamp01(-dist / 1.2f);

                // Solid color with very subtle vertical gradient
                float g = (float)y / size;
                Color c = Color.Lerp(color * 0.96f, color * 1.04f, g);
                c.a = alpha;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), (float)size);
    }

    public static Sprite CreateCircleSprite(Color color)
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float cx = size / 2f, cy = size / 2f, r = size / 2f - 4f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                float alpha = Mathf.Clamp01((r - dist) / 1.5f);
                Color c = color;
                // Small specular highlight
                float highlight = Mathf.Clamp01((10f - Vector2.Distance(new Vector2(x, y), new Vector2(cx - r * 0.3f, cy + r * 0.3f))) / 8f);
                c = Color.Lerp(c, Color.white, highlight * 0.4f);
                c.a *= alpha;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    public static Sprite CreateRoundedRectSprite(Color color)
    {
        int size = 128;
        int radius = 24;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = GetDistanceToRoundedRect(x, y, size, size, radius);
                float alpha = Mathf.Clamp01(-dist / 1.5f);
                Color c = color;
                c = Color.Lerp(c * 0.92f, c * 1.08f, (float)y / size);
                c.a *= alpha;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    static float GetDistanceToRoundedRect(float x, float y, float w, float h, float r)
    {
        Vector2 p = new Vector2(x - w / 2f, y - h / 2f);
        Vector2 b = new Vector2(w / 2f - r, h / 2f - r);
        Vector2 q = new Vector2(Mathf.Abs(p.x) - b.x, Mathf.Abs(p.y) - b.y);
        return Vector2.Max(q, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0.0f) - r;
    }
}