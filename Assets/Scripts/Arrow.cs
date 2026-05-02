using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>Direction enum used across the entire project.</summary>
public enum Direction { Up, Down, Left, Right }

/// <summary>
/// Arrow — Attached to each arrow GameObject. Handles visual state
/// and delegates move logic to GridManager.
/// Each direction has a fixed solid color.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Arrow : MonoBehaviour, IPointerDownHandler
{
    // ── Public Data ──────────────────────────────────────────────
    public Direction Dir { get; private set; }
    public int GridX { get; private set; }
    public int GridY { get; private set; }

    // ── References ───────────────────────────────────────────────
    GridManager _gridManager;
    SpriteRenderer _sr;

    Color _baseColor;
    bool _isHighlighted;
    Vector3 _originalScale;

    TrailRenderer _trail;
    ParticleSystem _activeTrailFX;

    // ─────────────────────────────────────────────────────────────
    /// <summary>Called by GridManager immediately after AddComponent.</summary>
    public void Init(GridManager manager, Direction dir, int x, int y, Vector3 worldPos)
    {
        _gridManager = manager;
        Dir = dir;
        GridX = x;
        GridY = y;
        transform.position = worldPos;

        _sr = GetComponent<SpriteRenderer>();
        _originalScale = transform.localScale;

        // Use fixed solid color per direction
        _baseColor = TextureGenerator.GetColorForDirection(dir);
        _sr.color = _baseColor;

        // Set rotation based on direction (assuming default right-facing sprite)
        float angle = 0f;
        switch (dir)
        {
            case Direction.Right: angle = 0f; break;
            case Direction.Up: angle = 90f; break;
            case Direction.Left: angle = 180f; break;
            case Direction.Down: angle = -90f; break;
        }
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Add a collider that wraps tightly around the sprite itself if missing.
        if (GetComponent<BoxCollider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();

        // Setup Trail
        GameObject trailGO = new GameObject("Trail");
        trailGO.transform.SetParent(transform);
        trailGO.transform.localPosition = Vector3.zero;
        _trail = trailGO.AddComponent<TrailRenderer>();
        _trail.time = 0.3f;
        _trail.startWidth = 0.4f;
        _trail.endWidth = 0f;
        _trail.material = new Material(Shader.Find("Sprites/Default"));
        _trail.startColor = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.7f);
        _trail.endColor = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0f);
        _trail.emitting = false;
        _trail.sortingOrder = 0;

        // Entrance pop animation
        transform.localScale = Vector3.zero;
        StartCoroutine(SimpleTween.ScaleTo(gameObject, _originalScale, 0.3f, SimpleTween.Ease.BackOut));
    }

    public void SetEmitting(bool emit)
    {
        _trail.emitting = emit;

        // Attach particle trail FX if available
        if (emit && FXManager.Instance != null)
        {
            FXManager.Instance.PlayArrowSlideStart(transform.position, _baseColor);
            _activeTrailFX = FXManager.Instance.PlayArrowSlideTrail(transform);
        }
    }

    /// <summary>Called before the arrow is destroyed to play exit FX.</summary>
    public void PlayExitFX()
    {
        if (FXManager.Instance != null)
        {
            FXManager.Instance.PlayArrowExit(transform.position, _baseColor);
        }

        // Detach trail FX so it finishes playing
        if (_activeTrailFX != null)
        {
            _activeTrailFX.transform.SetParent(null);
            _activeTrailFX.Stop();
            Object.Destroy(_activeTrailFX.gameObject, 2f);
            _activeTrailFX = null;
        }
    }

    public Color BaseColor => _baseColor;

    // ─────────────────────────────────────────────────────────────
    /// <summary>Called automatically by Unity EventSystem when clicked.</summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        TryMove();
    }

    /// <summary>Initiates a move attempt via GridManager.</summary>
    public void TryMove()
    {
        _gridManager.TryMoveArrow(this);
    }

    // ── Animations ───────────────────────────────────────────────
    /// <summary>Shake animation on failed move (collision).</summary>
    public void PlayShake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        Vector3 origin = transform.position;
        AudioManager.Instance.PlaySFX("tap"); // Fail sound

        // Spawn fail particle
        if (FXManager.Instance != null)
            FXManager.Instance.PlayArrowFail(transform.position);

        // Flash red and shake
        float elapsed = 0f;
        float duration = 0.35f;
        while (elapsed < duration)
        {
            float strength = (1f - (elapsed / duration)) * 0.18f;
            transform.position = origin + (Vector3)UnityEngine.Random.insideUnitCircle * strength;

            // Flash between base color and a bright red
            float flash = Mathf.PingPong(elapsed * 20f, 1f);
            _sr.color = Color.Lerp(_baseColor, new Color(1f, 0.15f, 0.15f), flash * 0.7f);

            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = origin;
        _sr.color = _baseColor;
    }

    // ─────────────────────────────────────────────────────────────
    /// <summary>Highlight this arrow as a hint (pulsating glow).</summary>
    public void ShowHint()
    {
        if (_isHighlighted) return;
        _isHighlighted = true;
        StartCoroutine(HintRoutine());
    }

    IEnumerator HintRoutine()
    {
        // Play hint FX
        AudioManager.Instance.PlaySFX("hint");
        if (FXManager.Instance != null)
            FXManager.Instance.PlayHint(transform.position, _baseColor);

        float elapsed = 0f;
        float duration = 1.5f;
        while (elapsed < duration)
        {
            float t = Mathf.PingPong(elapsed * 4f, 1f);
            transform.localScale = _originalScale * (1f + t * 0.15f);
            _sr.color = Color.Lerp(_baseColor, Color.white, t * 0.35f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = _originalScale;
        _sr.color = _baseColor;
        _isHighlighted = false;
    }
}