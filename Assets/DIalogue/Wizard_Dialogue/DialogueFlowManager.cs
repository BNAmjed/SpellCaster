using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class DialogueFlowManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform uiPanel; 
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject nextIconTriangle;
    public GameObject endIconSquare;

    [Header("Animation Settings")]
    public float slideDuration = 0.4f;
    [Tooltip("The Y position when the box is on screen. (Usually around 0 to 50)")]
    public float visibleY = 50f; 
    [Tooltip("The Y position when the box is hidden off screen. (Usually around -600)")]
    public float hiddenY = -600f; 

    [Header("Typing Settings")]
    public float typeSpeed = 0.04f;
    
    [Header("Audio Settings")]
    public AudioClip blipSound;
    [Range(0f, 1f)] public float blipVolume = 0.4f;
    private AudioSource audioSource;

    private string[] currentLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; 
    }

    public void StartDialogue(string npcName, string[] lines)
    {
        nameText.text = npcName;
        currentLines = lines;
        currentLineIndex = 0;
        dialogueText.text = ""; 
        
        nextIconTriangle.SetActive(false);
        endIconSquare.SetActive(false);
        
        // SNAP TO HIDDEN POSITION IMMEDIATELY (Prevents 1-frame flash)
        uiPanel.anchoredPosition = new Vector2(uiPanel.anchoredPosition.x, hiddenY);
        
        // Start the slide-in animation!
        StartCoroutine(SlideRoutine(true));
    }

    private IEnumerator SlideRoutine(bool slidingIn)
    {
        float elapsed = 0f;
        
        float startY = slidingIn ? hiddenY : visibleY;
        float endY = slidingIn ? visibleY : hiddenY;
        float currentX = uiPanel.anchoredPosition.x; // Keep it horizontally centered

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            
            // Smooth Step formula (Ease-In / Ease-Out)
            t = t * t * (3f - 2f * t);

            float newY = Mathf.Lerp(startY, endY, t);
            uiPanel.anchoredPosition = new Vector2(currentX, newY);
            
            yield return null;
        }

        uiPanel.anchoredPosition = new Vector2(currentX, endY); // Snap to perfect finish

        if (slidingIn)
        {
            PlayNextLine(); // Box arrived, start typing!
        }
        else
        {
            DialogueDirector.Instance.EndDialogueMode();
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Only allow E press if the panel is fully in its visible position
        if (Input.GetKeyDown(KeyCode.E) && Mathf.Approximately(uiPanel.anchoredPosition.y, visibleY))
        {
            if (isTyping)
            {
                CompleteLineInstantly();
            }
            else
            {
                currentLineIndex++;
                if (currentLineIndex < currentLines.Length) PlayNextLine();
                else EndDialogue();
            }
        }
    }

    private void PlayNextLine()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        nextIconTriangle.SetActive(false);
        endIconSquare.SetActive(false);
        typingCoroutine = StartCoroutine(TypeText(currentLines[currentLineIndex]));
    }

    private IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogueText.text = "";
        int charCount = 0;

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            charCount++;

            if (letter != ' ' && charCount % 3 == 0 && blipSound != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.15f);
                audioSource.PlayOneShot(blipSound, blipVolume);
            }
            yield return new WaitForSeconds(typeSpeed);
        }
        OnLineFinished();
    }

    private void CompleteLineInstantly()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = currentLines[currentLineIndex];
        OnLineFinished();
    }

    private void OnLineFinished()
    {
        isTyping = false;
        if (currentLineIndex >= currentLines.Length - 1) endIconSquare.SetActive(true);
        else nextIconTriangle.SetActive(true);
    }

    private void EndDialogue()
    {
        nextIconTriangle.SetActive(false);
        endIconSquare.SetActive(false);
        dialogueText.text = "";
        
        StartCoroutine(SlideRoutine(false));
    }
}