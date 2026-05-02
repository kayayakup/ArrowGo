using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// HintSystem — Finds an arrow that can safely exit and highlights it.
/// Does NOT reveal the full solution — only one arrow at a time.
/// </summary>
public class HintSystem : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────
    public static HintSystem Instance { get; private set; }

    public static void CreateInstance()
    {
        GameObject go = new GameObject("HintSystem");
        Instance = go.AddComponent<HintSystem>();
        DontDestroyOnLoad(go);
    }

    // ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Attempts to find and highlight a valid (unblocked) arrow.
    /// Returns true if a hint was found, false if no arrow can exit.
    /// </summary>
    public bool ShowHint()
    {
        if (GridManager.Instance == null) return false;
        if (GameManager.Instance.State != GameManager.GameState.Playing) return false;

        List<Arrow> candidates = GridManager.Instance.GetAllArrows();
        if (candidates == null || candidates.Count == 0) return false;

        // Shuffle to avoid always hinting the same arrow
        Shuffle(candidates);

        foreach (Arrow a in candidates)
        {
            if (GridManager.Instance.CanArrowExit(a))
            {
                a.ShowHint();
                AudioManager.Instance.PlaySFX("buttonClick");
                return true;
            }
        }

        // No arrow can exit — this shouldn't happen with valid levels
        UIManager.Instance.ShowToast("No hints available right now!");
        return false;
    }

    // ─────────────────────────────────────────────────────────────
    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}