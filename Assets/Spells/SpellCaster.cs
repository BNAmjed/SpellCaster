using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Handles microphone input, WAV encoding, server communication, and spell result processing.
/// Spells are gated: fireball requires fireballUnlocked, ice requires iceUnlocked.
/// Password phase checks for "moonlight" + happy emotion.
/// </summary>
public class SpellCaster : MonoBehaviour
{
    public static SpellCaster Instance;

    // ─── UI & Phase Flags ────────────────────────────────────────────────────────
    [Header("UI & Phase Flags")]
    public GameObject micUIPanel;
    public bool isMicActive      = false;
    public bool isPasswordPhase  = false;
    public bool fireballUnlocked = false;
    public bool iceUnlocked      = false;

    // ─── Password Settings ───────────────────────────────────────────────────────
    [Header("Password")]
    [Tooltip("The correct password the player must say. Default: moonlight (cloak of moon & stars).")]
    public string correctPassword = "moonlight";

    // ─── Voice Settings ──────────────────────────────────────────────────────────
    [Header("Voice Settings")]
    public float confidenceThreshold = 0.5f;
    public TextMeshProUGUI debugText;

    // ─── Mic & Server Settings ───────────────────────────────────────────────────
    [Header("Mic Settings")]
    public string    serverUrl      = "http://127.0.0.1:8000/cast_spell";
    public KeyCode   pushToTalkKey  = KeyCode.T;
    public Image     micIconImage;
    public RectTransform volumeBar;
    public int       sampleWindow   = 128;
    public float     micSensitivity = 15f;

    // ─── Private State ───────────────────────────────────────────────────────────
    private string    micName;
    private AudioClip recording;
    private bool      isRecording = false;
    private float     loudness    = 0f;

    // ────────────────────────────────────────────────────────────────────────────
    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (Microphone.devices.Length > 0)
            micName = Microphone.devices[0];

