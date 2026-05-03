using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Bootstrap — Attach ONLY this script to an empty GameObject in an empty scene.
/// Everything else is created programmatically at runtime.
/// 
/// For AudioManager and FXManager, assign prefabs or scene references
/// via the Inspector fields below. If left empty, empty instances are created.
/// </summary>
public class Bootstrap : MonoBehaviour
{
    public static Bootstrap Instance { get; private set; }

    [Header("Manager Prefabs (Optional — drag & drop)")]
    [Tooltip("Assign an AudioManager prefab with audio clips configured in Inspector")]
    public AudioManager audioManagerPrefab;

    [Tooltip("Assign an FXManager prefab with particle prefabs configured in Inspector")]
    public FXManager fxManagerPrefab;


    [Header("Game Prefabs")]
    [Tooltip("Beyaz renkli, sağa bakan standart Arrow prefab'ı")]
    public Arrow arrowPrefab;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Prevent this object from being destroyed on scene load
        DontDestroyOnLoad(gameObject);

        // ── 1. Camera ──────────────────────────────────────────
        SetupCamera();

        // ── 2. EventSystem (required for UI input) ────────────
        SetupEventSystem();

        // ── 3. Canvas ─────────────────────────────────────────
        GameObject canvasGO = SetupCanvas();

        // ── 4. Singleton Managers ──────────────────────────────
        //SetupAudioManager();
        //SetupFXManager();
        LevelManager.CreateInstance();
        GameManager.CreateInstance();
        GridManager.CreateInstance();
        HintSystem.CreateInstance();
        UIManager.CreateInstance(canvasGO);

        // ── 5. Input Handler ───────────────────────────────────
        // (Replaced by Physics2DRaycaster and IPointerDownHandler on Arrow)

        // ── 6. Start the game ──────────────────────────────────
        GameManager.Instance.StartGame();
    }

    void SetupAudioManager()
    {
        if (audioManagerPrefab != null)
        {
            // Instantiate from prefab (has Inspector-configured clips)
            AudioManager instance = Instantiate(audioManagerPrefab);
            instance.name = "AudioManager";
            DontDestroyOnLoad(instance.gameObject);
        }
        else
        {
            // Fallback: create empty AudioManager (will use synth tones)
            GameObject go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
            DontDestroyOnLoad(go);
        }
    }

    void SetupFXManager()
    {
        if (fxManagerPrefab != null)
        {
            // Instantiate from prefab (has Inspector-configured particle prefabs)
            FXManager instance = Instantiate(fxManagerPrefab);
            instance.name = "FXManager";
            DontDestroyOnLoad(instance.gameObject);
        }
        else
        {
            // Fallback: create empty FXManager (no particles, but won't crash)
            GameObject go = new GameObject("FXManager");
            go.AddComponent<FXManager>();
            DontDestroyOnLoad(go);
        }
    }


    static void SetupCamera()
    {
        GameObject camGO = GameObject.FindGameObjectWithTag("MainCamera");
        Camera cam = camGO.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        camGO.AddComponent<AudioListener>();
        camGO.AddComponent<Physics2DRaycaster>(); // Enables IPointerDownHandler for 2D colliders
    }

    static void SetupEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        
        GameObject esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        
        // Use the appropriate module depending on which input system is active
        // For compatibility with projects using the New Input System package:
        esGO.AddComponent<StandaloneInputModule>();
    }

    static GameObject SetupCanvas()
    {
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 2400);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        return canvasGO;
    }
}