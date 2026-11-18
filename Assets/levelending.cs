using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelEndManager : MonoBehaviour
{
    [Header("The Actors")]
    public GameObject player; 
    
    [Header("Player Cinematic Snap")]
    [Tooltip("The exact spot & rotation the player should slide to!")]
    public Transform playerCinematicSpot; 
    [Tooltip("How many seconds it takes to smoothly slide into position")]
    public float playerSmoothTransitionTime = 0.5f; // 🔥 NEW SMOOTH TIMING 🔥

    [Header("Skeleton Setup")]
    public GiantSkeletonChaser skeletonAI;
    public GameObject cinematicSkeleton;
    public string skeletonAnimTrigger = "FinalAttack";
    public float spawnDistanceBehind = 1.5f;
    public float xOffset = 0f; 
    public bool flipSkeletonRotation = true; 

    [Header("Camera Animation")]
    public Animator playerCameraAnimator;
    public string cameraAnimTrigger = "FallDown";

    [Header("UI & Transition")]
    public Image blackScreenFade; 
    public float fadeSpeed = 1.5f;
    public string nextSceneName = "DarkForestScene"; 

    [Header("Animation Timings")]
    public float timeBeforeSkeletonPushes = 2.0f;
    public float timeFallingBeforeBlackout = 1.5f;

    private bool cutsceneStarted = false;

    void Start()
    {
        if (cinematicSkeleton != null)
        {
            cinematicSkeleton.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !cutsceneStarted)
        {
            cutsceneStarted = true;
            StartCoroutine(PlayEndingCutscene());
        }
    }

    IEnumerator PlayEndingCutscene()
    {
        Debug.Log("🎬 Ending Cutscene Triggered!");

        // 1. Cleanly disable player controls
        if (player != null)
        {
            MonoBehaviour movementScript = player.GetComponent("SpellCasterController") as MonoBehaviour;
            if (movementScript != null) movementScript.enabled = false;
            
            // Disable CharacterController so it doesn't fight our smooth movement
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
        }

        // 2. 🔥 SMOOTHLY GLIDE THE PLAYER TO THE MARK 🔥
        if (playerCinematicSpot != null && player != null)
        {
            Vector3 startPos = player.transform.position;
            Quaternion startRot = player.transform.rotation;
            float elapsedTime = 0f;

            while (elapsedTime < playerSmoothTransitionTime)
            {
                elapsedTime += Time.deltaTime;
                // Calculate percentage of completion (0 to 1)
                float t = elapsedTime / playerSmoothTransitionTime;
                
                // Add a smooth ease-in and ease-out effect
                t = Mathf.SmoothStep(0f, 1f, t);

                // Lerp position and Slerp rotation
                player.transform.position = Vector3.Lerp(startPos, playerCinematicSpot.position, t);
                player.transform.rotation = Quaternion.Slerp(startRot, playerCinematicSpot.rotation, t);
                
                yield return null; // Wait for the next frame
            }

            // Ensure they are PERFECTLY aligned at the end of the transition
            player.transform.position = playerCinematicSpot.position;
            player.transform.rotation = playerCinematicSpot.rotation;
        }

        // 3. THE SWAP: Destroy the Chaser
        if (skeletonAI != null)
        {
            Destroy(skeletonAI.gameObject);
        }

        // 4. DYNAMICALLY SPAWN BEHIND THE PLAYER
        if (cinematicSkeleton != null && player != null)
        {
            Vector3 spawnPos = player.transform.position - (player.transform.forward * spawnDistanceBehind);
            spawnPos.x = player.transform.position.x + xOffset;
            spawnPos.y = cinematicSkeleton.transform.position.y;
            cinematicSkeleton.transform.position = spawnPos;

            cinematicSkeleton.transform.rotation = player.transform.rotation;

            if (flipSkeletonRotation)
            {
                cinematicSkeleton.transform.rotation *= Quaternion.Euler(0, 180f, 0);
            }

            cinematicSkeleton.SetActive(true); 
            
            Animator skeletonAnimator = cinematicSkeleton.GetComponent<Animator>();
            if (skeletonAnimator != null)
            {
                skeletonAnimator.SetTrigger(skeletonAnimTrigger);
            }
        }

        // 5. Wait for the tension/push
        yield return new WaitForSeconds(timeBeforeSkeletonPushes);

        Debug.Log("💥 Firing Camera Animation!");

        // 6. Fire the player's camera fall animation
        if (playerCameraAnimator != null)
        {
            playerCameraAnimator.applyRootMotion = false;
            playerCameraAnimator.enabled = true; 
            playerCameraAnimator.SetTrigger(cameraAnimTrigger);
        }

        // 7. Wait while the player "falls"
        yield return new WaitForSeconds(timeFallingBeforeBlackout);

        Debug.Log("🤕 Fading to black...");

        // 8. Fade the screen
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            if (blackScreenFade != null) blackScreenFade.color = new Color(0, 0, 0, alpha);
            yield return null; 
        }

        yield return new WaitForSeconds(1f);

        Debug.Log("Loading next scene...");
        SceneManager.LoadScene(nextSceneName);
    }
}