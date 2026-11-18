using System.Collections;
using UnityEngine;

public class WizardEncounterManager : MonoBehaviour
{
    public static WizardEncounterManager Instance;
    // ─── Audio Control ──────────────────────────────────────────────────────────
    [Header("Background Music Control")]
    [Tooltip("Drag the Audio Sources you want to STOP here (e.g., your Lost Woods sources)")]
    public GameObject objectToSilence;    
    [Tooltip("Delay in seconds before the Grow sound plays")]
    public float growSoundDelay = 0.5f;
    [Header("Magic Sounds")]
    public AudioSource magicSfxSource; // Drag an AudioSource from the Wizard here
    public AudioClip shrinkSound;
    public AudioClip growSound;
    [Tooltip("Drag the Audio Source you want to START here")]
    public AudioSource musicToStart;
    // ─── Audio Control ──────────────────────────────────────────────────────────
    [Header("Background Music Control")]

    [Tooltip("How many seconds the smooth crossfade will take")]
    public float crossfadeDuration = 2.0f;
    // ─── Wizard Setup ────────────────────────────────────────────────────────────
    [Header("Wizard Setup")]
    [Tooltip("Drag the 'wizard dance' child GameObject here — NOT Wiz_Pos")]
    public Animator wizardAnim;
    public DialogueAnimator dialogueAnimator;
    [Tooltip("Empty GameObject placed next to the campfire")]
    public Transform campfireTeleportPos;
    [Tooltip("Drag your Player or Main Camera here")]
    public Transform playerTransform;

    // ─── Animation Clip Lengths ──────────────────────────────────────────────────
    [Header("Animation Lengths (check your clip lengths in seconds)")]
    public float shrinkAnimLength = 1.0f;
    public float growAnimLength   = 1.0f;

    // ─── Visual & Audio Effects ──────────────────────────────────────────────────
    [Header("Visual Effects")]
    public ParticleSystem dustEffect;
    public ParticleSystem starsEffect;
    public ParticleSystem confettiEffect;

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioClip   successMusic;

    // ─── Internal State ──────────────────────────────────────────────────────────
    [HideInInspector] public bool isWaitingInTrainingArea = false;

    private bool      isCampfireLit     = false;
    public bool      isLookingAtPlayer = false;
    private Coroutine fireballMissRoutine;
    private Coroutine iceMissRoutine;

    // ────────────────────────────────────────────────────────────────────────────
    #region Unity Lifecycle

    private void Awake()
    {
        
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Auto-find the player's camera if the slot is empty!
        if (playerTransform == null && Camera.main != null)
            playerTransform = Camera.main.transform;
    
    }

    private void Update()
    {
        // Continuously rotate wizard to face player when flag is active
        if (!isLookingAtPlayer || playerTransform == null) return;

        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 5f);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Phase 2 → 3: Dance Success → Password Dialogue

