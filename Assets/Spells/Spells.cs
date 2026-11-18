using UnityEngine;

public class MagicController : MonoBehaviour
{
    [Header("Spawn Location")]
    public Transform handSpawnPoint;

    [Header("Ammunition")]
    public GameObject icePrefab; // Drag your ice particle/prefab here
    public GameObject fireballPrefab;
    public GameObject confettiPrefab;
    public GameObject dustPrefab;

    // --- NEW: Called by the SpellCaster when "Ice" + "Fear" is detected ---
    public void CastIce()
    {
        // false = throw it as a projectile/effect (don't attach to hand)
        CastSpell(icePrefab, false);
    }

    // Called by the SpellCaster when everything is correct
    public void CastFireball()
    {
        CastSpell(fireballPrefab, false);
    }

    // Called by the SpellCaster when the spell fails
    public void CastFizzle()
    {
        int coinFlip = Random.Range(0, 2);

        if (coinFlip == 0)
        {
            CastSpell(confettiPrefab, true);
        }
        else
        {
            CastSpell(dustPrefab, true);
        }
    }

    private void CastSpell(GameObject spellToCast, bool attachToHand)
    {
        if (handSpawnPoint == null || spellToCast == null) return;

        GameObject spawnedSpell = Instantiate(spellToCast, handSpawnPoint.position, handSpawnPoint.rotation);

        if (attachToHand)
        {
            spawnedSpell.transform.SetParent(handSpawnPoint);
        }
    }
}