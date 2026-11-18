using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneWakeUpManager : MonoBehaviour
{
    [Header("The Player")]
    public GameObject player; 

    [Header("Camera Animation")]
    public Animator playerCameraAnimator;
    public string wakeUpAnimTrigger = "WakeUp";
    
    [Tooltip("How many seconds until the player gets control back?")]
    public float wakeUpDuration = 3.0f;

    [Header("UI & Fade In")]
    public Image blackScreenFade; 
    public float fadeSpeed = 1.0f;

    void Start()
    {
        // 1. Immediately make the screen pitch black before the player sees anything
        if (blackScreenFade != null)
        {
            blackScreenFade.color = new Color(0, 0, 0, 1);
            blackScreenFade.gameObject.SetActive(true);
        }

        // 2. Start the wake-up cinematic
        StartCoroutine(WakeUpSequence());
    }

    IEnumerator WakeUpSequence()
    {
        Debug.Log("🌅 Wake Up Cinematic Started!");

        // 3. Disable player controls so they can't run around while "waking up"
        if (player != null)
        {
            MonoBehaviour movementScript = player.GetComponent("SpellCasterController") as MonoBehaviour;
            if (movementScript != null) movementScript.enabled = false;
            
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
        }

        // 4. Fire the camera's wake-up animation
        if (playerCameraAnimator != null)
        {
            playerCameraAnimator.enabled = true; 
            playerCameraAnimator.SetTrigger(wakeUpAnimTrigger);
        }

        // 5. Fade IN (Lerp the alpha from 1 down to 0)
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            if (blackScreenFade != null) blackScreenFade.color = new Color(0, 0, 0, alpha);
            yield return null; 
        }

        // 6. Wait for the rest of the animation to finish
        yield return new WaitForSeconds(wakeUpDuration);

        Debug.Log("🎮 Giving controls back to the player!");

        // 7. Turn OFF the camera animator (Crucial! Otherwise, your mouse-look script won't work)
        if (playerCameraAnimator != null)
        {
            playerCameraAnimator.enabled = false; 
        }

        // 8. Re-enable the player controls
        if (player != null)
        {
            MonoBehaviour movementScript = player.GetComponent("SpellCasterController") as MonoBehaviour;
            if (movementScript != null) movementScript.enabled = true;
            
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;
        }
    }
}