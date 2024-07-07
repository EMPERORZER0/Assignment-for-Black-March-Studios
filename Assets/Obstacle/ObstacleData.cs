using UnityEngine;

[CreateAssetMenu(fileName = "NewObstacleData", menuName = "Obstacle Data", order = 51)]
public class ObstacleData : ScriptableObject
{
    public bool[] obstacles = new bool[100];
}
