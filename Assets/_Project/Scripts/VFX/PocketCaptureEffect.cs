using UnityEngine;
using System.Collections.Generic;

public class PocketCaptureEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    public ParticleSystem captureParticles;
    public ParticleSystem sparkleParticles;
    public float effectDuration = 1.5f;

    [Header("Audio")]
    public AudioClip captureSound;
    public AudioClip sparkleSound;

    [Header("References")]
    public AudioSource audioSource;

    private Dictionary<CoinType, ParticleSystem> _coinParticles = new Dictionary<CoinType, ParticleSystem>();

    private void Start()
    {
        InitializeParticles();
    }

    private void InitializeParticles()
    {
        if (captureParticles != null)
        {
            var main = captureParticles.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
        }
    }

    public void PlayCaptureEffect(CoinType coinType, Vector3 position)
    {
        transform.position = position;

        ParticleSystem particlesToPlay = GetParticlesForCoinType(coinType);
        if (particlesToPlay != null)
        {
            SetParticleColor(particlesToPlay, coinType);
            particlesToPlay.Play();
        }

        if (captureParticles != null)
        {
            SetParticleColor(captureParticles, coinType);
            captureParticles.Play();
        }

        if (sparkleParticles != null)
        {
            sparkleParticles.Play();
        }

        PlayCaptureSound(coinType);

        Debug.Log($"Capture effect played for {coinType} at {position}");
    }

    private ParticleSystem GetParticlesForCoinType(CoinType coinType)
    {
        if (_coinParticles.TryGetValue(coinType, out ParticleSystem particles))
        {
            return particles;
        }
        return captureParticles;
    }

    private void SetParticleColor(ParticleSystem particles, CoinType coinType)
    {
        Color color = coinType switch
        {
            CoinType.White => Color.white,
            CoinType.Black => Color.black,
            CoinType.Queen => Color.red,
            _ => Color.white
        };

        var main = particles.main;
        main.startColor = color;
    }

    private void PlayCaptureSound(CoinType coinType)
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = coinType == CoinType.Queen ? sparkleSound : captureSound;
        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    public void StopAllEffects()
    {
        if (captureParticles != null && captureParticles.isPlaying)
        {
            captureParticles.Stop();
        }

        if (sparkleParticles != null && sparkleParticles.isPlaying)
        {
            sparkleParticles.Stop();
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
