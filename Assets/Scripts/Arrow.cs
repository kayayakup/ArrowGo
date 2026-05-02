using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>Direction enum used across the entire project.</summary>
public enum Direction { Up, Down, Left, Right }

/// <summary>
/// Arrow — Attached to each arrow GameObject. Handles visual state
/// and delegates move logic to GridManager.
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
        // Assign a completely unique random color to this specific arrow
        float hue = UnityEngine.Random.value;
        _baseColor = Color.HSVToRGB(hue, 0.75f, 0.95f);
        _sr.color = _baseColor;

        // Add a collider that wraps tightly around the sprite itself.
        // Unity automatically sizes the BoxCollider2D to match the SpriteRenderer bounds.
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
        _trail.startColor = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.6f);
        _trail.endColor = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0f);
        _trail.emitting = false;
        _trail.sortingOrder = 0;
    }

    public void SetEmitting(bool emit) => _trail.emitting = emit;

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
        
        float elapsed = 0f;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            float strength = (1f - (elapsed / duration)) * 0.15f;
            transform.position = origin + (Vector3)UnityEngine.Random.insideUnitCircle * strength;
            _sr.color = Color.Lerp(_baseColor, Color.red, strength * 4f);
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
        float elapsed = 0f;
        float duration = 1.5f;
        while (elapsed < duration)
        {
            float t = Mathf.PingPong(elapsed * 4f, 1f);
            transform.localScale = _originalScale * (1f + t * 0.15f);
            _sr.color = Color.Lerp(_baseColor, new Color(1.5f, 1.5f, 1.5f, 1f), t * 0.4f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = _originalScale;
        _sr.color = _baseColor;
        _isHighlighted = false;
    }
}