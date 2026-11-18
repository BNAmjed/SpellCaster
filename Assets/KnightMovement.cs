using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SpellCasterController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    private CharacterController controller;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource jumpSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip jumpClothClip;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] private Vector2 volumeRange = new Vector2(0.8f, 1.0f);
    private bool stepPlayedThisCycle = false;

    [Header("Mouse Look")]
    public float lookSensitivity = 2f;
    public float maxLookDownAngle = 45f;
    public float maxLookUpAngle = -90f;
    private float cameraPitch = 0f;

    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    public float speedTransitionLerp = 10f;
    private float currentSpeed;

    [Header("Jump & Snappy Physics")]
    public float jumpHeight = 1.2f;
    public float gravity = -35f;
    private Vector3 velocity;
    private bool wasGrounded;

    [Header("Procedural Head Bob")]
    // HOW TO ADJUST STEP FREQUENCY: 
    // The higher you set walkBobFreq and runBobFreq, the faster the sine wave cycles, 
    // which automatically triggers footsteps more frequently. Sync this with your speed!
    public float walkBobFreq = 10f;
    public float walkBobAmp = 0.05f;
    public float runBobFreq = 15f;
    public float runBobAmp = 0.1f;
    private float bobTimer = 0f;
    private float currentBobFreq;
    private float currentBobAmp;
    private Vector3 defaultCameraLocalPos;

    [Header("The 'Nervous' Twitch")]
    public float maxTwitchIntensity = 0.05f;
    public float minTwitchInterval = 1f;
    public float maxTwitchInterval = 4f;
    private float twitchTimer;
    private Vector3 currentTwitchOffset;

    [Header("Landing Impact")]
    public float landingDipAmount = 0.15f;
    public float landingTiltAngle = 3f;
    public float landingDuration = 0.3f;
    private bool isLanding;
    private Vector3 landingPosOffset;
    private float landingRotOffset;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null)
            defaultCameraLocalPos = playerCamera.transform.localPosition;

        twitchTimer = Random.Range(minTwitchInterval, maxTwitchInterval);
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovementAndJump();
        HandleProceduralCamera();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, maxLookUpAngle, maxLookDownAngle);
    }

    private void HandleMovementAndJump()
    {
        bool isGrounded = controller.isGrounded;

        // 1. Landing Audio & Impact Logic
        if (isGrounded && !wasGrounded)
        {
            StartCoroutine(LandingImpactRoutine());
            
            // Play a footstep slightly louder (1.3x multiplier) for the landing thud
            PlayFootstep(1.3f); 
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 moveInput = transform.right * moveX + transform.forward * moveZ;

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = (isRunning && moveInput.magnitude > 0) ? runSpeed : walkSpeed;
        
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedTransitionLerp);
        controller.Move(moveInput * currentSpeed * Time.deltaTime);

        // 2. Jump Audio & Physics Logic
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            // Play cloth rustle without cutting off existing footsteps
            if (jumpClothClip != null && jumpSource != null)
            {
                jumpSource.PlayOneShot(jumpClothClip, 1f);
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        wasGrounded = isGrounded;
    }

    private void HandleProceduralCamera()
    {
        if (playerCamera == null) return;

        Vector3 bobOffset = Vector3.zero;
        Vector3 moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (moveInput.magnitude > 0.1f && controller.isGrounded && !isLanding)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float targetFreq = isRunning ? runBobFreq : walkBobFreq;
            float targetAmp = isRunning ? runBobAmp : walkBobAmp;

            currentBobFreq = Mathf.Lerp(currentBobFreq, targetFreq, Time.deltaTime * speedTransitionLerp);
            currentBobAmp = Mathf.Lerp(currentBobAmp, targetAmp, Time.deltaTime * speedTransitionLerp);

            bobTimer += Time.deltaTime * currentBobFreq;
            
            // Generate the Sine wave value
            float sinValue = Mathf.Sin(bobTimer);
            bobOffset.y = sinValue * currentBobAmp;
            bobOffset.x = Mathf.Cos(bobTimer / 2f) * (currentBobAmp / 2f); 

            // 3. Audio Synchronization Logic
            // The sine wave drops from 1 to -1. When it hits the bottom (-0.95), the foot hits the ground.
            if (sinValue < -0.95f && !stepPlayedThisCycle)
            {
                PlayFootstep(1f);
                stepPlayedThisCycle = true; // Lock it so it doesn't fire 50 times at the bottom of the curve
            }
            // Reset the lock when the head starts moving back up
            else if (sinValue > 0f)
            {
                stepPlayedThisCycle = false;
            }
        }
        else
        {
            bobTimer = 0f;
            currentBobFreq = Mathf.Lerp(currentBobFreq, 0, Time.deltaTime * speedTransitionLerp);
            currentBobAmp = Mathf.Lerp(currentBobAmp, 0, Time.deltaTime * speedTransitionLerp);
            stepPlayedThisCycle = false; // Reset lock when standing still
        }

        twitchTimer -= Time.deltaTime;
        if (twitchTimer <= 0)
        {
            currentTwitchOffset = new Vector3(
                Random.Range(-1f, 1f), 
                Random.Range(-1f, 1f), 
                0) * maxTwitchIntensity;
                
            twitchTimer = Random.Range(minTwitchInterval, maxTwitchInterval);
        }
        currentTwitchOffset = Vector3.Lerp(currentTwitchOffset, Vector3.zero, Time.deltaTime * 15f);

        playerCamera.transform.localPosition = defaultCameraLocalPos + bobOffset + currentTwitchOffset + landingPosOffset;
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch + landingRotOffset, 0f, 0f);
    }

    private void PlayFootstep(float volumeMultiplier)
    {
        // Safety checks
        if (footstepClips == null || footstepClips.Length == 0 || footstepSource == null) return;

        // Pick a random clip
        int randomIndex = Random.Range(0, footstepClips.Length);
        footstepSource.clip = footstepClips[randomIndex];

        // Randomize pitch and volume for organic variation
        footstepSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        footstepSource.volume = Random.Range(volumeRange.x, volumeRange.y) * volumeMultiplier;

        // Play the sound
        footstepSource.Play();
    }

    private IEnumerator LandingImpactRoutine()
    {
        isLanding = true;
        float elapsed = 0f;

        float dipDuration = landingDuration * 0.3f;
        while (elapsed < dipDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dipDuration;
            landingPosOffset.y = Mathf.Lerp(0, -landingDipAmount, t);
            landingRotOffset = Mathf.Lerp(0, landingTiltAngle, t);
            yield return null;
        }

        elapsed = 0f;
        float recoverDuration = landingDuration * 0.7f;
        while (elapsed < recoverDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / recoverDuration;
            landingPosOffset.y = Mathf.Lerp(-landingDipAmount, 0, t);
            landingRotOffset = Mathf.Lerp(landingTiltAngle, 0, t);
            yield return null;
        }

        landingPosOffset = Vector3.zero;
        landingRotOffset = 0f;
        isLanding = false;
    }
}