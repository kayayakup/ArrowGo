using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        LevelData raw = _levels[_currentIndex];
        return FixConflictingArrows(raw);
    }

    public LevelData GetCurrentLevelData() 
    {
        return FixConflictingArrows(_levels[_currentIndex]);
    }

    /// <summary>
    /// Bir level'daki birbirine bakan (çakışan) okları düzeltir.
    /// Aynı satırda sağ-sol, aynı sütunda yukarı-aşağı çakışmalarını çözer.
    /// </summary>
    private LevelData FixConflictingArrows(LevelData level)
    {
        // Kopya oluştur (orijinali bozmamak için - derin kopya / deep copy)
        LevelData fixedLevel = new LevelData
        {
            width = level.width,
            height = level.height,
            name = level.name,
            arrows = new List<ArrowPlacement>()
        };

        foreach (var arrow in level.arrows)
        {
            fixedLevel.arrows.Add(new ArrowPlacement(arrow.row, arrow.col, arrow.direction));
        }

        bool changed = true;
        int maxIterations = 1000;
        
        while (changed && maxIterations > 0)
        {
            changed = false;
            maxIterations--;

            // Yatayda birbirine bakanları çöz (Aynı satır)
            // Mantık: Soldaki ok Sağa bakıyor, sağdaki ok Sola bakıyorsa birbirlerine bakıyorlardır.
            for (int r = 0; r < level.height; r++)
            {
                var rowArrows = fixedLevel.arrows.Where(a => a.row == r).OrderBy(a => a.col).ToList();
                for (int i = 0; i < rowArrows.Count - 1; i++)
                {
                    for (int j = i + 1; j < rowArrows.Count; j++)
                    {
                        if (rowArrows[i].direction == "right" && rowArrows[j].direction == "left")
                        {
                            rowArrows[i].direction = "left"; // Artık sola (dışa) bakıyor
                            rowArrows[j].direction = "right"; // Artık sağa (dışa) bakıyor
                            changed = true;
                        }
                    }
                }
            }

            // Dikeyde birbirine bakanları çöz (Aynı sütun)
            // Mantık: Alttaki ok Yukarı bakıyor, üstteki ok Aşağı bakıyorsa birbirlerine bakıyorlardır.
            for (int c = 0; c < level.width; c++)
            {
                var colArrows = fixedLevel.arrows.Where(a => a.col == c).OrderBy(a => a.row).ToList();
                for (int i = 0; i < colArrows.Count - 1; i++)
                {
                    for (int j = i + 1; j < colArrows.Count; j++)
                    {
                        if (colArrows[i].direction == "up" && colArrows[j].direction == "down")
                        {
                            colArrows[i].direction = "down"; // Artık aşağıya (dışa) bakıyor
                            colArrows[j].direction = "up"; // Artık yukarıya (dışa) bakıyor
                            changed = true;
                        }
                    }
                }
            }
        }
        
        return fixedLevel;
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Hand-crafted levels — each is solvable in the indicated order.
    /// The "correct" order is described in comments for each level.
    /// </summary>
    void BuildLevels()
{
    _levels = new List<LevelData>();

    // ==================== MEVCUT 10 SEVİYE (Tutorial -> Grand Master) ====================
    _levels.Add(new LevelData { name = "Tutorial", width = 3, height = 3, arrows = new() {
        new(2, 2, "up"), new(1, 2, "right"), new(1, 1, "up"), new(0, 1, "right"), new(0, 0, "up")
    }});

    _levels.Add(new LevelData { name = "Easy", width = 3, height = 3, arrows = new() {
        new(0, 2, "left"), new(1, 2, "left"), new(1, 1, "up"), new(1, 0, "up"), new(2, 0, "left"), new(2, 1, "down")
    }});

    _levels.Add(new LevelData { name = "Getting Tricky", width = 4, height = 4, arrows = new() {
        new(3, 3, "right"), new(3, 2, "up"), new(2, 2, "right"), new(2, 1, "up"), new(1, 1, "right"), new(1, 0, "up"), new(0, 0, "right"), new(0, 3, "down")
    }});

    _levels.Add(new LevelData { name = "Four by Four", width = 4, height = 4, arrows = new() {
        new(0, 0, "left"), new(1, 0, "left"), new(1, 1, "down"), new(2, 1, "left"), new(2, 2, "down"), new(3, 2, "left"), new(3, 3, "down"), new(0, 3, "right"), new(0, 2, "up")
    }});

    _levels.Add(new LevelData { name = "Five Alive", width = 5, height = 5, arrows = new() {
        new(4, 4, "up"), new(4, 3, "up"), new(3, 3, "right"), new(3, 2, "up"), new(2, 2, "right"), new(2, 1, "up"), new(1, 1, "right"), new(1, 0, "up"), new(0, 0, "right"), new(0, 4, "down"), new(1, 4, "left")
    }});

    _levels.Add(new LevelData { name = "Crossroads", width = 5, height = 5, arrows = new() {
        new(0, 4, "left"), new(1, 4, "left"), new(2, 4, "left"), new(2, 3, "up"), new(2, 2, "up"), new(3, 2, "left"), new(4, 2, "left"), new(4, 1, "up"), new(4, 0, "up"), new(3, 0, "right"), new(2, 0, "right"), new(1, 0, "right"), new(1, 1, "down"), new(0, 1, "right")
    }});

    _levels.Add(new LevelData { name = "Six Shooter", width = 6, height = 6, arrows = new() {
        new(5, 5, "right"), new(4, 5, "right"), new(4, 4, "up"), new(3, 4, "right"), new(3, 3, "up"), new(2, 3, "right"), new(2, 2, "up"), new(1, 2, "right"), new(1, 1, "up"), new(0, 1, "right"), new(0, 0, "up"), new(5, 0, "left"), new(5, 1, "down"), new(4, 1, "right")
    }});

    _levels.Add(new LevelData { name = "Dense Pack", width = 6, height = 6, arrows = new() {
        new(0, 0, "down"), new(0, 1, "down"), new(0, 2, "down"), new(1, 2, "left"), new(2, 2, "left"), new(2, 3, "down"), new(2, 4, "down"), new(3, 4, "left"), new(4, 4, "left"), new(4, 5, "down"), new(5, 5, "left"), new(5, 4, "up"), new(5, 3, "up"), new(4, 3, "right"), new(4, 2, "up")
    }});

    _levels.Add(new LevelData { name = "Lucky Seven", width = 7, height = 7, arrows = new() {
        new(6, 6, "up"), new(6, 5, "up"), new(5, 5, "right"), new(5, 4, "right"), new(4, 4, "up"), new(4, 3, "up"), new(3, 3, "right"), new(3, 2, "right"), new(2, 2, "up"), new(2, 1, "up"), new(1, 1, "right"), new(1, 0, "right"), new(0, 0, "up"), new(0, 6, "down"), new(1, 6, "left"), new(2, 6, "left"), new(3, 6, "left")
    }});

    _levels.Add(new LevelData { name = "Grand Master", width = 8, height = 8, arrows = new() {
        new(7, 0, "right"), new(6, 0, "right"), new(6, 1, "down"), new(5, 1, "right"), new(5, 2, "down"), new(4, 2, "right"), new(4, 3, "down"), new(3, 3, "right"), new(3, 4, "down"), new(2, 4, "right"), new(2, 5, "down"), new(1, 5, "right"), new(1, 6, "down"), new(0, 6, "right"), new(0, 7, "down"), new(7, 7, "up"), new(6, 7, "right"), new(5, 7, "right"), new(4, 7, "right"), new(3, 7, "right")
    }});

    // ==================== YENİ 40 SEVİYE (Level 11 -> Level 50) ====================
    // Level 11 - 3x3 Spiral
    _levels.Add(new LevelData { name = "Spiral 3x3", width = 3, height = 3, arrows = new() {
        new(0, 0, "right"), new(0, 1, "down"), new(1, 1, "right"), new(1, 2, "down"), new(2, 2, "left"), new(2, 1, "up"), new(1, 0, "right")
    }});

    // Level 12 - 4x4 Yarım Spiral
    _levels.Add(new LevelData { name = "Half Spiral", width = 4, height = 4, arrows = new() {
        new(0, 3, "down"), new(1, 3, "left"), new(1, 2, "down"), new(2, 2, "left"), new(2, 1, "down"), new(3, 1, "left"), new(3, 0, "up"), new(2, 0, "right"), new(1, 0, "right")
    }});

    // Level 13 - Çapraz Yılan (4x4)
    _levels.Add(new LevelData { name = "Snake 4x4", width = 4, height = 4, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "down"), new(1, 2, "left"), new(1, 1, "left"), new(1, 0, "down"), new(2, 0, "right"), new(2, 1, "right"), new(2, 2, "down"), new(3, 2, "left"), new(3, 1, "up")
    }});

    // Level 14 - Küçük Labirent (5x5)
    _levels.Add(new LevelData { name = "Little Maze", width = 5, height = 5, arrows = new() {
        new(0, 4, "down"), new(1, 4, "left"), new(1, 3, "down"), new(2, 3, "left"), new(2, 2, "down"), new(3, 2, "left"), new(3, 1, "down"), new(4, 1, "left"), new(4, 0, "up"), new(3, 0, "right"), new(2, 0, "right"), new(2, 1, "up"), new(1, 1, "right"), new(0, 1, "right")
    }});

    // Level 15 - Döngüsüz Beşli (5x5)
    _levels.Add(new LevelData { name = "Acyclic 5", width = 5, height = 5, arrows = new() {
        new(4, 0, "up"), new(3, 0, "right"), new(3, 1, "up"), new(2, 1, "right"), new(2, 2, "up"), new(1, 2, "right"), new(1, 3, "up"), new(0, 3, "right"), new(0, 4, "down"), new(1, 4, "left")
    }});

    // Level 16 - Çift Sarmal (5x5)
    _levels.Add(new LevelData { name = "Double Helix", width = 5, height = 5, arrows = new() {
        new(0, 0, "down"), new(1, 0, "right"), new(1, 1, "down"), new(2, 1, "right"), new(2, 2, "down"), new(3, 2, "right"), new(3, 3, "down"), new(4, 3, "right"), new(4, 4, "up"), new(3, 4, "left"), new(3, 3, "up"), new(2, 3, "left"), new(2, 2, "up"), new(1, 2, "left"), new(1, 1, "up")
    }});

    // Level 17 - Yılan 6x6 (Basit)
    _levels.Add(new LevelData { name = "Snake 6", width = 6, height = 6, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "right"), new(0, 3, "right"), new(0, 4, "down"), new(1, 4, "left"), new(1, 3, "left"), new(1, 2, "left"), new(1, 1, "left"), new(1, 0, "down"), new(2, 0, "right"), new(2, 1, "right"), new(2, 2, "right"), new(2, 3, "right"), new(2, 4, "down"), new(3, 4, "left"), new(3, 3, "left"), new(3, 2, "left"), new(3, 1, "left"), new(3, 0, "down"), new(4, 0, "right"), new(4, 1, "right"), new(4, 2, "right"), new(4, 3, "right"), new(4, 4, "down")
    }});

    // Level 18 - İç İçe Kareler (6x6)
    _levels.Add(new LevelData { name = "Nested Squares", width = 6, height = 6, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "right"), new(0, 3, "right"), new(0, 4, "right"), new(0, 5, "down"), new(1, 5, "left"), new(2, 5, "left"), new(3, 5, "left"), new(4, 5, "left"), new(5, 5, "down"), new(5, 4, "up"), new(5, 3, "up"), new(5, 2, "up"), new(5, 1, "up"), new(5, 0, "up"), new(4, 0, "right"), new(3, 0, "right"), new(2, 0, "right"), new(1, 0, "right")
    }});

    // Level 19 - Zigzag 6x6
    _levels.Add(new LevelData { name = "Zigzag 6", width = 6, height = 6, arrows = new() {
        new(0, 0, "down"), new(1, 0, "right"), new(1, 1, "up"), new(0, 1, "right"), new(0, 2, "down"), new(1, 2, "right"), new(1, 3, "up"), new(0, 3, "right"), new(0, 4, "down"), new(1, 4, "right"), new(1, 5, "up"), new(2, 5, "left"), new(2, 4, "down"), new(3, 4, "left"), new(3, 3, "up"), new(4, 3, "left"), new(4, 2, "down"), new(5, 2, "left")
    }});

    // Level 20 - Kırık Merdiven (7x7)
    _levels.Add(new LevelData { name = "Broken Stairs", width = 7, height = 7, arrows = new() {
        new(0, 6, "down"), new(1, 6, "left"), new(1, 5, "down"), new(2, 5, "left"), new(2, 4, "down"), new(3, 4, "left"), new(3, 3, "down"), new(4, 3, "left"), new(4, 2, "down"), new(5, 2, "left"), new(5, 1, "down"), new(6, 1, "left"), new(6, 0, "up")
    }});

    // Level 21 - Spiral 7x7
    _levels.Add(new LevelData { name = "Spiral 7", width = 7, height = 7, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "right"), new(0, 3, "right"), new(0, 4, "right"), new(0, 5, "right"), new(0, 6, "down"), new(1, 6, "left"), new(2, 6, "left"), new(3, 6, "left"), new(4, 6, "left"), new(5, 6, "left"), new(6, 6, "down"), new(6, 5, "up"), new(6, 4, "up"), new(6, 3, "up"), new(6, 2, "up"), new(6, 1, "up"), new(6, 0, "up"), new(5, 0, "right"), new(4, 0, "right"), new(3, 0, "right"), new(2, 0, "right"), new(1, 0, "right"), new(1, 1, "down"), new(1, 2, "down"), new(1, 3, "down"), new(1, 4, "down"), new(1, 5, "right"), new(2, 5, "up"), new(3, 5, "up"), new(4, 5, "up"), new(5, 5, "left"), new(5, 4, "down"), new(5, 3, "down"), new(5, 2, "down"), new(4, 2, "left"), new(3, 2, "left"), new(2, 2, "up")
    }});

    // Level 22 - Sekizgen Deseni (8x8)
    _levels.Add(new LevelData { name = "Octagon", width = 8, height = 8, arrows = new() {
        new(0, 0, "down"), new(1, 0, "right"), new(1, 1, "down"), new(2, 1, "right"), new(2, 2, "down"), new(3, 2, "right"), new(3, 3, "down"), new(4, 3, "right"), new(4, 4, "down"), new(5, 4, "right"), new(5, 5, "down"), new(6, 5, "right"), new(6, 6, "down"), new(7, 6, "right"), new(7, 7, "up"), new(6, 7, "left"), new(5, 7, "left"), new(4, 7, "left"), new(3, 7, "left"), new(2, 7, "left"), new(1, 7, "left"), new(0, 7, "up")
    }});

    // Level 23 - Çift Yılan (8x8)
    _levels.Add(new LevelData { name = "Twin Snake", width = 8, height = 8, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "right"), new(0, 3, "right"), new(0, 4, "right"), new(0, 5, "right"), new(0, 6, "right"), new(0, 7, "down"), new(1, 7, "left"), new(2, 7, "left"), new(3, 7, "left"), new(4, 7, "left"), new(5, 7, "left"), new(6, 7, "left"), new(7, 7, "down"), new(7, 6, "up"), new(7, 5, "up"), new(7, 4, "up"), new(7, 3, "up"), new(7, 2, "up"), new(7, 1, "up"), new(7, 0, "up")
    }});

    // Level 24 - Labirent 8x8
    _levels.Add(new LevelData { name = "Maze 8", width = 8, height = 8, arrows = new() {
        new(0, 0, "right"), new(0, 1, "down"), new(1, 1, "right"), new(1, 2, "down"), new(2, 2, "right"), new(2, 3, "down"), new(3, 3, "right"), new(3, 4, "down"), new(4, 4, "right"), new(4, 5, "down"), new(5, 5, "right"), new(5, 6, "down"), new(6, 6, "right"), new(6, 7, "down"), new(7, 7, "left"), new(7, 6, "up"), new(6, 6, "left"), new(6, 5, "up"), new(5, 5, "left"), new(5, 4, "up"), new(4, 4, "left"), new(4, 3, "up"), new(3, 3, "left"), new(3, 2, "up"), new(2, 2, "left"), new(2, 1, "up"), new(1, 1, "left"), new(1, 0, "up")
    }});

    // Level 25 - Dikey Spiral (5x9) - farklı boyut
    _levels.Add(new LevelData { name = "Vertical Spiral", width = 5, height = 9, arrows = new() {
        new(0, 0, "down"), new(1, 0, "right"), new(1, 1, "down"), new(2, 1, "right"), new(2, 2, "down"), new(3, 2, "right"), new(3, 3, "down"), new(4, 3, "right"), new(4, 4, "down"), new(5, 4, "right"), new(5, 5, "down"), new(6, 5, "right"), new(6, 6, "down"), new(7, 6, "right"), new(7, 7, "down"), new(8, 7, "right"), new(8, 8, "up"), new(7, 8, "left"), new(6, 8, "left"), new(5, 8, "left"), new(4, 8, "left"), new(3, 8, "left"), new(2, 8, "left"), new(1, 8, "left"), new(0, 8, "up")
    }});

    // Level 26 - Yatay Spiral (9x5)
    _levels.Add(new LevelData { name = "Horizontal Spiral", width = 9, height = 5, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "right"), new(0, 3, "right"), new(0, 4, "right"), new(0, 5, "right"), new(0, 6, "right"), new(0, 7, "right"), new(0, 8, "down"), new(1, 8, "left"), new(2, 8, "left"), new(3, 8, "left"), new(4, 8, "left"), new(4, 7, "up"), new(3, 7, "right"), new(2, 7, "right"), new(1, 7, "right"), new(1, 6, "down"), new(2, 6, "left"), new(3, 6, "left"), new(3, 5, "up"), new(2, 5, "right"), new(1, 5, "right")
    }});

    // Level 27 - Yılan 4x8
    _levels.Add(new LevelData { name = "Snake 4x8", width = 4, height = 8, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "right"), new(0, 3, "down"), new(1, 3, "left"), new(1, 2, "left"), new(1, 1, "left"), new(1, 0, "down"), new(2, 0, "right"), new(2, 1, "right"), new(2, 2, "right"), new(2, 3, "down"), new(3, 3, "left"), new(3, 2, "left"), new(3, 1, "left"), new(3, 0, "down"), new(4, 0, "right"), new(4, 1, "right"), new(4, 2, "right"), new(4, 3, "down"), new(5, 3, "left"), new(5, 2, "left"), new(5, 1, "left"), new(5, 0, "down"), new(6, 0, "right"), new(6, 1, "right"), new(6, 2, "right"), new(6, 3, "down"), new(7, 3, "left")
    }});

    // Level 28 - Ters Spiral 6x6
    _levels.Add(new LevelData { name = "Reverse Spiral", width = 6, height = 6, arrows = new() {
        new(5, 5, "up"), new(4, 5, "left"), new(4, 4, "up"), new(3, 4, "left"), new(3, 3, "up"), new(2, 3, "left"), new(2, 2, "up"), new(1, 2, "left"), new(1, 1, "up"), new(0, 1, "left"), new(0, 0, "down"), new(1, 0, "right"), new(2, 0, "right"), new(3, 0, "right"), new(4, 0, "right"), new(5, 0, "down"), new(5, 1, "right"), new(5, 2, "right"), new(5, 3, "right"), new(5, 4, "right")
    }});

    // Level 29 - Köşegen Yürüyüş (7x7)
    _levels.Add(new LevelData { name = "Diagonal Walk", width = 7, height = 7, arrows = new() {
        new(0, 0, "down"), new(1, 0, "right"), new(1, 1, "down"), new(2, 1, "right"), new(2, 2, "down"), new(3, 2, "right"), new(3, 3, "down"), new(4, 3, "right"), new(4, 4, "down"), new(5, 4, "right"), new(5, 5, "down"), new(6, 5, "right"), new(6, 6, "up"), new(5, 6, "left"), new(4, 6, "left"), new(3, 6, "left"), new(2, 6, "left"), new(1, 6, "left"), new(0, 6, "up")
    }});

    // Level 30 - Karmaşık Labirent (8x8)
    _levels.Add(new LevelData { name = "Complex Maze", width = 8, height = 8, arrows = new() {
        new(0, 0, "down"), new(1, 0, "right"), new(1, 1, "down"), new(2, 1, "right"), new(2, 2, "down"), new(3, 2, "right"), new(3, 3, "down"), new(4, 3, "right"), new(4, 4, "down"), new(5, 4, "right"), new(5, 5, "down"), new(6, 5, "right"), new(6, 6, "down"), new(7, 6, "right"), new(7, 7, "up"), new(6, 7, "left"), new(5, 7, "left"), new(5, 6, "up"), new(4, 6, "left"), new(3, 6, "left"), new(3, 5, "up"), new(2, 5, "left"), new(1, 5, "left"), new(1, 4, "up"), new(0, 4, "right")
    }});

    // Level 31 - 3x3 Ters Yılan
    _levels.Add(new LevelData { name = "Reverse Snake 3", width = 3, height = 3, arrows = new() {
        new(2, 0, "up"), new(1, 0, "right"), new(1, 1, "up"), new(0, 1, "right"), new(0, 2, "down"), new(1, 2, "left")
    }});

    // Level 32 - 4x4 Ters Spiral
    _levels.Add(new LevelData { name = "Reverse Spiral 4", width = 4, height = 4, arrows = new() {
        new(3, 3, "up"), new(2, 3, "left"), new(2, 2, "up"), new(1, 2, "left"), new(1, 1, "up"), new(0, 1, "left"), new(0, 0, "down"), new(1, 0, "right"), new(2, 0, "right"), new(3, 0, "down"), new(3, 1, "right")
    }});

    // Level 33 - Çift Katman (5x5)
    _levels.Add(new LevelData { name = "Double Layer", width = 5, height = 5, arrows = new() {
        new(0, 0, "right"), new(0, 1, "down"), new(1, 1, "right"), new(1, 2, "down"), new(2, 2, "right"), new(2, 3, "down"), new(3, 3, "right"), new(3, 4, "down"), new(4, 4, "left"), new(4, 3, "up"), new(3, 3, "left"), new(3, 2, "up"), new(2, 2, "left"), new(2, 1, "up"), new(1, 1, "left"), new(1, 0, "up")
    }});

    // Level 34 - Saat Yönü Spiral (6x6)
    _levels.Add(new LevelData { name = "CW Spiral 6", width = 6, height = 6, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "right"), new(0, 3, "right"), new(0, 4, "right"), new(0, 5, "down"), new(1, 5, "left"), new(2, 5, "left"), new(3, 5, "left"), new(4, 5, "left"), new(5, 5, "down"), new(5, 4, "up"), new(5, 3, "up"), new(5, 2, "up"), new(5, 1, "up"), new(5, 0, "up"), new(4, 0, "right"), new(3, 0, "right"), new(2, 0, "right"), new(1, 0, "right"), new(1, 1, "down"), new(1, 2, "down"), new(1, 3, "down"), new(1, 4, "right")
    }});

    // Level 35 - 7x7 Karma
    _levels.Add(new LevelData { name = "Mix 7x7", width = 7, height = 7, arrows = new() {
        new(0, 0, "down"), new(1, 0, "right"), new(1, 1, "down"), new(2, 1, "right"), new(2, 2, "down"), new(3, 2, "right"), new(3, 3, "down"), new(4, 3, "right"), new(4, 4, "down"), new(5, 4, "right"), new(5, 5, "down"), new(6, 5, "right"), new(6, 6, "up"), new(5, 6, "left"), new(4, 6, "left"), new(4, 5, "up"), new(3, 5, "left"), new(2, 5, "left"), new(2, 4, "up"), new(1, 4, "left"), new(0, 4, "up")
    }});

    // Level 36 - 8x8 Çift Döngü
    _levels.Add(new LevelData { name = "Double Loop", width = 8, height = 8, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "down"), new(1, 2, "left"), new(1, 1, "left"), new(1, 0, "down"), new(2, 0, "right"), new(2, 1, "right"), new(2, 2, "down"), new(3, 2, "left"), new(3, 1, "left"), new(3, 0, "down"), new(4, 0, "right"), new(4, 1, "right"), new(4, 2, "down"), new(5, 2, "left"), new(5, 1, "left"), new(5, 0, "down"), new(6, 0, "right"), new(6, 1, "right"), new(6, 2, "down"), new(7, 2, "left"), new(7, 1, "left"), new(7, 0, "up")
    }});

    // Level 37 - Uzun Yılan (5x10)
    _levels.Add(new LevelData { name = "Long Snake", width = 5, height = 10, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "right"), new(0, 3, "right"), new(0, 4, "down"), new(1, 4, "left"), new(1, 3, "left"), new(1, 2, "left"), new(1, 1, "left"), new(1, 0, "down"), new(2, 0, "right"), new(2, 1, "right"), new(2, 2, "right"), new(2, 3, "right"), new(2, 4, "down"), new(3, 4, "left"), new(3, 3, "left"), new(3, 2, "left"), new(3, 1, "left"), new(3, 0, "down"), new(4, 0, "right"), new(4, 1, "right"), new(4, 2, "right"), new(4, 3, "right"), new(4, 4, "down"), new(5, 4, "left"), new(5, 3, "left"), new(5, 2, "left"), new(5, 1, "left"), new(5, 0, "down"), new(6, 0, "right"), new(6, 1, "right"), new(6, 2, "right"), new(6, 3, "right"), new(6, 4, "down"), new(7, 4, "left"), new(7, 3, "left"), new(7, 2, "left"), new(7, 1, "left"), new(7, 0, "down"), new(8, 0, "right"), new(8, 1, "right"), new(8, 2, "right"), new(8, 3, "right"), new(8, 4, "down"), new(9, 4, "left")
    }});

    // Level 38 - 9x9 Kare İçinde Kare
    _levels.Add(new LevelData { name = "Square in Square", width = 9, height = 9, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "right"), new(0, 3, "right"), new(0, 4, "right"), new(0, 5, "right"), new(0, 6, "right"), new(0, 7, "right"), new(0, 8, "down"), new(1, 8, "left"), new(2, 8, "left"), new(3, 8, "left"), new(4, 8, "left"), new(5, 8, "left"), new(6, 8, "left"), new(7, 8, "left"), new(8, 8, "down"), new(8, 7, "up"), new(8, 6, "up"), new(8, 5, "up"), new(8, 4, "up"), new(8, 3, "up"), new(8, 2, "up"), new(8, 1, "up"), new(8, 0, "up"), new(7, 0, "right"), new(6, 0, "right"), new(5, 0, "right"), new(4, 0, "right"), new(3, 0, "right"), new(2, 0, "right"), new(1, 0, "right"), new(1, 1, "down"), new(1, 2, "down"), new(1, 3, "down"), new(1, 4, "down"), new(1, 5, "down"), new(1, 6, "down"), new(1, 7, "right"), new(2, 7, "up"), new(3, 7, "up"), new(4, 7, "up"), new(5, 7, "up"), new(6, 7, "up"), new(7, 7, "left"), new(7, 6, "down"), new(7, 5, "down"), new(7, 4, "down"), new(7, 3, "down"), new(7, 2, "down")
    }});

    // Level 39 - Zor Labirent (9x9)
    _levels.Add(new LevelData { name = "Hard Maze", width = 9, height = 9, arrows = new() {
        new(0, 0, "right"), new(0, 1, "down"), new(1, 1, "right"), new(1, 2, "down"), new(2, 2, "right"), new(2, 3, "down"), new(3, 3, "right"), new(3, 4, "down"), new(4, 4, "right"), new(4, 5, "down"), new(5, 5, "right"), new(5, 6, "down"), new(6, 6, "right"), new(6, 7, "down"), new(7, 7, "right"), new(7, 8, "down"), new(8, 8, "left"), new(8, 7, "up"), new(7, 7, "left"), new(7, 6, "up"), new(6, 6, "left"), new(6, 5, "up"), new(5, 5, "left"), new(5, 4, "up"), new(4, 4, "left"), new(4, 3, "up"), new(3, 3, "left"), new(3, 2, "up"), new(2, 2, "left"), new(2, 1, "up"), new(1, 1, "left"), new(1, 0, "up")
    }});

    // Level 40 - Dairesel Yol (8x8)
    _levels.Add(new LevelData { name = "Circular Path", width = 8, height = 8, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "right"), new(0, 3, "right"), new(0, 4, "right"), new(0, 5, "right"), new(0, 6, "right"), new(0, 7, "down"), new(1, 7, "left"), new(2, 7, "left"), new(3, 7, "left"), new(4, 7, "left"), new(5, 7, "left"), new(6, 7, "left"), new(7, 7, "down"), new(7, 6, "up"), new(7, 5, "up"), new(7, 4, "up"), new(7, 3, "up"), new(7, 2, "up"), new(7, 1, "up"), new(7, 0, "up"), new(6, 0, "right"), new(5, 0, "right"), new(4, 0, "right"), new(3, 0, "right"), new(2, 0, "right"), new(1, 0, "right"), new(1, 1, "down"), new(1, 2, "down"), new(1, 3, "down"), new(1, 4, "down"), new(1, 5, "down"), new(1, 6, "right")
    }});

    // Level 41 - Artan Zorluk 1 (6x6)
    _levels.Add(new LevelData { name = "Difficulty Spike 1", width = 6, height = 6, arrows = new() {
        new(0, 0, "down"), new(1, 0, "right"), new(1, 1, "down"), new(2, 1, "right"), new(2, 2, "down"), new(3, 2, "right"), new(3, 3, "down"), new(4, 3, "right"), new(4, 4, "down"), new(5, 4, "right"), new(5, 5, "up"), new(4, 5, "left"), new(3, 5, "left"), new(3, 4, "up"), new(2, 4, "left"), new(1, 4, "left"), new(1, 3, "up"), new(0, 3, "right")
    }});

    // Level 42 - Artan Zorluk 2 (7x7)
    _levels.Add(new LevelData { name = "Difficulty Spike 2", width = 7, height = 7, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "down"), new(1, 2, "left"), new(1, 1, "left"), new(1, 0, "down"), new(2, 0, "right"), new(2, 1, "right"), new(2, 2, "down"), new(3, 2, "left"), new(3, 1, "left"), new(3, 0, "down"), new(4, 0, "right"), new(4, 1, "right"), new(4, 2, "down"), new(5, 2, "left"), new(5, 1, "left"), new(5, 0, "down"), new(6, 0, "right"), new(6, 1, "right"), new(6, 2, "up")
    }});

    // Level 43 - Artan Zorluk 3 (8x8)
    _levels.Add(new LevelData { name = "Difficulty Spike 3", width = 8, height = 8, arrows = new() {
        new(0, 0, "down"), new(1, 0, "right"), new(1, 1, "down"), new(2, 1, "right"), new(2, 2, "down"), new(3, 2, "right"), new(3, 3, "down"), new(4, 3, "right"), new(4, 4, "down"), new(5, 4, "right"), new(5, 5, "down"), new(6, 5, "right"), new(6, 6, "down"), new(7, 6, "right"), new(7, 7, "up"), new(6, 7, "left"), new(5, 7, "left"), new(5, 6, "up"), new(4, 6, "left"), new(3, 6, "left"), new(3, 5, "up"), new(2, 5, "left"), new(1, 5, "left"), new(1, 4, "up"), new(0, 4, "right")
    }});

    // Level 44 - Şaşırtmalı 5x5
    _levels.Add(new LevelData { name = "Tricky 5", width = 5, height = 5, arrows = new() {
        new(0, 0, "right"), new(0, 1, "down"), new(1, 1, "right"), new(1, 2, "down"), new(2, 2, "right"), new(2, 3, "down"), new(3, 3, "right"), new(3, 4, "down"), new(4, 4, "left"), new(4, 3, "up"), new(3, 3, "left"), new(3, 2, "up"), new(2, 2, "left"), new(2, 1, "up"), new(1, 1, "left")
    }});

    // Level 45 - Yılan 9x9 (Basit)
    _levels.Add(new LevelData { name = "Snake 9", width = 9, height = 9, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "right"), new(0, 3, "right"), new(0, 4, "right"), new(0, 5, "right"), new(0, 6, "right"), new(0, 7, "right"), new(0, 8, "down"), new(1, 8, "left"), new(2, 8, "left"), new(3, 8, "left"), new(4, 8, "left"), new(5, 8, "left"), new(6, 8, "left"), new(7, 8, "left"), new(8, 8, "down"), new(8, 7, "up"), new(8, 6, "up"), new(8, 5, "up"), new(8, 4, "up"), new(8, 3, "up"), new(8, 2, "up"), new(8, 1, "up"), new(8, 0, "up")
    }});

    // Level 46 - Labirent 9x9 (Orta)
    _levels.Add(new LevelData { name = "Maze 9", width = 9, height = 9, arrows = new() {
        new(0, 0, "right"), new(0, 1, "down"), new(1, 1, "right"), new(1, 2, "down"), new(2, 2, "right"), new(2, 3, "down"), new(3, 3, "right"), new(3, 4, "down"), new(4, 4, "right"), new(4, 5, "down"), new(5, 5, "right"), new(5, 6, "down"), new(6, 6, "right"), new(6, 7, "down"), new(7, 7, "right"), new(7, 8, "down"), new(8, 8, "left"), new(8, 7, "up"), new(7, 7, "left"), new(7, 6, "up"), new(6, 6, "left"), new(6, 5, "up"), new(5, 5, "left"), new(5, 4, "up"), new(4, 4, "left"), new(4, 3, "up"), new(3, 3, "left"), new(3, 2, "up"), new(2, 2, "left"), new(2, 1, "up"), new(1, 1, "left"), new(1, 0, "up")
    }});

    // Level 47 - Zikzak 10x5 (Geniş)
    _levels.Add(new LevelData { name = "Zigzag Wide", width = 10, height = 5, arrows = new() {
        new(0, 0, "down"), new(1, 0, "right"), new(1, 1, "up"), new(0, 1, "right"), new(0, 2, "down"), new(1, 2, "right"), new(1, 3, "up"), new(0, 3, "right"), new(0, 4, "down"), new(1, 4, "right"), new(1, 5, "up"), new(0, 5, "right"), new(0, 6, "down"), new(1, 6, "right"), new(1, 7, "up"), new(0, 7, "right"), new(0, 8, "down"), new(1, 8, "right"), new(1, 9, "up"), new(2, 9, "left"), new(2, 8, "down"), new(3, 8, "left"), new(3, 7, "up"), new(4, 7, "left"), new(4, 6, "down")
    }});

    // Level 48 - Çapraz Geçiş (8x8)
    _levels.Add(new LevelData { name = "Cross Pass", width = 8, height = 8, arrows = new() {
        new(0, 0, "right"), new(0, 1, "down"), new(1, 1, "right"), new(1, 2, "down"), new(2, 2, "right"), new(2, 3, "down"), new(3, 3, "right"), new(3, 4, "down"), new(4, 4, "right"), new(4, 5, "down"), new(5, 5, "right"), new(5, 6, "down"), new(6, 6, "right"), new(6, 7, "down"), new(7, 7, "up"), new(6, 7, "left"), new(5, 7, "left"), new(5, 6, "up"), new(4, 6, "left"), new(3, 6, "left"), new(3, 5, "up"), new(2, 5, "left"), new(1, 5, "left"), new(1, 4, "up"), new(0, 4, "right")
    }});

    // Level 49 - Zorlu 10x10 (İlk Kısım)
    _levels.Add(new LevelData { name = "Challenge 10x10", width = 10, height = 10, arrows = new() {
        new(0, 0, "right"), new(0, 1, "right"), new(0, 2, "right"), new(0, 3, "right"), new(0, 4, "right"), new(0, 5, "right"), new(0, 6, "right"), new(0, 7, "right"), new(0, 8, "right"), new(0, 9, "down"), new(1, 9, "left"), new(2, 9, "left"), new(3, 9, "left"), new(4, 9, "left"), new(5, 9, "left"), new(6, 9, "left"), new(7, 9, "left"), new(8, 9, "left"), new(9, 9, "down"), new(9, 8, "up"), new(9, 7, "up"), new(9, 6, "up"), new(9, 5, "up"), new(9, 4, "up"), new(9, 3, "up"), new(9, 2, "up"), new(9, 1, "up"), new(9, 0, "up"), new(8, 0, "right"), new(7, 0, "right"), new(6, 0, "right"), new(5, 0, "right"), new(4, 0, "right"), new(3, 0, "right"), new(2, 0, "right"), new(1, 0, "right"), new(1, 1, "down"), new(1, 2, "down"), new(1, 3, "down"), new(1, 4, "down"), new(1, 5, "down"), new(1, 6, "down"), new(1, 7, "down"), new(1, 8, "right")
    }});

    // Level 50 - Final Boss (10x10)
    _levels.Add(new LevelData { name = "Final Boss", width = 10, height = 10, arrows = new() {
        new(0, 0, "down"), new(1, 0, "right"), new(1, 1, "down"), new(2, 1, "right"), new(2, 2, "down"), new(3, 2, "right"), new(3, 3, "down"), new(4, 3, "right"), new(4, 4, "down"), new(5, 4, "right"), new(5, 5, "down"), new(6, 5, "right"), new(6, 6, "down"), new(7, 6, "right"), new(7, 7, "down"), new(8, 7, "right"), new(8, 8, "down"), new(9, 8, "right"), new(9, 9, "up"), new(8, 9, "left"), new(7, 9, "left"), new(7, 8, "up"), new(6, 8, "left"), new(5, 8, "left"), new(5, 7, "up"), new(4, 7, "left"), new(3, 7, "left"), new(3, 6, "up"), new(2, 6, "left"), new(1, 6, "left"), new(1, 5, "up"), new(0, 5, "right"), new(0, 4, "right"), new(0, 3, "right"), new(0, 2, "right"), new(0, 1, "right")
    }});
}
}