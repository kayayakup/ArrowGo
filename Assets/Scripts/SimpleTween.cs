using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// SimpleTween — Coroutine-based tweening utility with modern easing.
/// </summary>
public static class SimpleTween
{
    public enum Ease { Linear, QuadIn, QuadOut, QuadInOut, CubicIn, CubicOut, CubicInOut, BackOut, ElasticOut }

    public static IEnumerator MoveTo(GameObject obj, Vector3 target, float duration, Ease ease = Ease.CubicOut, Action onComplete = null)
    {
        if (obj == null) yield break;
        Vector3 start = obj.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (obj == null) yield break;
            float t = GetEase(elapsed / duration, ease);
            obj.transform.position = Vector3.LerpUnclamped(start, target, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (obj != null) obj.transform.position = target;
        onComplete?.Invoke();
    }

    public static IEnumerator ScaleTo(GameObject obj, Vector3 target, float duration, Ease ease = Ease.CubicOut, Action onComplete = null)
    {
        if (obj == null) yield break;
        Vector3 start = obj.transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (obj == null) yield break;
            float t = GetEase(elapsed / duration, ease);
            obj.transform.localScale = Vector3.LerpUnclamped(start, target, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (obj != null) obj.transform.localScale = target;
        onComplete?.Invoke();
    }

    public static IEnumerator FadeTo(GameObject obj, float targetAlpha, float duration, Ease ease = Ease.Linear, Action onComplete = null)
    {
        if (obj == null) yield break;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        
        float startAlpha = sr != null ? sr.color.a : (cg != null ? cg.alpha : 1f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (obj == null) yield break;
            float t = GetEase(elapsed / duration, ease);
            float a = Mathf.Lerp(startAlpha, targetAlpha, t);
            if (sr != null) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, a);
            if (cg != null) cg.alpha = a;
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (sr != null) sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, targetAlpha);
        if (cg != null) cg.alpha = targetAlpha;
        onComplete?.Invoke();
    }

    public static float GetEase(float t, Ease ease)
    {
        switch (ease)
        {
            case Ease.QuadIn: return t * t;
            case Ease.QuadOut: return t * (2 - t);
            case Ease.QuadInOut: return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
            case Ease.CubicIn: return t * t * t;
            case Ease.CubicOut: return 1 - Mathf.Pow(1 - t, 3);
            case Ease.CubicInOut: return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
            case Ease.BackOut:
                float c1 = 1.70158f;
                float c3 = c1 + 1;
                return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
            case Ease.ElasticOut:
                float c4 = (2 * Mathf.PI) / 3;
                return t == 0 ? 0 : t == 1 ? 1 : Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * c4) + 1;
            default: return t;
        }
    }
}