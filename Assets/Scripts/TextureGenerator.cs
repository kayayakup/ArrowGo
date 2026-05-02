using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TextureGenerator — Updated to create distinct, high-quality arrow shapes.
/// </summary>
public static class TextureGenerator
{
    static Dictionary<string, Sprite> _cache = new();
    static Color[] _dirColors;

    static void EnsureColors()
    {
        if (_dirColors != null) return;
        _dirColors = new Color[4];
        for (int i = 0; i < 4; i++)
        {
            float hue = (i * 0.25f + Random.Range(0f, 0.15f)) % 1f;
            _dirColors[i] = Color.HSVToRGB(hue, 0.8f, 0.95f);
        }
    }

    public static Color GetColorForDirection(Direction dir)
    {
        EnsureColors();
        return _dirColors[(int)dir];
    }

    public static Sprite CreateArrowSprite(Direction dir)
    {
        string key = "ArrowShape_" + dir;
        if (_cache.TryGetValue(key, out Sprite cached)) return cached;

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // ── Arrow Shape with AA ────────────────────────────
                if (IsInsideArrow(x, y, size, dir, out float dist))
                {
                    float aa = Mathf.Clamp01(-dist / 0.8f);
                    if (aa > 0)
                    {
                        // Clean gradient for the arrow body (grayscale for tinting)
                        float grad = (x + y) / (float)(size * 2);
                        Color col = Color.Lerp(new Color(0.55f, 0.55f, 0.55f), Color.white, grad);
                        
                        // Subtle inner glow
                        float innerBorder = Mathf.Clamp01((dist + 4f) / 3f);
                        col = Color.Lerp(col, Color.white, innerBorder * 0.35f);
                        
                        // Dark outline at the very edge to pop from background
                        float outline = Mathf.Clamp01((dist + 2.5f) / 2.5f);
                        col = Color.Lerp(col, new Color(0.1f, 0.1f, 0.1f), outline * 0.6f);
                        
                        col.a *= aa;
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

    public static Sprite CreateSquareSprite(Color color)
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float g = (float)y / size;
                pixels[y * size + x] = Color.Lerp(color * 0.95f, color * 1.05f, g);
            }
        }
        tex.SetPixels(pixels); tex.Apply();
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
                float highlight = Mathf.Clamp01((10f - Vector2.Distance(new Vector2(x, y), new Vector2(cx - r*0.3f, cy + r*0.3f))) / 8f);
                c = Color.Lerp(c, Color.white, highlight * 0.5f);
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
                c = Color.Lerp(c * 0.9f, c * 1.1f, (float)y / size);
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