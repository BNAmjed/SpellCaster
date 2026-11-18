using UnityEngine;
using System.Collections;

public class CampfireController : MonoBehaviour
{
    [Header("Visual Effects")]
    public ParticleSystem fireParticles; // Changed to ParticleSystem for smoother stopping
    public Light fireLight;
    public float maxLightIntensity = 1.5f;

    [Header("Audio")]
    public AudioSource fireAudio;
    public float maxAudioVolume = 0.7f;

    [Header("Smoothness Tuning")]
    public float fadeSpeed = 2f; 
    public bool isLit = false;

    private Coroutine fadeRoutine;

    private void Start()
    {
        // Initialize visuals to off
        if (fireParticles != null) fireParticles.Stop();
        if (fireLight != null) fireLight.intensity = 0;
        if (fireAudio != null) { fireAudio.volume = 0; fireAudio.Stop(); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fireball") && !isLit)
        {
            Ignite();
        }
        else if (other.CompareTag("Ice") && isLit)
        {
            Extinguish();
        }
    }

    public void Ignite()
    {
        isLit = true;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeFire(true));
        
        WizardEncounterManager.Instance?.SetCampfireState(true);
    }

    public void Extinguish()
    {
        isLit = false;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeFire(false));

        WizardEncounterManager.Instance?.SetCampfireState(false);
    }

    private IEnumerator FadeFire(bool turningOn)
    {
        float targetIntensity = turningOn ? maxLightIntensity : 0f;
        float targetVolume = turningOn ? maxAudioVolume : 0f;

        if (turningOn)
        {
            if (fireParticles != null) fireParticles.Play();
            if (fireAudio != null) fireAudio.Play();
        }
        else
        {
            // Stop emitting new particles, but let existing ones fly away naturally
            if (fireParticles != null) fireParticles.Stop();
        }

        // Smoothly lerp the light and sound
        while (Mathf.Abs(fireLight.intensity - targetIntensity) > 0.01f)
        {
            fireLight.intensity = Mathf.MoveTowards(fireLight.intensity, targetIntensity, Time.deltaTime * fadeSpeed);
            if (fireAudio != null)
                fireAudio.volume = Mathf.MoveTowards(fireAudio.volume, targetVolume, Time.deltaTime * fadeSpeed);
            
            yield return null;
        }

        if (!turningOn && fireAudio != null) fireAudio.Stop();
    }

    // Optional: Simple Flicker Effect
    void Update()
    {
        if (isLit && fireLight != null)
        {
            fireLight.intensity = maxLightIntensity + Mathf.PingPong(Time.time * 2, 0.3f);
        }
    }
}