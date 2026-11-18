using UnityEngine;

public class SkeletonTripwire : MonoBehaviour
{
    [Tooltip("Drag the Skeleton body here so the tripwire knows what to trigger")]
    public AnimatedSkeletonTrap targetSkeleton;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (targetSkeleton != null)
            {
                // We now pass the 'other.transform' (the player) to the skeleton!
                targetSkeleton.FireAnimation(other.transform);
            }
        }
    }
}