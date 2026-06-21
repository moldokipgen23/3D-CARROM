using UnityEngine;
using System.Collections;

public class VictoryEffect : MonoBehaviour
{
    [Header("Particle Systems")]
    public ParticleSystem confettiParticles;
    public ParticleSystem fireworksParticles;
    public ParticleSystem sparkleRing;

    [Header("Audio")]
    public AudioClip victorySound;
    public AudioClip fireworksSound;

    [Header("Visual")]
    public Light victoryLight;
    public float lightIntensity = 3f;
    public float lightDuration = 2f;

    [Header("Animation")]
    public Animator victoryAnimator;
    public float effectDelay = 0.5f;

    [Header("References")]
    public AudioSource audioSource;

    private void Start()
    {
        if (victoryLight != null)
        {
            victoryLight.enabled = false;
        }
    }

    public void PlayVictoryEffect(bool isDraw = false)
    {
        StartCoroutine(VictorySequence(isDraw));
    }

    private IEnumerator VictorySequence(bool isDraw)
    {
        yield return new WaitForSeconds(effectDelay);

        if (!isDraw)
        {
            PlayConfetti();
            PlayFireworks();
            PlayVictorySound();
            StartCoroutine(PulseLight());
        }
        else
        {
            PlayDrawEffect();
        }

        if (victoryAnimator != null)
        {
            victoryAnimator.SetTrigger("Victory");
        }
    }

    private void PlayConfetti()
    {
        if (confettiParticles != null)
        {
            confettiParticles.Play();
        }
    }

    private void PlayFireworks()
    {
        if (fireworksParticles != null)
        {
            fireworksParticles.Play();
        }

        if (sparkleRing != null)
        {
            sparkleRing.Play();
        }
    }

    private void PlayVictorySound()
    {
        if (audioSource == null) return;

        if (victorySound != null)
        {
            audioSource.PlayOneShot(victorySound);
        }

        if (fireworksSound != null)
        {
            StartCoroutine(PlayDelayedSound(fireworksSound, 0.5f));
        }
    }

    private IEnumerator PlayDelayedSound(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator PulseLight()
    {
        if (victoryLight == null) yield break;

        victoryLight.enabled = true;
        float elapsed = 0f;

        while (elapsed < lightDuration)
        {
            float t = elapsed / lightDuration;
            victoryLight.intensity = Mathf.Lerp(0f, lightIntensity, Mathf.Sin(t * Mathf.PI));
            elapsed += Time.deltaTime;
            yield return null;
        }

        victoryLight.enabled = false;
    }

    private void PlayDrawEffect()
    {
        if (confettiParticles != null)
        {
            var main = confettiParticles.main;
            main.startColor = Color.yellow;
            confettiParticles.Play();
        }

        if (audioSource != null && victorySound != null)
        {
            audioSource.PlayOneShot(victorySound);
        }
    }

    public void StopAllEffects()
    {
        StopAllCoroutines();

        if (confettiParticles != null && confettiParticles.isPlaying)
            confettiParticles.Stop();

        if (fireworksParticles != null && fireworksParticles.isPlaying)
            fireworksParticles.Stop();

        if (sparkleRing != null && sparkleRing.isPlaying)
            sparkleRing.Stop();

        if (victoryLight != null)
            victoryLight.enabled = false;

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }
}
