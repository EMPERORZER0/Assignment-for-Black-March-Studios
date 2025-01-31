using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject cubePrefab;
    public int gridSize = 10;
    //public float spacing = 1f;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                //Vector3 position = new Vector3(x * spacing, 0, y * spacing);
                Vector3 position = new Vector3(x , 0, y); //For no space between cubes
                GameObject cube = Instantiate(cubePrefab, position, Quaternion.Euler(-90f, 0f, 0f));
                cube.name = $"Cube_{x+1}_{y+1}";
                cube.AddComponent<TileInfo>().SetPosition(x, y);
            }
        }
    }
}
