using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Stash : MonoBehaviour
{
    [SerializeField] private GameObject MainAsset;
    [SerializeField] private SpriteRenderer box1;
    [SerializeField] private SpriteRenderer box2;
    [SerializeField] private SpriteRenderer glass;
    [SerializeField] private List<Item> m_ListItem;
    public Vector2Int index;
    public bool CanPick = true;

    public List<Item> ListItem => m_ListItem;

    public int ItemCount => m_ListItem.Count;

    public void Init()
    {
        for (int i = 0; i < m_ListItem.Count; i++)
        {
        }

        CanPick = true;
    }

    public void ApplyConfig(BoxConfig config, ItemDatabase db)
    {
        if (config == null)
        {
            Debug.LogWarning($"{name}: BoxConfig null trong ApplyConfig.");
            return;
        }

        box1.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");
        box2.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");
        glass.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");

        // duyệt qua từng slot
        for (int i = 0; i < m_ListItem.Count; i++)
        {
            var slot = m_ListItem[i];
            if (slot == null) continue;

            if (i < config.itemIds.Count)
            {
                int itemId = config.itemIds[i];
                var def = db != null ? db.GetById(itemId) : null;

                if (def != null && def.icon != null)
                {
                    slot.Init(def.id, def.icon, 7 - config.gridPos.y);
                    //slot.sprite = def.icon;
                }
                else
                {
                    // không có icon / không tìm thấy item -> ẩn slot
                    //slot.sprite = null;
                }
            }
            else
            {
                // không có item tương ứng -> ẩn slot
                //slot.sprite = null;
            }
        }
    }

    public void SetIndex(int x, int y)
    {
        index.x = x;
        index.y = y;
    }

    public void AddItem(Item itemGo)
    {
        m_ListItem.Add(itemGo);
    }

    public void SetCanPick(bool canPick)
    {
        CanPick = canPick;
        glass.gameObject.SetActive(!canPick);
    }

    public Item PopTopItem()
    {
        if (m_ListItem.Count == 0) return null;
        Item go = m_ListItem[m_ListItem.Count - 1];
        m_ListItem.RemoveAt(m_ListItem.Count - 1);
        return go;
    }

    public void OnPick()
    {
        CanPick = false;
        MainAsset.transform.DOScale(0, 0.5f).SetDelay(0.4f).SetEase(Ease.InBack).OnComplete(() => { /*Destroy(gameObject);*/ });
    }
}