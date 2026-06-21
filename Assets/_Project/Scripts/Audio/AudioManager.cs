using UnityEngine;
using System;
using System.Collections.Generic;

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

    [Header("Settings")]
    public int sfxPoolSize = 10;
    public float musicFadeDuration = 1f;

    private Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
    private float _sfxVolume = 1f;
    private float _musicVolume = 1f;
    private bool _isMusicEnabled = true;
    private bool _isSFXEnabled = true;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject sfxObj = new GameObject($"SFX_Source_{i}");
            sfxObj.transform.parent = transform;
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _sfxPool.Enqueue(source);
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (!_isMusicEnabled || musicSource == null || clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        StartCoroutine(CrossfadeMusic(clip, loop));
    }

    private System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip, bool loop)
    {
        if (musicSource.isPlaying)
        {
            float startVol = musicSource.volume;
            for (float t = 0; t < musicFadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / musicFadeDuration);
                yield return null;
            }
        }

        musicSource.clip = newClip;
        musicSource.loop = loop;
        musicSource.Play();

        for (float t = 0; t < musicFadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, _musicVolume, t / musicFadeDuration);
            yield return null;
        }

        musicSource.volume = _musicVolume;
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (!_isSFXEnabled || clip == null) return;

        AudioSource source = GetAvailableSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume * _sfxVolume;
            source.pitch = pitch;
            source.Play();
        }
    }

    public void PlayRandomSFX(AudioClip[] clips, float volume = 1f)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        PlaySFX(randomClip, volume);
    }

    public void PlayCoinHit()
    {
        PlayRandomSFX(coinHitSounds);
    }

    public void PlayBoardHit()
    {
        PlayRandomSFX(boardHitSounds);
    }

    public void PlayPocketSound()
    {
        PlayRandomSFX(pocketSounds);
    }

    public void PlayStrikerShoot()
    {
        PlaySFX(strikerShootSound);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound, 0.7f);
    }

    public void PlayFoulSound()
    {
        PlaySFX(foulSound, 0.8f);
    }

    private AudioSource GetAvailableSource()
    {
        if (_sfxPool.Count > 0)
        {
            return _sfxPool.Dequeue();
        }

        GameObject sfxObj = new GameObject("SFX_Source_Extra");
        sfxObj.transform.parent = transform;
        AudioSource source = sfxObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        return source;
    }

    public void ReturnSource(AudioSource source)
    {
        if (source != null)
        {
            source.Stop();
            _sfxPool.Enqueue(source);
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
        {
            musicSource.volume = _musicVolume;
        }
    }

    public void ToggleMusic(bool enabled)
    {
        _isMusicEnabled = enabled;
        if (!enabled)
        {
            StopMusic();
        }
    }

    public void ToggleSFX(bool enabled)
    {
        _isSFXEnabled = enabled;
    }

    public float GetSFXVolume() => _sfxVolume;
    public float GetMusicVolume() => _musicVolume;
    public bool IsMusicEnabled() => _isMusicEnabled;
    public bool IsSFXEnabled() => _isSFXEnabled;
}
