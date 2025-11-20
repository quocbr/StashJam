#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LevelEditorWindow : EditorWindow
{
    private LevelData _level;
    private ItemDatabase _itemDatabase;
    private Vector2 _gridScroll;
    private Vector2 _inspectorScroll;
    private BoxConfig _selectedBox;

    private int _cellSize = 64;
    private int[] _itemIds;
    private string[] _itemNames;

    // Cache danh sách ô bị chiếm dụng (Key: Ô bị chiếm, Value: Ô nguồn)
    private Dictionary<Vector2Int, Vector2Int> _reservedCells = new Dictionary<Vector2Int, Vector2Int>();

    [MenuItem("Tools/Level Editor (Box + Items)")]
    public static void Open() { GetWindow<LevelEditorWindow>("Level Editor"); }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        _level = (LevelData)EditorGUILayout.ObjectField("Level Data", _level, typeof(LevelData), false);
        _itemDatabase = (ItemDatabase)EditorGUILayout.ObjectField("Item Database", _itemDatabase, typeof(ItemDatabase), false);
        EditorGUILayout.EndVertical();

        if (_level == null) return;
        if (_itemDatabase != null) BuildItemOptions();

        // Tính toán lại vùng bị chiếm
        CalculateReservedCells();

        EditorGUILayout.Space(4);
        DrawLevelSettings();
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();

        // CỘT TRÁI: GRID
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
        _gridScroll = EditorGUILayout.BeginScrollView(_gridScroll);
        DrawGrid();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // CỘT PHẢI: INSPECTOR
        EditorGUILayout.BeginVertical("box", GUILayout.Width(380));
        _inspectorScroll = EditorGUILayout.BeginScrollView(_inspectorScroll);
        DrawSelectedBoxInspector();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private void CalculateReservedCells()
    {
        _reservedCells.Clear();
        if (_level.boxes == null) return;

        foreach (var box in _level.boxes)
        {
            if (box.isStackSpawner)//&& box.direction != BoxDirection.None)
            {
                Vector2Int targetPos = box.gridPos;
                switch (box.direction)
                {
                    case BoxDirection.Up: targetPos.y += 1; break;
                    case BoxDirection.Down: targetPos.y -= 1; break;
                    case BoxDirection.Left: targetPos.x -= 1; break;
                    case BoxDirection.Right: targetPos.x += 1; break;
                }
                if (!_reservedCells.ContainsKey(targetPos))
                    _reservedCells.Add(targetPos, box.gridPos);
            }
        }
    }

    private void DrawLevelSettings()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        int newWidth = EditorGUILayout.IntField("Width", _level.width);
        int newHeight = EditorGUILayout.IntField("Height", _level.height);
        _cellSize = EditorGUILayout.IntSlider("Cell Size", _cellSize, 40, 120);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_level, "Change Level Settings");
            _level.width = Mathf.Max(1, newWidth);
            _level.height = Mathf.Max(1, newHeight);
            EditorUtility.SetDirty(_level);
        }

        if (GUILayout.Button("Clear All Boxes"))
        {
            if (EditorUtility.DisplayDialog("Confirm", "Clear all boxes?", "Yes", "No"))
            {
                Undo.RecordObject(_level, "Clear Boxes");
                _level.boxes.Clear();
                _selectedBox = null;
                EditorUtility.SetDirty(_level);
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawGrid()
    {
        if (_level.width <= 0 || _level.height <= 0) return;

        for (int y = _level.height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < _level.width; x++)
            {
                var box = _level.GetBoxAt(x, y);
                Rect cellRect = GUILayoutUtility.GetRect(_cellSize, _cellSize, GUILayout.ExpandWidth(false));
                DrawCell(cellRect, x, y, box);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawCell(Rect rect, int x, int y, BoxConfig box)
    {
        Color bgColor = new Color(0.17f, 0.17f, 0.17f);
        EditorGUI.DrawRect(rect, bgColor);

        Vector2Int currentPos = new Vector2Int(x, y);
        bool isReserved = _reservedCells.ContainsKey(currentPos);

        if (box != null)
        {
            bool isSelected = (_selectedBox == box);
            Color borderColor = isSelected ? new Color(1f, 0.8f, 0.3f) : new Color(0.4f, 0.4f, 0.4f);
            if (box.isStackSpawner) borderColor = new Color(0.6f, 0.4f, 1f);
            if (isReserved) borderColor = Color.red;

            FrameRect(rect, borderColor, 2);
            EditorGUI.DrawRect(new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6), new Color(0.1f, 0.1f, 0.1f));

            // Mũi tên hướng
            if (box.isStackSpawner)//&& box.direction != BoxDirection.None)
            {
                string arrow = GetArrowString(box.direction);
                GUI.Label(rect, arrow, new GUIStyle(EditorStyles.largeLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(1f, 1f, 1f, 0.3f) },
                    fontSize = 24,
                    fontStyle = FontStyle.Bold
                });
            }

            DrawItemPreviewCompact(rect, box);

            if (isReserved)
            {
                GUI.Label(rect, "ERR!", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.red } });
            }
        }
        else if (isReserved)
        {
            // Ô ĐÍCH (GHOST BOX)
            // Vẽ box mờ + item sắp sinh ra
            Vector2Int sourcePos = _reservedCells[currentPos];
            BoxConfig sourceBox = _level.GetBoxAt(sourcePos.x, sourcePos.y);
            bool isSourceSelected = (_selectedBox == sourceBox);

            // Nếu đang chọn box nguồn -> Highlight ô đích này lên
            Color ghostBg = isSourceSelected ? new Color(0.6f, 0.4f, 1f, 0.2f) : new Color(1f, 1f, 1f, 0.05f);
            EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2), ghostBg);

            Color ghostBorder = new Color(0.6f, 0.4f, 1f, 0.4f);
            FrameRect(rect, ghostBorder, 2);

            // Vẽ Preview Item tiếp theo trong stack
            if (sourceBox != null && sourceBox.spawnStack.Count > 0 && _itemDatabase != null)
            {
                var nextBoxData = sourceBox.spawnStack[0];
                if (nextBoxData.itemIds.Count > 0)
                {
                    int previewId = nextBoxData.itemIds[0];
                    var def = _itemDatabase.GetById(previewId);
                    if (def != null && def.icon != null)
                    {
                        var c = GUI.color;
                        GUI.color = new Color(1, 1, 1, 0.4f); // Mờ
                        float padding = 10f;
                        float size = Mathf.Min(rect.width, rect.height) - padding * 2;
                        Rect iconRect = new Rect(rect.x + (rect.width - size) / 2f, rect.y + (rect.height - size) / 2f, size, size);
                        GUI.DrawTexture(iconRect, def.icon.texture, ScaleMode.ScaleToFit);
                        GUI.color = c;
                    }
                }
            }

            GUI.Label(rect, "Target", new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.LowerCenter,
                normal = { textColor = new Color(0.7f, 0.7f, 1f, 0.6f) }
            });
        }
        else
        {
            EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2), new Color(0.25f, 0.25f, 0.25f));
        }

        HandleInput(rect, x, y, box, isReserved);
    }

    private void HandleInput(Rect rect, int x, int y, BoxConfig box, bool isReserved)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            if (e.button == 0) // Trái
            {
                // [NEW] Nếu click vào ô Target -> Tự động chọn Box Spawner gốc
                if (box == null && isReserved)
                {
                    Vector2Int sourcePos = _reservedCells[new Vector2Int(x, y)];
                    _selectedBox = _level.GetBoxAt(sourcePos.x, sourcePos.y);
                    GUI.changed = true;
                    e.Use();
                    return;
                }

                if (box == null)
                {
                    Undo.RecordObject(_level, "Create Box");
                    box = new BoxConfig { gridPos = new Vector2Int(x, y) };
                    EnsureBoxItems(box.itemIds);
                    _level.boxes.Add(box);
                    EditorUtility.SetDirty(_level);
                }
                _selectedBox = box;
                GUI.changed = true;
                e.Use();
            }
            else if (e.button == 1) // Phải
            {
                if (box != null)
                {
                    Undo.RecordObject(_level, "Remove Box");
                    _level.boxes.Remove(box);
                    if (_selectedBox == box) _selectedBox = null;
                    EditorUtility.SetDirty(_level);
                }
                e.Use();
            }
        }
    }

    private void DrawItemPreviewCompact(Rect rect, BoxConfig box)
    {
        if (_itemDatabase == null || box.itemIds.Count == 0) return;

        // Vẽ 1 item đại diện to ở giữa
        int id = box.itemIds[0];
        var def = _itemDatabase.GetById(id);
        if (def == null) return;

        float padding = 6f;
        float size = Mathf.Min(rect.width, rect.height) - padding * 2;
        Rect iconRect = new Rect(rect.x + (rect.width - size) / 2f, rect.y + (rect.height - size) / 2f, size, size);

        if (def.icon != null)
        {
            GUI.DrawTexture(iconRect, def.icon.texture, ScaleMode.ScaleToFit);
            // Hiển thị số lượng stack (nếu > 0)
            if (box.isStackSpawner && box.spawnStack.Count > 0)
            {
                GUI.Label(new Rect(rect.xMax - 25, rect.y, 25, 20),
                    $"+{box.spawnStack.Count}",
                    new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.UpperRight, normal = { textColor = Color.green } });
            }
        }
        else
        {
            EditorGUI.DrawRect(iconRect, def.color);
        }
    }

    private void FrameRect(Rect rect, Color color, int thickness)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
    }

    // ================= INSPECTOR (ĐÃ SỬA LỖI SAVE) =================

    private void DrawSelectedBoxInspector()
    {
        if (_selectedBox == null)
        {
            EditorGUILayout.HelpBox("Chọn một box để chỉnh sửa.", MessageType.Info);
            return;
        }

        // --- 1. General ---
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("General Config", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        bool locked = EditorGUILayout.Toggle("Is Locked", _selectedBox.isLocked);
        // Grid Pos
        Vector2Int newPos = _selectedBox.gridPos;
        newPos.x = EditorGUILayout.IntSlider("Grid X", newPos.x, 0, Mathf.Max(0, _level.width - 1));
        newPos.y = EditorGUILayout.IntSlider("Grid Y", newPos.y, 0, Mathf.Max(0, _level.height - 1));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_level, "Modify Box General");
            _selectedBox.isLocked = locked;
            if (newPos != _selectedBox.gridPos)
            {
                bool isBlocked = _reservedCells.ContainsKey(newPos) || _level.GetBoxAt(newPos.x, newPos.y) != null;
                if (!isBlocked) _selectedBox.gridPos = newPos;
            }
            EditorUtility.SetDirty(_level);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // --- 2. Spawner ---
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Spawner Config", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        bool isStack = EditorGUILayout.Toggle("Is Stack Spawner", _selectedBox.isStackSpawner);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_level, "Toggle Spawner");
            _selectedBox.isStackSpawner = isStack;
            EditorUtility.SetDirty(_level);
        }

        if (_selectedBox.isStackSpawner)
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Spawn Direction:");

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("▲", GUILayout.Width(30))) ChangeDirection(BoxDirection.Up);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("◄", GUILayout.Width(30))) ChangeDirection(BoxDirection.Left);
            //if (GUILayout.Button("●", GUILayout.Width(30))) ChangeDirection(BoxDirection.None);
            if (GUILayout.Button("►", GUILayout.Width(30))) ChangeDirection(BoxDirection.Right);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("▼", GUILayout.Width(30))) ChangeDirection(BoxDirection.Down);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"Current: {_selectedBox.direction}", EditorStyles.centeredGreyMiniLabel);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // --- 3. Items ---
        EditorGUILayout.BeginVertical("box");
        string title = _selectedBox.isStackSpawner ? "Initial Box (On Board)" : "Box Items";
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        Draw4ItemSlots(_selectedBox.itemIds);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_level, "Modify Base Items");
            EditorUtility.SetDirty(_level);
        }
        EditorGUILayout.EndVertical();

        // --- 4. Stack Queue ---
        if (_selectedBox.isStackSpawner)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Spawn Queue ({_selectedBox.spawnStack.Count})", EditorStyles.boldLabel);

            // Nút ADD - Quan trọng: Phải SetDirty
            if (GUILayout.Button("+ Add Box to Stack", GUILayout.Height(24)))
            {
                Undo.RecordObject(_level, "Add Stack Box");
                var newStack = new BoxStackData();
                EnsureBoxItems(newStack.itemIds);
                _selectedBox.spawnStack.Add(newStack);
                EditorUtility.SetDirty(_level);
            }

            EditorGUILayout.Space(2);

            for (int i = 0; i < _selectedBox.spawnStack.Count; i++)
            {
                var stackData = _selectedBox.spawnStack[i];
                EditorGUILayout.BeginVertical("helpBox");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Spawn #{i + 1}", EditorStyles.boldLabel);

                // Nút REMOVE - Quan trọng: Phải SetDirty
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    Undo.RecordObject(_level, "Remove Stack Box");
                    _selectedBox.spawnStack.RemoveAt(i);
                    EditorUtility.SetDirty(_level);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break; // Break loop
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginChangeCheck();
                Draw4ItemSlots(stackData.itemIds);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Modify Stack Items");
                    EditorUtility.SetDirty(_level);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
            EditorGUILayout.EndVertical();
        }
    }

    private void ChangeDirection(BoxDirection newDir)
    {
        if (_selectedBox.direction != newDir)
        {
            Undo.RecordObject(_level, "Change Direction");
            _selectedBox.direction = newDir;
            EditorUtility.SetDirty(_level);
        }
    }

    private void Draw4ItemSlots(List<int> items)
    {
        EnsureBoxItems(items);
        if (_itemIds == null) return;

        for (int i = 0; i < 4; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Slot {i}", GUILayout.Width(45));
            int cur = items[i];
            int next = EditorGUILayout.IntPopup(cur, _itemNames, _itemIds);
            items[i] = next; // Update list trực tiếp (sẽ được check bởi EditorGUI.BeginChangeCheck ở ngoài)

            var def = _itemDatabase.GetById(next);
            if (def != null && def.icon != null)
                GUI.DrawTexture(GUILayoutUtility.GetRect(18, 18, GUILayout.Width(18)), def.icon.texture);

            EditorGUILayout.EndHorizontal();
        }
    }

    private void EnsureBoxItems(List<int> items)
    {
        int defId = (_itemIds != null && _itemIds.Length > 0) ? _itemIds[0] : 0;
        while (items.Count < 4) items.Add(defId);
        while (items.Count > 4) items.RemoveAt(items.Count - 1);
    }

    private void BuildItemOptions()
    {
        if (_itemDatabase == null) return;
        _itemDatabase.BuildLookup();
        int c = _itemDatabase.items.Count;
        _itemIds = new int[c];
        _itemNames = new string[c];
        for (int i = 0; i < c; i++)
        {
            var d = _itemDatabase.items[i];
            _itemIds[i] = d.id;
            _itemNames[i] = d.displayName;
        }
    }

    private string GetArrowString(BoxDirection dir)
    {
        switch (dir)
        {
            case BoxDirection.Up: return "▲";
            case BoxDirection.Down: return "▼";
            case BoxDirection.Left: return "◄";
            case BoxDirection.Right: return "►";
            default: return "";
        }
    }
}
#endif