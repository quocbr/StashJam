using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;

public class LevelSpawner : MonoBehaviour
{
    [Header("Data")] public LevelData levelData; // asset LevelData

    public ItemDatabase itemDatabase;

    [Header("Grid Settings")] public float cellSize = 1f;

    public Vector3 gridOrigin = Vector3.zero;

    [Header("Prefabs")] public Stash boxPrefab; // prefab box có item

    public GameObject boxPrefab1; // (không dùng nữa nếu đã dùng mảng obj[])

    [Header("Runtime")] [Tooltip("Root chứa toàn bộ box của level hiện tại. Nếu để trống sẽ tự tạo.")]
    public Transform levelRoot;

    [SerializeField] private GameObject[] obj; // 16 prefab viền (index = tileIndex)

    [ShowInInspector] [ReadOnly] [Tooltip("Ma trận lưu index của box. -1 là ô trống/lề.")]
    private int[,] levelIndexMatrix;

    public Stash[,] stashGrid;

    [Button]
    private void Test()
    {
        SpawnLevel();
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
                    // Ô này là box
                    var boxCfg = levelData.boxes[boxIndex];

                    Stash box = Instantiate(boxPrefab, levelRoot);
                    box.name = $"Box_{boxCfg.gridPos.x}_{boxCfg.gridPos.y}";
                    box.transform.localPosition = localPos;
                    box.transform.localRotation = Quaternion.identity;
                    box.transform.localScale = Vector3.one;

                    box.ApplyConfig(boxCfg, itemDatabase);

                    stashGrid[row, col] = box;
                }
                else
                {
                    // Ô lề -> chọn prefab viền dựa theo hàng xóm
                    GameObject borderPrefab = GetBorderPrefab(row, col);

                    if (borderPrefab != null)
                    {
                        GameObject border = Instantiate(borderPrefab, levelRoot);
                        //border.name = $"Padding_{col}_{row}";
                        border.transform.localPosition = localPos;
                        border.transform.localRotation = Quaternion.identity;
                        border.transform.localScale = Vector3.one;
                    }

                    stashGrid[row, col] = null;
                }
            }
        }
    }

    /// <summary>
    /// Chọn prefab viền dựa theo hàng xóm trong levelIndexMatrix.
    /// row = chỉ số dòng (y), col = chỉ số cột (x)
    /// </summary>
    private GameObject GetBorderPrefab(int row, int col)
    {
        if (levelIndexMatrix == null || obj == null || obj.Length < 16)
            return null;

        int maxRows = levelIndexMatrix.GetLength(0);
        int maxCols = levelIndexMatrix.GetLength(1);

        // Ô có box nếu index != -1
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

        // (Optional) Debug xem tileIndex
        // Debug.Log($"Border [{row},{col}] -> mask={tileIndex}");

        return selected;
    }

    // ----------------- Helper -----------------

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