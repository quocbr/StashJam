using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Stash : MonoBehaviour
{
    [Header("References")] [SerializeField]
    protected GameObject MainAsset;

    [SerializeField] protected GameObject itemContainer;

    [SerializeField] private SpriteRenderer box1;
    [SerializeField] private SpriteRenderer box2;
    [SerializeField] private SpriteRenderer glass;
    [SerializeField] private SpriteRenderer lockSprite;
    [SerializeField] private SpriteRenderer keyLockSprite;
    [SerializeField] private List<Item> m_ListItem;
    [SerializeField] private List<Transform> m_PosItem;
    [SerializeField] private Item preFabItem;
    [SerializeField] private Stash stackVisual;
    [SerializeField] private Sprite[] l_LockSprite;
    [SerializeField] private Sprite[] l_KeyLockSprite;

    [Header("Runtime Info")] public Vector2Int index;

    public bool isEat = false;
    public bool CanPick = true;
    public bool isLock = false;
    public bool isHidden = false;
    private KeyLockType keyLockType;
    private LockType lockType;
    [ShowInInspector] public Queue<BoxStackData> pendingStack = new Queue<BoxStackData>();

    private int sortLayer;
    public List<Item> ListItem => m_ListItem;
    public int ItemCount => m_ListItem.Count;
    public bool IsStackSpawner => pendingStack.Count > 0;
    public SpriteRenderer Glass => glass;

    private void OnEnable()
    {
        EventManager.AddListener<UnLockStash>(OnUnLockStashCallBack);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<UnLockStash>(OnUnLockStashCallBack);
    }

    private void OnUnLockStashCallBack(UnLockStash obj)
    {
        if (isLock && (int)lockType == (int)obj.KeyLockType)
        {
            SetLock(false, LockType.None);
        }
    }

    public virtual void Init()
    {
        CanPick = true;
        isLock = false;
        isHidden = false;
        isEat = false;
    }

    public void SetupSpawner(List<BoxStackData> stackData)
    {
        pendingStack = new Queue<BoxStackData>(stackData);
    }

    public void SetVisualStack(Stash stack)
    {
        stackVisual = stack;
        stackVisual.UpdateStack(pendingStack.Count);
    }

    public void ApplyConfig(BoxConfig config, ItemDatabase db)
    {
        if (config == null) return;
        sortLayer = 7 - config.gridPos.y;
        isHidden = config.isHidden;

        box1.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");
        box2.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");
        glass.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");
        lockSprite.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");
        keyLockSprite.sortingLayerID = SortingLayer.NameToID($"{7 - config.gridPos.y}");

        m_ListItem.Clear();
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

        //Lock
        SetLock(config.hasLock, config.lockType);
        //KeyLock
        SetKeyLock(config.hasKeyLock, config.keyLockType);

        SetupSpawner(config.spawnStack);
    }

    public void SetIndex(int row, int col)
    {
        index = new Vector2Int(row, col);
    }

    public void SetCanPick(bool canPick)
    {
        CanPick = canPick;
        if (glass != null)
        {
            //glass.gameObject.SetActive(!canPick);
            if (!canPick)
            {
                glass.gameObject.SetActive(true);
            }
            else
            {
                AnimOpen();
            }
        }

        if (canPick && isHidden)
        {
            SetHidden(false);
        }
    }

    public void AnimOpen()
    {
        glass.transform.DOLocalMoveY(0.4f, 0.4f);
        glass.DOFade(0.2f, 0.4f).OnComplete(() => { glass.gameObject.SetActive(false); });
    }

    public void SetHidden(bool isHidden)
    {
        this.isHidden = isHidden;
        for (int i = 0; i < m_ListItem.Count; i++)
        {
            m_ListItem[i].SetVisualHidden(isHidden);
        }
    }

    public void OnPick()
    {
        CanPick = false;
        isEat = true;

        MainAsset.transform.DOScale(0, 0.4f).SetEase(Ease.InBack);
        for (int i = 0; i < m_ListItem.Count; i++)
        {
            //m_ListItem[i].transform.DOScale(1.1f, 0.2f);
            Utils_Custom.PlayAnimation(m_ListItem[i].skeletonAnimation, "Jump", loop: true);
        }

        if (keyLockType != KeyLockType.None)
        {
            UnLockStash cb = new UnLockStash();
            cb.KeyLockType = keyLockType;
            EventManager.Trigger(cb);
            SetKeyLock(false, KeyLockType.None);
        }
    }

    [Button]
    public void HandlerItem()
    {
        if (pendingStack.Count > 0)
        {
            isEat = false;

            BoxStackData nextData = pendingStack.Dequeue();
            UpdateVisuals(nextData);
            itemContainer.transform.localPosition = Vector3.zero;
            MainAsset.transform.DOScale(1f, 0.4f).SetDelay(0.5f).SetEase(Ease.OutBack).OnStart(() =>
                {
                    glass.transform.localPosition = Vector3.up * 0.16f;
                    glass.DOFade(1, 0);
                    SetCanPick(false);
                })
                .OnComplete(() => { SetCanPick(true); });
            itemContainer.transform.DOScale(1f, 0.4f).From(0).SetDelay(0.5f).SetEase(Ease.OutBack);
        }
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
                    slot.Init(def.id, def.icon, 7 - index.x, data.isHidden, i);
                }
            }

            m_ListItem.Add(slot);
        }
    }

    public void SetLock(bool locked, LockType lockType)
    {
        isLock = locked;
        if (lockType == LockType.None)
        {
            lockSprite.gameObject.SetActive(false);
            return;
        }

        this.lockType = lockType;
        if (lockSprite != null)
        {
            if (locked)
            {
                lockSprite.gameObject.SetActive(true);
                lockSprite.sprite = l_LockSprite[(int)lockType - 1];
            }
            else
            {
                lockSprite.gameObject.SetActive(false);
            }
        }
    }

    public void SetKeyLock(bool locked, KeyLockType lockType)
    {
        keyLockType = lockType;
        if (lockType == KeyLockType.None)
        {
            keyLockSprite.gameObject.SetActive(false);
            return;
        }

        if (keyLockSprite != null)
        {
            if (locked)
            {
                keyLockSprite.gameObject.SetActive(true);
                keyLockSprite.sprite = l_KeyLockSprite[(int)lockType - 1];
            }
            else
            {
                keyLockSprite.gameObject.SetActive(false);
            }
        }
    }

    protected virtual void UpdateStack(int stack)
    {
    }

    public virtual void SetBoxDirection(BoxDirection direction)
    {
    }
}