using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AudioManager — Manages BGM and SFX channels.
/// All audio clips are assigned via Inspector (drag & drop).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ── Inspector: Background Music ─────────────────────────────
    [Header("Background Music")]
    public AudioClip bgMusicClip;

    // ── Inspector: SFX Clips ────────────────────────────────────
    [Header("SFX — Arrow Actions")]
    [Tooltip("Played when an arrow is tapped")]
    public AudioClip tapClip;

    [Tooltip("Played when an arrow slides out successfully")]
    public AudioClip slideClip;

    [Tooltip("Played on a successful move")]
    public AudioClip successClip;

    [Header("SFX — Negative Feedback")]
    [Tooltip("Played on collision / failed move")]
    public AudioClip failClip;

    [Tooltip("Played when a life is lost")]
    public AudioClip lifeLostClip;

    [Header("SFX — Game Events")]
    [Tooltip("Played when level is completed")]
    public AudioClip levelCompleteClip;

    [Tooltip("Played on button click")]
    public AudioClip buttonClickClip;

    [Tooltip("Played on game over")]
    public AudioClip gameOverClip;

    [Tooltip("Played when hint is shown")]
    public AudioClip hintClip;

    // ── Internal ────────────────────────────────────────────────
    AudioSource _bgmSource;
    AudioSource _sfxSource;

    float _musicVolume = 0.8f;
    float _sfxVolume = 1.0f;

    Dictionary<string, AudioClip> _clips = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.volume = _musicVolume;
        _bgmSource.clip = bgMusicClip;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.volume = _sfxVolume;

        // Map clip names to Inspector-assigned clips
        RegisterClip("tap", tapClip);
        RegisterClip("slide", slideClip);
        RegisterClip("success", successClip);
        RegisterClip("fail", failClip);
        RegisterClip("lifeLost", lifeLostClip);
        RegisterClip("levelComplete", levelCompleteClip);
        RegisterClip("buttonClick", buttonClickClip);
        RegisterClip("gameOver", gameOverClip);
        RegisterClip("hint", hintClip);
    }

    void RegisterClip(string name, AudioClip clip)
    {
        if (clip != null) _clips[name] = clip;
    }

    public void PlayMusic()
    {
        if (_bgmSource != null && _bgmSource.clip != null && !_bgmSource.isPlaying)
            _bgmSource.Play();
    }

    public void StopMusic() => _bgmSource?.Stop();

    public void PlaySFX(string clipName)
    {
        if (_sfxSource == null) return;
        _sfxSource.pitch = 1f + Random.Range(-0.08f, 0.08f);
        if (_clips.TryGetValue(clipName, out AudioClip clip))
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        else
            Debug.LogWarning("Missing SFX clip: " + clipName);
    }

    public void SetMusicVolume(float vol)
    {
        _musicVolume = Mathf.Clamp01(vol);
        if (_bgmSource != null) _bgmSource.volume = _musicVolume;
    }

    public void SetSFXVolume(float vol)
    {
        _sfxVolume = Mathf.Clamp01(vol);
        if (_sfxSource != null) _sfxSource.volume = _sfxVolume;
    }

}