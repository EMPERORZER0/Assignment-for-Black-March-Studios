using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public ObstacleData obstacleData;
    public GridGenerator gridGenerator;
    private EnemyAI enemyAI;

    private Vector3 targetPosition;
    private bool isMoving;
    private Queue<Vector3> pathQueue;
    private Rigidbody rb;
    private bool isPlayerTurn = true;

    void Start()
    {
        targetPosition = transform.position;
        rb = GetComponent<Rigidbody>();
        enemyAI = FindObjectOfType<EnemyAI>();
        enemyAI.OnMovementFinished += EnablePlayerTurn;
    }

    void Update()
    {
        if (isMoving)
        {
            MoveAlongPath();
        }
        else if (isPlayerTurn)
        {
            GetMouseInput();
        }
    }

    void GetMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Cast a ray from the mouse position
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TileInfo tileInfo = hit.collider.GetComponent<TileInfo>();
                if (tileInfo != null && !IsObstacle(tileInfo.GetPosition()))
                {
                    Vector2Int start = new Vector2Int((int)(transform.position.x ), (int)(transform.position.z ));
                    Vector2Int end = tileInfo.GetPosition();
                    List<Vector2Int> path = FindPath(start, end); //Find the path using A* Algorithm
                    if (path != null)
                    {
                        pathQueue = new Queue<Vector3>();
                        foreach (Vector2Int point in path)
                        {
                            pathQueue.Enqueue(new Vector3(point.x, 0, point.y));
                        }
                        isMoving = true;
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
                transform.position = nextPosition;
                pathQueue.Dequeue();
            }
        }
        else
        {
            isMoving = false;
            isPlayerTurn = false;
            enemyAI.MoveTowards(transform.position); // Trigger enemy movement
        }
    }

    // A* pathfinding algorithm
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

                float tentativeGScore = gScore[current] + 1;

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

    // Reconstruct the path from the cameFrom dictionary
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

    // Check if a position is an obstacle
    bool IsObstacle(Vector2Int position)
    {
        int index = position.y * gridGenerator.gridSize + position.x;
        Vector2Int enemyPosition = new Vector2Int((int)(enemyAI.transform.position.x ), (int)(enemyAI.transform.position.z ));
        return obstacleData.obstacles[index] || enemyPosition == position;
    }

    void EnablePlayerTurn()
    {
        isPlayerTurn = true;
    }

    private void OnDestroy()
    {
        if (enemyAI != null)
        {
            enemyAI.OnMovementFinished -= EnablePlayerTurn;
        }
    }
}
