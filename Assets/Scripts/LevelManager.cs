using UnityEngine;
using System.Collections.Generic;

// ── Data Structures ──────────────────────────────────────────────────────────
[System.Serializable]
public class ArrowPlacement
{
    public int row;
    public int col;
    public string direction; // "up" | "down" | "left" | "right"
    public ArrowPlacement(int r, int c, string d) { row = r; col = c; direction = d; }
}

[System.Serializable]
public class LevelData
{
    public int width;
    public int height;
    public List<ArrowPlacement> arrows = new();
    public string name = "";
}

/// <summary>
/// LevelManager — Holds all level data, manages current level index,
/// and provides level load / unlock / restart methods.
/// </summary>
public class LevelManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────
    public static LevelManager Instance { get; private set; }

    public static void CreateInstance()
    {
        GameObject go = new GameObject("LevelManager");
        Instance = go.AddComponent<LevelManager>();
        DontDestroyOnLoad(go);
    }

    // ─────────────────────────────────────────────────────────────
    List<LevelData> _levels;
    int _currentIndex = 0;

    public int LevelCount => _levels.Count;
    public int CurrentLevelIndex => _currentIndex;
    public int UnlockedUpTo => PlayerPrefs.GetInt("UnlockedLevel", 0);

    // ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildLevels();
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>Returns the LevelData for a given index.</summary>
    public LevelData GetLevelData(int index)
    {
        _currentIndex = Mathf.Clamp(index, 0, _levels.Count - 1);
        return _levels[_currentIndex];
    }

    public LevelData GetCurrentLevelData() => _levels[_currentIndex];

    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Hand-crafted levels — each is solvable in the indicated order.
    /// The "correct" order is described in comments for each level.
    /// </summary>
    void BuildLevels()
    {
        _levels = new List<LevelData>();

        // ── Level 1 — 3×3 Tutorial ────────────────────────────────
        // Solve: (0,2)Up → (2,0)Right → (1,1)Left → (0,0)Up
        _levels.Add(new LevelData
        {
            name = "Tutorial",
            width = 3,
            height = 3,
            arrows = new() {
                new(2, 0, "up"),
                new(0, 1, "right"),
                new(1, 1, "left"),
                new(0, 0, "up"),
            }
        });

        // ── Level 2 — 3×3 ─────────────────────────────────────────
        _levels.Add(new LevelData
        {
            name = "Easy",
            width = 3,
            height = 3,
            arrows = new() {
                new(0, 2, "up"),
                new(2, 2, "left"),
                new(1, 0, "right"),
                new(0, 1, "right"),
                new(2, 0, "up"),
            }
        });

        // ── Level 3 — 4×4 ─────────────────────────────────────────
        _levels.Add(new LevelData
        {
            name = "Getting Tricky",
            width = 4,
            height = 4,
            arrows = new() {
                new(0, 3, "right"),
                new(3, 3, "down"),
                new(1, 2, "up"),
                new(2, 2, "left"),
                new(0, 1, "up"),
                new(3, 1, "left"),
                new(1, 0, "right"),
                new(2, 0, "up"),
            }
        });

        // ── Level 4 — 4×4 ─────────────────────────────────────────
        _levels.Add(new LevelData
        {
            name = "Four by Four",
            width = 4,
            height = 4,
            arrows = new() {
                new(0, 0, "up"),
                new(1, 0, "left"),
                new(2, 0, "right"),
                new(3, 0, "up"),
                new(0, 2, "right"),
                new(1, 2, "down"),
                new(2, 2, "left"),
                new(3, 2, "down"),
                new(0, 3, "right"),
                new(3, 3, "left"),
            }
        });

        // ── Level 5 — 5×5 ─────────────────────────────────────────
        _levels.Add(new LevelData
        {
            name = "Five Alive",
            width = 5,
            height = 5,
            arrows = new() {
                new(0, 4, "right"),
                new(4, 4, "left"),
                new(2, 3, "up"),
                new(0, 2, "up"),
                new(4, 2, "down"),
                new(1, 1, "right"),
                new(3, 1, "left"),
                new(0, 0, "right"),
                new(4, 0, "up"),
                new(2, 0, "left"),
            }
        });

        // ── Level 6 — 5×5 ─────────────────────────────────────────
        _levels.Add(new LevelData
        {
            name = "Crossroads",
            width = 5,
            height = 5,
            arrows = new() {
                new(2, 4, "up"),
                new(0, 3, "right"),
                new(4, 3, "left"),
                new(1, 2, "right"),
                new(3, 2, "left"),
                new(2, 2, "down"),
                new(0, 1, "up"),
                new(4, 1, "up"),
                new(2, 0, "right"),
                new(1, 0, "up"),
                new(3, 0, "up"),
            }
        });

        // ── Level 7 — 6×6 ─────────────────────────────────────────
        _levels.Add(new LevelData
        {
            name = "Six Shooter",
            width = 6,
            height = 6,
            arrows = new() {
                new(0, 5, "right"),
                new(5, 5, "down"),
                new(2, 4, "up"),
                new(4, 4, "left"),
                new(1, 3, "right"),
                new(3, 3, "down"),
                new(5, 3, "left"),
                new(0, 2, "up"),
                new(2, 2, "right"),
                new(4, 2, "up"),
                new(1, 1, "down"),
                new(3, 1, "right"),
                new(5, 1, "up"),
                new(0, 0, "right"),
                new(2, 0, "right"),
                new(4, 0, "up"),
            }
        });

        // ── Level 8 — 6×6 ─────────────────────────────────────────
        _levels.Add(new LevelData
        {
            name = "Dense Pack",
            width = 6,
            height = 6,
            arrows = new() {
                new(1, 5, "up"),    new(3, 5, "left"), new(5, 5, "left"),
                new(0, 4, "right"), new(2, 4, "down"), new(4, 4, "up"),
                new(1, 3, "left"),  new(3, 3, "right"),new(5, 3, "down"),
                new(0, 2, "up"),    new(2, 2, "up"),   new(4, 2, "left"),
                new(1, 1, "right"), new(3, 1, "down"), new(5, 1, "up"),
                new(0, 0, "right"), new(2, 0, "right"),new(4, 0, "up"),
            }
        });

        // ── Level 9 — 7×7 ─────────────────────────────────────────
        _levels.Add(new LevelData
        {
            name = "Lucky Seven",
            width = 7,
            height = 7,
            arrows = new() {
                new(0,6,"right"), new(3,6,"left"),  new(6,6,"down"),
                new(1,5,"up"),    new(4,5,"right"),
                new(0,4,"up"),    new(2,4,"left"),  new(5,4,"down"),
                new(3,3,"right"), new(6,3,"left"),
                new(1,2,"down"),  new(4,2,"up"),
                new(0,1,"right"), new(2,1,"right"), new(5,1,"up"),
                new(1,0,"right"), new(3,0,"up"),    new(6,0,"left"),
            }
        });

        // ── Level 10 — 8×8 ────────────────────────────────────────
        _levels.Add(new LevelData
        {
            name = "Grand Master",
            width = 8,
            height = 8,
            arrows = new() {
                new(0,7,"right"), new(3,7,"left"),  new(5,7,"left"),  new(7,7,"down"),
                new(1,6,"up"),    new(4,6,"down"),  new(6,6,"left"),
                new(0,5,"up"),    new(2,5,"right"), new(5,5,"up"),    new(7,5,"left"),
                new(3,4,"left"),  new(4,4,"right"),
                new(1,3,"down"),  new(6,3,"down"),
                new(0,2,"right"), new(2,2,"up"),    new(5,2,"left"),  new(7,2,"up"),
                new(3,1,"right"), new(4,1,"left"),
                new(1,0,"right"), new(2,0,"right"), new(5,0,"up"),    new(6,0,"up"),
            }
        });
    }
}