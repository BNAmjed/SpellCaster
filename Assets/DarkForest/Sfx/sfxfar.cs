using UnityEngine;
using UnityEngine.Audio;

public class DynamicReverb : MonoBehaviour
{
    public AudioMixer mixer;
    public Transform player;
    public float maxDistance = 20f;
    public float minDistance = 2f;

    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        
        // Calculate a 0 to 1 value based on distance
        float t = Mathf.InverseLerp(minDistance, maxDistance, dist);
        
        // Map t to Decibels (-80dB is silent, 0dB is full volume)
        // As you get closer (t goes to 0), reverb volume goes to -80
        float reverbLevel = Mathf.Lerp(-80f, 0f, t);
        
        mixer.SetFloat("ReverbVol", reverbLevel);
    }
}