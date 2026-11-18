using UnityEngine;

public class StopMotionEffect : MonoBehaviour
{
    public int targetFPS = 15;
    private Animator anim;
    private float lastUpdateTime;

    void Start()
    {
        anim = GetComponent<Animator>();
        // Disable normal update so we can manualy pulse it
        anim.enabled = false; 
    }

    void Update()
    {
        float interval = 1f / targetFPS;
        if (Time.time >= lastUpdateTime + interval)
        {
            // Update the animation by the time passed since last 'step'
            anim.Update(Time.time - lastUpdateTime);
            lastUpdateTime = Time.time;
        }
    }
}