        if (micUIPanel != null)
            micUIPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isMicActive) return;

        // Push-to-talk
        if (Input.GetKeyDown(pushToTalkKey) && !isRecording)
            StartRecording();

        if (Input.GetKeyUp(pushToTalkKey) && isRecording)
            StopRecordingAndSend();

        // Volume visualisation
        if (isRecording)
        {
            UpdateLoudness();
            VisualizeLevel();
        }
        else if (volumeBar != null)
        {
            volumeBar.localScale = Vector3.Lerp(
                volumeBar.localScale,
                new Vector3(0f, 1f, 1f),
                Time.deltaTime * 10f
            );
        }
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Public API

    /// <summary>Called by DialogueDirector (Phase4_MicInput) to show the mic UI.</summary>
    public void EnableMicrophone()
    {
        isMicActive = true;
        if (micUIPanel != null) micUIPanel.SetActive(true);
        if (debugText  != null) debugText.text = "Hold [T] and speak your answer!";
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Recording

    private void StartRecording()
    {
        isRecording = true;
        if (micIconImage != null) micIconImage.color = Color.white;
        if (debugText    != null) debugText.text = "Listening...";
        recording = Microphone.Start(micName, false, 10, 16000);
    }

    private void StopRecordingAndSend()
    {
        isRecording = false;
        if (micIconImage != null) micIconImage.color = Color.gray;
        if (debugText    != null) debugText.text = "Sending...";

        int position = Microphone.GetPosition(micName);
        Microphone.End(micName);

        if (position <= 0) return; // Nothing recorded

        float[] samples = new float[position];
        recording.GetData(samples, 0);

        AudioClip trimmedClip = AudioClip.Create("Trimmed", position, 1, 16000, false);
        trimmedClip.SetData(samples, 0);

        byte[] wavData = EncodeToWAV(trimmedClip);
        StartCoroutine(SendAudioToServer(wavData));
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Server Communication

    private IEnumerator SendAudioToServer(byte[] audioData)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("audio_file", audioData, "spell.wav", "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                if (debugText != null) debugText.text = "Got result!";
                ProcessSpellResult(www.downloadHandler.text);
            }
            else
            {
                if (debugText != null) debugText.text = "Server error: " + www.error;
                Debug.LogWarning("[SpellCaster] Server error: " + www.error);
            }
        }
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Result Processing

    private void ProcessSpellResult(string json)
    {
        SpellData data = JsonUtility.FromJson<SpellData>(json);
        if (data == null)
        {
            Debug.LogWarning("[SpellCaster] Failed to parse SpellData JSON.");
            return;
        }

        string spokenLower  = data.spoken_text.ToLower().Trim();
        string emotionLower = data.emotion.ToLower().Trim();
        bool   aboveThreshold = data.confidence >= confidenceThreshold;

        if (debugText != null)
            debugText.text = $"Heard: \"{data.spoken_text}\" | Emotion: {data.emotion} | Conf: {data.confidence:F2}";

        // ── PASSWORD PHASE ────────────────────────────────────────────────────
        if (isPasswordPhase)
        {
            bool correctWord  = spokenLower.Contains(correctPassword.ToLower());
            bool happyEmotion = emotionLower == "happy" || emotionLower == "joy";

            if (correctWord && happyEmotion && aboveThreshold)
            {
                isPasswordPhase = false; // Lock further password attempts
                WizardEncounterManager.Instance.OnPasswordCorrect();
                return; // ← Safety return: prevents fall-through
            }

            if (correctWord && !happyEmotion)
            {
                WizardEncounterManager.Instance.OnWrongEmotion();
                return;
            }

            // Wrong word (or too quiet)
            WizardEncounterManager.Instance.OnWrongWord();
            return;
        }

        // ── SPELL PHASE ───────────────────────────────────────────────────────
        MagicController magicController = Object.FindFirstObjectByType<MagicController>();
        if (magicController == null)
        {
            Debug.LogWarning("[SpellCaster] MagicController not found in scene.");
            return;
        }

        // FIREBALL — requires unlock + "fireball" word + anger emotion
        if (fireballUnlocked && spokenLower.Contains("fireball") && emotionLower == "anger")
        {
            magicController.CastFireball();
            WizardEncounterManager.Instance?.OnSpellCasted("fireball");
            return; // ← Safety return
        }

        // ICE — requires unlock + "ice" word + fear emotion
        if (iceUnlocked && spokenLower.Contains("ice") && emotionLower == "fear")
        {
            magicController.CastIce();
            WizardEncounterManager.Instance?.OnSpellCasted("ice");
            return; // ← Safety return
        }

        // Fizzle: word recognized but conditions not met (wrong emotion, locked, etc.)
        magicController.CastFizzle();
    }

    #endregion

    // ────────────────────────────────────────────────────────────────────────────
    #region Volume Helpers (minified for clarity)

    private void UpdateLoudness()
    {
        int mp = Microphone.GetPosition(micName) - (sampleWindow + 1);
        if (mp < 0) return;
        float[] wd = new float[sampleWindow];
        recording.GetData(wd, mp);
        float total = 0f;
        foreach (float s in wd) total += Mathf.Abs(s);
        loudness = total / sampleWindow;
    }

    private void VisualizeLevel()
    {
        if (volumeBar == null) return;
        float targetX = Mathf.Clamp(loudness * micSensitivity, 0f, 3f);
        volumeBar.localScale = new Vector3(
            Mathf.Lerp(volumeBar.localScale.x, targetX, Time.deltaTime * 15f),
            1f, 1f
        );
    }

    private byte[] EncodeToWAV(AudioClip clip)
    {
        float[] s = new float[clip.samples];
        clip.GetData(s, 0);
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write("RIFF".ToCharArray());
            bw.Write(36 + s.Length * 2);
            bw.Write("WAVE".ToCharArray());
            bw.Write("fmt ".ToCharArray());
            bw.Write(16);
            bw.Write((short)1);
            bw.Write((short)clip.channels);
            bw.Write(clip.frequency);
            bw.Write(clip.frequency * clip.channels * 2);
            bw.Write((short)(clip.channels * 2));
            bw.Write((short)16);
            bw.Write("data".ToCharArray());
            bw.Write(s.Length * 2);
            foreach (float f in s) bw.Write((short)(f * short.MaxValue));
            return ms.ToArray();
        }
    }

    #endregion
}

// ── Data Model ────────────────────────────────────────────────────────────────
[System.Serializable]
public class SpellData
{
    public string spoken_text;
    public string emotion;
    public float  confidence;
}