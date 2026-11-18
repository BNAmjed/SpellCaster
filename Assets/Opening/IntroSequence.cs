using UnityEngine;
using System.Collections;

public class SeamlessIntroManager : MonoBehaviour
{
    [Header("The Player")]
    [Tooltip("The Animator component on your Player Camera")]
    public Animator playerCameraAnimator;
    [Tooltip("Your player's movement/look script (e.g., FirstPersonController)")]
    public MonoBehaviour playerControlScript;

    [Header("The Monster")]
    public Animator skeletonAnimator;
    public string screamAnimationName = "Scream"; 
    public GiantSkeletonChaser skeletonAI;

    [Header("Timing")]
    [Tooltip("Exact length of the intro animation in seconds")]
    public float introDuration = 6.5f;

    void Start()
    {
        // 1. Lock the player's manual controls
        if (playerControlScript != null) 
            playerControlScript.enabled = false;

        // 2. Ensure the camera animation is playing
        if (playerCameraAnimator != null)
            playerCameraAnimator.enabled = true;

        // 4. Start the countdown
        StartCoroutine(PlaySeamlessIntro());
    }

    IEnumerator PlaySeamlessIntro()
    {
        // Wait for the camera animation to finish
        yield return new WaitForSeconds(introDuration);

        // --- THE HANDOFF ---
        // Disable the Animator. This unlocks the camera so the mouse can move it again!
        if (playerCameraAnimator != null)
            playerCameraAnimator.enabled = false;

        // Turn the player's controls back on
        if (playerControlScript != null) 
            playerControlScript.enabled = true;

        // RELEASE THE SKELETON
        if (skeletonAI != null) 
            skeletonAI.BeginChase();
            
        Debug.Log("Seamless Intro finished. You have control. RUN!");
    }
}