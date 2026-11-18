using System.Collections;
using UnityEngine;

public class DanceChallengeManager : MonoBehaviour
{
    public static DanceChallengeManager Instance;

    [Header("UI & Visuals")]
    public GameObject danceSubTextUI; 
    public ParticleSystem successParticles;

    [Header("Audio Sources")]
    public AudioSource loopingAudioSource;  // The spin sound
    public AudioSource oneShotAudioSource;  // Jumps and Victory

    [Header("Audio Clips")]
    public AudioClip spinLoopSFX;
    public AudioClip jumpSFX;
    public AudioClip victorySFX;

    [Header("Dance Tuning")]
    public float requiredHoldTime = 3f;
    public float movementGracePeriod = 1f; 

    [Header("Pitch Settings")]
    public float minPitch = 0.8f;
    public float maxPitch = 2.0f;
    
    private bool isChallengeActive = false;
    private float currentHoldTimer = 0f;
    private float mouseGraceTimer = 0f;
    private float lastFrameYRotation;
    private bool isJumping = false;

    private void Awake() 
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartDanceChallenge()
    {
        if (danceSubTextUI != null) danceSubTextUI.SetActive(true);
        lastFrameYRotation = Camera.main.transform.eulerAngles.y;
        
        currentHoldTimer = 0f;
        mouseGraceTimer = movementGracePeriod; 
        isChallengeActive = true;

        if (loopingAudioSource != null && spinLoopSFX != null)
        {
            loopingAudioSource.clip = spinLoopSFX;
            loopingAudioSource.loop = true;
            loopingAudioSource.pitch = minPitch;
        }
    }

    private void Update()
    {
        if (!isChallengeActive) return;

        
        if (!isChallengeActive) return;

        // 1. Detection
        float currentYRot = Camera.main.transform.eulerAngles.y;
        float deltaRot = Mathf.Abs(Mathf.DeltaAngle(lastFrameYRotation, currentYRot));
        lastFrameYRotation = currentYRot;

        if (deltaRot > 0.1f) mouseGraceTimer = movementGracePeriod;
        else mouseGraceTimer -= Time.deltaTime;

        if (Input.GetButtonDown("Jump") && oneShotAudioSource != null && jumpSFX != null)
            oneShotAudioSource.PlayOneShot(jumpSFX, 0.7f);

        isJumping = Input.GetButton("Jump");

        // 2. Calculation
        bool hasMouseMovement = mouseGraceTimer > 0f;
        if (hasMouseMovement && isJumping)
        {
            currentHoldTimer += Time.deltaTime;
            if (loopingAudioSource != null && !loopingAudioSource.isPlaying) loopingAudioSource.Play();
        }
        else if (!hasMouseMovement)
        {
            currentHoldTimer = 0f;
        }

        currentHoldTimer = Mathf.Clamp(currentHoldTimer, 0f, requiredHoldTime);

        // 3. Audio Pitching
        if (loopingAudioSource != null)
        {
            if (currentHoldTimer > 0)
            {
                float progress = currentHoldTimer / requiredHoldTime;
                loopingAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, progress);
            }
            else if (loopingAudioSource.isPlaying)
            {
                loopingAudioSource.Pause(); 
            }
        }

        if (currentHoldTimer >= requiredHoldTime) TriggerSuccess();
    }

    private void TriggerSuccess()
    {
        isChallengeActive = false;
        
        // Kill the looping sound immediately
        if (loopingAudioSource != null) 
        {
            loopingAudioSource.Stop();
            loopingAudioSource.loop = false;
        }

        if (danceSubTextUI != null) danceSubTextUI.SetActive(false);
        StartCoroutine(SuccessSequenceRoutine());
    }

    private IEnumerator SuccessSequenceRoutine()
    {
        if (victorySFX != null && oneShotAudioSource != null) 
            oneShotAudioSource.PlayOneShot(victorySFX, 1f);
        
        if (successParticles != null) successParticles.Play();

        yield return new WaitForSeconds(0.6f); // Short beat for victory sound

        if (WizardEncounterManager.Instance != null)
            WizardEncounterManager.Instance.StartPasswordDialogueDirectly();
    }
}