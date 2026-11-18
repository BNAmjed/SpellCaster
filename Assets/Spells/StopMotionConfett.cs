using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StopMotionParticles : MonoBehaviour
{
    [Header("Animation Feel")]
    [Tooltip("12 FPS gives a crunchy, retro anime feel. 60 is smooth.")]
    public int targetFPS = 12; 
    
    private ParticleSystem ps;
    private float timer;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        
        // Pause the smooth Unity physics engine immediately. 
        ps.Pause(); 

        // --- GARBAGE COLLECTION LOGIC ---
        // Find out the absolute maximum time a particle is allowed to live
        float maxLife = ps.main.startLifetime.constantMax;
        float duration = ps.main.duration;
        
        // Destroy this entire GameObject after the particles are guaranteed to be dead.
        // We add a tiny 0.1f buffer just to be safe.
        Destroy(gameObject, maxLife + duration + 0.1f);
    }

    void Update()
    {
        timer += Time.deltaTime;
        float frameStep = 1f / targetFPS;

        // If enough time has passed for one "crunchy" frame...
        while (timer >= frameStep)
        {
            // Shove the simulation forward by exactly one frame, then pause again.
            ps.Simulate(frameStep, true, false); 
            timer -= frameStep;
        }
    }
}