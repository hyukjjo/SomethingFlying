using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType
{
    Ground = 0,
    Air
}

[System.Serializable]
public class ObstacleData
{
    public GameObject ObstaclePrefab;
    public ObstacleType ObstacleType;
    public float MinimumX;
    public float MaximumX;
    public float MinimumY;
    public float MaximumY;
}


public class ObstacleController : MonoBehaviour
{
    public int InitialObstaclesCount = 0;

    [SerializeField]
    public ObstacleData[] ObstacleDatas;

    void Start()
    {
        GenerateInitialObstacles();
    }

    void GenerateInitialObstacles()
    {
        for (int i = 0; i < InitialObstaclesCount; i++)
        {
            // Randomly select an obstacle data
            int randomIndex = Random.Range(0, ObstacleDatas.Length);
            ObstacleData selectedObstacleData = ObstacleDatas[randomIndex];

            // Random position between MinimumHeight and MaximumHeight
            float randomX = Random.Range(selectedObstacleData.MinimumX, selectedObstacleData.MaximumX);
            float randomY = Random.Range(selectedObstacleData.MinimumY, selectedObstacleData.MaximumY);
            Vector3 spawnPosition = new Vector3(randomX, randomY, transform.position.z);

            // Instantiate the obstacle prefab
            Instantiate(selectedObstacleData.ObstaclePrefab, spawnPosition, Quaternion.identity, transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
