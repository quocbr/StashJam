// --- EDITOR WINDOW ---

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    // --- CẤU HÌNH UI ---
    private int _cellSize = 64;
    private Vector2 _gridScroll;
    private Vector2 _inspectorScroll;
    private ItemDatabase _itemDatabase;
    private int[] _itemIds;
    private string[] _itemNames;

    // --- DỮ LIỆU ---
    private LevelData _level;

    // --- LOGIC ---
    private Dictionary<Vector2Int, Vector2Int> _reservedCells = new Dictionary<Vector2Int, Vector2Int>();
    private BoxConfig _selectedBox;

    private void OnGUI()
    {
        EditorGUILayout.Space();

        // 1. Header: Chọn Data
        EditorGUILayout.BeginVertical("box");
        _level = (LevelData)EditorGUILayout.ObjectField("Level Data", _level, typeof(LevelData), false);
        _itemDatabase =
            (ItemDatabase)EditorGUILayout.ObjectField("Item Database", _itemDatabase, typeof(ItemDatabase), false);
        EditorGUILayout.EndVertical();

        if (_level == null)
        {
            EditorGUILayout.HelpBox("Vui lòng chọn LevelData!", MessageType.Info);
            return;
        }

        if (_itemDatabase != null) BuildItemOptions();

        // 2. Tính toán Logic
        CalculateReservedCells();

        // 3. Settings & Validation
        EditorGUILayout.Space(4);
        DrawLevelSettings();
        EditorGUILayout.Space(4);
        DrawValidationStats();

        EditorGUILayout.Space(4);

        // 4. Main Body: Grid & Inspector
        EditorGUILayout.BeginHorizontal();

        // --- CỘT TRÁI: GRID ---
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Level Grid", EditorStyles.boldLabel);
        _gridScroll = EditorGUILayout.BeginScrollView(_gridScroll);
        DrawGrid();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // --- CỘT PHẢI: INSPECTOR ---
        EditorGUILayout.BeginVertical("box", GUILayout.Width(380));
        EditorGUILayout.LabelField("Inspector", EditorStyles.boldLabel);
        _inspectorScroll = EditorGUILayout.BeginScrollView(_inspectorScroll);
        DrawSelectedBoxInspector();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    [MenuItem("Tools/Level Editor (Box + Items)")]
    public static void Open()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    // ================== LOGIC VẼ GRID ==================

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
        // Nền ô lưới
        Color bgColor = new Color(0.17f, 0.17f, 0.17f);
        EditorGUI.DrawRect(rect, bgColor);

        Vector2Int currentPos = new Vector2Int(x, y);
        bool isReserved = _reservedCells.ContainsKey(currentPos);

        if (box != null)
        {
            // --- VẼ BOX THỰC ---
            bool isSelected = _selectedBox == box;

            // Màu viền
            Color borderColor = isSelected ? new Color(1f, 0.8f, 0.3f) : new Color(0.4f, 0.4f, 0.4f);
            if (box.isStackSpawner) borderColor = new Color(0.6f, 0.4f, 1f); // Tím cho Spawner
            if (isReserved) borderColor = Color.red; // Lỗi trùng lặp

            FrameRect(rect, borderColor, 2);
            EditorGUI.DrawRect(new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6),
                new Color(0.1f, 0.1f, 0.1f));

            // 1. Mũi tên hướng (Spawner)
            if (box.isStackSpawner)
            {
                string arrow = GetArrowString(box.direction);
                GUI.Label(rect, arrow, new GUIStyle(EditorStyles.largeLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(1f, 1f, 1f, 0.15f) },
                    fontSize = 24,
                    fontStyle = FontStyle.Bold
                });
            }

            // 2. Vẽ Item
            DrawItemPreview2x2(rect, box);

            // 3. Trạng thái Hidden
            if (box.isHidden)
            {
                GUI.Label(new Rect(rect.x + 2, rect.y + 2, 70, 20), "(Hidden)",
                    new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.cyan }, fontSize = 14 });
                EditorGUI.DrawRect(new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6),
                    new Color(0, 0, 0, 0.3f));
            }

            // 4. HIỂN THỊ LOCK & KEYLOCK TRÊN GRID
            if (box.hasLock)
            {
                GUI.Label(new Rect(rect.x + 2, rect.y + 17, 70, 20), "(Lock)",
                    new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.green }, fontSize = 14 });
                EditorGUI.DrawRect(new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6),
                    new Color(0, 0, 0, 0.3f));
            }

            if (box.hasKeyLock)
            {
                GUI.Label(new Rect(rect.x + 2, rect.y + 27, 70, 20), "(Key)",
                    new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.magenta }, fontSize = 14 });
                EditorGUI.DrawRect(new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6),
                    new Color(0, 0, 0, 0.3f));
            }

            // 5. Báo lỗi Reserved
            if (isReserved)
            {
                GUI.Label(rect, "ERR!",
                    new GUIStyle(EditorStyles.boldLabel)
                        { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.red } });
            }
        }
        else if (isReserved)
        {
            // --- VẼ GHOST BOX (Target) ---
            Vector2Int sourcePos = _reservedCells[currentPos];
            BoxConfig sourceBox = _level.GetBoxAt(sourcePos.x, sourcePos.y);
            bool isSourceSelected = _selectedBox == sourceBox;

            Color ghostBg = isSourceSelected ? new Color(0.6f, 0.4f, 1f, 0.2f) : new Color(1f, 1f, 1f, 0.05f);
            EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2), ghostBg);
            Color ghostBorder = new Color(0.6f, 0.4f, 1f, 0.4f);
            FrameRect(rect, ghostBorder, 2);

            GUI.Label(rect, "Target", new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 0.7f, 1f, 0.6f) }
            });
        }
        else
        {
            // Ô TRỐNG
            EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2),
                new Color(0.25f, 0.25f, 0.25f));
        }

        HandleInput(rect, x, y, box, isReserved);
    }

    private void DrawItemPreview2x2(Rect rect, BoxConfig box)
    {
        if (_itemDatabase == null || box.itemIds.Count == 0) return;

        float halfW = (rect.width - 6) / 2f;
        float halfH = (rect.height - 6) / 2f;
        float startX = rect.x + 3;
        float startY = rect.y + 3;

        int count = Mathf.Min(4, box.itemIds.Count);

        for (int i = 0; i < count; i++)
        {
            int id = box.itemIds[i];
            var def = _itemDatabase.GetById(id);
            if (def == null) continue;

            float x = startX + i % 2 * halfW;
            float y = startY + i / 2 * halfH;

            Rect iconRect = new Rect(x + 1, y + 1, halfW - 2, halfH - 2);
            if (def.icon != null) DrawSprite(iconRect, def.icon);
            else EditorGUI.DrawRect(iconRect, def.color);
        }

        if (box.isStackSpawner)
        {
            int stackCount = box.spawnStack.Count;
            if (stackCount > 0)
            {
                string labelText = $"+{stackCount}";
                var style = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.LowerRight,
                    fontSize = 11,
                    normal = { textColor = Color.green }
                };
                Rect textRect = new Rect(rect.x, rect.yMax - 18, rect.width - 4, 16);
                GUI.color = Color.black;
                GUI.Label(new Rect(textRect.x + 1, textRect.y + 1, textRect.width, textRect.height), labelText, style);
                GUI.color = Color.white;
                GUI.Label(textRect, labelText, style);
            }
        }
    }

    private void DrawSprite(Rect rect, Sprite sprite)
    {
        if (sprite == null || sprite.texture == null) return;
        Rect spriteRect = sprite.rect;
        Texture2D tex = sprite.texture;
        Rect uv = new Rect(spriteRect.x / tex.width, spriteRect.y / tex.height, spriteRect.width / tex.width,
            spriteRect.height / tex.height);
        GUI.DrawTextureWithTexCoords(rect, tex, uv);
    }

    private void HandleInput(Rect rect, int x, int y, BoxConfig box, bool isReserved)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            if (e.button == 0) // Left Click
            {
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
            else if (e.button == 1) // Right Click
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

    // ================== INSPECTOR ==================

    private void DrawSelectedBoxInspector()
    {
        if (_selectedBox == null)
        {
            EditorGUILayout.HelpBox("Chọn một box trên lưới để chỉnh sửa.", MessageType.Info);
            return;
        }

        // --- 1. General ---
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("General Config", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();

        bool hidden = EditorGUILayout.Toggle("Is Hidden", _selectedBox.isHidden);

        // --- LOCK CONFIG UI ---
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Lock Settings", EditorStyles.miniBoldLabel);
        bool hasLock = EditorGUILayout.Toggle("Has Lock", _selectedBox.hasLock);
        LockType lockType = _selectedBox.lockType;
        if (hasLock)
        {
            EditorGUI.indentLevel++;
            lockType = (LockType)EditorGUILayout.EnumPopup("Lock Type", _selectedBox.lockType);
            EditorGUI.indentLevel--;
        }

        // --- KEYLOCK CONFIG UI ---
        bool hasKeyLock = EditorGUILayout.Toggle("Has Key Lock", _selectedBox.hasKeyLock);
        KeyLockType keyLockType = _selectedBox.keyLockType;
        if (hasKeyLock)
        {
            EditorGUI.indentLevel++;
            keyLockType = (KeyLockType)EditorGUILayout.EnumPopup("Key Lock Type", _selectedBox.keyLockType);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(5);

        Vector2Int newPos = _selectedBox.gridPos;
        newPos.x = EditorGUILayout.IntSlider("Grid X", newPos.x, 0, Mathf.Max(0, _level.width - 1));
        newPos.y = EditorGUILayout.IntSlider("Grid Y", newPos.y, 0, Mathf.Max(0, _level.height - 1));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_level, "Modify Box General");
            _selectedBox.isHidden = hidden;

            _selectedBox.hasLock = hasLock;
            _selectedBox.lockType = lockType;
            _selectedBox.hasKeyLock = hasKeyLock;
            _selectedBox.keyLockType = keyLockType;

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
            if (GUILayout.Button("►", GUILayout.Width(30))) ChangeDirection(BoxDirection.Right);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("▼", GUILayout.Width(30))) ChangeDirection(BoxDirection.Down);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(2);
            _selectedBox.targetStackCount = EditorGUILayout.IntField("Target Total", _selectedBox.targetStackCount);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(5);

        // --- 3. Items (Box Chính) ---
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

        // --- 4. Spawn Stack ---
        if (_selectedBox.isStackSpawner)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Spawn Queue ({_selectedBox.spawnStack.Count})", EditorStyles.boldLabel);

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

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Spawn #{i + 1}", EditorStyles.boldLabel, GUILayout.Width(80));

                // Toggle Hidden cho Box Stack
                stackData.isHidden = EditorGUILayout.ToggleLeft("Hidden", stackData.isHidden, GUILayout.Width(60));

                GUILayout.FlexibleSpace();
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    Undo.RecordObject(_level, "Remove Stack Box");
                    _selectedBox.spawnStack.RemoveAt(i);
                    EditorUtility.SetDirty(_level);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                // --- LOCK/KEYLOCK CHO STACK ---
                EditorGUILayout.BeginHorizontal();
                stackData.hasLock = EditorGUILayout.Toggle("Lock", stackData.hasLock, GUILayout.Width(50));
                if (stackData.hasLock)
                    stackData.lockType = (LockType)EditorGUILayout.EnumPopup(stackData.lockType, GUILayout.Width(70));

                GUILayout.Space(10);

                stackData.hasKeyLock = EditorGUILayout.Toggle("Key", stackData.hasKeyLock, GUILayout.Width(50));
                if (stackData.hasKeyLock)
                    stackData.keyLockType =
                        (KeyLockType)EditorGUILayout.EnumPopup(stackData.keyLockType, GUILayout.Width(70));
                EditorGUILayout.EndHorizontal();
                // ------------------------------

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
            items[i] = next;
            var def = _itemDatabase.GetById(next);
            if (def != null && def.icon != null)
            {
                Rect r = GUILayoutUtility.GetRect(18, 18, GUILayout.Width(18));
                DrawSprite(r, def.icon);
            }

            EditorGUILayout.EndHorizontal();
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

    // ================== HELPERS ==================

    private void CalculateReservedCells()
    {
        _reservedCells.Clear();
        if (_level.boxes == null) return;
        foreach (var box in _level.boxes)
        {
            if (box.isStackSpawner)
            {
                Vector2Int targetPos = box.gridPos;
                switch (box.direction)
                {
                    case BoxDirection.Up: targetPos.y += 1; break;
                    case BoxDirection.Down: targetPos.y -= 1; break;
                    case BoxDirection.Left: targetPos.x -= 1; break;
                    case BoxDirection.Right: targetPos.x += 1; break;
                }

                if (!_reservedCells.ContainsKey(targetPos)) _reservedCells.Add(targetPos, box.gridPos);
            }
        }
    }

    private void DrawValidationStats()
    {
        if (_level == null || _level.boxes == null) return;
        Dictionary<int, int> itemCounts = new Dictionary<int, int>();

        void AddCount(int id)
        {
            if (!itemCounts.ContainsKey(id)) itemCounts[id] = 0;
            itemCounts[id]++;
        }

        foreach (var box in _level.boxes)
        {
            if (box.itemIds != null)
                foreach (var id in box.itemIds)
                    AddCount(id);
            if (box.isStackSpawner && box.spawnStack != null)
            {
                foreach (var stackBox in box.spawnStack)
                    if (stackBox.itemIds != null)
                        foreach (var id in stackBox.itemIds)
                            AddCount(id);
            }
        }

        List<string> errors = new List<string>();
        foreach (var kvp in itemCounts)
        {
            if (kvp.Value % 3 != 0)
            {
                string n = kvp.Key.ToString();
                if (_itemDatabase != null)
                {
                    var d = _itemDatabase.GetById(kvp.Key);
                    if (d != null) n = d.displayName;
                }

                int rem = kvp.Value % 3;
                int missing = 3 - rem;
                errors.Add($"'{n}': Đang có {kvp.Value} (Dư {rem}, Thiếu {missing})");
            }
        }

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
        if (errors.Count == 0)
        {
            GUI.backgroundColor = Color.green;
            EditorGUILayout.HelpBox("OK! Tất cả item đều chia hết cho 3.", MessageType.Info);
        }
        else
        {
            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            EditorGUILayout.HelpBox($"Lỗi: Có {errors.Count} loại item lẻ bộ!", MessageType.Error);
            foreach (var e in errors) EditorGUILayout.LabelField("• " + e, EditorStyles.miniLabel);
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();
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
            Undo.RecordObject(_level, "Change Settings");
            _level.width = Mathf.Max(1, newWidth);
            _level.height = Mathf.Max(1, newHeight);
            EditorUtility.SetDirty(_level);
        }

        if (GUILayout.Button("Clear All Boxes"))
        {
            if (EditorUtility.DisplayDialog("Confirm", "Clear?", "Yes", "No"))
            {
                Undo.RecordObject(_level, "Clear");
                _level.boxes.Clear();
                _selectedBox = null;
                EditorUtility.SetDirty(_level);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void EnsureBoxItems(List<int> items)
    {
        int defId = _itemIds != null && _itemIds.Length > 0 ? _itemIds[0] : 0;
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

    private void FrameRect(Rect rect, Color color, int thickness)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
    }
}
#endif