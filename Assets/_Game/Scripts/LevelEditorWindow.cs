#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class LevelEditorWindow : EditorWindow
{
    private LevelData _level;
    private ItemDatabase _itemDatabase;

    private Vector2 _gridScroll;
    private Vector2 _inspectorScroll;

    private BoxConfig _selectedBox;

    // Ô lưới to hơn chút cho dễ nhìn icon
    private int _cellSize = 56;

    // dữ liệu cho popup chọn item
    private int[] _itemIds;
    private string[] _itemNames;
    private int _newItemId;

    [MenuItem("Tools/Level Editor (Box + Items)")]
    public static void Open()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        // ====== HEADER: chọn LevelData & ItemDatabase ======
        EditorGUILayout.BeginVertical("box");
        _level = (LevelData)EditorGUILayout.ObjectField("Level Data", _level, typeof(LevelData), false);
        _itemDatabase = (ItemDatabase)EditorGUILayout.ObjectField("Item Database", _itemDatabase, typeof(ItemDatabase), false);
        EditorGUILayout.EndVertical();

        if (_level == null)
        {
            EditorGUILayout.HelpBox("Chọn một LevelData asset để bắt đầu.", MessageType.Info);
            return;
        }

        if (_itemDatabase == null)
        {
            EditorGUILayout.HelpBox("Chọn một ItemDatabase để hiển thị & chọn item.", MessageType.Warning);
        }
        else
        {
            BuildItemOptions();
        }

        EditorGUILayout.Space(4);
        DrawLevelSettings();
        EditorGUILayout.Space(4);

        // ====== BODY: 2 cột - Grid & Inspector ======
        EditorGUILayout.BeginHorizontal();

        // -------- Cột trái: GRID --------
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Level Grid", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        _gridScroll = EditorGUILayout.BeginScrollView(_gridScroll);
        DrawGrid();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        // -------- Cột phải: INSPECTOR --------
        EditorGUILayout.BeginVertical("box", GUILayout.Width(360));
        EditorGUILayout.LabelField("Box Inspector", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        _inspectorScroll = EditorGUILayout.BeginScrollView(_inspectorScroll);
        DrawSelectedBoxInspector();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    // ================= LEVEL SETTINGS =================

    private void DrawLevelSettings()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        EditorGUI.BeginChangeCheck();

        int newWidth = EditorGUILayout.IntField("Width", _level.width);
        int newHeight = EditorGUILayout.IntField("Height", _level.height);

        _cellSize = EditorGUILayout.IntSlider("Cell Size", _cellSize, 40, 96);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_level, "Change level size");
            _level.width = Mathf.Max(1, newWidth);
            _level.height = Mathf.Max(1, newHeight);
            EditorUtility.SetDirty(_level);
        }

        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All Boxes", GUILayout.Height(22)))
        {
            if (EditorUtility.DisplayDialog("Clear?", "Xoá toàn bộ box trong level?", "Yes", "No"))
            {
                Undo.RecordObject(_level, "Clear level boxes");
                _level.boxes.Clear();
                _selectedBox = null;
                EditorUtility.SetDirty(_level);
            }
        }

        if (GUILayout.Button("Deselect Box", GUILayout.Height(22)))
        {
            _selectedBox = null;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    // ================= GRID DRAW =================

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
        // nền grid
        Color bgColor = new Color(0.17f, 0.17f, 0.17f);
        EditorGUI.DrawRect(rect, bgColor);

        if (box != null)
        {
            bool isSelected = (_selectedBox == box);

            // border box
            Color borderColor = isSelected ? new Color(1f, 0.8f, 0.3f) : new Color(0.4f, 0.4f, 0.4f);
            EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2), borderColor);

            // nền bên trong box
            EditorGUI.DrawRect(new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6), Color.black);

            // vẽ stack item (icon hoặc thanh màu + text)
            int maxShow = Mathf.Min(4, box.itemIds.Count);
            if (maxShow > 0 && _itemDatabase != null)
            {
                float stackAreaHeight = rect.height - 18f;
                float rowHeight = stackAreaHeight / Mathf.Max(3, maxShow + 1);

                var itemTextStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white },
                    fontSize = Mathf.Clamp(_cellSize / 6, 8, 12),
                    clipping = TextClipping.Clip
                };

                for (int i = 0; i < maxShow; i++)
                {
                    int id = box.itemIds[i];
                    var def = _itemDatabase.GetById(id);

                    float rowY = rect.yMax - 4 - rowHeight * (i + 1);
                    Rect rowRect = new Rect(rect.x + 5, rowY, rect.width - 10, rowHeight - 2);

                    if (def != null && def.icon != null && def.icon.texture != null)
                    {
                        Texture2D tex = def.icon.texture;
                        float size = Mathf.Min(rowRect.width, rowRect.height);
                        Rect iconRect = new Rect(
                            rowRect.x + (rowRect.width - size) * 0.5f,
                            rowRect.y + (rowRect.height - size) * 0.5f,
                            size,
                            size
                        );
                        GUI.DrawTexture(iconRect, tex, ScaleMode.ScaleToFit, true);
                    }
                    else
                    {
                        Color c = def != null ? def.color : new Color(0.5f, 0.5f, 0.5f);
                        EditorGUI.DrawRect(rowRect, c);

                        string code = def != null
                            ? (!string.IsNullOrEmpty(def.shortCode) ? def.shortCode : def.displayName)
                            : id.ToString();

                        GUI.Label(rowRect, code, itemTextStyle);
                    }
                }
            }

            // text nhỏ: toạ độ & số item
            string topText = $"({x},{y})";
            string bottomText = box.itemIds.Count > 0 ? $"{box.itemIds.Count} items" : "Empty";

            var topStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = Color.white },
                fontSize = Mathf.Clamp(_cellSize / 5, 9, 13)
            };
            var bottomStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.LowerRight,
                normal = { textColor = Color.gray },
                fontSize = Mathf.Clamp(_cellSize / 6, 8, 12)
            };

            Rect topRect = new Rect(rect.x + 4, rect.y + 2, rect.width - 8, 14);
            Rect bottomRect = new Rect(rect.x + 4, rect.yMax - 15, rect.width - 8, 13);

            GUI.Label(topRect, topText, topStyle);
            GUI.Label(bottomRect, bottomText, bottomStyle);
        }
        else
        {
            // ô trống
            EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2),
                new Color(0.25f, 0.25f, 0.25f));
        }

        // Tooltip chi tiết
        string tooltip = box == null
            ? $"({x},{y})\nNo box"
            : BuildBoxTooltip(box, x, y);

        GUI.Label(rect, new GUIContent("", tooltip));

        // ====== XỬ LÝ CLICK / DOUBLE-CLICK ======
        Event e = Event.current;
        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            if (e.button == 1 && e.clickCount == 1)
            {
                // Right-click: xoá box nếu có
                HandleRightClickCell(x, y, box);
                e.Use();
            }
            else if (e.button == 0)
            {
                if (e.clickCount == 2)
                {
                    // Double-click mở BoxDetailWindow
                    HandleDoubleClickCell(x, y, box);
                }
                else
                {
                    // Single-click: chọn / tạo box
                    HandleLeftClickCell(x, y, box);
                }
                e.Use();
            }
        }
    }

    private string BuildBoxTooltip(BoxConfig box, int x, int y)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"Box at ({x},{y})");
        sb.AppendLine($"Locked: {box.isLocked}");
        sb.AppendLine("Items (bottom → top):");

        if (box.itemIds.Count == 0)
        {
            sb.AppendLine("- (empty)");
        }
        else if (_itemDatabase == null)
        {
            for (int i = 0; i < box.itemIds.Count; i++)
                sb.AppendLine($"[{i}] ID = {box.itemIds[i]}");
        }
        else
        {
            for (int i = 0; i < box.itemIds.Count; i++)
            {
                int id = box.itemIds[i];
                var def = _itemDatabase.GetById(id);
                string name = def != null ? def.displayName : "(Unknown)";
                sb.AppendLine($"[{i}] {id} - {name}");
            }
        }

        return sb.ToString();
    }

    private void HandleLeftClickCell(int x, int y, BoxConfig box)
    {
        Undo.RecordObject(_level, "Select / Create box");

        if (box == null)
        {
            box = new BoxConfig
            {
                gridPos = new Vector2Int(x, y)
            };
            _level.boxes.Add(box);
        }

        _selectedBox = box;
        EditorUtility.SetDirty(_level);
    }

    private void HandleRightClickCell(int x, int y, BoxConfig box)
    {
        if (box != null)
        {
            Undo.RecordObject(_level, "Remove box");
            _level.boxes.Remove(box);
            if (_selectedBox == box) _selectedBox = null;
            EditorUtility.SetDirty(_level);
        }
    }

    private void HandleDoubleClickCell(int x, int y, BoxConfig box)
    {
        // Nếu chưa có box, tạo mới
        if (box == null)
        {
            Undo.RecordObject(_level, "Create box");
            box = new BoxConfig
            {
                gridPos = new Vector2Int(x, y)
            };
            _level.boxes.Add(box);
            EditorUtility.SetDirty(_level);
        }

        _selectedBox = box;

        // Mở cửa sổ chi tiết box
        if (_itemDatabase != null)
        {
            BoxDetailWindow.Open(_level, box, _itemDatabase);
        }
        else
        {
            Debug.LogWarning("Double-click: cần ItemDatabase để edit chi tiết item.");
        }
    }

    // ================= BOX INSPECTOR (CỘT PHẢI) =================

    private void DrawSelectedBoxInspector()
    {
        if (_selectedBox == null)
        {
            EditorGUILayout.HelpBox(
                "Click 1 lần vào ô bên trái để chọn box.\nDouble-click để mở cửa sổ Box Detail.\nRight-click để xoá box.",
                MessageType.Info);
            return;
        }

        // ----- Block: General info -----
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Thông tin box", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        EditorGUI.BeginChangeCheck();
        Vector2Int newPos = _selectedBox.gridPos;
        newPos.x = EditorGUILayout.IntSlider("Grid X", newPos.x, 0, Mathf.Max(0, _level.width - 1));
        newPos.y = EditorGUILayout.IntSlider("Grid Y", newPos.y, 0, Mathf.Max(0, _level.height - 1));
        if (EditorGUI.EndChangeCheck())
        {
            var other = _level.GetBoxAt(newPos.x, newPos.y);
            if (other == null || other == _selectedBox)
            {
                Undo.RecordObject(_level, "Move box");
                _selectedBox.gridPos = newPos;
                EditorUtility.SetDirty(_level);
            }
            else
            {
                EditorGUILayout.HelpBox("Ô này đã có box khác, không thể di chuyển tới.", MessageType.Warning);
            }
        }

        _selectedBox.isLocked = EditorGUILayout.Toggle("Is Locked", _selectedBox.isLocked);

        EditorGUILayout.Space(2);
        if (GUILayout.Button("Open Box Detail Window", GUILayout.Height(22)))
        {
            if (_itemDatabase != null)
            {
                BoxDetailWindow.Open(_level, _selectedBox, _itemDatabase);
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(4);

        // ----- Block: Items -----
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Items (bottom → top)", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        if (_itemIds == null || _itemIds.Length == 0)
        {
            EditorGUILayout.HelpBox("ItemDatabase chưa có item nào.", MessageType.Warning);
            EditorGUILayout.EndVertical();
            return;
        }

        int removeIndex = -1;
        int moveUpIndex = -1;
        int moveDownIndex = -1;

        for (int i = 0; i < _selectedBox.itemIds.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            int currentId = _selectedBox.itemIds[i];
            int newId = EditorGUILayout.IntPopup(
                $"[{i}]",
                currentId,
                _itemNames,
                _itemIds
            );
            _selectedBox.itemIds[i] = newId;

            if (GUILayout.Button("▲", GUILayout.Width(24)))
                moveUpIndex = i;
            if (GUILayout.Button("▼", GUILayout.Width(24)))
                moveDownIndex = i;
            if (GUILayout.Button("X", GUILayout.Width(24)))
                removeIndex = i;

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (moveUpIndex >= 0 && moveUpIndex < _selectedBox.itemIds.Count - 1)
        {
            Undo.RecordObject(_level, "Move item up");
            var temp = _selectedBox.itemIds[moveUpIndex];
            _selectedBox.itemIds[moveUpIndex] = _selectedBox.itemIds[moveUpIndex + 1];
            _selectedBox.itemIds[moveUpIndex + 1] = temp;
            EditorUtility.SetDirty(_level);
        }

        if (moveDownIndex > 0 && moveDownIndex < _selectedBox.itemIds.Count)
        {
            Undo.RecordObject(_level, "Move item down");
            var temp = _selectedBox.itemIds[moveDownIndex];
            _selectedBox.itemIds[moveDownIndex] = _selectedBox.itemIds[moveDownIndex - 1];
            _selectedBox.itemIds[moveDownIndex - 1] = temp;
            EditorUtility.SetDirty(_level);
        }

        if (removeIndex >= 0)
        {
            Undo.RecordObject(_level, "Remove item");
            _selectedBox.itemIds.RemoveAt(removeIndex);
            EditorUtility.SetDirty(_level);
        }

        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Thêm item mới", EditorStyles.boldLabel);

        _newItemId = EditorGUILayout.IntPopup(
            "New item type",
            _newItemId,
            _itemNames,
            _itemIds
        );

        if (GUILayout.Button("+ Add Item", GUILayout.Height(22)))
        {
            Undo.RecordObject(_level, "Add item");
            _selectedBox.itemIds.Add(_newItemId);
            EditorUtility.SetDirty(_level);
        }

        EditorGUILayout.EndVertical();
    }

    // ================= ITEM OPTIONS =================

    private void BuildItemOptions()
    {
        if (_itemDatabase == null || _itemDatabase.items == null)
        {
            _itemIds = null;
            _itemNames = null;
            return;
        }

        _itemDatabase.BuildLookup();

        int count = _itemDatabase.items.Count;
        _itemIds = new int[count];
        _itemNames = new string[count];

        for (int i = 0; i < count; i++)
        {
            var def = _itemDatabase.items[i];
            if (def == null) continue;

            _itemIds[i] = def.id;

            string name = !string.IsNullOrEmpty(def.displayName)
                ? def.displayName
                : def.name;

            if (!string.IsNullOrEmpty(def.shortCode))
                name = $"{def.shortCode} - {name}";

            _itemNames[i] = name;
        }

        if (_itemIds.Length > 0)
        {
            bool found = false;
            for (int i = 0; i < _itemIds.Length; i++)
            {
                if (_itemIds[i] == _newItemId)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                _newItemId = _itemIds[0];
        }
    }
}


/// <summary>
/// Cửa sổ chi tiết cho một box, mở bằng double-click.
/// Có icon to, chia block rõ ràng.
/// </summary>
public class BoxDetailWindow : EditorWindow
{
    private LevelData _level;
    private BoxConfig _box;
    private ItemDatabase _itemDatabase;

    private Vector2 _scroll;

    private int[] _itemIds;
    private string[] _itemNames;
    private int _newItemId;

    // kích thước icon
    private const float PreviewIconSize = 56f;
    private const float RowIconSize = 40f;

    public static void Open(LevelData level, BoxConfig box, ItemDatabase db)
    {
        var win = CreateInstance<BoxDetailWindow>();
        win._level = level;
        win._box = box;
        win._itemDatabase = db;
        win.BuildItemOptions();

        if (win._itemIds != null && win._itemIds.Length > 0)
            win._newItemId = win._itemIds[0];

        win.titleContent = new GUIContent($"Box ({box.gridPos.x},{box.gridPos.y})");
        win.minSize = new Vector2(380, 360);
        win.ShowUtility();
    }

    private void OnGUI()
    {
        if (_level == null || _box == null)
        {
            EditorGUILayout.HelpBox("Box/Level bị null. Hãy mở lại từ LevelEditor.", MessageType.Error);
            if (GUILayout.Button("Close")) Close();
            return;
        }

        if (_itemDatabase == null)
        {
            EditorGUILayout.HelpBox("Cần ItemDatabase để edit item.", MessageType.Error);
            if (GUILayout.Button("Close")) Close();
            return;
        }

        EditorGUILayout.LabelField($"Box Detail – ({_box.gridPos.x}, {_box.gridPos.y})", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        // ========== BLOCK 1: Thông tin cơ bản ==========
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Thông tin chung", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        EditorGUILayout.LabelField("Grid Position:", $"{_box.gridPos.x}, {_box.gridPos.y}");
        _box.isLocked = EditorGUILayout.Toggle("Is Locked", _box.isLocked);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(4);

        // ========== BLOCK 2: Preview icon các item ==========
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Preview items (bottom → top)", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        EditorGUILayout.BeginHorizontal();
        if (_box.itemIds.Count == 0)
        {
            GUILayout.Label("(Empty)");
        }
        else
        {
            for (int i = 0; i < _box.itemIds.Count; i++)
            {
                int id = _box.itemIds[i];
                var def = _itemDatabase.GetById(id);
                Texture2D tex = (def != null && def.icon != null) ? def.icon.texture : null;

                GUILayout.BeginVertical(GUILayout.Width(PreviewIconSize + 8));

                Rect r = GUILayoutUtility.GetRect(PreviewIconSize, PreviewIconSize, GUILayout.ExpandWidth(false));
                if (tex != null)
                {
                    GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit, true);
                }
                else
                {
                    // fallback: ô màu + chữ
                    Color c = def != null ? def.color : new Color(0.5f, 0.5f, 0.5f);
                    EditorGUI.DrawRect(r, c);
                    string label = def != null
                        ? (!string.IsNullOrEmpty(def.shortCode) ? def.shortCode : def.displayName)
                        : id.ToString();
                    var style = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Color.white },
                        clipping = TextClipping.Clip
                    };
                    GUI.Label(r, label, style);
                }

                GUILayout.Label($"[{i}]", EditorStyles.centeredGreyMiniLabel);

                GUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(4);

        // ========== BLOCK 3: Danh sách item chi tiết ==========
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Danh sách item (bottom → top)", EditorStyles.boldLabel);
        EditorGUILayout.Space(2);

        if (_itemIds == null || _itemIds.Length == 0)
        {
            EditorGUILayout.HelpBox("ItemDatabase chưa có item nào.", MessageType.Warning);
        }
        else
        {
            int removeIndex = -1;
            int moveUpIndex = -1;
            int moveDownIndex = -1;

            for (int i = 0; i < _box.itemIds.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                int currentId = _box.itemIds[i];
                var def = _itemDatabase.GetById(currentId);
                Texture2D tex = (def != null && def.icon != null) ? def.icon.texture : null;

                // icon bên trái (to hơn)
                Rect iconRect = GUILayoutUtility.GetRect(RowIconSize, RowIconSize, GUILayout.Width(RowIconSize), GUILayout.Height(RowIconSize));
                if (tex != null)
                {
                    GUI.DrawTexture(iconRect, tex, ScaleMode.ScaleToFit, true);
                }
                else
                {
                    Color c = def != null ? def.color : new Color(0.5f, 0.5f, 0.5f);
                    EditorGUI.DrawRect(iconRect, c);
                    var miniStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Color.white },
                        clipping = TextClipping.Clip
                    };
                    string sc = def != null
                        ? (!string.IsNullOrEmpty(def.shortCode) ? def.shortCode : def.displayName)
                        : currentId.ToString();
                    GUI.Label(iconRect, sc, miniStyle);
                }

                // popup chọn loại item
                int newId = EditorGUILayout.IntPopup(
                    $"[{i}]",
                    currentId,
                    _itemNames,
                    _itemIds
                );
                _box.itemIds[i] = newId;

                if (GUILayout.Button("▲", GUILayout.Width(24)))
                    moveUpIndex = i;
                if (GUILayout.Button("▼", GUILayout.Width(24)))
                    moveDownIndex = i;
                if (GUILayout.Button("X", GUILayout.Width(24)))
                    removeIndex = i;

                EditorGUILayout.EndHorizontal();
            }

            // ngăn cách phần list và phần add
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (moveUpIndex >= 0 && moveUpIndex < _box.itemIds.Count - 1)
            {
                Undo.RecordObject(_level, "Move item up");
                var temp = _box.itemIds[moveUpIndex];
                _box.itemIds[moveUpIndex] = _box.itemIds[moveUpIndex + 1];
                _box.itemIds[moveUpIndex + 1] = temp;
                EditorUtility.SetDirty(_level);
            }

            if (moveDownIndex > 0 && moveDownIndex < _box.itemIds.Count)
            {
                Undo.RecordObject(_level, "Move item down");
                var temp = _box.itemIds[moveDownIndex];
                _box.itemIds[moveDownIndex] = _box.itemIds[moveDownIndex - 1];
                _box.itemIds[moveDownIndex - 1] = temp;
                EditorUtility.SetDirty(_level);
            }

            if (removeIndex >= 0)
            {
                Undo.RecordObject(_level, "Remove item");
                _box.itemIds.RemoveAt(removeIndex);
                EditorUtility.SetDirty(_level);
            }

            EditorGUILayout.Space(2);

            EditorGUILayout.LabelField("Thêm item mới", EditorStyles.boldLabel);
            _newItemId = EditorGUILayout.IntPopup(
                "New item type",
                _newItemId,
                _itemNames,
                _itemIds
            );

            if (GUILayout.Button("+ Add Item", GUILayout.Height(24)))
            {
                Undo.RecordObject(_level, "Add item");
                _box.itemIds.Add(_newItemId);
                EditorUtility.SetDirty(_level);
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(4);

        if (GUILayout.Button("Close", GUILayout.Height(24)))
        {
            Close();
        }
    }

    private void BuildItemOptions()
    {
        if (_itemDatabase == null || _itemDatabase.items == null)
        {
            _itemIds = null;
            _itemNames = null;
            return;
        }

        _itemDatabase.BuildLookup();

        int count = _itemDatabase.items.Count;
        _itemIds = new int[count];
        _itemNames = new string[count];

        for (int i = 0; i < count; i++)
        {
            var def = _itemDatabase.items[i];
            if (def == null) continue;

            _itemIds[i] = def.id;

            string name = !string.IsNullOrEmpty(def.displayName)
                ? def.displayName
                : def.name;

            if (!string.IsNullOrEmpty(def.shortCode))
                name = $"{def.shortCode} - {name}";

            _itemNames[i] = name;
        }

        if (_itemIds.Length > 0)
        {
            bool found = false;
            for (int i = 0; i < _itemIds.Length; i++)
            {
                if (_itemIds[i] == _newItemId)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                _newItemId = _itemIds[0];
        }
    }
}
#endif
