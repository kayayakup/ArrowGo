using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// LevelGenerator — Generates random solvable levels of any grid size.
/// Uses a backtracking solver to verify solvability before returning.
/// 
/// Usage: LevelGenerator.Generate(5, 5) → returns a valid LevelData
/// </summary>
public static class LevelGenerator
{
    // Maximum solver attempts before giving up on a layout
    const int MAX_SOLVE_ATTEMPTS = 500;
    const int MAX_GEN_ATTEMPTS = 200;

    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Generates a random solvable level with the given dimensions
    /// and approximately (width * height * density) arrows.
    /// density is 0..1, typical 0.4–0.7.
    /// </summary>
    public static LevelData Generate(int width, int height, float density = 0.5f)
    {
        int targetArrows = Mathf.Max(2, Mathf.RoundToInt(width * height * density));

        for (int attempt = 0; attempt < MAX_GEN_ATTEMPTS; attempt++)
        {
            LevelData candidate = GenerateCandidate(width, height, targetArrows);
            if (candidate != null && IsSolvable(candidate))
                return candidate;
        }

        // Fallback: return a minimal guaranteed-solvable level
        return GenerateTrivial(width, height);
    }

    // ─────────────────────────────────────────────────────────────
    static LevelData GenerateCandidate(int w, int h, int count)
    {
        // Collect all cells and shuffle
        var cells = new List<(int col, int row)>();
        for (int r = 0; r < h; r++)
            for (int c = 0; c < w; c++)
                cells.Add((c, r));

        Shuffle(cells);

        string[] dirs = { "up", "down", "left", "right" };
        var level = new LevelData { width = w, height = h, name = $"{w}x{h} Random" };

        for (int i = 0; i < Mathf.Min(count, cells.Count); i++)
        {
            string dir = dirs[Random.Range(0, 4)];
            level.arrows.Add(new ArrowPlacement(cells[i].row, cells[i].col, dir));
        }

        return level;
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>Backtracking solver — returns true if the level is solvable.</summary>
    static bool IsSolvable(LevelData level)
    {
        // Build a mutable grid
        string[,] grid = new string[level.width, level.height];
        foreach (var a in level.arrows)
            grid[a.col, a.row] = a.direction;

        return Solve(grid, level.width, level.height, 0);
    }

    static bool Solve(string[,] grid, int w, int h, int depth)
    {
        if (depth > MAX_SOLVE_ATTEMPTS) return false;

        // Find all arrows that can currently exit
        var candidates = new List<(int c, int r)>();
        for (int r = 0; r < h; r++)
        {
            for (int c = 0; c < w; c++)
            {
                if (grid[c, r] == null) continue;
                if (CanExit(grid, w, h, c, r)) candidates.Add((c, r));
            }
        }

        // No arrows left — success
        bool anyArrow = false;
        for (int r = 0; r < h; r++)
            for (int c = 0; c < w; c++)
                if (grid[c, r] != null) { anyArrow = true; break; }

        if (!anyArrow) return true;
        if (candidates.Count == 0) return false; // Stuck

        // Try each candidate (first valid one wins — greedy is fine for verification)
        foreach (var (c, r) in candidates)
        {
            string saved = grid[c, r];
            grid[c, r] = null;

            if (Solve(grid, w, h, depth + 1)) return true;

            grid[c, r] = saved; // backtrack
        }

        return false;
    }

    static bool CanExit(string[,] grid, int w, int h, int col, int row)
    {
        string dir = grid[col, row];
        (int dc, int dr) = dir switch
        {
            "right" => (1, 0),
            "left" => (-1, 0),
            "up" => (0, 1),
            "down" => (0, -1),
            _ => (0, 0)
        };

        int c = col + dc, r = row + dr;
        while (c >= 0 && c < w && r >= 0 && r < h)
        {
            if (grid[c, r] != null) return false;
            c += dc; r += dr;
        }
        return true; // Reached the edge
    }

    // ─────────────────────────────────────────────────────────────
    static LevelData GenerateTrivial(int w, int h)
    {
        // Minimal guaranteed solvable: all arrows point to the nearest edge
        var level = new LevelData { width = w, height = h, name = "Generated" };
        for (int c = 0; c < w; c++)
        {
            string dir = c < w / 2 ? "left" : "right";
            level.arrows.Add(new ArrowPlacement(h / 2, c, dir));
        }
        return level;
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