using UnityEngine;

public class TrainingAreaTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            if (WizardEncounterManager.Instance != null && WizardEncounterManager.Instance.isWaitingInTrainingArea)
            {
                WizardEncounterManager.Instance.StartFireTraining();
            }
        }
    }
}