using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// The main Spawner behaviour.
/// </summary>
public class Spawner : MonoBehaviour
{
    /// <summary>
    /// Should we spawn obstacles?
    /// </summary>
    public bool spawnObstacles = true;

    /// <summary>
    /// Mean frequency of spawning as n per second.
    /// </summary>
    public float spawnFrequencyMean = 1.0f;
    
    /// <summary>
    /// Standard deviation of the frequency of spawning as n per second.
    /// </summary>
    public float spawnFrequencyStd = 0.5f;
    
    /// <summary>
    /// Position offset of the spawned obstacles.
    /// </summary>
    public float3 spawnOffset = new float3(0.0f, 0.0f, 0.0f);
    
    /// <summary>
    /// Size of the spawned obstacles.
    /// </summary>
    public float spawnSizeMin = 1.0f;
    public float spawnSizeMax = 1.0f;
    
    /// <summary>
    /// Layer used for the spawned obstacles.
    /// </summary>
    public string spawnLayer = "Obstacle";

    /// <summary>
    /// Prefab used for the spawned obstacles.
    /// </summary>
    public GameObject obstaclePrefab;

    /// <summary>
    /// Accumulated time since the last spawn in seconds.
    /// </summary>
    private float spawnAccumulator = 0.0f;

    /// <summary>
    /// Number of seconds since the last spawn.
    /// </summary>
    private float nextSpawnIn = 0.0f;

    /// <summary>
    /// Called before the first frame update.
    /// </summary>
    void Start()
    { ResetSpawn(); }

    /// <summary>
    /// Update called once per frame.
    /// </summary>
    void Update()
    {
        if (spawnObstacles)
        { // Check if we should spawn.
            spawnAccumulator += Time.deltaTime;
            if (spawnAccumulator >= nextSpawnIn)
            { // Spawn at most one obstacle per frame.
                spawnAccumulator -= nextSpawnIn;
                nextSpawnIn = RandomNormal(spawnFrequencyMean, spawnFrequencyStd);
                float size = Random.Range(spawnSizeMin, spawnSizeMax);
                SpawnObstacle(transform.position, size, RandomBool());
            }
        }
    }

    /// <summary>
    /// Spawn obstacle if there is enough space.
    /// </summary>
    public void SpawnObstacle(Vector2 position, float spawnSize, bool spawnDown, int layer = 0)
    {
        // Spawn the obstacle.
        var obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity);

        // Move it to the target location.
        obstacle.transform.position += (Vector3)(spawnDown ? 
            spawnOffset + (1.0f - spawnSize) / 2.0f : 
            -spawnOffset - (1.0f - spawnSize) / 2.0f
        );
        
        // Scale it.
        obstacle.transform.localScale = new Vector3(spawnSize, spawnSize, spawnSize);
        
        // Move the obstacle into the correct layer.
        obstacle.layer = LayerMask.NameToLayer(spawnLayer);
        
        if(layer == 0 && Random.Range(0f,1f) < 0.5f) SpawnObstacle(position + new Vector2(spawnSize,0), spawnSize, spawnDown);
        if(layer < 3 && spawnSize<0.5f && Random.Range(0f,1f) < 1-spawnSize) SpawnObstacle(position + new Vector2(0,spawnSize * (spawnDown ? -1:1)), spawnSize, spawnDown, layer+1);
    }
    

    /// <summary>
    /// Clear all currently generated obstacles.
    /// </summary>
    public void ClearObstacles()
    {
        // Get obstacle layer to filter with.
        var obstacleLayer = LayerMask.NameToLayer(spawnLayer);
        foreach (Transform child in transform)
        { // Go through all children and destroy any obstacle found.
            if (child.gameObject.layer == obstacleLayer) 
            { Destroy(child.gameObject); }
        }
    }
    
    /// <summary>
    /// Reset the spawner to default state.
    /// </summary>
    public void ResetSpawn()
    {
        spawnAccumulator = 0.0f;
        nextSpawnIn = RandomNormal(spawnFrequencyMean, spawnFrequencyStd);
    }

    /// <summary>
    /// Modify current speed of all of the obstacles.
    /// </summary>
    public void ModifyObstacleSpeed(float multiplier)
    {
        // Get obstacle layer to filter with.
        var obstacleLayer = LayerMask.NameToLayer(spawnLayer);
        // Modify only the x-axis movement.
        var xMultiplier = new Vector2(multiplier, 1.0f);
        foreach (Transform child in transform)
        { // Iterate through all children, modifying current speed of obstacles.
            if (child.gameObject.layer == obstacleLayer) 
            { child.GetComponent<Rigidbody2D>().velocity *= xMultiplier; }
        }
    }

    /// <summary>
    /// Simple RNG for Normal distributed numbers with given
    /// mean and standard deviation.
    /// </summary>
    /// <param name="mean">Mean of the generated values.</param>
    /// <param name="std">Standard deviation of the generated values.</param>
    /// <returns>Returns random value from the normal distribution.</returns>
    public static float RandomNormal(float mean, float std)
    {
        var v1 = 1.0f - Random.value;
        var v2 = 1.0f - Random.value;
        
        var standard = Math.Sqrt(-2.0f * Math.Log(v1)) * Math.Sin(2.0f * Math.PI * v2);
        
        return (float)(mean + std * standard);
    }
    
    /// <summary>
    /// Generate a random bool - coin flip.
    /// </summary>
    /// <returns>Return a random boolean value.</returns>
    public static bool RandomBool()
    { return Random.value >= 0.5; }
}
