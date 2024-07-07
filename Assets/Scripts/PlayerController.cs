using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask obstacleLayer;
    public ObstacleData obstacleData;
    public GridGenerator gridGenerator;

    private Vector3 targetPosition;
    private bool isMoving;
    private Queue<Vector3> pathQueue;
    private Rigidbody rb;

    void Start()
    {
        targetPosition = transform.position;
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component if it exists
    }

    void Update()
    {
        if (isMoving)
        {
            MoveAlongPath();
        }
        else
        {
            GetMouseInput();
        }
    }

    void GetMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TileInfo tileInfo = hit.collider.GetComponent<TileInfo>();
                if (tileInfo != null && !IsObstacle(tileInfo.GetPosition()))
                {
                    Debug.Log($"Clicked on tile: {tileInfo.GetPosition()}");
                    Vector2Int start = new Vector2Int((int)(transform.position.x / gridGenerator.spacing), (int)(transform.position.z / gridGenerator.spacing));
                    Vector2Int end = tileInfo.GetPosition();
                    List<Vector2Int> path = FindPath(start, end);
                    if (path != null)
                    {
                        pathQueue = new Queue<Vector3>();
                        foreach (Vector2Int point in path)
                        {
                            pathQueue.Enqueue(new Vector3(point.x * gridGenerator.spacing, 0, point.y * gridGenerator.spacing));
                        }
                        Debug.Log("Path found and movement started");
                        isMoving = true;
                    }
                    else
                    {
                        Debug.Log("No valid path found");
                    }
                }
            }
        }
    }

    void MoveAlongPath()
    {
        if (pathQueue.Count > 0)
        {
            Vector3 nextPosition = pathQueue.Peek();
            Debug.Log($"Moving towards {nextPosition}");

            if (rb != null)
            {
                rb.MovePosition(Vector3.MoveTowards(rb.position, nextPosition, moveSpeed * Time.deltaTime));
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);
            }

            if (Vector3.Distance(transform.position, nextPosition) < 0.01f)
            {
                Debug.Log($"Reached {nextPosition}");
                transform.position = nextPosition;
                pathQueue.Dequeue();
            }
        }
        else
        {
            Debug.Log("Finished moving");
            isMoving = false;
        }
    }

    bool IsObstacle(Vector2Int position)
    {
        int index = position.y * gridGenerator.gridSize + position.x;
        bool isObstacle = obstacleData.obstacles[index];
        Debug.Log($"Tile {position} is obstacle: {isObstacle}");
        return isObstacle;
    }

    List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> openList = new List<Vector2Int>();
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();

        openList.Add(start);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);

        Debug.Log($"Starting pathfinding from {start} to {end}");

        while (openList.Count > 0)
        {
            Vector2Int current = openList[0];
            foreach (Vector2Int node in openList)
            {
                if (fScore[node] < fScore[current])
                {
                    current = node;
                }
            }

            if (current == end)
            {
                Debug.Log("Path found");
                return ReconstructPath(cameFrom, current);
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach (Vector2Int neighbor in GetNeighbors(current))
            {
                if (closedList.Contains(neighbor) || IsObstacle(neighbor))
                {
                    continue;
                }

                float tentativeGScore = gScore[current] + 1; // Distance between current and neighbor is 1

                if (!openList.Contains(neighbor))
                {
                    openList.Add(neighbor);
                }
                else if (tentativeGScore >= gScore[neighbor])
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, end);
            }
        }

        Debug.Log("No path found");
        return null; // No path found
    }

    float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    List<Vector2Int> GetNeighbors(Vector2Int node)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbor = node + direction;
            if (neighbor.x >= 0 && neighbor.x < gridGenerator.gridSize && neighbor.y >= 0 && neighbor.y < gridGenerator.gridSize)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        List<Vector2Int> totalPath = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }
}
