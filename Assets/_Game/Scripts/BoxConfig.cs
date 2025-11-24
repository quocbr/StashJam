using System;
using System.Collections.Generic;
using UnityEngine;

// Enum hướng sinh ra
public enum BoxDirection
{
    Up,
    Down,
    Left,
    Right
}

// Dữ liệu của một cái Box nằm trong hàng đợi (Stack)
[Serializable]
public class BoxStackData
{
    // Mỗi box trong stack cũng phải có danh sách item riêng của nó
    public List<int> itemIds = new List<int>();
    public bool isHidden = false;
}

[Serializable]
public class BoxConfig
{
    public int targetStackCount;
    public Vector2Int gridPos;

    public bool isHidden = false;

    // Item của chính cái Box đang đặt trên sàn (Spawner)
    public List<int> itemIds = new List<int>();
    public bool isLocked;

    // --- CẤU HÌNH STACK SPAWNER ---
    public bool isStackSpawner;
    public BoxDirection direction = BoxDirection.Up;

    // Danh sách các Box sẽ lần lượt sinh ra
    public List<BoxStackData> spawnStack = new List<BoxStackData>();
}