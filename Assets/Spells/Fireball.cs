using UnityEngine;

public class FireballEffect : MonoBehaviour
{
    [Header("Bullet Stats")]
    public float speed = 15f;
    public float lifeTime = 1.5f;

    [Header("Impact & Decay Dynamics")]
    public float shrinkDuration = 0.3f; // How long it spends shrinking before death
    public float groundFriction = 20f;  // How violently it decelerates when sliding

    private float age = 0f;
    private Vector3 originalScale;
    private bool isGrounded = false;

    void Start()
    {
        // Store the starting size so we know what to shrink down from
        originalScale = transform.localScale;
    }

    void Update()
    {
        age += Time.deltaTime; // The fireball ages every frame

        // 1. FRICTION LOGIC: If it hit the floor, bleed off speed
        if (isGrounded)
        {
            // MoveTowards smoothly drops the speed to 0 without going into negative numbers (backward)
            speed = Mathf.MoveTowards(speed, 0f, groundFriction * Time.deltaTime);
        }

        // 2. MOVEMENT LOGIC
        transform.Translate(Vector2.right * speed * Time.deltaTime);

        // 3. DECAY LOGIC: Are we in the final moments of its life?
        float timeRemaining = lifeTime - age;
        
        if (timeRemaining <= shrinkDuration)
        {
            // Calculate a percentage (1.0 to 0.0) of how much time is left in the shrink window
            float shrinkRatio = Mathf.Clamp01(timeRemaining / shrinkDuration);
            transform.localScale = originalScale * shrinkRatio; // Apply the shrink
        }

        // 4. EXECUTION
        if (age >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check what we hit. 
        if (collision.CompareTag("Ground"))
        {
            // Hit the floor: Don't die, just start sliding and slowing down
            isGrounded = true;
        }
        else 
        {
            // Hit a wall, enemy, or anything else: Die instantly
            Debug.Log("Fireball shattered on impact with: " + collision.name);
            Destroy(gameObject);
        }
    }
}