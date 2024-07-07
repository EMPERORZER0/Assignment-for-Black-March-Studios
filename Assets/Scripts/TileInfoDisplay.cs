using UnityEngine;
using TMPro;

public class TileInfoDisplay : MonoBehaviour
{
    public TextMeshProUGUI tileInfoText;

    void Update()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            TileInfo tileInfo = hit.collider.GetComponent<TileInfo>();
            if (tileInfo != null)
            {
                Vector2Int position = tileInfo.GetPosition();
                tileInfoText.text = $"Tile: ({position.x+1}, {position.y + 1})";
            }
        }
    }
}
