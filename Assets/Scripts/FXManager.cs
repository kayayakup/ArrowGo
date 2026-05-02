using UnityEngine;

/// <summary>
/// FXManager — Manages all particle effects. 
/// Assign ParticleSystem prefabs via Inspector (drag - drop).
/// Particles are spawned at runtime at the specified position.
/// </summary>
public class FXManager : MonoBehaviour
{
    public static FXManager Instance { get; private set; }

    // ── Inspector: Particle Prefabs ─────────────────────────────
    [Header("Arrow Slide FX")]
    [Tooltip("Particle burst played when an arrow starts sliding out")]
    public ParticleSystem arrowSlideStartFX;

    [Tooltip("Particle trail following the arrow as it slides")]
    public ParticleSystem arrowSlideTrailFX;

    [Header("Collision / Fail FX")]
    [Tooltip("Particle burst played on a failed move (collision)")]
    public ParticleSystem arrowFailFX;

    [Tooltip("Particle effect when a life/heart is lost")]
    public ParticleSystem lifeLostFX;

    [Header("Success FX")]
    [Tooltip("Particle burst when an arrow successfully exits the grid")]
    public ParticleSystem arrowExitFX;

    [Tooltip("Big celebration particles on level complete")]
    public ParticleSystem levelCompleteFX;

    [Header("UI Interaction FX")]
    [Tooltip("Small particle pop on button click")]
    public ParticleSystem buttonClickFX;

    [Tooltip("Particle effect when hint arrow pulses")]
    public ParticleSystem hintFX;

    [Header("Game Over FX")]
    [Tooltip("Dramatic particle effect on game over")]
    public ParticleSystem gameOverFX;

    // ─────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>Spawn a particle effect at a world position. Returns the instance for chaining.</summary>
    public ParticleSystem PlayFX(ParticleSystem prefab, Vector3 worldPosition)
    {
        if (prefab == null) return null;

        ParticleSystem instance = Instantiate(prefab, worldPosition, Quaternion.identity);
        
        // Auto-destroy after the particle system finishes
        var main = instance.main;
        float totalDuration = main.duration + main.startLifetime.constantMax;
        Destroy(instance.gameObject, totalDuration + 0.5f);

        instance.Play();
        return instance;
    }

    /// <summary>Spawn a particle effect at a world position with a custom color.</summary>
    public ParticleSystem PlayFX(ParticleSystem prefab, Vector3 worldPosition, Color color)
    {
        ParticleSystem instance = PlayFX(prefab, worldPosition);
        if (instance != null)
        {
            var main = instance.main;
            main.startColor = color;
        }
        return instance;
    }

    /// <summary>Spawn and parent a trail FX to a moving object.</summary>
    public ParticleSystem PlayTrailFX(ParticleSystem prefab, Transform parent)
    {
        if (prefab == null || parent == null) return null;

        ParticleSystem instance = Instantiate(prefab, parent.position, Quaternion.identity, parent);
        instance.Play();
        return instance;
    }

    // ── Convenience Methods ─────────────────────────────────────
    public void PlayArrowSlideStart(Vector3 pos, Color color) => PlayFX(arrowSlideStartFX, pos, color);
    public ParticleSystem PlayArrowSlideTrail(Transform parent) => PlayTrailFX(arrowSlideTrailFX, parent);
    public void PlayArrowFail(Vector3 pos) => PlayFX(arrowFailFX, pos);
    public void PlayLifeLost(Vector3 pos) => PlayFX(lifeLostFX, pos);
    public void PlayArrowExit(Vector3 pos, Color color) => PlayFX(arrowExitFX, pos, color);
    public void PlayLevelComplete(Vector3 pos) => PlayFX(levelCompleteFX, pos);
    public void PlayButtonClick(Vector3 pos) => PlayFX(buttonClickFX, pos);
    public void PlayHint(Vector3 pos, Color color) => PlayFX(hintFX, pos, color);
    public void PlayGameOver(Vector3 pos) => PlayFX(gameOverFX, pos);
}
