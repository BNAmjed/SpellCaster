using UnityEngine;

public class FallingTreeTrigger : MonoBehaviour
{
    [Header("References")]
    public GameObject giantTree;
    private AudioSource treeAudio; // Internal reference for the audio

    [Header("Fall Settings")]
    public float fallDuration = 2.0f;
    public float targetXRotation = 75f;

    private bool isFalling = false;
    private float elapsedTime = 0f;
    
    private Quaternion startRotation;
    private Quaternion endRotation;

    void Start()
    {
        // Cache the AudioSource at the start to save performance
        if (giantTree != null)
        {
            treeAudio = giantTree.GetComponent<AudioSource>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Only trigger if it's the player AND we aren't already falling
        if (other.CompareTag("Player") && !isFalling)
        {
            if (giantTree == null)
            {
                Debug.LogError("Assign the Giant Tree to the script slot!");
                return;
            }

            // --- AUDIO LOGIC ---
            if (treeAudio != null)
            {
                treeAudio.Play();
                Debug.Log("Playing tree fall audio.");
            }
            else
            {
                Debug.LogWarning("No AudioSource found on the Giant Tree object!");
            }

            // Capture initial state
            startRotation = giantTree.transform.rotation;
            Vector3 currentEuler = giantTree.transform.eulerAngles;
            endRotation = Quaternion.Euler(targetXRotation, currentEuler.y, currentEuler.z);

            isFalling = true;
            Debug.Log("Tree fall initiated with x² easing.");
        }
    }

    void Update()
    {
        if (isFalling && giantTree != null)
        {
            elapsedTime += Time.deltaTime;

            // Normalized time (0 to 1)
            float t = Mathf.Clamp01(elapsedTime / fallDuration);

            // Apply x² logic for accelerating fall
            float easedT = t * t;
            
            // Apply the Lerp for rotation
            giantTree.transform.rotation = Quaternion.Lerp(startRotation, endRotation, easedT);

            if (t >= 1f)
            {
                isFalling = false;
                Debug.Log("Tree fall complete.");
            }
        }
    }
}