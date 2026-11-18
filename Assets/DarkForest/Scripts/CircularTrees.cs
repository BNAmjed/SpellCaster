using UnityEngine;

public class CircularForestGenerator : MonoBehaviour
{
    [Header("Tree Prefabs")]
    [Tooltip("Drag one or more tree prefabs here. It will pick randomly!")]
    public GameObject[] treePrefabs;

    [Header("Forest Shape")]
    [Tooltip("How many circles of trees to make radiating outward")]
    public int numberOfRings = 5;
    
    [Tooltip("The distance between each circle")]
    public float radiusSpacing = 5f;
    
    [Tooltip("How many trees to put on the very first, innermost circle")]
    public int treesOnInnerRing = 6;

    [Header("Organic Chaos")]
    [Tooltip("How far a tree can randomly wander off its perfect circular track")]
    public float positionJitter = 1.5f;
    [Tooltip("Randomize tree size slightly?")]
    public bool randomizeScale = true;

    [Header("Ground Snapping")]
    [Tooltip("How high up to start looking for the ground")]
    public float raycastHeight = 50f;
    public LayerMask groundLayer = ~0; // Default to all layers

    // 🔥 This allows you to right-click the script in the Inspector and hit "Generate Forest" 🔥
    [ContextMenu("Generate Forest")]
    public void GenerateTrees()
    {
        if (treePrefabs == null || treePrefabs.Length == 0)
        {
            Debug.LogError("🌲 You forgot to add Tree Prefabs to the Generator!");
            return;
        }

        // Clean up any old trees if you are regenerating
        ClearForest();

        Vector3 centerPoint = transform.position;

        // Loop outward, ring by ring
        for (int currentRing = 1; currentRing <= numberOfRings; currentRing++)
        {
            float currentRadius = currentRing * radiusSpacing;
            
            // Multiply trees by the ring number so the outer circles stay dense!
            int treesInThisRing = treesOnInnerRing * currentRing; 
            float angleStep = 360f / treesInThisRing;

            for (int i = 0; i < treesInThisRing; i++)
            {
                // Calculate the perfect circle angle
                float angleDegrees = i * angleStep;
                
                // Add a random rotation offset so the circles aren't perfectly aligned
                angleDegrees += Random.Range(-angleStep / 3f, angleStep / 3f);
                
                // Convert degrees to radians for the math
                float angleRadians = angleDegrees * Mathf.Deg2Rad;

                // Find the perfect X and Z position on the circle
                float xPos = centerPoint.x + Mathf.Cos(angleRadians) * currentRadius;
                float zPos = centerPoint.z + Mathf.Sin(angleRadians) * currentRadius;

                // Add organic jitter
                xPos += Random.Range(-positionJitter, positionJitter);
                zPos += Random.Range(-positionJitter, positionJitter);

                Vector3 spawnPos = new Vector3(xPos, centerPoint.y + raycastHeight, zPos);

                // Snap the tree perfectly to the terrain/floor
                if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayer))
                {
                    SpawnTree(hit.point);
                }
                else
                {
                    // Fallback if there's no collider below it
                    SpawnTree(new Vector3(xPos, centerPoint.y, zPos));
                }
            }
        }
    }

    [ContextMenu("Clear Forest")]
    public void ClearForest()
    {
        // Destroy all child objects (trees) to give you a clean slate
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void SpawnTree(Vector3 position)
    {
        // Pick a random tree from your array
        GameObject randomTreePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
        
        // Spawn it
        GameObject newTree = Instantiate(randomTreePrefab, position, Quaternion.identity);
        
        // Parent it to this generator so your hierarchy doesn't get messy
        newTree.transform.parent = this.transform;

        // Randomize the Y rotation so they don't all face the exact same way
        newTree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);

        // Slightly randomize the scale to make it look natural
        if (randomizeScale)
        {
            float randomScale = Random.Range(0.8f, 1.2f);
            newTree.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        }
    }

    // Draws visible circles in the Unity Editor so you can preview the size!
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        for (int i = 1; i <= numberOfRings; i++)
        {
            Gizmos.DrawWireSphere(transform.position, i * radiusSpacing);
        }
    }
}