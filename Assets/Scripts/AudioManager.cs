using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AudioManager — Manages BGM and SFX channels with procedural synthesis fallback.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public static void CreateInstance()
    {
        GameObject go = new GameObject("AudioManager");
        Instance = go.AddComponent<AudioManager>();
        DontDestroyOnLoad(go);
    }

    AudioSource _bgmSource;
    AudioSource _sfxSource;

    float _musicVolume = 0.8f;
    float _sfxVolume = 1.0f;

    static readonly string[] ClipNames = { "tap", "slide", "success", "fail", "levelComplete", "buttonClick", "lifeLost" };
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
        _bgmSource.clip = Resources.Load<AudioClip>("Audio/bgMusic");

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.volume = _sfxVolume;

        foreach (string name in ClipNames)
        {
            AudioClip clip = Resources.Load<AudioClip>("Audio/" + name);
            if (clip != null) _clips[name] = clip;
        }
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
            StartCoroutine(PlaySynthTone(clipName));
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

    IEnumerator PlaySynthTone(string clipName)
    {
        float freq = GetSynthFreq(clipName);
        float duration = GetSynthDuration(clipName);
        AudioClip synth = SynthesiseTone(freq, duration, clipName);
        _sfxSource.PlayOneShot(synth, _sfxVolume * 0.4f);
        yield return new WaitForSeconds(duration + 0.1f);
        Destroy(synth);
    }

    static float GetSynthFreq(string name) => name switch
    {
        "tap" => 800f,
        "slide" => 400f,
        "success" => 1200f,
        "fail" => 150f,
        "levelComplete" => 900f,
        "buttonClick" => 600f,
        "lifeLost" => 250f,
        _ => 440f
    };

    static float GetSynthDuration(string name) => name switch
    {
        "levelComplete" => 0.8f, "slide" => 0.3f, "fail" => 0.5f, "lifeLost" => 0.4f, _ => 0.1f
    };

    static AudioClip SynthesiseTone(float frequency, float duration, string name)
    {
        int sampleRate = 44100;
        int samples = Mathf.CeilToInt(sampleRate * duration);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = 1f - (t / duration);
            float val = Mathf.Sin(2f * Mathf.PI * frequency * t) * env;
            if (name == "slide") val = Mathf.Sin(2f * Mathf.PI * (frequency * (1f + t * 2f)) * t) * env;
            data[i] = val * 0.3f;
        }
        AudioClip clip = AudioClip.Create("Synth_" + name, samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}