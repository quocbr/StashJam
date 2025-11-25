using System;
using System.Collections.Generic;
using UnityEngine;

// --- ENUM DEFINITIONS ---
public enum BoxDirection
{
    Up,
    Down,
    Left,
    Right
}

public enum LockType
{
    None = 0,
    Yellow = 1,
    Red = 2
}

public enum KeyLockType
{
    None = 0,
    Yellow = 1,
    Red = 2
}

// --- DATA CLASSES ---

// Dữ liệu của một cái Box nằm trong hàng đợi (Stack)
[Serializable]
public class BoxStackData
{
    public List<int> itemIds = new List<int>();
    public bool isHidden = false;

    // --- LOCK & KEYLOCK ---
    public bool hasLock;
    public LockType lockType;

    public bool hasKeyLock;
    public KeyLockType keyLockType;
}

[Serializable]
public class BoxConfig
{
    public int targetStackCount;
    public Vector2Int gridPos;
    public bool isHidden = false;

    // Item của chính cái Box đang đặt trên sàn
    public List<int> itemIds = new List<int>();

    // --- LOCK & KEYLOCK ---
    // (Đã thay thế bool isLocked cũ bằng hệ thống mới chi tiết hơn)
    public bool hasLock;
    public LockType lockType;

    public bool hasKeyLock;
    public KeyLockType keyLockType;

    // --- CẤU HÌNH STACK SPAWNER ---
    public bool isStackSpawner;
    public BoxDirection direction = BoxDirection.Up;

    public List<BoxStackData> spawnStack = new List<BoxStackData>();
}