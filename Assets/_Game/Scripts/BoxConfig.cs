using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoxConfig
{
    public Vector2Int gridPos;

    // Mỗi item bên trong box là một ID (int) trỏ sang ItemDefinition
    public List<int> itemIds = new List<int>();

    public bool isLocked;
}