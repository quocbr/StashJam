using System;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;

public class Level : MonoBehaviour
{
    [Header("Data")] public LevelData levelData; // asset LevelData

    public ItemDatabase itemDatabase;

    [Header("Grid Settings")] public float cellSize = 1f;

    public Vector3 gridOrigin = Vector3.zero;

    [Header("Prefabs")] public Stash boxPrefab;
    public Stash stackPrefab;

    [Header("Runtime")] [Tooltip("Root chứa toàn bộ box của level hiện tại. Nếu để trống sẽ tự tạo.")]
    public Transform levelRoot;

    [SerializeField] private GameObject[] obj;

    [Header("Prefabs - Diagonal Borders (16 Cases)")]
    [InfoBox("Index tính theo Bitmask: TL=1, TR=2, BL=4, BR=8. Tổng cộng 16 trường hợp.")]
    public GameObject[] cornerBorders;

    public List<Stash> Stash = new List<Stash>();
    public Transform max;
    public Transform min;

    private int[,] levelIndexMatrix;

    public Stash[,] stashGrid;

    private void OnEnable()
    {
        EventManager.AddListener<OnStashDestroy>(OnStashPickCallBack);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnStashDestroy>(OnStashPickCallBack);
    }

    private void OnStashPickCallBack(OnStashDestroy onStashPick)
    {
        if (onStashPick.Stash.IsStackSpawner) return;
        if (onStashPick.Stash.index.x - 1 >= 0 && stashGrid[onStashPick.Stash.index.x - 1, onStashPick.Stash.index.y])
        {
            stashGrid[onStashPick.Stash.index.x - 1, onStashPick.Stash.index.y].SetCanPick(true);
        }

        if (onStashPick.Stash.index.x + 1 < stashGrid.GetLength(0) &&
            stashGrid[onStashPick.Stash.index.x + 1, onStashPick.Stash.index.y])
        {
            stashGrid[onStashPick.Stash.index.x + 1, onStashPick.Stash.index.y].SetCanPick(true);
        }

        if (onStashPick.Stash.index.y - 1 >= 0 &&
            stashGrid[onStashPick.Stash.index.x, onStashPick.Stash.index.y - 1])
        {
            stashGrid[onStashPick.Stash.index.x, onStashPick.Stash.index.y - 1].SetCanPick(true);
        }

        if (onStashPick.Stash.index.y + 1 < stashGrid.GetLength(1) &&
            stashGrid[onStashPick.Stash.index.x, onStashPick.Stash.index.y + 1])
        {
            stashGrid[onStashPick.Stash.index.x, onStashPick.Stash.index.y + 1].SetCanPick(true);
        }

        Stash.Remove(onStashPick.Stash);
    }

    [Button]
    private void Test()
    {
        SpawnLevel();
    }

    public void Init()
    {
        int[,] originalMatrix = new int[levelData.height, levelData.width];
        InitializeMatrix(originalMatrix, levelData.height, levelData.width, -1);

        for (int i = 0; i < levelData.boxes.Count; i++)
        {
            var boxCfg = levelData.boxes[i];
            Vector2Int pos = boxCfg.gridPos;

            if (pos.y >= 0 && pos.y < levelData.height &&
                pos.x >= 0 && pos.x < levelData.width)
            {
                originalMatrix[pos.y, pos.x] = i;
            }
        }

        int newHeight = levelData.height + 1;
        int newWidth = levelData.width + 2;

        int[,] paddedMatrix = new int[newHeight, newWidth];
        InitializeMatrix(paddedMatrix, newHeight, newWidth, -1);

        for (int y = 0; y < levelData.height; y++)
        {
            for (int x = 0; x < levelData.width; x++)
            {
                int boxIndex = originalMatrix[y, x];
                if (boxIndex != -1)
                {
                    int newY = y; // giữ nguyên (hàng lề ở dưới)
                    int newX = x + 1; // dịch phải 1 (lề trái)
                    paddedMatrix[newY, newX] = boxIndex;
                }
            }
        }

        // Lưu lại
        levelIndexMatrix = paddedMatrix;
        stashGrid = new Stash[newHeight, newWidth];
        for (int i = 0; i < Stash.Count; i++)
        {
            stashGrid[Stash[i].index.x, Stash[i].index.y] = Stash[i];
        }

        for (int i = 0; i < levelData.boxes.Count; i++)
        {
            var boxCfg = levelData.boxes[i];

            if (boxCfg.isStackSpawner)
            {
                int r = boxCfg.gridPos.y;
                int c = boxCfg.gridPos.x + 1;

                Stash spawnerStash = stashGrid[r, c];

                if (spawnerStash != null)
                {
                    int tRow = r;
                    int tCol = c;
                    switch (boxCfg.direction)
                    {
                        case BoxDirection.Up: tRow += 1; break;
                        case BoxDirection.Down: tRow -= 1; break;
                        case BoxDirection.Left: tCol -= 1; break;
                        case BoxDirection.Right: tCol += 1; break;
                    }

                    spawnerStash.SetupSpawner(boxCfg.spawnStack);
                    Stash x = stashGrid[tRow, tCol];
                    x.Init();
                    spawnerStash.SetVisualStack(x);
                }
            }
        }
    }

