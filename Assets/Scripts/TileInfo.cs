using UnityEngine;

public class TileInfo : MonoBehaviour
{
    public int x, y;

    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector2Int GetPosition()
    {
        return new Vector2Int(x, y);
    }
}
