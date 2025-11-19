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

    [Header("Prefabs")] public Stash boxPrefab; // prefab box có item

    [Header("Runtime")]
    [Tooltip("Root chứa toàn bộ box của level hiện tại. Nếu để trống sẽ tự tạo.")]
    public Transform levelRoot;

    [SerializeField] private GameObject[] obj; // 16 prefab viền (index = tileIndex)

    [Header("Prefabs - Diagonal Borders (Outer Corners)")]
    [InfoBox("Dùng khi KHÔNG CÓ hàng xóm thẳng, nhưng có hàng xóm ở góc chéo.")]
    public GameObject corner_Connect_TL; // Box ở hướng 1 (Top-Left)

    public GameObject corner_Connect_TR; // Box ở hướng 3 (Top-Right)
    public GameObject corner_Connect_BR; // Box ở hướng 5 (Bottom-Right)
    public GameObject corner_Connect_BL; // Box ở hướng 7 (Bottom-Left)

    public List<Stash> Stash = new List<Stash>();

    private int[,] levelIndexMatrix;

    public Stash[,] stashGrid;
    public Transform max;
    public Transform min;

    private void OnEnable()
    {
        EventManager.AddListener<OnStashPick>(OnStashPickCallBack);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnStashPick>(OnStashPickCallBack);
    }

    private void OnStashPickCallBack(OnStashPick onStashPick)
    {
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
    }

    public void SpawnLevel()
    {
        if (levelData == null || itemDatabase == null || boxPrefab == null)
        {
            Debug.LogError("LevelSpawner: Thiếu Data hoặc Prefab!");
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
        }

        // ================== PHASE 2: pad thêm 1 hàng + 2 cột ==================
        int newHeight = levelData.height + 1;
        int newWidth = levelData.width + 2;

        int[,] paddedMatrix = new int[newHeight, newWidth];
        InitializeMatrix(paddedMatrix, newHeight, newWidth, -1);

        // dịch tất cả box vào trong (chừa 1 cột trái, 1 cột phải, 1 hàng dưới)
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
                else
                {
                    // --- IS BORDER / EMPTY ---
                    stashGrid[row, col] = null;

                    // BƯỚC 1: Kiểm tra 4 hướng chính (Thẳng)
                    // Nếu có hàng xóm thẳng -> Dùng bộ 16 Tile cũ
                    int orthoMask = GetOrthogonalMask(row, col, maxRows, maxCols);

                    if (orthoMask > 0)
                    {
                        if (obj != null && orthoMask < obj.Length && obj[orthoMask] != null)
                        {
                            if (obj[orthoMask] == null) return;
                            GameObject go = Instantiate(obj[orthoMask], levelRoot);
                            go.transform.localPosition = localPos;
                            go.transform.localRotation = Quaternion.identity;
                            go.transform.localScale = Vector3.one;
                        }
                    }
                    else
                    {
                        // BƯỚC 2: Nếu KHÔNG có hàng xóm thẳng (orthoMask == 0)
                        // Kiểm tra 4 góc chéo -> Dùng bộ Corner mới
                        GameObject cornerPrefab = GetDiagonalCornerPrefab(row, col, maxRows, maxCols);

                        if (cornerPrefab != null)
                        {
                            if (cornerPrefab == null) return;
                            GameObject go = Instantiate(cornerPrefab, levelRoot);
                            go.transform.localPosition = localPos;
                            go.transform.localRotation = Quaternion.identity;
                            go.transform.localScale = Vector3.one;
                        }
                    }
                }
            }
        }

        SetBlock();
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

    private GameObject GetBorderPrefab(int row, int col)
    {
        if (levelIndexMatrix == null || obj == null || obj.Length < 16)
            return null;

        int maxRows = levelIndexMatrix.GetLength(0);
        int maxCols = levelIndexMatrix.GetLength(1);

        bool top = row - 1 >= 0 && levelIndexMatrix[row - 1, col] != -1;
        bool down = row + 1 < maxRows && levelIndexMatrix[row + 1, col] != -1;
        bool left = col - 1 >= 0 && levelIndexMatrix[row, col - 1] != -1;
        bool right = col + 1 < maxCols && levelIndexMatrix[row, col + 1] != -1;

        int tileIndex = 0;
        if (top) tileIndex += 1; // 0001
        if (down) tileIndex += 2; // 0010
        if (left) tileIndex += 4; // 0100
        if (right) tileIndex += 8; // 1000

        GameObject selected = obj[tileIndex];

        return selected;
    }

    // ----------------- Helper -----------------

    private int GetOrthogonalMask(int row, int col, int maxRows, int maxCols)
    {
        bool top = IsBox(row - 1, col, maxRows, maxCols);
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

    // Trả về Prefab góc chéo phù hợp
    private GameObject GetDiagonalCornerPrefab(int row, int col, int maxRows, int maxCols)
    {
        // Kiểm tra xem có Box ở các góc chéo không
        bool tl = IsBox(row - 1, col - 1, maxRows, maxCols); // Top-Left (Góc 1)
        bool tr = IsBox(row - 1, col + 1, maxRows, maxCols); // Top-Right (Góc 3)
        bool br = IsBox(row + 1, col + 1, maxRows, maxCols); // Bottom-Right (Góc 5)
        bool bl = IsBox(row + 1, col - 1, maxRows, maxCols); // Bottom-Left (Góc 7)

        // Logic map visual:
        // Nếu Box nằm ở hướng 1 (Top-Left), ta cần hiển thị miếng bo ở hướng ngược lại (Bottom-Right) để ôm lấy nó.

        if (tl) return corner_Connect_TL;
        if (tr) return corner_Connect_TR;
        if (br) return corner_Connect_BR;
        if (bl) return corner_Connect_BL;

        return null;
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