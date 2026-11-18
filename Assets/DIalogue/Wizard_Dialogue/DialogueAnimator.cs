using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class DialogueAnimator : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private AudioSource audioSource;

    [Header("Audio Clips")]
    public AudioClip reflectionAudioClip;

    void Awake()
    {
        // Automatically grab the components on this object
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAnimation(string animTriggerName)
    {
        if (animator != null)
        {
            // We use Triggers because state machines handle them cleanly
            animator.SetTrigger(animTriggerName); 
        }
        else
        {
            Debug.LogWarning("Animator is missing on " + gameObject.name);
        }
    }

    public void PlayAudio(string audioName)
    {
        if (audioSource == null) return;

        // In a real game you might have a dictionary of clips, 
        // but for this specific sequence, we just need the reflect audio.
        if (audioName == "Reflect_Audio" && reflectionAudioClip != null)
        {
            audioSource.clip = reflectionAudioClip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Audio clip not assigned for: " + audioName);
        }
    }
}