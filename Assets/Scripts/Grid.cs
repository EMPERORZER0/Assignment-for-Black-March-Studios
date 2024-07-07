using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject cubePrefab;
    public int gridSize = 10;
    public float spacing = 1f;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 1; x < gridSize+1; x++)
        {
            for (int y = 1; y < gridSize+1; y++)
            {
                Vector3 position = new Vector3(x * spacing, 0, y * spacing);
                //Vector3 position = new Vector3(x , 0, y); //For no space between cubes
                GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
                cube.name = $"Cube_{x}_{y}";
                cube.AddComponent<TileInfo>().SetPosition(x, y);
            }
        }
    }
}
