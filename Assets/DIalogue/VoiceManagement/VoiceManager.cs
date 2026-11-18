using UnityEngine;
using UnityEngine.UI;

public class VoicePhaseManager : MonoBehaviour
{
    public static VoicePhaseManager Instance;

    [Header("Mic UI References")]
    public GameObject micUIPanel;      
    public Image micIconImage;         
    public RectTransform volumeBar;    // Your white rectangle

    [Header("Colors")]
    public Color idleColor = new Color(0.5f, 0.5f, 0.5f, 1f); 
    public Color activeColor = Color.white;                   

    [Header("Input & Settings")]
    public KeyCode pushToTalkKey = KeyCode.T;
    public int sampleWindow = 128;     
    public float micSensitivity = 15f; // Increase if the bar is too small
    public float maxBarWidth = 2.0f;   // How wide the bar is allowed to get

    private bool isPhase4Active = false;
    private bool isRecording = false;
    
    private AudioClip micClip;
    private string deviceName;
    private float loudness = 0f;

    private void Awake()
    {
        Instance = this;
        if (micUIPanel != null) micUIPanel.SetActive(false);

        if (Microphone.devices.Length > 0) {
            deviceName = Microphone.devices[0];
        } else {
            Debug.LogError("No Microphone detected!");
        }

        // Hide the volume bar by default (scale X to 0)
        if (volumeBar != null) volumeBar.localScale = new Vector3(0, 1, 1);
    }

    public void StartMicPhase()
    {
        isPhase4Active = true;
        if (micUIPanel != null) micUIPanel.SetActive(true);
        micIconImage.color = idleColor;
        if (volumeBar != null) volumeBar.localScale = new Vector3(0, 1, 1);
    }

    private void Update()
    {
        if (!isPhase4Active) return;

        // Input
        if (Input.GetKeyDown(pushToTalkKey)) StartRecording();
        if (Input.GetKeyUp(pushToTalkKey)) StopRecording();

        // Always run the visualizer so it can smoothly animate down to 0 when you let go
        UpdateLoudness();
        VisualizeLevel();
    }

    private void StartRecording()
    {
        isRecording = true;
        micIconImage.color = activeColor;
        micClip = Microphone.Start(deviceName, true, 10, 44100);
    }

    private void StopRecording()
    {
        isRecording = false;
        micIconImage.color = idleColor;
        Microphone.End(deviceName);
    }

    private void UpdateLoudness()
    {
        // If we aren't recording, force loudness to 0
        if (!isRecording || micClip == null)
        {
            loudness = 0f;
            return;
        }

        int micPosition = Microphone.GetPosition(deviceName) - (sampleWindow + 1); 
        if (micPosition < 0) return;

        float[] waveData = new float[sampleWindow];
        micClip.GetData(waveData, micPosition);

        float totalLoudness = 0;
        for (int i = 0; i < sampleWindow; i++) {
            totalLoudness += Mathf.Abs(waveData[i]);
        }

        loudness = totalLoudness / sampleWindow;
    }

    private void VisualizeLevel()
    {
        if (volumeBar != null)
        {
            float targetX = 0f;

            // Only calculate a width if we are picking up sound above a tiny threshold
            if (loudness > 0.005f) 
            {
                targetX = Mathf.Clamp(loudness * micSensitivity, 0f, maxBarWidth);
            }

            // Smoothly lerp the X scale. It will grow when talking, and shrink to 0 when quiet/stopped.
            float smoothX = Mathf.Lerp(volumeBar.localScale.x, targetX, Time.deltaTime * 15f);
            volumeBar.localScale = new Vector3(smoothX, 1, 1);
        }
    }
}