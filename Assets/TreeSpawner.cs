using UnityEngine;

public class SimpleValleyForest : MonoBehaviour
{
    [Header("Asset Setup")]
    public GameObject treePrefab;
    public int forestLength = 100;
    public int forestWidth = 50;
    public float density = 5f; // Space between trees

    [Header("The Valley Logic")]
    [Tooltip("How much the trees rise as they move away from the center (X=0)")]
    public float heightMultiplier = 0.5f; 
    [Tooltip("Width of the clear path in the middle")]
    public float pathWidth = 6f;

    void Start()
    {
        if (treePrefab == null) return;
        GenerateForest();
    }

    void GenerateForest()
    {
        for (float z = 0; z < forestLength; z += density)
        {
            for (float x = -forestWidth / 2f; x < forestWidth / 2f; x += density)
            {
                // 1. Calculate distance from the center path
                float distanceFromPath = Mathf.Abs(x);

                // 2. Skip spawning if we are inside the path zone
                if (distanceFromPath < pathWidth / 2f) continue;

                // 3. Simple Math: The further from 0, the higher the Y
                // We subtract the path width so the elevation starts at the edge of the path
                float elevation = (distanceFromPath - (pathWidth / 2f)) * heightMultiplier;
                
                Vector3 spawnPos = new Vector3(x, elevation, z);

                // 4. Randomize Position slightly so it doesn't look like a grid
                spawnPos.x += Random.Range(-1f, 1f);
                spawnPos.z += Random.Range(-1f, 1f);

                // 5. Spawn and Scale
                GameObject tree = Instantiate(treePrefab, spawnPos, Quaternion.identity);
                tree.transform.SetParent(this.transform);

                // 6. Scale Logic: Original (1) +/- 0.2f
                float randomScale = 1.0f + Random.Range(-0.2f, 0.2f);
                tree.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

                // 7. Random Rotation (so they don't look identical)
                tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
        }
    }
}