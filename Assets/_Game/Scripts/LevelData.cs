using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Level/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Grid size")]
    public int width = 7;
    public int height = 7;

    [Header("Boxes in this level")]
    public List<BoxConfig> boxes = new List<BoxConfig>();

    /// <summary>
    /// Tìm BoxConfig tại vị trí (x, y) trên grid. Không có thì trả về null.
    /// </summary>
    public BoxConfig GetBoxAt(int x, int y)
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].gridPos.x == x && boxes[i].gridPos.y == y)
                return boxes[i];
        }
        return null;
    }
}

#if UNITY_EDITOR
#endif


