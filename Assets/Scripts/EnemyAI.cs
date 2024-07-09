using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour, IAI
{
    public float moveSpeed = 5f;
    public ObstacleData obstacleData;
    public GridGenerator gridGenerator;
    private PlayerController playerController;
    private Vector3 targetPosition;
    private bool isMoving;
    private Queue<Vector3> pathQueue;
    private Rigidbody rb;

    public delegate void MovementFinishedHandler();
    public event MovementFinishedHandler OnMovementFinished;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isMoving)
        {
            MoveAlongPath();
        }
    }

    public void MoveTowards(Vector3 playerPosition)
    {
        if (isMoving) return;

        Vector2Int enemyPosition = new Vector2Int((int)(transform.position.x), (int)(transform.position.z));
        Vector2Int playerGridPosition = new Vector2Int((int)(playerPosition.x ), (int)(playerPosition.z ));

        List<Vector2Int> possibleTargets = GetDirectNeighbors(playerGridPosition);
        foreach (Vector2Int target in possibleTargets)
        {
            if (!IsObstacle(target) && target != enemyPosition)
            {
                List<Vector2Int> path = FindPath(enemyPosition, target);
                if (path != null && path.Count > 1)
                {
                    pathQueue = new Queue<Vector3>();
                    foreach (Vector2Int point in path)
                    {
                        pathQueue.Enqueue(new Vector3(point.x, 0, point.y));
                    }
                    pathQueue.Dequeue(); // Remove the first tile (current position)
                    isMoving = true;
                    return;
                }
            }
        }

        Debug.Log("No valid path found to move towards the player.");
        OnMovementFinished?.Invoke();
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
            OnMovementFinished?.Invoke();
        }
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

            foreach (Vector2Int neighbor in GetDirectNeighbors(current))
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

    List<Vector2Int> GetDirectNeighbors(Vector2Int node)
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

    bool IsObstacle(Vector2Int position)
    {
        int index = position.y * gridGenerator.gridSize + position.x;
        Vector2Int enemyPosition = new Vector2Int((int)(transform.position.x), (int)(transform.position.z));
        Vector2Int playerPosition = new Vector2Int((int)(playerController.transform.position.x ), (int)(playerController.transform.position.z));
        return obstacleData.obstacles[index] || enemyPosition == position || playerPosition == position;
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