    public void SpawnLevel()
    {
        if (levelData == null || itemDatabase == null || boxPrefab == null)
        {
            return;
        }

        EnsureLevelRoot();
        ClearLevel();

        // ================== PHASE 1: build ma trận gốc ==================
        int[,] originalMatrix = new int[levelData.height, levelData.width];
        InitializeMatrix(originalMatrix, levelData.height, levelData.width, -1);

        for (int i = 0; i < levelData.boxes.Count; i++)
        {
            var boxCfg = levelData.boxes[i];
            Vector2Int pos = boxCfg.gridPos;

            if (pos.y >= 0 && pos.y < levelData.height &&
                pos.x >= 0 && pos.x < levelData.width)
            {
                originalMatrix[pos.y, pos.x] = i;
            }

            if (boxCfg.isStackSpawner) //&& boxCfg.direction != BoxDirection.None)
            {
                Vector2Int targetPos = pos;
                switch (boxCfg.direction)
                {
                    case BoxDirection.Up: targetPos.y += 1; break;
                    case BoxDirection.Down: targetPos.y -= 1; break;
                    case BoxDirection.Left: targetPos.x -= 1; break;
                    case BoxDirection.Right: targetPos.x += 1; break;
                }

                if (targetPos.y >= 0 && targetPos.y < levelData.height &&
                    targetPos.x >= 0 && targetPos.x < levelData.width)
                {
                    originalMatrix[targetPos.y, targetPos.x] = 100;
                }
            }
        }

        // ================== PHASE 2: pad thêm 1 hàng + 2 cột ==================
        int newHeight = levelData.height + 1;
        int newWidth = levelData.width + 2;

        int[,] paddedMatrix = new int[newHeight, newWidth];
        InitializeMatrix(paddedMatrix, newHeight, newWidth, -1);

        for (int y = 0; y < levelData.height; y++)
        {
            for (int x = 0; x < levelData.width; x++)
            {
                int boxIndex = originalMatrix[y, x];
                if (boxIndex != -1)
                {
                    int newY = y;
                    int newX = x + 1;
                    paddedMatrix[newY, newX] = boxIndex;
                }
            }
        }

        levelIndexMatrix = paddedMatrix;

        // ================== PHASE 3: spawn box + border ==================
        stashGrid = new Stash[newHeight, newWidth];

        float centerX = (newWidth - 1) / 2f;
        float centerY = (newHeight - 1) / 2f;

        int maxRows = levelIndexMatrix.GetLength(0);
        int maxCols = levelIndexMatrix.GetLength(1);

        for (int row = 0; row < newHeight; row++)
        {
            for (int col = 0; col < newWidth; col++)
            {
                int boxIndex = levelIndexMatrix[row, col];

                // Tính vị trí world/local
                Vector3 localPos = gridOrigin + new Vector3(
                    (col - centerX) * cellSize,
                    (row - centerY) * cellSize,
                    0f
                );

                if (boxIndex != -1)
                {
                    if (boxIndex == 100)
                    {
                        Stash box = Instantiate(stackPrefab, levelRoot);
                        box.transform.localPosition = localPos;
                        box.transform.localRotation = Quaternion.identity;
                        box.transform.localScale = Vector3.one;
                        box.SetIndex(row, col);
                        stashGrid[row, col] = box;
                        Stash.Add(box);
                    }
                    else
                    {
                        var boxCfg = levelData.boxes[boxIndex];

                        Stash box = Instantiate(boxPrefab, levelRoot);
                        box.name = $"Box_{boxCfg.gridPos.x}_{boxCfg.gridPos.y}";
                        box.transform.localPosition = localPos;
                        box.transform.localRotation = Quaternion.identity;
                        box.transform.localScale = Vector3.one;
                        box.SetIndex(row, col);

                        box.ApplyConfig(boxCfg, itemDatabase);
                        Stash.Add(box);

                        stashGrid[row, col] = box;
                    }
                }
                else
                {
                    // --- IS BORDER / EMPTY ---
                    stashGrid[row, col] = null;

                    // BƯỚC 1: Kiểm tra 4 hướng chính (Thẳng)
                    int orthoMask = GetOrthogonalMask(row, col, maxRows, maxCols);

                    if (orthoMask > 0)
                    {
                        // Xử lý viền thẳng (như cũ)
                        if (obj != null && orthoMask < obj.Length && obj[orthoMask] != null)
                        {
                            GameObject go = Instantiate(obj[orthoMask], levelRoot);
                            go.transform.localPosition = localPos;
                            go.transform.localRotation = Quaternion.identity;
                            go.transform.localScale = Vector3.one;
                        }
                    }
                    else
                    {
                        // BƯỚC 2: Nếu KHÔNG có hàng xóm thẳng -> Kiểm tra 16 trường hợp chéo
                        int diagMask = GetDiagonalMask(row, col, maxRows, maxCols);

                        if (diagMask > 0 && cornerBorders != null && diagMask < cornerBorders.Length)
                        {
                            GameObject prefab = cornerBorders[diagMask];
                            if (prefab != null)
                            {
                                GameObject go = Instantiate(prefab, levelRoot);
                                go.transform.localPosition = localPos;
                                go.transform.localRotation = Quaternion.identity;
                                go.transform.localScale = Vector3.one;
                            }
                        }
                    }
                }
            }
        }

        SetBlock();
        UpdateMinMaxPositions(centerX, centerY);
    }

