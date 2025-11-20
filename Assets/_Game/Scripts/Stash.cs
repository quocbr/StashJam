using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject MainAsset;
    [SerializeField] private SpriteRenderer box1;
    [SerializeField] private SpriteRenderer box2;
    [SerializeField] private SpriteRenderer glass;
    [SerializeField] private List<Item> m_ListItem;
    [SerializeField] private List<Transform> m_PosItem;
    [SerializeField] private Item preFabItem;

    [Header("Runtime Info")]
    public Vector2Int index; // Tọa độ trên Grid
    public bool CanPick = true;
    // Hàng đợi chứa các BoxStackData (Box con) chờ được sinh ra
    [ShowInInspector] public Queue<BoxStackData> pendingStack = new Queue<BoxStackData>();
    private int sortLayer;
    // --------------------------

    public List<Item> ListItem => m_ListItem;
    public int ItemCount => m_ListItem.Count;
    public bool IsStackSpawner => pendingStack.Count > 0;

    public void Init()
    {
        CanPick = true;
    }

    /// <summary>
    /// Hàm cài đặt dữ liệu cho Spawner (được gọi từ Level.cs khi spawn)
    /// </summary>
    public void SetupSpawner(List<BoxStackData> stackData)
    {
        this.pendingStack = new Queue<BoxStackData>(stackData);
    }

    /// <summary>
    /// Hiển thị Item lên Box dựa trên Config
    /// </summary>
    public void ApplyConfig(BoxConfig config, ItemDatabase db)
    {
        if (config == null) return;
        // 1. Setup Layer hiển thị để không bị chồng chéo sai
        // Giả sử trục Y càng cao thì Layer càng thấp để tạo độ sâu
        sortLayer = 7 - config.gridPos.y;

        box1.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");
        box2.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");
        glass.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");

        m_ListItem.Clear();
        // 2. Hiển thị Item
        for (int i = 0; i < config.itemIds.Count; i++)
        {
            Item slot = Instantiate(preFabItem, m_PosItem[i]);
            slot.transform.localPosition = Vector3.zero;

            if (i < config.itemIds.Count)
            {
                int itemId = config.itemIds[i];
                var def = LevelManager.Ins.itemDatabase.GetById(itemId);

                if (def != null && def.icon != null)
                {
                    // Slot item nằm trên box nên order phải cao hơn box
                    slot.Init(def.id, def.icon, sortLayer);
                }
            }
            m_ListItem.Add(slot);
        }
        SetupSpawner(config.spawnStack);
    }

    public void SetIndex(int row, int col)
    {
        index = new Vector2Int(row, col);
    }

    public void SetCanPick(bool canPick)
    {
        CanPick = canPick;
        if (glass != null) glass.gameObject.SetActive(!canPick);
    }

    public void OnPick()
    {
        CanPick = false; // Khóa click ngay lập tức

        // Hiệu ứng thu nhỏ (Biến mất)
        MainAsset.transform.DOScale(0, 0.4f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // --- LOGIC MỚI: KIỂM TRA STACK ---
                if (pendingStack.Count > 0)
                {
                    // 1. Lấy dữ liệu box tiếp theo
                    BoxStackData nextData = pendingStack.Dequeue();

                    // 2. Cập nhật hình ảnh item mới
                    UpdateVisuals(nextData.itemIds);

                    // 3. Hiệu ứng hiện ra lại (Scale Up)
                    MainAsset.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            CanPick = true; // Cho phép ăn tiếp
                        });
                }
            });
    }
    // Hàm cập nhật hiển thị item (được tách ra để dùng lại)
    private void UpdateVisuals(List<int> itemIds)
    {
        m_ListItem.Clear();
        for (int i = 0; i < itemIds.Count; i++)
        {
            Item slot = Instantiate(preFabItem, m_PosItem[i]);
            slot.transform.localPosition = Vector3.zero;

            if (i < itemIds.Count)
            {
                int itemId = itemIds[i];
                var def = LevelManager.Ins.itemDatabase.GetById(itemId);

                if (def != null && def.icon != null)
                {
                    slot.Init(def.id, def.icon, 7 - index.y + 1);
                }

            }
            m_ListItem.Add(slot);
        }
    }
}