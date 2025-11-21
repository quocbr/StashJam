using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stash : MonoBehaviour
{
    [Header("References")] [SerializeField]
    protected GameObject MainAsset;

    [SerializeField] private SpriteRenderer box1;
    [SerializeField] private SpriteRenderer box2;
    [SerializeField] private SpriteRenderer glass;
    [SerializeField] private List<Item> m_ListItem;
    [SerializeField] private List<Transform> m_PosItem;
    [SerializeField] private Item preFabItem;
    [SerializeField] private Stash stackVisual;

    [Header("Runtime Info")] public Vector2Int index; // Tọa độ trên Grid

    public bool CanPick = true;

    // Hàng đợi chứa các BoxStackData (Box con) chờ được sinh ra
    [ShowInInspector] public Queue<BoxStackData> pendingStack = new Queue<BoxStackData>();

    private int sortLayer;
    // --------------------------

    public List<Item> ListItem => m_ListItem;
    public int ItemCount => m_ListItem.Count;
    public bool IsStackSpawner => pendingStack.Count > 0;

    public virtual void Init()
    {
        CanPick = true;
    }

    /// <summary>
    /// Hàm cài đặt dữ liệu cho Spawner (được gọi từ Level.cs khi spawn)
    /// </summary>
    public void SetupSpawner(List<BoxStackData> stackData)
    {
        pendingStack = new Queue<BoxStackData>(stackData);
    }

    public void SetVisualStack(Stash stack)
    {
        stackVisual = stack;
        stackVisual.UpdateStack(pendingStack.Count);
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
                    slot.Init(def.id, def.icon, sortLayer, config.isHidden, i);
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
        CanPick = false;

        MainAsset.transform.DOScale(0, 0.4f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                if (pendingStack.Count > 0)
                {
                    BoxStackData nextData = pendingStack.Dequeue();
                    UpdateVisuals(nextData);
                    MainAsset.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack)
                        .OnComplete(() => { CanPick = true; });
                }
                else
                {
                    MainAsset.SetActive(false);
                    OnStashDestroy cb = new OnStashDestroy();
                    cb.Stash = this;
                    EventManager.Trigger(cb);
                }
            });
    }

    private void UpdateVisuals(BoxStackData data)
    {
        stackVisual.UpdateStack(pendingStack.Count);
        m_ListItem.Clear();
        for (int i = 0; i < data.itemIds.Count; i++)
        {
            Item slot = Instantiate(preFabItem, m_PosItem[i]);
            slot.transform.localPosition = Vector3.zero;

            if (i < data.itemIds.Count)
            {
                int itemId = data.itemIds[i];
                var def = LevelManager.Ins.itemDatabase.GetById(itemId);

                if (def != null && def.icon != null)
                {
                    slot.Init(def.id, def.icon, 7 - index.y + 1, data.isHidden, i);
                }
            }

            m_ListItem.Add(slot);
        }
    }

    protected virtual void UpdateStack(int stack)
    {
    }
}