using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GiantSkeletonChaser : MonoBehaviour
{
    [Header("Chase Settings")]
    public float chaseSpeed = 8f; 
    
    [Header("Components")]
    public Animator skeletonAnimator;

    [Header("Intro Scream Setup")]
    public AudioSource vocalAudioSource; 
    public AudioClip screamClip;
    public string screamAnimTrigger = "Scream";
    
    [Tooltip("Total time he stands still before running")]
    public float screamDuration = 2.5f;
    
    [Tooltip("How many seconds to wait before playing the sound effect?")]
    public float screamSoundDelay = 0.5f; // 🔥 JUST DELAYS THE AUDIO 🔥

    [Header("Footstep Audio")]
    public AudioSource footstepAudioSource;
    public AudioClip footstepClip;
    public float timeBetweenSteps = 0.5f;
    private float stepTimer = 0f;

    private bool isChasing = false;
    private bool sequenceStarted = false;

    void Start()
    {
        // Triggers the exact second the scene loads
        StartIntroSequence();
    }

    // We keep this here so IntroSequence.cs doesn't break!
    public void BeginChase()
    {
        StartIntroSequence();
    }

    private void StartIntroSequence()
    {
        if (!sequenceStarted)
        {
            sequenceStarted = true;
            StartCoroutine(GameStartSequence());
        }
    }

    IEnumerator GameStartSequence()
    {
        // 1. INSTANTLY trigger the scream animation
        if (skeletonAnimator != null) 
            skeletonAnimator.SetTrigger(screamAnimTrigger);

        // 2. Wait for the audio delay (if you set one)
        if (screamSoundDelay > 0f)
            yield return new WaitForSeconds(screamSoundDelay);
        
        // 3. Play the scream sound exactly when you want it
        if (vocalAudioSource != null && screamClip != null)
            vocalAudioSource.PlayOneShot(screamClip);

        // 4. Wait for the REST of the animation to finish
        // (We subtract the delay so the total stand-still time is always exactly screamDuration)
        float remainingTime = screamDuration - screamSoundDelay;
        if (remainingTime > 0f)
            yield return new WaitForSeconds(remainingTime);

        // 5. Start the chase and trigger the run animation!
        isChasing = true;
        
        if (skeletonAnimator != null) 
            skeletonAnimator.SetTrigger("StartRunning");
    }

    void Update()
    {
        if (isChasing)
        {
            // Push the skeleton straight forward
            transform.Translate(Vector3.forward * chaseSpeed * Time.deltaTime * (-1));

            // Run the footstep timer loop
            HandleFootsteps();
        }
    }

    void HandleFootsteps()
    {
        if (footstepAudioSource == null || footstepClip == null) return;

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            footstepAudioSource.PlayOneShot(footstepClip);
            stepTimer = timeBetweenSteps; 
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isChasing && other.CompareTag("Player"))
        {
            isChasing = false;
            
            if (footstepAudioSource != null) footstepAudioSource.Stop();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}