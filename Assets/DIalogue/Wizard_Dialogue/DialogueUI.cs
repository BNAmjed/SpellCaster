using UnityEngine;
using TMPro; // Standard for Unity UI Text

public class DialogueUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject dialoguePanel;
    public GameObject passwordPanel;

    [Header("UI Elements")]
    public TextMeshProUGUI dialogueText;
    public TMP_InputField passwordInputField;

    public void ShowDialogue(string text)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = text;
        
        // Hide password panel by default when showing standard text
        passwordPanel.SetActive(false); 
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        passwordPanel.SetActive(false);
    }

    public void ShowPasswordInputArea()
    {
        passwordPanel.SetActive(true);
        passwordInputField.text = ""; // Clear old text
        passwordInputField.ActivateInputField(); // Auto-focus the cursor
    }

    public void LockInput()
    {
        passwordInputField.interactable = false;
    }

    public void UnlockInput()
    {
        passwordInputField.interactable = true;
        passwordInputField.text = ""; // Clear the wrong attempt
        passwordInputField.ActivateInputField();
    }
}