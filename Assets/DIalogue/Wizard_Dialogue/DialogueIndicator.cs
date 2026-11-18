using UnityEngine;
using System.Collections; // <--- THIS IS THE MISSING LINE
using System.Collections.Generic; // Good practice to have this too

public class DialogueIndicator : MonoBehaviour
{
    [Header("References")]
    public GameObject indicatorParent; // The whole group
    public Transform pyramidMesh;      // ONLY the yellow pyramid mesh
    public Transform letterE;          // The 'E' text/sprite
    public Transform wizardFaceTarget;

    [Header("Smooth Transitions")]
    public float fadeSpeed = 2.5f;     // How fast it fades (lower = smoother)
    public float floatSpeed = 2f;
    public float floatHeight = 0.15f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 90f; // Degrees per second

    [Header("Phase 1 Dialogue")]
    public string[] introLines = new string[] 
    {
        "Ah, a visitor! But not just anyone may pass through my realm...",
        "To prove your spirit is worthy, you must dance with me!",
        "Spin into the air with all your might — and hold that energy for 3 full seconds!",
        "Show me you have the soul of a true adventurer... now DANCE!"
    };

    private bool playerInZone = false;
    private bool hasTriggeredPhase1 = false;
    private Vector3 startPos;
    private CanvasGroup uiCanvasGroup;
    private Transform playerTransform;

    private void Awake()
    {
        playerTransform = Camera.main.transform; // Assume camera is player's eyes
        if (indicatorParent != null)
        {
            startPos = indicatorParent.transform.localPosition;
            uiCanvasGroup = indicatorParent.GetComponent<CanvasGroup>();
            if (uiCanvasGroup == null) uiCanvasGroup = indicatorParent.AddComponent<CanvasGroup>();
            
            uiCanvasGroup.alpha = 0f;
            indicatorParent.SetActive(false); 
        }
    }

    private void Update()
    {
        if (hasTriggeredPhase1 || indicatorParent == null) return;

        // --- 1. HANDLE ROTATION & BILLBOARDING ---
        if (pyramidMesh != null)
        {
            // Rotate the pyramid on its local Y axis
            pyramidMesh.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }

        if (letterE != null && playerInZone)
        {
            // Make the 'E' always look at the player so it's readable
            letterE.LookAt(playerTransform);
            letterE.Rotate(0, 180, 0); // Flip if it's backwards
        }

        // --- 2. HANDLE SMOOTH TRANSITION ---
        float targetAlpha = playerInZone ? 1f : 0f;
        
        // Linear move towards the target alpha for a buttery smooth feel
        uiCanvasGroup.alpha = Mathf.MoveTowards(uiCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        if (uiCanvasGroup.alpha > 0)
        {
            if (!indicatorParent.activeSelf) indicatorParent.SetActive(true);

            // Bobbing effect
            float newY = startPos.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
            indicatorParent.transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
        }
        else if (indicatorParent.activeSelf)
        {
            indicatorParent.SetActive(false);
        }

        // --- 3. INPUT ---
        if (playerInZone && Input.GetKeyDown(KeyCode.E) && uiCanvasGroup.alpha > 0.8f)
        {
            hasTriggeredPhase1 = true;
            playerInZone = false; // Stop the fade logic
            StartCoroutine(FadeOutAndStart());
        }
    }

    private IEnumerator FadeOutAndStart()
    {
        // Smoothly fade out before the dialogue pops
        while (uiCanvasGroup.alpha > 0)
        {
            uiCanvasGroup.alpha -= Time.deltaTime * fadeSpeed * 2;
            yield return null;
        }
        indicatorParent.SetActive(false);
        DialogueDirector.Instance.EnterDialogueMode(wizardFaceTarget, introLines, GamePhase.Phase2_Dance);
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) playerInZone = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) playerInZone = false; }
}