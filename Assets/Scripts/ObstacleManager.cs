using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public ObstacleData obstacleData;
    public GameObject obstaclePrefab;
    public GridGenerator gridGenerator; // Reference to the GridGenerator
    private int gridSize;
    //private float spacing;

    void Start()
    {
        gridSize = gridGenerator.gridSize; // Get the gridSize from GridGenerator
        //spacing = gridGenerator.spacing;  // Get the spacing from GridGenerator
        GenerateObstacles();
    }

    void GenerateObstacles()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                int index = y * gridSize + x;
                if (obstacleData.obstacles[index])
                {
                    Vector3 position = new Vector3(x, 0.2f, y); //For no space between obstacles
                    //Vector3 position = new Vector3(x * spacing, 0.5f, y * spacing); // for spacing
                    Instantiate(obstaclePrefab, position, Quaternion.identity);
                }
            }
        }
    }
}