    /// <summary>
    /// Called by DanceChallengeManager after the victory beat.
    /// Stops dancing animation, enables look-at, starts password dialogue.
    /// </summary>
    public void StartPasswordDialogueDirectly()
    {
        // --- NEW: Switch the music instantly! ---
        SwitchBackgroundMusic();
        // ----------------------------------------

        if (wizardAnim != null)
            wizardAnim.SetBool("IsDancing", false);

        isLookingAtPlayer = true;

        string[] askLines =
        {
            "Impressive moves! Now prove your mind is as sharp as your feet.",
            "I wear a cloak woven from moon and <color=#AADDFF>stars</color>...",
            "Speak the secret word that my cloak whispers. Say it with <b>JOY</b> in your heart!"
        };
        DialogueDirector.Instance.EnterDialogueMode(transform, askLines, GamePhase.Phase4_MicInput);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Phase 4: Password Feedback

    public void OnWrongWord()
    {
        string[] lines =
        {
            "Hmm... that word holds no magic here.",
            "Look at my cloak — moon and <color=#AADDFF>stars</color>. Let them guide your tongue!"
        };
        DialogueDirector.Instance.EnterDialogueMode(transform, lines, GamePhase.Phase4_MicInput);
    }

    public void OnWrongEmotion()
    {
        string[] lines =
        {
            "The word rings true... but I feel no joy!",
            "Say it again — with happiness bursting from your heart!"
        };
        DialogueDirector.Instance.EnterDialogueMode(transform, lines, GamePhase.Phase4_MicInput);
    }

    public void OnPasswordWrong() => OnWrongWord();

    /// <summary>
    /// Password + happy emotion correct.
    /// Plays success music, dialogue ends → Phase5_Teleport → Vanish() auto-called.
    /// </summary>
    public void OnPasswordCorrect()
    {
        PlaySuccessMusic();

        string[] lines =
        {
            "YES! <color=#FFE566>Moonlight</color>! The stars sing your name!",
            "Wait for me by the campfire — I shall meet you there!"
        };
        DialogueDirector.Instance.EnterDialogueMode(transform, lines, GamePhase.Phase5_Teleport);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Phase 5: Vanish — Shrink then Move to Campfire

    public void Vanish() => StartCoroutine(VanishSequence());

    private IEnumerator VanishSequence()
    {
        Debug.Log("1. Vanish Started");
        isLookingAtPlayer = false; 

        // --- ADD THE SHRINK SOUND HERE ---
        if (magicSfxSource != null && shrinkSound != null)
            magicSfxSource.PlayOneShot(shrinkSound);
        // ---------------------------------

        if (wizardAnim != null)
            wizardAnim.SetTrigger("Shrink");

        PlayEffect(dustEffect);
        
        // ... rest of the VanishSequence stays the same ...
        Debug.Log("1. Vanish Started");
        isLookingAtPlayer = false; 

        if (wizardAnim != null)
            wizardAnim.SetTrigger("Shrink");

        PlayEffect(dustEffect);

        // Wait for shrink to finish
        yield return new WaitForSeconds(shrinkAnimLength);

        Debug.Log("2. Attempting Teleport to: " + campfireTeleportPos.position);

        // FIX: CharacterControllers block transform.position!
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // Turn off physics briefly

        // Move the wizard
        if (campfireTeleportPos != null) {
            transform.position = campfireTeleportPos.position;
        } else {
            Debug.LogError("CampfireTeleportPos is MISSING in the Inspector!");
        }

        if (cc != null) cc.enabled = true; // Turn physics back on

        Debug.Log("3. Teleport Complete. Current Pos: " + transform.position);
        
        isWaitingInTrainingArea = true;
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Phase 5 → 6: Player Arrives → Grow → Training Dialogue

    /// <summary>Called by TrainingAreaTrigger when player enters the campfire zone.</summary>
    public void StartFireTraining()
    {
        isWaitingInTrainingArea = false;
        StartCoroutine(AppearAtCampfire());
    }

    private IEnumerator AppearAtCampfire()
    {
        // Wizard is already at campfire position, frozen tiny
        PlayEffect(dustEffect);
        PlayEffect(starsEffect);
        PlayEffect(confettiEffect);

        // --- ADD THE GROW SOUND HERE ---
        if (magicSfxSource != null && growSound != null)
            magicSfxSource.PlayOneShot(growSound);
        // -------------------------------

        if (wizardAnim != null)
        {
            wizardAnim.SetTrigger("Grow");
        }
        // Wizard is already at campfire position, frozen tiny on last Shrinking frame
        PlayEffect(dustEffect);
        PlayEffect(starsEffect);
        PlayEffect(confettiEffect);

        // Trigger Growing state.
        // wizardAnim.Play("Growing", 0, 1f) forces normalized time = 1.0 (end of clip)
        // Combined with Speed = -1 on the Growing state, it plays BACKWARDS: tiny → full size
        if (wizardAnim != null)
        {
            wizardAnim.SetTrigger("Grow");
            //wizardAnim.Play("Growing", 0, 1f); // "Growing" = exact state name in Animator
        }

        // Wait for grow to finish
        yield return new WaitForSeconds(growAnimLength);

        // Full size — start looking at player again
        isLookingAtPlayer = true;

        string[] training =
        {
            "Welcome, apprentice! Let us begin your training.",
            "First — the spell of flame! Shout <color=#FF4422><b>FIREBALL</b></color> with all your ANGER...",
            "...and hurl it at the campfire wood! Let your fury be the spark!"
        };
        DialogueDirector.Instance.EnterDialogueMode(transform, training, GamePhase.Phase6_TrainingFire);
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Phase 6: Fireball Miss Detection

    public void OnSpellCasted(string spell)
    {
        if (spell == "fireball" && !isCampfireLit)
        {
            if (fireballMissRoutine != null) StopCoroutine(fireballMissRoutine);
            fireballMissRoutine = StartCoroutine(FireballMissCheck());
        }
        else if (spell == "ice" && isCampfireLit)
        {
            if (iceMissRoutine != null) StopCoroutine(iceMissRoutine);
            iceMissRoutine = StartCoroutine(IceMissCheck());
        }
    }

    private IEnumerator FireballMissCheck()
    {
        yield return new WaitForSeconds(2.5f);
        if (!isCampfireLit)
        {
            string[] lines = { "You missed! Aim <color=#FF4422><b>FIREBALL</b></color> at the logs — right there!" };
            DialogueDirector.Instance.EnterDialogueMode(transform, lines, GamePhase.None);
        }
    }

    private IEnumerator IceMissCheck()
    {
        yield return new WaitForSeconds(2.5f);
        if (isCampfireLit)
        {
            string[] lines = { "So close! Direct <color=#44AAFF><b>ICE</b></color> straight at the flames!" };
            DialogueDirector.Instance.EnterDialogueMode(transform, lines, GamePhase.None);
        }
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Phase 6 → 7: Campfire State

    public void SetCampfireState(bool lit)
    {
        bool wasLit   = isCampfireLit;
        isCampfireLit = lit;

        if (lit && !wasLit)
        {
            if (fireballMissRoutine != null) StopCoroutine(fireballMissRoutine);

            string[] lines =
            {
                "<color=#FF9900>Great work!</color> You feel that heat? That is raw power!",
                "But fire draws monsters from the dark...",
                "Douse it fast! Shout <color=#44AAFF><b>ICE</b></color> with <b>FEAR</b> in your voice — then aim true!"
            };
            DialogueDirector.Instance.EnterDialogueMode(transform, lines, GamePhase.Phase7_TrainingIce);

            if (SpellCaster.Instance != null) SpellCaster.Instance.iceUnlocked = true;
        }
        else if (!lit && wasLit)
        {
            if (iceMissRoutine != null) StopCoroutine(iceMissRoutine);

            string[] lines =
            {
                "<color=#AADDFF>Wonderful!</color> Masterful balance of fire and ice.",
                "You are ready, young mage. The realm awaits your power!"
            };
            DialogueDirector.Instance.EnterDialogueMode(transform, lines, GamePhase.Done);
        }
    }

    public void OnCampfireHit(bool lit) => SetCampfireState(lit);

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Helpers
    public void SwitchBackgroundMusic()
    {
        StartCoroutine(CrossfadeMusicRoutine());
    }
    private IEnumerator CrossfadeMusicRoutine()
    {
        if (objectToSilence == null) yield break;

        // 1. Find ALL audio sources on that object and its children
        AudioSource[] sourcesToStop = objectToSilence.GetComponentsInChildren<AudioSource>();
        
        // 2. Capture their starting volumes
        float[] originalVolumes = new float[sourcesToStop.Length];
        for (int i = 0; i < sourcesToStop.Length; i++)
        {
            originalVolumes[i] = sourcesToStop[i].volume;
        }

        // 3. Prepare the New Music
        float targetStartVolume = 1f; 
        if (musicToStart != null)
        {
            targetStartVolume = musicToStart.volume;
            musicToStart.volume = 0f;
            musicToStart.Play();
        }

        // 4. Smoothly Fade
        float timeElapsed = 0f;
        while (timeElapsed < crossfadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / crossfadeDuration;

            // Fade out everything on the old object
            for (int i = 0; i < sourcesToStop.Length; i++)
            {
                if (sourcesToStop[i] != null)
                    sourcesToStop[i].volume = Mathf.Lerp(originalVolumes[i], 0f, t);
            }

            // Fade in the new music
            if (musicToStart != null)
                musicToStart.volume = Mathf.Lerp(0f, targetStartVolume, t);

            yield return null;
        }

        // 5. THE KILL SWITCH: Deactivate all old sources entirely
        foreach (AudioSource source in sourcesToStop)
        {
            if (source != null)
            {
                source.Stop();
                source.enabled = false; // Disables the component
            }
        }

        // Optional: Deactivate the entire object if it only holds music
        // objectToSilence.SetActive(false);

        if (musicToStart != null) musicToStart.volume = targetStartVolume;
    }
    private void PlaySuccessMusic()
    {
        if (musicSource != null && successMusic != null)
        {
            musicSource.clip = successMusic;
            musicSource.Play();
        }
    }

    private void PlayEffect(ParticleSystem ps)
    {
        if (ps != null) ps.Play();
    }

    #endregion
}