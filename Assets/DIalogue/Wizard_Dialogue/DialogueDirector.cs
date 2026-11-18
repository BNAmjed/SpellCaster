using System.Collections;
using UnityEngine;

public enum GamePhase
{
    None,
    Phase1_Intro,
    Phase2_Dance,
    Phase3_PasswordDialogue,
    Phase4_MicInput,       // Password via mic
    Phase5_Teleport,       // Wizard vanishes, player goes to campfire
    Phase6_TrainingFire,   // Learn FIREBALL
    Phase7_TrainingIce,    // Learn ICE
    Done
}

/// <summary>
/// Orchestrates dialogue sequences and phase transitions.
/// Freezes player movement during dialogue, pans camera toward the NPC,
/// and triggers the correct game system when dialogue ends.
/// </summary>
public class DialogueDirector : MonoBehaviour
{
    public static DialogueDirector Instance;

    // ─── References ─────────────────────────────────────────────────────────────
    [Header("UI Reference")]
    public GameObject dialogueBoxUI;

    [Header("Player & Camera Freeze")]
    public Camera mainCamera;
    public MonoBehaviour[] scriptsToFreeze; // e.g., CharacterController, CameraLook

    [Header("Cinematic Settings")]
    public float cameraTransitionTime = 1.0f;

    // ─── Private State ───────────────────────────────────────────────────────────
    private Quaternion originalCamRot;
    private GamePhase  upcomingPhase;

    // ────────────────────────────────────────────────────────────────────────────
    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Public API

    /// <summary>
    /// Freezes the player, pans the camera toward lookTarget, and starts dialogue.
    /// nextPhase is triggered automatically when the dialogue finishes.
    /// </summary>
    public void EnterDialogueMode(Transform lookTarget, string[] lines, GamePhase nextPhase)
    {
        upcomingPhase = nextPhase;

        // Freeze player movement/look scripts
        SetPlayerScripts(false);

        // Pan camera toward the wizard/NPC
        if (mainCamera != null && lookTarget != null)
            StartCoroutine(PanCameraToTarget(lookTarget, entering: true));

        // Start the dialogue box
        if (dialogueBoxUI != null)
        {
            dialogueBoxUI.SetActive(true);
            dialogueBoxUI.GetComponent<DialogueFlowManager>().StartDialogue("Wizard", lines);
        }
    }

    /// <summary>Called by DialogueFlowManager when the last line is dismissed.</summary>
    public void EndDialogueMode()
    {
        // Re-enable player
        SetPlayerScripts(true);

        // Pan camera back
        if (mainCamera != null)
            StartCoroutine(PanCameraToTarget(null, entering: false));

        // Trigger the next game phase
        HandlePhaseTransition(upcomingPhase);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Phase Transitions

    private void HandlePhaseTransition(GamePhase phase)
    {
        switch (phase)
        {
            // ── Dance challenge
            case GamePhase.Phase2_Dance:
                if (DanceChallengeManager.Instance != null)
                    DanceChallengeManager.Instance.StartDanceChallenge();
                break;

            // ── Enable microphone for password / spell input
            case GamePhase.Phase4_MicInput:
                SpellCaster spellCaster = Object.FindFirstObjectByType<SpellCaster>();
                if (spellCaster != null)
                {
                    spellCaster.isPasswordPhase = true;
                    spellCaster.EnableMicrophone();
                }
                break;

            // ── Wizard teleports to campfire
            case GamePhase.Phase5_Teleport:
                WizardEncounterManager.Instance?.Vanish();
                break;

            // ── Unlock fireball after the fire-training dialogue finishes
            case GamePhase.Phase6_TrainingFire:
                if (SpellCaster.Instance != null)
                {
                    SpellCaster.Instance.fireballUnlocked = true;
                    SpellCaster.Instance.isPasswordPhase  = false; // ensure password phase off
                    // Keep mic active so player can cast spells
                    SpellCaster.Instance.isMicActive = true;
                }
                break;

            // ── Unlock ice after the ice-training dialogue finishes
            case GamePhase.Phase7_TrainingIce:
                if (SpellCaster.Instance != null)
                {
                    SpellCaster.Instance.iceUnlocked  = true;
                    SpellCaster.Instance.isMicActive  = true;
                }
                break;

            // ── Encounter complete
            case GamePhase.Done:
                Debug.Log("[DialogueDirector] Encounter complete! Transition to next scene or reward here.");
                break;

            case GamePhase.None:
            default:
                // No automated action — mic stays on so the player can retry
                if (SpellCaster.Instance != null && SpellCaster.Instance.isMicActive)
                    SpellCaster.Instance.isMicActive = true;
                break;
        }
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Helpers

    private void SetPlayerScripts(bool enabled)
    {
        if (scriptsToFreeze == null) return;
        foreach (MonoBehaviour s in scriptsToFreeze)
            if (s != null) s.enabled = enabled;
    }

    private IEnumerator PanCameraToTarget(Transform target, bool entering)
    {
        Quaternion startRot = mainCamera.transform.rotation;
        Quaternion endRot;

        if (entering)
        {
            originalCamRot = mainCamera.transform.rotation;
            Vector3 dir    = target.position - mainCamera.transform.position;
            endRot         = Quaternion.LookRotation(dir);
        }
        else
        {
            endRot = originalCamRot;
        }

        float elapsed = 0f;
        while (elapsed < cameraTransitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cameraTransitionTime;
            t = t * t * (3f - 2f * t); // Smooth step
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        mainCamera.transform.rotation = endRot;
    }

    #endregion
}