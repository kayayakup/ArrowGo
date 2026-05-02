using UnityEngine;
using System;

/// <summary>
/// GameManager — Central singleton that owns game state, lives, and move tracking.
/// Coordinates GridManager, UIManager, LevelManager, and AudioManager.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    public static void CreateInstance()
    {
        GameObject go = new GameObject("GameManager");
        Instance = go.AddComponent<GameManager>();
        DontDestroyOnLoad(go);
    }

    // ── Constants ────────────────────────────────────────────────
    public const int MAX_LIVES = 3;

    // ── State ────────────────────────────────────────────────────
    public enum GameState { MainMenu, LevelSelect, Playing, LevelComplete, GameOver }

    public GameState State { get; private set; } = GameState.MainMenu;
    public int CurrentLives { get; private set; } = MAX_LIVES;
    public int MoveCount { get; private set; } = 0;
    public int CurrentLevel { get; private set; } = 0;
    public bool HadNoMistakes { get; private set; } = true;

    // ── Events ───────────────────────────────────────────────────
    public event Action<int> OnLivesChanged;
    public event Action<int> OnMoveCountChanged;
    public event Action<GameState> OnStateChanged;

    // ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>Called by Bootstrap after all managers are ready.</summary>
    public void StartGame()
    {
        CurrentLives = MAX_LIVES;
        CurrentLevel = PlayerPrefs.GetInt("UnlockedLevel", 0);
        ChangeState(GameState.LevelSelect);
        UIManager.Instance.ShowLevelSelectScreen();
    }

    // ── State Changes ────────────────────────────────────────────
    void ChangeState(GameState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(newState);
    }

    // ─────────────────────────────────────────────────────────────
    public void LoadLevel(int levelIndex)
    {
        CurrentLevel = levelIndex;
        CurrentLives = MAX_LIVES;
        MoveCount = 0;
        HadNoMistakes = true;
        ChangeState(GameState.Playing);

        LevelData data = LevelManager.Instance.GetLevelData(levelIndex);
        GridManager.Instance.SetupGrid(data);
        UIManager.Instance.ShowGameplayUI();
        UIManager.Instance.UpdateLives(CurrentLives);
        UIManager.Instance.UpdateMoveCount(MoveCount);
        AudioManager.Instance.PlayMusic();
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>Called by GridManager when an arrow successfully exits.</summary>
    public void RegisterSuccessfulMove()
    {
        MoveCount++;
        OnMoveCountChanged?.Invoke(MoveCount);
        UIManager.Instance.UpdateMoveCount(MoveCount);
        AudioManager.Instance.PlaySFX("success");
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>Called by GridManager when an arrow collides.</summary>
    public void RegisterFailedMove()
    {
        HadNoMistakes = false;
        CurrentLives = Mathf.Max(0, CurrentLives - 1);
        OnLivesChanged?.Invoke(CurrentLives);
        UIManager.Instance.UpdateLives(CurrentLives);
        AudioManager.Instance.PlaySFX("lifeLost");

        if (CurrentLives <= 0)
        {
            GameOver();
        }
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>Called by GridManager when all arrows are removed.</summary>
    public void CompleteLevel()
    {
        ChangeState(GameState.LevelComplete);

        // Unlock next level
        int unlockedMax = PlayerPrefs.GetInt("UnlockedLevel", 0);
        int nextLevel = CurrentLevel + 1;
        if (nextLevel > unlockedMax && nextLevel < LevelManager.Instance.LevelCount)
        {
            PlayerPrefs.SetInt("UnlockedLevel", nextLevel);
        }

        // Stars: 3 = no mistakes, 2 = 1 mistake, 1 = survived
        int stars = HadNoMistakes ? 3 : (CurrentLives == MAX_LIVES - 1 ? 2 : 1);

        // Save stars
        string key = "Stars_" + CurrentLevel;
        int prev = PlayerPrefs.GetInt(key, 0);
        if (stars > prev) PlayerPrefs.SetInt(key, stars);
        PlayerPrefs.Save();

        AudioManager.Instance.PlaySFX("levelComplete");
        UIManager.Instance.ShowLevelCompletePanel(stars, nextLevel, CurrentLevel);
    }

    // ─────────────────────────────────────────────────────────────
    public void GameOver()
    {
        ChangeState(GameState.GameOver);
        AudioManager.Instance.PlaySFX("fail");
        UIManager.Instance.ShowGameOverPanel();
    }

    // ─────────────────────────────────────────────────────────────
    public void RestartLevel() => LoadLevel(CurrentLevel);
    public void NextLevel()
    {
        int next = CurrentLevel + 1;
        if (next < LevelManager.Instance.LevelCount)
            LoadLevel(next);
        else
            UIManager.Instance.ShowLevelSelectScreen();
    }
    public void ReturnToLevelSelect()
    {
        GridManager.Instance.ClearGrid();
        ChangeState(GameState.LevelSelect);
        UIManager.Instance.ShowLevelSelectScreen();
    }
}