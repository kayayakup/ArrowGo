using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// GridManager — Owns the 2D grid array, handles arrow placement, 
/// slide logic (collision detection), animations, and win detection.
/// </summary>
public class GridManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────
    public static GridManager Instance { get; private set; }

    public static void CreateInstance()
    {
        GameObject go = new GameObject("GridManager");
        Instance = go.AddComponent<GridManager>();
        DontDestroyOnLoad(go);
    }

    // ── Grid Data ────────────────────────────────────────────────
    Arrow[,] _grid;           // null = empty cell
    int _width, _height;
    int _arrowsRemaining;
    bool _isAnimating;    // block input during animation

    // ── Visual Settings ──────────────────────────────────────────
    public float CellSize { get; private set; } = 1.1f;
    public float CellGap { get; private set; } = 0.05f;
    Vector3 _gridOrigin;

    // ── Cell Colors ──────────────────────────────────────────────
    // Alternating solid cell colors for a clean checkerboard look
    static readonly Color CellColorA = new Color(0.92f, 0.92f, 0.96f, 1f);
    static readonly Color CellColorB = new Color(0.86f, 0.86f, 0.92f, 1f);

    // ── Parent GameObject ─────────────────────────────────────────
    GameObject _gridRoot;

    // ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>Builds the entire visual grid from LevelData.</summary>
    public void SetupGrid(LevelData data)
    {
        ClearGrid();

        _width = data.width;
        _height = data.height;
        _grid = new Arrow[_width, _height];
        _arrowsRemaining = 0;
        _isAnimating = false;

        // Adapt camera size to grid
        float totalW = _width * (CellSize + CellGap);
        float totalH = _height * (CellSize + CellGap);
        
        float aspect = (float)Screen.width / Screen.height;
        // Add 2f to total size (1 unit padding on each side)
        float requiredSizeY = (totalH + 2f) / 2f;
        float requiredSizeX = ((totalW + 2f) / 2f) / aspect;
        
        Camera.main.orthographicSize = Mathf.Max(requiredSizeX, requiredSizeY);

        // Centre grid at world origin
        _gridOrigin = new Vector3(
            -(_width * (CellSize + CellGap)) * 0.5f + (CellSize + CellGap) * 0.5f,
            -(_height * (CellSize + CellGap)) * 0.5f + (CellSize + CellGap) * 0.5f + 0.5f,
            0f
        );

        // Create parent root
        _gridRoot = new GameObject("GridRoot");

        // Draw background cells
        foreach (var placement in data.arrows)
        {
            DrawBackgroundCell(placement.col, placement.row);
        }
        // Draw all background cells for the full grid
        for (int r = 0; r < _height; r++)
            for (int c = 0; c < _width; c++)
                DrawBackgroundCell(c, r);

        // Place arrows
        foreach (var placement in data.arrows)
        {
            Direction dir = ParseDirection(placement.direction);
            PlaceArrow(placement.col, placement.row, dir);
            _arrowsRemaining++;
        }
    }

    // ─────────────────────────────────────────────────────────────
    void DrawBackgroundCell(int col, int row)
    {
        GameObject cell = new GameObject($"Cell_{col}_{row}");
        cell.transform.SetParent(_gridRoot.transform);
        cell.transform.position = GridToWorld(col, row);

        SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();

        // Alternating solid cell colors — clean checkerboard
        bool isEven = (col + row) % 2 == 0;
        Color cellColor = isEven ? CellColorA : CellColorB;

        sr.sprite = TextureGenerator.CreateSquareSprite(Color.white);
        sr.color = cellColor;
        sr.sortingOrder = -1;
        // Sprite is 1 world unit natively, scale to fill cell
        cell.transform.localScale = Vector3.one * CellSize;
    }

    // ─────────────────────────────────────────────────────────────
    void PlaceArrow(int col, int row, Direction dir)
    {
        Arrow arrow;
        Vector3 worldPos = GridToWorld(col, row);

        if (Bootstrap.Instance != null && Bootstrap.Instance.arrowPrefab != null)
        {
            arrow = Instantiate(Bootstrap.Instance.arrowPrefab, worldPos, Quaternion.identity, _gridRoot.transform);
            arrow.name = $"Arrow_{col}_{row}";
            arrow.transform.localScale = Vector3.one * (CellSize * 0.75f);
        }
        else
        {
            // Fallback
            GameObject go = new GameObject($"Arrow_{col}_{row}");
            go.transform.SetParent(_gridRoot.transform);
            go.transform.position = worldPos;
            go.transform.localScale = Vector3.one * (CellSize * 0.75f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = TextureGenerator.CreateArrowSprite(dir);
            sr.sortingOrder = 1;

            arrow = go.AddComponent<Arrow>();
        }

        arrow.Init(this, dir, col, row, worldPos);
        _grid[col, row] = arrow;
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>Core move logic — called when player taps an arrow.</summary>
    public void TryMoveArrow(Arrow arrow)
    {
        if (_isAnimating) return;
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        int col = arrow.GridX;
        int row = arrow.GridY;
        Direction dir = arrow.Dir;

        // Trace path in direction
        int dc = DirToColDelta(dir);
        int dr = DirToRowDelta(dir);

        int c = col + dc;
        int r = row + dr;
        bool blocked = false;

        while (c >= 0 && c < _width && r >= 0 && r < _height)
        {
            if (_grid[c, r] != null)
            {
                blocked = true;
                break;
            }
            c += dc;
            r += dr;
        }

        if (blocked)
        {
            // Collision — lose a life, play shake
            GameManager.Instance.RegisterFailedMove();
            arrow.PlayShake();
            StartCoroutine(ScreenShake(0.15f, 0.1f));
        }
        else
        {
            // Success — animate slide to exit, then remove
            _isAnimating = true;
            _grid[col, row] = null;

            Vector3 exitPos = GridToWorld(c, r);
            exitPos += new Vector3(dc * CellSize * 3f, dr * CellSize * 3f, 0f);

            GameManager.Instance.RegisterSuccessfulMove();
            AudioManager.Instance.PlaySFX("slide");
            arrow.SetEmitting(true);

            StartCoroutine(AnimateArrowExit(arrow, exitPos));
        }
    }

    // ─────────────────────────────────────────────────────────────
    IEnumerator AnimateArrowExit(Arrow arrow, Vector3 exitPos)
    {
        // Slide with overshoot
        yield return StartCoroutine(SimpleTween.MoveTo(
            arrow.gameObject, exitPos, 0.4f, SimpleTween.Ease.CubicIn));

        // Play exit FX before destroying
        arrow.PlayExitFX();

        Destroy(arrow.gameObject);
        _arrowsRemaining--;
        _isAnimating = false;

        if (_arrowsRemaining <= 0)
        {
            yield return new WaitForSeconds(0.4f);
            GameManager.Instance.CompleteLevel();
        }
    }

    IEnumerator ScreenShake(float duration, float magnitude)
    {
        Vector3 origin = Camera.main.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            Camera.main.transform.position = new Vector3(origin.x + x, origin.y + y, origin.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.position = origin;
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>Returns all arrows currently on the grid (for hint system).</summary>
    public List<Arrow> GetAllArrows()
    {
        var list = new List<Arrow>();
        if (_grid == null) return list;
        for (int r = 0; r < _height; r++)
            for (int c = 0; c < _width; c++)
                if (_grid[c, r] != null) list.Add(_grid[c, r]);
        return list;
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>Checks if an arrow can exit (no blocker in its path).</summary>
    public bool CanArrowExit(Arrow arrow)
    {
        int dc = DirToColDelta(arrow.Dir);
        int dr = DirToRowDelta(arrow.Dir);
        int c = arrow.GridX + dc;
        int r = arrow.GridY + dr;

        while (c >= 0 && c < _width && r >= 0 && r < _height)
        {
            if (_grid[c, r] != null) return false;
            c += dc;
            r += dr;
        }
        return true;
    }

    // ─────────────────────────────────────────────────────────────
    public void ClearGrid()
    {
        if (_gridRoot != null) Destroy(_gridRoot);
        _grid = null;
        _arrowsRemaining = 0;
        _isAnimating = false;
    }

    // ── Helpers ──────────────────────────────────────────────────
    public Vector3 GridToWorld(int col, int row) =>
        _gridOrigin + new Vector3(col * (CellSize + CellGap), row * (CellSize + CellGap), 0f);

    public bool WorldToGrid(Vector3 world, out int col, out int row)
    {
        if (_grid == null) { col = row = -1; return false; }
        col = Mathf.RoundToInt((world.x - _gridOrigin.x) / (CellSize + CellGap));
        row = Mathf.RoundToInt((world.y - _gridOrigin.y) / (CellSize + CellGap));
        return col >= 0 && col < _width && row >= 0 && row < _height;
    }

    public Arrow GetArrowAt(int col, int row) =>
        (_grid != null && col >= 0 && col < _width && row >= 0 && row < _height)
            ? _grid[col, row]
            : null;

    public bool IsAnimating => _isAnimating;

    static int DirToColDelta(Direction d) => d == Direction.Left ? -1 : d == Direction.Right ? 1 : 0;
    static int DirToRowDelta(Direction d) => d == Direction.Down ? -1 : d == Direction.Up ? 1 : 0;

    static Direction ParseDirection(string s) => s.ToLower() switch
    {
        "up" => Direction.Up,
        "down" => Direction.Down,
        "left" => Direction.Left,
        "right" => Direction.Right,
        _ => Direction.Right
    };
}