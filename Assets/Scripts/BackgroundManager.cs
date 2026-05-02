using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// BackgroundManager — Cycles through a list of background sprites smoothly.
/// Assign sprites in the Inspector. It creates UI Images to render them.
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance { get; private set; }

    [Header("Background Settings")]
    [Tooltip("List of background sprites to cycle through. Drag & drop from Inspector.")]
    public Sprite[] backgroundSprites;
    
    [Tooltip("Time in seconds before switching to the next background")]
    public float switchInterval = 10f;
    
    [Tooltip("Duration of the crossfade transition")]
    public float fadeDuration = 1.5f;

    public Image _bgImage1;
    public Image _bgImage2;
    private bool _usingImage1 = true;
    private int _currentIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (backgroundSprites == null || backgroundSprites.Length == 0)
        {
            Debug.LogWarning("BackgroundManager: No sprites assigned!");
            return;
        }

        if (_bgImage1 == null || _bgImage2 == null)
        {
            Debug.LogWarning("BackgroundManager: UI Image references not assigned in Inspector!");
            return;
        }

        // Initial setup
        _bgImage1.sprite = backgroundSprites[0];
        _bgImage1.color = Color.white;
        
        _bgImage2.color = new Color(1f, 1f, 1f, 0f);

        if (backgroundSprites.Length > 1)
        {
            StartCoroutine(CycleBackgroundsRoutine());
        }
    }
    IEnumerator CycleBackgroundsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(switchInterval);

            _currentIndex = (_currentIndex + 1) % backgroundSprites.Length;
            Sprite nextSprite = backgroundSprites[_currentIndex];

            Image currentImage = _usingImage1 ? _bgImage1 : _bgImage2;
            Image nextImage = _usingImage1 ? _bgImage2 : _bgImage1;

            nextImage.sprite = nextSprite;
            // Bring the next image slightly forward to fade it in over the current one
            nextImage.transform.SetAsLastSibling(); 

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                
                // Use a smooth ease for the fade
                float easedT = t * t * (3f - 2f * t); 
                
                nextImage.color = new Color(1f, 1f, 1f, easedT);
                yield return null;
            }
            
            nextImage.color = Color.white;
            currentImage.color = new Color(1f, 1f, 1f, 0f);

            _usingImage1 = !_usingImage1;
        }
    }
}
