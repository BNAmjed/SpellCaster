using UnityEngine;
using System.Collections;

// Forces Unity to ensure an AudioSource exists on this object
[RequireComponent(typeof(AudioSource))] 
public class AnimatedSkeletonTrap : MonoBehaviour
{
    [Header("References")]
    public Animator skullAnimator;

    [Header("The Trap (Slow Down)")]
    public float slowMultiplier = 0.2f;
    public float slowDuration = 2f;

    [Header("Look At Settings")]
    [Tooltip("How fast the skeleton turns to lock onto the player")]
    public float turnSpeed = 10f;
    
    private Transform playerTarget;
    
    // Both of these are now private! No more dragging and dropping.
    private AudioSource scareAudio; 
    private ParticleSystem dirtParticles; 

    private bool hasTriggered = false;
    private bool hasGrabbed = false;

    void Awake()
    {
        // 1. Grab the AudioSource attached directly to this main skeleton body
        scareAudio = GetComponent<AudioSource>();

        // 2. Search through all child objects in the hierarchy and grab the Particle System!
        dirtParticles = GetComponentInChildren<ParticleSystem>();
        
        if (dirtParticles == null)
        {
            Debug.LogWarning("No Particle System found inside the skeleton's children! Did you forget to add the dirt effect?", this);
        }
    }

    public void FireAnimation(Transform playerTransform)
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            playerTarget = playerTransform; 
            
            if (skullAnimator != null) skullAnimator.SetTrigger("RiseTrigger");
            if (scareAudio != null) scareAudio.Play();
            if (dirtParticles != null) dirtParticles.Play();
        }
    }

    void Update()
    {
        if (hasTriggered && playerTarget != null)
        {
            Vector3 directionToPlayer = playerTarget.position - transform.position;
            directionToPlayer.y = 0; 

            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasGrabbed && hasTriggered)
        {
            hasGrabbed = true;
            StartCoroutine(ApplySlowdown(other.gameObject));
        }
    }

    IEnumerator ApplySlowdown(GameObject target)
    {
        var moveScript = target.GetComponent<SpellCasterController>();
        if (moveScript != null)
        {
            Debug.Log("🩸 GRABBED BY SKULL! Speed crippled!");
            
            float originalWalk = moveScript.walkSpeed;
            float originalRun = moveScript.runSpeed;

            moveScript.walkSpeed = originalWalk * slowMultiplier;
            moveScript.runSpeed = originalRun * slowMultiplier;

            yield return new WaitForSeconds(slowDuration);

            moveScript.walkSpeed = originalWalk;
            moveScript.runSpeed = originalRun;
            Debug.Log("🏃 Broke free! Speed restored.");
            
            playerTarget = null; 
        }
    }
}