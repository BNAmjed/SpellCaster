using UnityEngine;

public class WizardEyeFollow : MonoBehaviour
{
    [Header("Tuning")]
    public float lookSpeed = 5f;
    
    [Tooltip("If the eyes look up instead of forward, change X to 90 or -90")]
    public Vector3 rotationOffset = new Vector3(90, 0, 0); 

    private Quaternion initialLocalRotation;

    void Start()
    {
        initialLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        if (WizardEncounterManager.Instance == null || 
            WizardEncounterManager.Instance.playerTransform == null || 
            !WizardEncounterManager.Instance.isLookingAtPlayer)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, initialLocalRotation, Time.deltaTime * lookSpeed);
            return;
        }

        Vector3 targetPos = WizardEncounterManager.Instance.playerTransform.position;
        Vector3 directionToPlayer = targetPos - transform.position;
        
        // Calculate where to look, then add the manual offset to fix the "Shy" rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(rotationOffset);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lookSpeed);
    }
}