    private void UpdateMinMaxPositions(float centerX, float centerY)
    {
        int minRowIndex = 0;
        int minColIndex = 1;
        Vector3 minPos = GetLocalPosition(minRowIndex, minColIndex, centerX, centerY);
        minPos.y -= 0.6f;
        UpdateMarker(ref min, "Point_Min (BL)", minPos);

        int maxRowIndex = levelData.height - 1;
        int maxColIndex = levelData.width;
        Vector3 maxPos = GetLocalPosition(maxRowIndex, maxColIndex, centerX, centerY);
        UpdateMarker(ref max, "Point_Max (TR)", maxPos);
    }

    private void UpdateMarker(ref Transform target, string name, Vector3 localPosition)
    {
        if (target == null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform,
                false);
            target = go.transform;
        }

        target.name = name;
        target.localPosition = localPosition;
    }

    private Vector3 GetLocalPosition(int row, int col, float centerX, float centerY)
    {
        return gridOrigin + new Vector3(
            (col - centerX) * cellSize,
            (row - centerY) * cellSize,
            0f
        );
    }

    private void SetBlock()
    {
        int maxRows = stashGrid.GetLength(0);
        int maxCols = stashGrid.GetLength(1);

        for (int row = 0; row < maxRows; row++)
        {
            for (int col = 0; col < maxCols; col++)
            {
                if (stashGrid[row, col] == null)
                {
                    continue;
                }
                else if (row == 0)
                {
                    stashGrid[row, col].SetCanPick(true);
                    continue;
                }

                int tileIndex = 0;
                bool top = row - 1 >= 0 && stashGrid[row - 1, col] != null;
                bool down = row + 1 < maxRows && stashGrid[row + 1, col] != null;
                bool left = col - 1 >= 0 && stashGrid[row, col - 1] != null;
                bool right = col + 1 < maxCols && stashGrid[row, col + 1] != null;

                if (top) tileIndex += 1;
                if (down) tileIndex += 2;
                if (left) tileIndex += 4;
                if (right) tileIndex += 8;

                if (tileIndex != 0)
                {
                    stashGrid[row, col].SetCanPick(false);
                }
            }
        }
    }

    private int GetOrthogonalMask(int row, int col, int maxRows, int maxCols)
    {
        bool top = row == 0 || IsBox(row - 1, col, maxRows, maxCols);
        bool down = IsBox(row + 1, col, maxRows, maxCols);
        bool left = IsBox(row, col - 1, maxRows, maxCols);
        bool right = IsBox(row, col + 1, maxRows, maxCols);

        int index = 0;
        if (top) index += 1; // 0001
        if (down) index += 2; // 0010
        if (left) index += 4; // 0100
        if (right) index += 8; // 1000
        return index;
    }

    private void InitializeMatrix(int[,] matrix, int height, int width, int value)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                matrix[y, x] = value;
            }
        }
    }

    private int GetDiagonalMask(int row, int col, int maxRows, int maxCols)
    {
        bool tl = IsBox(row - 1, col - 1, maxRows, maxCols);
        bool tr = IsBox(row - 1, col + 1, maxRows, maxCols);
        bool bl = IsBox(row + 1, col - 1, maxRows, maxCols);
        bool br = IsBox(row + 1, col + 1, maxRows, maxCols);

        int mask = 0;
        if (tl) mask += 1;
        if (tr) mask += 2;
        if (bl) mask += 4;
        if (br) mask += 8;

        return mask;
    }

    private bool IsBox(int r, int c, int maxR, int maxC)
    {
        if (r < 0 || r >= maxR || c < 0 || c >= maxC) return false;
        return levelIndexMatrix[r, c] != -1;
    }

    private void ClearLevel()
    {
        if (levelRoot == null) return;

        for (int i = levelRoot.childCount - 1; i >= 0; i--)
        {
            if (Application.isEditor && !Application.isPlaying)
                DestroyImmediate(levelRoot.GetChild(i).gameObject);
            else
                Destroy(levelRoot.GetChild(i).gameObject);
        }

        Stash.Clear();
    }

    private void EnsureLevelRoot()
    {
        if (levelRoot != null) return;

        string rootName = levelData != null ? $"Level_{levelData.name}" : "LevelRoot";
        GameObject rootGO = new GameObject(rootName);
        rootGO.transform.SetParent(transform, false);
        levelRoot = rootGO.transform;
    }
}