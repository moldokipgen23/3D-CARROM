using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Central audio hub. It bootstraps itself before the first scene, synthesises
/// every clip at runtime and survives scene changes. Imported clips can still
/// be slotted into the public fields later and will take priority.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;

    [Header("Sound Effects")]
    public AudioClip[] coinHitSounds;
    public AudioClip[] boardHitSounds;
    public AudioClip[] pocketSounds;
    public AudioClip strikerShootSound;
    public AudioClip buttonClickSound;
    public AudioClip foulSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;

    [Header("Settings")]
    public int sfxPoolSize = 10;
    public float musicFadeDuration = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.16f;

    private AudioSource[] _sources;
    private int _nextSource;
    private float _sfxVolume = 1f;
    private float _musicVolume = 0.16f;
    private bool _isMusicEnabled = true;
    private bool _isSFXEnabled = true;
    private bool _muted;
    private float _lastCoinTime;
    private float _lastBoardTime;

    private const string MutePrefKey = "carrom_sound_muted";

    public static AudioManager Instance { get; private set; }
    public bool IsMuted { get { return _muted; } }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        // A surviving instance (or one already placed in a scene) wins. Do not
        // cache a "bootstrapped" flag on the static, or re-entering play mode
        // with domain reload disabled would leave the game silent.
        if (Instance != null) return;
        if (FindObjectOfType<AudioManager>() != null) return;

        GameObject go = new GameObject("[Audio]");
        go.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _muted = PlayerPrefs.GetInt(MutePrefKey, 0) == 1;
        ApplyMute();

        InitializePool();
        BuildProceduralClips();
        StartAmbient();
    }

    private void InitializePool()
    {
        int count = Mathf.Max(2, sfxPoolSize);
        _sources = new AudioSource[count];

        for (int i = 0; i < count; i++)
        {
            GameObject sfxObj = new GameObject($"SFX_Source_{i}");
            sfxObj.transform.SetParent(transform, false);

            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 1f;
            _sources[i] = source;
        }
    }

    /// <summary>Fills any empty clip slots with runtime-synthesised sounds.</summary>
    private void BuildProceduralClips()
    {
        if (coinHitSounds == null || coinHitSounds.Length == 0)
            coinHitSounds = new[] { ProceduralAudio.CoinClack() };

        if (boardHitSounds == null || boardHitSounds.Length == 0)
            boardHitSounds = new[] { ProceduralAudio.RailThud() };

        if (pocketSounds == null || pocketSounds.Length == 0)
            pocketSounds = new[] { ProceduralAudio.PocketDrop() };

        if (strikerShootSound == null) strikerShootSound = ProceduralAudio.Strike();
        if (buttonClickSound == null) buttonClickSound = ProceduralAudio.UiClick();
        if (foulSound == null) foulSound = ProceduralAudio.FoulBuzz();
        if (victorySound == null) victorySound = ProceduralAudio.VictorySting();
        if (defeatSound == null) defeatSound = ProceduralAudio.DefeatSting();
    }

    private void StartAmbient()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform, false);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
        }

        _musicVolume = Mathf.Clamp01(musicVolume);

        AudioClip ambient = menuMusic != null ? menuMusic : ProceduralAudio.AmbientLoop();
        musicSource.clip = ambient;
        musicSource.loop = true;
        musicSource.volume = _musicVolume;
        if (ambient != null && _isMusicEnabled && !_muted)
            musicSource.Play();
    }

    // ------------------------------------------------------------------ music

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (!_isMusicEnabled || _muted || musicSource == null || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        StartCoroutine(CrossfadeMusic(clip, loop));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, bool loop)
    {
        if (musicSource.isPlaying)
        {
            float startVol = musicSource.volume;
            for (float t = 0; t < musicFadeDuration; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / musicFadeDuration);
                yield return null;
            }
        }

        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        for (float t = 0; t < musicFadeDuration; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, _musicVolume, t / musicFadeDuration);
            yield return null;
        }

        musicSource.volume = _musicVolume;
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();
    }

    // -------------------------------------------------------------------- sfx

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (_muted || !_isSFXEnabled || clip == null) return;

        AudioSource source = NextSource();
        if (source == null) return;

        source.Stop();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume) * _sfxVolume;
        source.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
        source.Play();
    }

    public void PlayRandomSFX(AudioClip[] clips, float volume = 1f)
    {
        if (clips == null || clips.Length == 0) return;
        PlaySFX(clips[UnityEngine.Random.Range(0, clips.Length)], volume);
    }

    public void PlayCoinHit()
    {
        PlayCoinHit(1f);
    }

    /// <summary>Coin contact; intensity is the impact speed in m/s.</summary>
    public void PlayCoinHit(float intensity)
    {
        if (coinHitSounds == null || coinHitSounds.Length == 0) return;

        // A break generates dozens of contacts in the same frame. Merge them
        // into one slightly randomised clack so the mix stays musical.
        if (Time.unscaledTime - _lastCoinTime < 0.022f) return;
        _lastCoinTime = Time.unscaledTime;

        float volume = Mathf.Clamp(0.10f + Mathf.Abs(intensity) * 0.10f, 0.10f, 0.62f);
        float pitch = UnityEngine.Random.Range(0.92f, 1.14f);
        PlaySFX(coinHitSounds[UnityEngine.Random.Range(0, coinHitSounds.Length)], volume, pitch);
    }

    public void PlayBoardHit()
    {
        PlayBoardHit(1f);
    }

    public void PlayBoardHit(float intensity)
    {
        if (boardHitSounds == null || boardHitSounds.Length == 0) return;
        if (Time.unscaledTime - _lastBoardTime < 0.03f) return;
        _lastBoardTime = Time.unscaledTime;

        float volume = Mathf.Clamp(0.16f + Mathf.Abs(intensity) * 0.07f, 0.16f, 0.55f);
        PlaySFX(boardHitSounds[UnityEngine.Random.Range(0, boardHitSounds.Length)],
            volume, UnityEngine.Random.Range(0.94f, 1.06f));
    }

    public void PlayPocketSound()
    {
        PlayRandomSFX(pocketSounds, 0.70f);
    }

    public void PlayStrikerShoot()
    {
        PlaySFX(strikerShootSound, 0.55f, UnityEngine.Random.Range(0.96f, 1.05f));
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound, 0.45f);
    }

    public void PlayFoulSound()
    {
        PlaySFX(foulSound, 0.70f);
    }

    public void PlayVictory()
    {
        PlaySFX(victorySound, 0.80f);
    }

    public void PlayDefeat()
    {
        PlaySFX(defeatSound, 0.75f);
    }

    private AudioSource NextSource()
    {
        if (_sources == null || _sources.Length == 0) return null;

        for (int i = 0; i < _sources.Length; i++)
        {
            int index = (_nextSource + i) % _sources.Length;
            if (!_sources[index].isPlaying)
            {
                _nextSource = (index + 1) % _sources.Length;
                return _sources[index];
            }
        }

        AudioSource fallback = _sources[_nextSource];
        _nextSource = (_nextSource + 1) % _sources.Length;
        return fallback;
    }

    // Kept for backwards compatibility with older callers.
    private AudioSource GetAvailableSource()
    {
        return NextSource();
    }

    public void ReturnSource(AudioSource source)
    {
        if (source != null) source.Stop();
    }

    // ---------------------------------------------------------------- settings

    public bool ToggleMuted()
    {
        _muted = !_muted;
        PlayerPrefs.SetInt(MutePrefKey, _muted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMute();
        return _muted;
    }

    public void SetMuted(bool muted)
    {
        _muted = muted;
        PlayerPrefs.SetInt(MutePrefKey, _muted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMute();
    }

    private void ApplyMute()
    {
        AudioListener.volume = _muted ? 0f : 1f;

        if (musicSource != null)
        {
            if (_muted)
                musicSource.Pause();
            else if (_isMusicEnabled && musicSource.clip != null)
                musicSource.UnPause();
        }
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = _musicVolume;
    }

    public void ToggleMusic(bool enabled)
    {
        _isMusicEnabled = enabled;
        if (!enabled) StopMusic();
    }

    public void ToggleSFX(bool enabled)
    {
        _isSFXEnabled = enabled;
    }

    public float GetSFXVolume() { return _sfxVolume; }
    public float GetMusicVolume() { return _musicVolume; }
    public bool IsMusicEnabled() { return _isMusicEnabled; }
    public bool IsSFXEnabled() { return _isSFXEnabled; }
}
