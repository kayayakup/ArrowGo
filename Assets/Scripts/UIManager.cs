using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UIManager — Modern, animated UI with premium aesthetics.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public static void CreateInstance(GameObject canvasGO)
    {
        GameObject go = new GameObject("UIManager");
        Instance = go.AddComponent<UIManager>();
        Instance._canvasTransform = canvasGO.transform;
        DontDestroyOnLoad(go);
    }

    Transform _canvasTransform;

    GameObject _gameplayPanel;
    GameObject _levelSelectPanel;
    GameObject _levelCompletePanel;
    GameObject _gameOverPanel;
    GameObject _settingsPanel;
    GameObject _toastGO;

    TextMeshProUGUI _levelLabel;
    TextMeshProUGUI _moveLabel;
    Image[] _heartImages;

    // Vibrant Solid Color Palette
    static readonly Color PanelBg = new Color(0.10f, 0.12f, 0.16f, 1.0f); // Solid dark
    static readonly Color SolidBg = new Color(0.18f, 0.22f, 0.30f, 1.0f); // Replaces GlassBg
    static readonly Color AccentPrimary = new Color(0.25f, 0.55f, 1.00f, 1.0f); // Royal Blue
    static readonly Color AccentSuccess = new Color(0.20f, 0.80f, 0.40f, 1.0f); // Emerald Green
    static readonly Color AccentDanger = new Color(0.95f, 0.30f, 0.30f, 1.0f); // Vivid Red
    static readonly Color HeartFull = new Color(1.00f, 0.25f, 0.45f, 1.0f);
    static readonly Color HeartEmpty = new Color(0.30f, 0.30f, 0.35f, 1.0f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ShowGameplayUI()
    {
        StartCoroutine(SwitchPanel(_gameplayPanel, () => {
            if (_gameplayPanel == null) BuildGameplayPanel();
            _gameplayPanel.SetActive(true);
            int lvl = GameManager.Instance.CurrentLevel;
            _levelLabel.text = "Level " + (lvl + 1);
            return _gameplayPanel;
        }));
    }

    public void ShowLevelSelectScreen()
    {
        StartCoroutine(SwitchPanel(_levelSelectPanel, () => {
            if (_levelSelectPanel != null) Destroy(_levelSelectPanel);
            BuildLevelSelectPanel();
            _levelSelectPanel.SetActive(true);
            return _levelSelectPanel;
        }));
    }

    public void ShowLevelCompletePanel(int stars, int nextLevelIndex, int currentLevel)
    {
        StartCoroutine(SwitchPanel(_levelCompletePanel, () => {
            if (_levelCompletePanel != null) Destroy(_levelCompletePanel);
            BuildLevelCompletePanel(stars, nextLevelIndex);
            _levelCompletePanel.SetActive(true);
            return _levelCompletePanel;
        }, true));
    }

    public void ShowGameOverPanel()
    {
        StartCoroutine(SwitchPanel(_gameOverPanel, () => {
            if (_gameOverPanel != null) Destroy(_gameOverPanel);
            BuildGameOverPanel();
            _gameOverPanel.SetActive(true);
            return _gameOverPanel;
        }, true));
    }

    IEnumerator SwitchPanel(GameObject current, System.Func<GameObject> builder, bool overlay = false)
    {
        if (!overlay)
        {
            if (_gameplayPanel != null && _gameplayPanel.activeSelf) yield return FadePanel(_gameplayPanel, 0f, 0.15f);
            if (_levelSelectPanel != null && _levelSelectPanel.activeSelf) yield return FadePanel(_levelSelectPanel, 0f, 0.15f);
            HideAllPanels();
        }

        GameObject next = builder();
        yield return FadePanel(next, 1f, 0.25f);
    }

    IEnumerator FadePanel(GameObject go, float target, float duration)
    {
        if (go == null) yield break;
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        
        if (target > 0) go.SetActive(true);
        yield return SimpleTween.FadeTo(go, target, duration, SimpleTween.Ease.CubicOut);
        if (target <= 0) go.SetActive(false);
    }

    public void UpdateLives(int lives)
    {
        if (_heartImages == null) return;
        for (int i = 0; i < _heartImages.Length; i++)
        {
            bool full = i < lives;
            _heartImages[i].color = full ? HeartFull : HeartEmpty;
            if (!full && _heartImages[i].transform.localScale.x > 0.5f)
                StartCoroutine(SimpleTween.ScaleTo(_heartImages[i].gameObject, Vector3.one * 0.8f, 0.2f, SimpleTween.Ease.BackOut));
        }
    }

    public void UpdateMoveCount(int moves)
    {
        if (_moveLabel != null) _moveLabel.text = "MOVES: " + moves;
    }

    public void ShowToast(string msg)
    {
        if (_toastGO != null) Destroy(_toastGO);
        _toastGO = CreateContainer("Toast", _canvasTransform, new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), Vector2.zero, new Vector2(500f, 65f));
        _toastGO.AddComponent<Image>().sprite = TextureGenerator.CreateRoundedRectSprite(PanelBg);
        
        TextMeshProUGUI txt = CreateText("Text", _toastGO.transform, msg, 24, TextAlignmentOptions.Center, Color.white);
        FillRect(txt.rectTransform);
        
        CanvasGroup cg = _toastGO.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        StartCoroutine(ToastRoutine(cg));
    }

    IEnumerator ToastRoutine(CanvasGroup cg)
    {
        yield return SimpleTween.FadeTo(cg.gameObject, 1f, 0.25f, SimpleTween.Ease.CubicOut);
        yield return new WaitForSeconds(1.8f);
        yield return SimpleTween.FadeTo(cg.gameObject, 0f, 0.3f, SimpleTween.Ease.CubicOut);
        if (cg != null) Destroy(cg.gameObject);
    }

    void HideAllPanels()
    {
        if (_gameplayPanel != null) _gameplayPanel.SetActive(false);
        if (_levelSelectPanel != null) _levelSelectPanel.SetActive(false);
        if (_levelCompletePanel != null) _levelCompletePanel.SetActive(false);
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
    }

    void BuildGameplayPanel()
    {
        _gameplayPanel = CreatePanel("GameplayPanel", Color.clear);
        
        // Header
        GameObject header = CreateContainer("Header", _gameplayPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(600f, 100f));
        _levelLabel = CreateText("LevelLabel", header.transform, "LEVEL 1", 36, TextAlignmentOptions.Center, Color.white);
        _levelLabel.fontStyle = FontStyles.Bold;
        FillRect(_levelLabel.rectTransform);

        // Hearts
        GameObject heartsRow = CreateContainer("Hearts", _gameplayPanel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(250f, 60f));
        _heartImages = new Image[GameManager.MAX_LIVES];
        for (int i = 0; i < GameManager.MAX_LIVES; i++)
        {
            GameObject h = new GameObject("Heart_" + i);
            h.transform.SetParent(heartsRow.transform, false);
            RectTransform hrt = h.AddComponent<RectTransform>();
            hrt.sizeDelta = new Vector2(48f, 48f);
            hrt.anchoredPosition = new Vector2((i - 1) * 64f, 0f);
            Image img = h.AddComponent<Image>();
            img.sprite = TextureGenerator.CreateCircleSprite(HeartFull);
            _heartImages[i] = img;
        }

        // Moves
        _moveLabel = CreateText("MoveLabel", _gameplayPanel.transform, "MOVES: 0", 24, TextAlignmentOptions.Center, new Color(1, 1, 1, 0.6f));
        SetAnchors(_moveLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -210f), new Vector2(300f, 40f));

        // Bottom Nav
        CreateButton("HintBtn", _gameplayPanel.transform, "HINT", new Vector2(0.3f, 0.1f), new Vector2(0.3f, 0.1f), Vector2.zero, new Vector2(180f, 70f), SolidBg, () => HintSystem.Instance.ShowHint());
        CreateButton("MenuBtn", _gameplayPanel.transform, "MENU", new Vector2(0.7f, 0.1f), new Vector2(0.7f, 0.1f), Vector2.zero, new Vector2(180f, 70f), SolidBg, () => GameManager.Instance.ReturnToLevelSelect());
    }

    void BuildLevelSelectPanel()
    {
        _levelSelectPanel = CreatePanel("LevelSelect", PanelBg);
        
        TextMeshProUGUI title = CreateText("Title", _levelSelectPanel.transform, "ARROW GO", 72, TextAlignmentOptions.Center, Color.white);
        title.fontStyle = FontStyles.Bold | FontStyles.Italic;
        SetAnchors(title.rectTransform, new Vector2(0.5f, 0.9f), new Vector2(0.5f, 0.9f), Vector2.zero, new Vector2(800f, 100f));

        // Grid
        GameObject content = CreateScrollableGrid(_levelSelectPanel.transform, out RectTransform contentRT);
        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 0);
        for (int i = 0; i < LevelManager.Instance.LevelCount; i++)
        {
            int idx = i;
            bool isUnlocked = i <= unlocked;
            GameObject btnGO = new GameObject("Lvl_" + i);
            btnGO.transform.SetParent(contentRT, false);
            
            Image bg = btnGO.AddComponent<Image>();
            bg.sprite = TextureGenerator.CreateRoundedRectSprite(isUnlocked ? SolidBg : new Color(0.2f, 0.2f, 0.25f, 1.0f));
            
            if (isUnlocked)
            {
                Button b = btnGO.AddComponent<Button>();
                b.onClick.AddListener(() => GameManager.Instance.LoadLevel(idx));
                CreateText("Num", btnGO.transform, (i + 1).ToString(), 44, TextAlignmentOptions.Center, Color.white).rectTransform.offsetMin = Vector2.zero;
            }
            else
            {
                CreateText("Lock", btnGO.transform, "LOCKED", 18, TextAlignmentOptions.Center, new Color(1, 1, 1, 0.3f)).rectTransform.offsetMin = Vector2.zero;
            }
        }

        // Settings Button
        CreateButton("Settings", _levelSelectPanel.transform, "SETTINGS", new Vector2(0.5f, 0.08f), new Vector2(0.5f, 0.08f), Vector2.zero, new Vector2(220f, 60f), SolidBg, () => ShowSettings());
    }

    void BuildLevelCompletePanel(int stars, int nextLevelIndex)
    {
        _levelCompletePanel = CreatePanel("LevelComplete", new Color(0.1f, 0.1f, 0.15f, 1.0f)); // Solid background
        GameObject card = CreateContainer("Card", _levelCompletePanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 500f));
        card.AddComponent<Image>().sprite = TextureGenerator.CreateRoundedRectSprite(SolidBg);
        
        CreateText("Title", card.transform, "EXCELLENT!", 48, TextAlignmentOptions.Center, AccentSuccess).rectTransform.anchoredPosition = new Vector2(0, 180f);
        
        string starStr = "";
        for (int s = 0; s < 3; s++) starStr += s < stars ? "★" : "☆";
        TextMeshProUGUI st = CreateText("Stars", card.transform, starStr, 80, TextAlignmentOptions.Center, new Color(1, 0.9f, 0.2f));
        st.rectTransform.anchoredPosition = new Vector2(0, 80f);

        CreateButton("Next", card.transform, "NEXT", new Vector2(0.5f, 0.3f), new Vector2(0.5f, 0.3f), Vector2.zero, new Vector2(250f, 80f), AccentSuccess, () => GameManager.Instance.NextLevel());
        CreateButton("Menu", card.transform, "MENU", new Vector2(0.5f, 0.1f), new Vector2(0.5f, 0.1f), Vector2.zero, new Vector2(200f, 60f), Color.gray, () => GameManager.Instance.ReturnToLevelSelect());
    }

    void BuildGameOverPanel()
    {
        _gameOverPanel = CreatePanel("GameOver", new Color(0.25f, 0.05f, 0.05f, 1.0f)); // Solid dark red
        CreateText("Title", _gameOverPanel.transform, "OUT OF LIVES", 56, TextAlignmentOptions.Center, Color.white).rectTransform.anchoredPosition = new Vector2(0, 100f);
        CreateButton("Retry", _gameOverPanel.transform, "RETRY", new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(300f, 90f), AccentDanger, () => GameManager.Instance.RestartLevel());
    }

    void ShowSettings()
    {
        if (_settingsPanel != null) { Destroy(_settingsPanel); _settingsPanel = null; return; }

        _settingsPanel = CreatePanel("Settings", new Color(0.1f, 0.1f, 0.15f, 1.0f));
        GameObject card = CreateContainer("Card", _settingsPanel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 400f));
        card.AddComponent<Image>().sprite = TextureGenerator.CreateRoundedRectSprite(SolidBg);

        CreateText("Title", card.transform, "SETTINGS", 32, TextAlignmentOptions.Center, Color.white).rectTransform.anchoredPosition = new Vector2(0, 140f);

        AddSliderRow(card.transform, "Music", PlayerPrefs.GetFloat("MusicVolume", 0.8f), 40f, v => {
            AudioManager.Instance.SetMusicVolume(v);
            PlayerPrefs.SetFloat("MusicVolume", v);
        });

        AddSliderRow(card.transform, "SFX", PlayerPrefs.GetFloat("SFXVolume", 1f), -40f, v => {
            AudioManager.Instance.SetSFXVolume(v);
            PlayerPrefs.SetFloat("SFXVolume", v);
        });

        CreateButton("Close", card.transform, "DONE", new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f), Vector2.zero, new Vector2(180f, 60f), AccentSuccess, () => {
            PlayerPrefs.Save();
            Destroy(_settingsPanel);
            _settingsPanel = null;
        });

        StartCoroutine(FadePanel(_settingsPanel, 1f, 0.2f));
    }

    void AddSliderRow(Transform parent, string label, float initial, float yPos, System.Action<float> onChange)
    {
        CreateText(label, parent, label, 20, TextAlignmentOptions.Left, new Color(1, 1, 1, 0.6f)).rectTransform.anchoredPosition = new Vector2(-150f, yPos + 35f);
        
        GameObject slGO = CreateContainer(label + "_Slider", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, yPos), new Vector2(350f, 30f));
        Slider sl = slGO.AddComponent<Slider>();
        
        GameObject bg = CreateContainer("BG", slGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        bg.AddComponent<Image>().color = new Color(1, 1, 1, 0.1f);
        
        GameObject fillArea = CreateContainer("FillArea", slGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        GameObject fill = CreateContainer("Fill", fillArea.transform, Vector2.zero, new Vector2(initial, 1), Vector2.zero, Vector2.zero);
        fill.AddComponent<Image>().color = AccentPrimary;
        sl.fillRect = fill.GetComponent<RectTransform>();

        sl.minValue = 0; sl.maxValue = 1; sl.value = initial;
        sl.onValueChanged.AddListener(v => onChange(v));
    }

    // Factory Helpers
    static GameObject CreatePanel(string name, Color bgColor)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(Instance._canvasTransform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        if (bgColor.a > 0) go.AddComponent<Image>().color = bgColor;
        go.AddComponent<CanvasGroup>().alpha = 0f;
        return go;
    }

    static GameObject CreateContainer(string name, Transform parent, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = min; rt.anchorMax = max;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return go;
    }

    static TextMeshProUGUI CreateText(string name, Transform parent, string text, int size, TextAlignmentOptions align, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.alignment = align; tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    static Button CreateButton(string name, Transform parent, string label, Vector2 min, Vector2 max, Vector2 pos, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = CreateContainer(name, parent, min, max, pos, size);
        Image img = go.AddComponent<Image>();
        img.sprite = TextureGenerator.CreateRoundedRectSprite(color);
        img.type = Image.Type.Sliced;
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(() => 
        {
            AudioManager.Instance.PlaySFX("buttonClick");
            if (FXManager.Instance != null)
                FXManager.Instance.PlayButtonClick(go.transform.position);
        });
        btn.onClick.AddListener(onClick);
        
        TextMeshProUGUI txt = CreateText("Label", go.transform, label, 24, TextAlignmentOptions.Center, Color.white);
        FillRect(txt.rectTransform);
        txt.fontStyle = FontStyles.Bold;
        return btn;
    }

    static GameObject CreateScrollableGrid(Transform parent, out RectTransform content)
    {
        GameObject sv = CreateContainer("ScrollView", parent, new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.8f), Vector2.zero, Vector2.zero);
        ScrollRect sr = sv.AddComponent<ScrollRect>();
        GameObject vp = CreateContainer("Viewport", sv.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        vp.AddComponent<RectMask2D>();
        GameObject cnt = CreateContainer("Content", vp.transform, new Vector2(0, 1), new Vector2(1, 1), Vector2.zero, Vector2.zero);
        cnt.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
        GridLayoutGroup glg = cnt.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(160, 160); glg.spacing = new Vector2(20, 20); glg.padding = new RectOffset(20, 20, 20, 20);
        cnt.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.viewport = vp.GetComponent<RectTransform>();
        sr.content = cnt.GetComponent<RectTransform>();
        sr.horizontal = false;
        content = cnt.GetComponent<RectTransform>();
        return sv;
    }

    static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = min; rt.anchorMax = max; rt.anchoredPosition = pos; rt.sizeDelta = size;
    }

    static void FillRect(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}