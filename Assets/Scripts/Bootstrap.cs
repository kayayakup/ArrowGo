using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Bootstrap — Attach ONLY this script to an empty GameObject in an empty scene.
/// Everything else is created programmatically at runtime.
public class Bootstrap : MonoBehaviour
{
    static Bootstrap _instance;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // Prevent this object from being destroyed on scene load
        DontDestroyOnLoad(gameObject);

        // ── 1. Camera ──────────────────────────────────────────
        SetupCamera();

        // ── 2. EventSystem (required for UI input) ────────────
        SetupEventSystem();

        // ── 3. Canvas ─────────────────────────────────────────
        GameObject canvasGO = SetupCanvas();

        // ── 4. Singleton Managers ──────────────────────────────
        AudioManager.CreateInstance();
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

    static void SetupCamera()
    {
        GameObject camGO = new GameObject("MainCamera");
        camGO.tag = "MainCamera";
        Camera cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        // Soft pastel background
        cam.backgroundColor = new Color(0.94f, 0.94f, 0.98f);
        cam.nearClipPlane = -10f;
        cam.farClipPlane = 10f;
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
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        return canvasGO;
    }
}