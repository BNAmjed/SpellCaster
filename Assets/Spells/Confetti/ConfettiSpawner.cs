using UnityEngine;

public class CustomConfettiBurst : MonoBehaviour
{
    [Header("Ammunition")]
    public GameObject confettiPiecePrefab; 
    public int pieceCount = 25;            
    public float burstForce = 7f;          

    void Start()
    {
        Detonate();
        
        // Clean up the spawner object after 4 seconds
        Destroy(gameObject, 4f); 
    }

    void Detonate()
    {
        for (int i = 0; i < pieceCount; i++)
        {
            // 1. Manufacture the piece
            GameObject piece = Instantiate(confettiPiecePrefab, transform.position, Quaternion.identity);

            // 2. Paint it AUTOMATICALLY
            SpriteRenderer sr = piece.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // Random.ColorHSV generates a random color based on Hue, Saturation, and Value.
                // We lock Saturation and Value to exactly 1 so it's always a bright, 100% pure neon color, never dull or gray.
                sr.color = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
            }

            // 3. Apply chaotic physics
            Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Random direction and force
                Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f)).normalized;
                rb.AddForce(randomDir * Random.Range(burstForce * 0.5f, burstForce), ForceMode2D.Impulse);
                
                // Random spin
                rb.AddTorque(Random.Range(-15f, 15f), ForceMode2D.Impulse);
            }

            // 4. Set expiration date
            Destroy(piece, Random.Range(1.5f, 3.0f));
        }
    }
}