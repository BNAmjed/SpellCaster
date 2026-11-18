using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PS1RetroCamera : MonoBehaviour
{
    [Header("PS1 Resolution")]
    public int horizontalResolution = 320;
    public int verticalResolution = 240;

    private RenderTexture retroTexture;

    void Start()
    {
        // Create a low-res canvas
        retroTexture = new RenderTexture(horizontalResolution, verticalResolution, 16);
        // Force the pixels to look sharp, not blurry!
        retroTexture.filterMode = FilterMode.Point; 
        
        // Tell the camera to render to this low-res canvas
        GetComponent<Camera>().targetTexture = retroTexture;
    }

    void OnGUI()
    {
        // Stretch the low-res canvas across the entire modern screen
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), retroTexture);
    }
}