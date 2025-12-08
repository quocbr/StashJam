using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class ConveyorController : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private BoxSoldOut prefabBox;
    [SerializeField] private AutoCenterLayout autoCenterLayout;
    [SerializeField] private SpriteRenderer visual;
    private Tween _checkMathTween;

    private Tween _refillTween;
    private bool isRunning = false; // Cờ kiểm soát hoạt động chung
    private List<Vector3> m_ListPos = new List<Vector3>();

    private Queue<List<Item>> refillQueue = new Queue<List<Item>>();
    private Dictionary<int, int> targetIndices = new Dictionary<int, int>();

    public SpriteRenderer Visual => visual;

    #region RESET LOGIC

    [Button("RESET LEVEL")]
    public void ResetConveyor()
    {
        Controller.Ins.DestroyAllChildren();

        if (_checkMathTween != null) _checkMathTween.Kill();
        if (_refillTween != null) _refillTween.Kill();
        transform.DOKill();

        StopAllCoroutines();
        isRunning = false;
        _isProcessing = false;

        while (_stashInputQueue.Count > 0)
        {
            var batch = _stashInputQueue.Dequeue();
            foreach (var item in batch.ListItem)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
        }

        while (_pendingOverflow.Count > 0)
        {
            var item = _pendingOverflow.Dequeue();
            if (item != null)
                Destroy(item.gameObject);
        }

        while (refillQueue.Count > 0)
        {
            var batch = refillQueue.Dequeue();
            foreach (var item in batch)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
        }

        while (q.Count > 0)
        {
            var batch = q.Dequeue();
            foreach (var item in batch)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }
        }

        // 3. Clear hàng đợi
        _stashInputQueue.Clear();
        _pendingOverflow.Clear();
        refillQueue.Clear();
        q.Clear(); // Thêm clear queue 'q'

        // 4. Xóa Item trên băng chuyền chính (m_ConveyorList)
        if (m_ConveyorList != null)
        {
            foreach (var slot in m_ConveyorList)
            {
                if (slot != null)
                {
                    // Nếu Slot đang giữ Item thì Destroy Gameobject Item đó đi
                    if (slot.CurrentItem != null)
                    {
                        Destroy(slot.CurrentItem.gameObject);
                    }

                    // Set Slot về null
                    slot.SetItem(null);
                }
            }
        }

        // 5. Xóa Item trên hàng chờ phụ (m_QueueConveyor)
        if (m_QueueConveyor != null)
        {
            foreach (var slot in m_QueueConveyor)
            {
                if (slot != null)
                {
                    if (slot.CurrentItem != null)
                    {
                        Destroy(slot.CurrentItem.gameObject);
                    }

                    slot.SetItem(null);
                }
            }
        }

        InitItems();
        // 6. Bật cờ chạy lại SAU KHI khởi tạo xong
        isRunning = true;
        UpdateItemCountUI();
    }

    #endregion

    #region MOVEMENT LOGIC

    [Header("--- Rotation Settings ---")] [SerializeField]
    private bool rotateItems = true;

    [Tooltip("Nếu ảnh gốc hướng lên trên thì điền -90. Nếu hướng sang phải thì điền 0.")] [SerializeField]
    private float rotationOffset = 0f;

    private void MoveItemContinuous(ConveyorItem item)
    {
        if (item == null) return;
        int id = item.GetInstanceID();
        if (!targetIndices.ContainsKey(id)) return;

        int targetIndex = targetIndices[id];
        Vector3 targetPos = m_ListPos[targetIndex];
        if (rotateItems)
        {
            Vector3 direction = (targetPos - item.transform.position).normalized;

            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                Quaternion targetRotation = Quaternion.AngleAxis(angle + rotationOffset, Vector3.forward);

                item.arrow.transform.rotation = Quaternion.Lerp(item.arrow.transform.rotation, targetRotation,
                    rotationSpeed * Time.deltaTime);
            }
        }

        float step = moveSpeed * Time.deltaTime;
        item.transform.position = Vector3.MoveTowards(item.transform.position, targetPos, step);

        if (Vector3.Distance(item.transform.position, targetPos) < 0.001f)
        {
            item.transform.position = targetPos;
            item.SetIndex(targetIndex);

            int nextTarget = (targetIndex + 1) % m_ListPos.Count;

            if (isLinearPath && targetIndex == m_ListPos.Count - 1)
            {
                item.transform.position = m_ListPos[0];
                item.SetIndex(0);
                nextTarget = 1;

                if (rotateItems && m_ListPos.Count > 1)
                {
                    Vector3 startDir = (m_ListPos[1] - m_ListPos[0]).normalized;
                    float startAngle = Mathf.Atan2(startDir.y, startDir.x) * Mathf.Rad2Deg;
                    item.transform.rotation = Quaternion.AngleAxis(startAngle + rotationOffset, Vector3.forward);
                }
            }

            targetIndices[id] = nextTarget;
        }
    }

    #endregion

    #region CONFIGURATION

    [Header("--- Movement Settings ---")] [SerializeField]
    private float moveSpeed = 2.0f;

    [SerializeField] private float rotationSpeed = 15f;
    //[SerializeField] private bool rotateWithMovement = true;

    [SerializeField] private bool isLinearPath = true;

    [Header("--- Curve Settings ---")] [SerializeField]
    private bool smoothCorners = true;

    [SerializeField] private float cornerRadius = 0.5f;
    [SerializeField] private int cornerResolution = 10;

    #endregion

    #region REFERENCES

    [Header("--- References ---")] [SerializeField]
    private List<ConveyorItem> m_ConveyorList;

    [SerializeField] private List<ConveyorItem> m_QueueConveyor;
    [SerializeField] private List<Transform> m_Waypoints;

    #endregion

    #region UNITY EVENTS

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        EventManager.AddListener<OnStashPick>(OnStashPickCallBack);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnStashPick>(OnStashPickCallBack);
        // THÊM: Dọn dẹp tweens khi component bị disable
        transform.DOKill();
        if (_checkMathTween != null) _checkMathTween.Kill();
        if (_refillTween != null) _refillTween.Kill();
    }

    private void FixedUpdate()
    {
        if (!isRunning || m_ListPos.Count < 2) return;

        for (int i = 0; i < m_ConveyorList.Count; i++)
        {
            MoveItemContinuous(m_ConveyorList[i]);
        }
    }

    #endregion

    #region INITIALIZATION (PATH & VISUALS)

    public void Init()
    {
        InitPositions();
        InitItems();
        isRunning = true;
    }

    private void InitPositions()
    {
        m_ListPos.Clear();
        if (m_Waypoints == null || m_Waypoints.Count < 2) return;
        if (!smoothCorners)
        {
            foreach (var p in m_Waypoints)
                if (p)
                    m_ListPos.Add(p.position);
            return;
        }

        int count = m_Waypoints.Count;
        for (int i = 0; i < count; i++)
        {
            Vector3 pPrev = m_Waypoints[(i - 1 + count) % count].position;
            Vector3 pCurr = m_Waypoints[i].position;
            Vector3 pNext = m_Waypoints[(i + 1) % count].position;
            if (isLinearPath && (i == 0 || i == count - 1))
            {
                m_ListPos.Add(pCurr);
                continue;
            }

            AddRoundedCorner(pPrev, pCurr, pNext);
        }
    }

    private void AddRoundedCorner(Vector3 pPrev, Vector3 pCurr, Vector3 pNext)
    {
        Vector3 dirToPrev = (pPrev - pCurr).normalized;
        Vector3 dirToNext = (pNext - pCurr).normalized;
        float actualRadius = Mathf.Min(cornerRadius,
            Mathf.Min(Vector3.Distance(pCurr, pPrev), Vector3.Distance(pCurr, pNext)) / 2f);

        Vector3 startCurve = pCurr + dirToPrev * actualRadius;
        Vector3 endCurve = pCurr + dirToNext * actualRadius;

        for (int j = 0; j <= cornerResolution; j++)
        {
            float t = j / (float)cornerResolution;
            Vector3 point = (1 - t) * (1 - t) * startCurve + 2 * (1 - t) * t * pCurr + t * t * endCurve;
            m_ListPos.Add(point);
        }
    }

    private void InitItems()
    {
        if (m_ListPos.Count == 0) return;
        targetIndices.Clear();
        int totalPathPoints = m_ListPos.Count;
        int totalItems = m_ConveyorList.Count;

        for (int i = 0; i < totalItems; i++)
        {
            var item = m_ConveyorList[i];
            int pathIndex = Mathf.RoundToInt((float)i / totalItems * totalPathPoints) % totalPathPoints;

            item.transform.position = m_ListPos[pathIndex];
            item.SetIndex(pathIndex);

            int next = (pathIndex + 1) % totalPathPoints;
            targetIndices[item.GetInstanceID()] = next;
        }
    }

    #endregion

    #region GAMEPLAY LOGIC (INPUT & MATCHING)

    private bool _isProcessing = false;
    private Queue<Stash> _stashInputQueue = new Queue<Stash>();

    private Queue<Item> _pendingOverflow = new Queue<Item>();

    private void UpdateItemCountUI()
    {
        if (text == null) return;

        int currentCount = 0;

        if (m_ConveyorList != null)
        {
            currentCount += m_ConveyorList.Count(x => !x.IsEmpty);
        }

        text.text = $"{currentCount}/10";
    }

    // 1. NHẬN INPUT TỪ NGƯỜI DÙNG
    private void OnStashPickCallBack(OnStashPick cb)
    {
        // THAY ĐỔI QUAN TRỌNG: Chỉ chấp nhận input khi băng chuyền đang chạy (isRunning = true)
        if (!isRunning || cb.listItem == null || cb.listItem.Count == 0) return;

        _stashInputQueue.Enqueue(cb.Stash);
        TryProcessNextBatch();
    }

    // 2. ĐIỀU PHỐI VIÊN
    private void TryProcessNextBatch()
    {
        // THAY ĐỔI QUAN TRỌNG: Kiểm tra cả isRunning
        if (!isRunning || _isProcessing || _stashInputQueue.Count == 0) return;

        _isProcessing = true;
        Stash itemsToProcess = _stashInputQueue.Dequeue();
        OnStashDestroy cb = new OnStashDestroy();
        cb.Stash = itemsToProcess;
        EventManager.Trigger(cb);

        _pendingOverflow.Clear();

        ProcessBatch(itemsToProcess.ListItem);
        itemsToProcess.HandlerItem();
    }

    // 3. XỬ LÝ GÓI ITEM
    private void ProcessBatch(List<Item> items)
    {
        int itemIndex = 0;
        var emptyConveyorSlots = m_ConveyorList.Where(c => c.IsEmpty).ToList();
        foreach (var slot in emptyConveyorSlots)
        {
            if (itemIndex >= items.Count) break;
            slot.SetItem(items[itemIndex]);
            itemIndex++;
        }

        if (itemIndex < items.Count && m_QueueConveyor != null)
        {
            var emptyQueueSlots = m_QueueConveyor.Where(q => q.IsEmpty).ToList();
            foreach (var qSlot in emptyQueueSlots)
            {
                if (itemIndex >= items.Count) break;
                qSlot.SetItem(items[itemIndex]);
                itemIndex++;
            }
        }

        while (itemIndex < items.Count)
        {
            _pendingOverflow.Enqueue(items[itemIndex]);
            itemIndex++;
        }

        UpdateItemCountUI();
        CheckMath();
    }

    [Button]
    // 4. KIỂM TRA MATCH 3
    private void CheckMath()
    {
        if (_checkMathTween != null && _checkMathTween.IsActive()) _checkMathTween.Kill();

        _checkMathTween = DOVirtual.DelayedCall(0.45f, () =>
        {
            if (!isRunning) return;

            Dictionary<int, List<ConveyorItem>> itemGroups = new Dictionary<int, List<ConveyorItem>>();

            void AddToGroup(ConveyorItem item)
            {
                if (item != null && !item.IsEmpty && item.CurrentItem != null)
                {
                    int id = item.CurrentItem.ID;
                    if (!itemGroups.ContainsKey(id)) itemGroups[id] = new List<ConveyorItem>();
                    itemGroups[id].Add(item);
                }
            }

            m_ConveyorList.ForEach(AddToGroup);
            if (m_QueueConveyor != null) m_QueueConveyor.ForEach(AddToGroup);

            bool isEarn = false;
            foreach (var group in itemGroups.Values)
            {
                int matches = group.Count / 3;

                if (matches > 0)
                {
                    isEarn = true;

                    for (int j = 0; j < matches; j++)
                    {
                        List<Item> batch3Items = new List<Item>();

                        for (int i = 0; i < 3; i++)
                        {
                            int realIndex = j * 3 + i;

                            if (realIndex < group.Count && group[realIndex] != null)
                            {
                                var itemLogic = group[realIndex].CurrentItem;

                                if (itemLogic != null && !itemLogic.isEat)
                                {
                                    itemLogic.isEat = true;

                                    batch3Items.Add(itemLogic);

                                    group[realIndex].Earn();
                                }
                            }
                        }

                        if (batch3Items.Count > 0)
                        {
                            BoxSoldOut x = Instantiate(prefabBox, autoCenterLayout.transform);
                            autoCenterLayout.AddBox(x);

                            DOVirtual.DelayedCall(0.5f, () =>
                            {
                                // Kiểm tra an toàn: Nếu box x bị hủy trong lúc chờ, dừng lại.
                                if (x == null) return;

                                x.FlyToBox(batch3Items, () =>
                                {
                                    // Kiểm tra an toàn: Chỉ remove khi x và layout còn tồn tại
                                    if (x != null && autoCenterLayout != null)
                                    {
                                        autoCenterLayout.RemoveBox(x);
                                    }
                                });
                            }).SetTarget(gameObject); // Gắn target vào Controller
                        }
                    }
                }
            }

            if (isEarn)
            {
                UpdateItemCountUI();
            }

            if (isEarn || _pendingOverflow.Count > 0)
            {
                CallRefillDelayed();
            }
            else
            {
                OnBatchFinished();
            }
        }).SetTarget(gameObject); // <-- SetTarget để tự động kill khi object này bị Destroy
    }

    private Queue<List<Item>> q = new Queue<List<Item>>();
    private bool isProcessingSoldOut = false;

    private void ProcessNextSoldOut()
    {
        if (isProcessingSoldOut || q.Count == 0) return;
        isProcessingSoldOut = true;
        List<Item> currentBatch = q.Dequeue();

        AnimSoldOut(currentBatch);
    }

    private void AnimSoldOut(List<Item> items)
    {
        BoxSoldOut x = Instantiate(prefabBox, autoCenterLayout.transform);
        x.FlyToBox(items);

        DOVirtual.DelayedCall(0.5f, () =>
        {
            isProcessingSoldOut = false;
            ProcessNextSoldOut();
        });
    }

    private void CallRefillDelayed()
    {
        if (_refillTween != null && _refillTween.IsActive()) _refillTween.Kill();

        _refillTween = DOVirtual.DelayedCall(0.3f, RefillFromQueueAndOverflow)
            .SetTarget(gameObject);
    }

    private void RefillFromQueueAndOverflow()
    {
        if (!isRunning) return; // Kiểm tra an toàn

        bool anyActionTaken = false;

        if (m_QueueConveyor != null && m_QueueConveyor.Count > 0)
        {
            var validSourceItems = m_QueueConveyor.Where(q => !q.IsEmpty).ToList();
            var emptyTargets = m_ConveyorList.Where(c => c.IsEmpty).OrderBy(c => c.Index).ToList();

            int sourceIdx = 0;
            foreach (var target in emptyTargets)
            {
                if (sourceIdx >= validSourceItems.Count) break;

                var source = validSourceItems[sourceIdx];
                target.SetItem(source.CurrentItem);
                source.SetItem(null);

                anyActionTaken = true;
                sourceIdx++;
            }
        }

        // --- GIAI ĐOẠN B: Đẩy từ PendingOverflow vào bất kỳ chỗ nào trống (Conveyor hoặc Queue) ---
        if (_pendingOverflow.Count > 0)
        {
            var emptyConveyors = m_ConveyorList.Where(c => c.IsEmpty).OrderBy(c => c.Index).ToList();
            foreach (var slot in emptyConveyors)
            {
                if (_pendingOverflow.Count == 0) break;

                var item = _pendingOverflow.Dequeue();
                slot.SetItem(item);
                anyActionTaken = true;
            }

            if (m_QueueConveyor != null && _pendingOverflow.Count > 0)
            {
                var emptyQueues = m_QueueConveyor.Where(q => q.IsEmpty).ToList();
                foreach (var qSlot in emptyQueues)
                {
                    if (_pendingOverflow.Count == 0) break;

                    var item = _pendingOverflow.Dequeue();
                    qSlot.SetItem(item);
                    anyActionTaken = true;
                }
            }
        }

        if (anyActionTaken)
        {
            UpdateItemCountUI();
            CheckMath();
        }
        else
        {
            OnBatchFinished();
        }
    }

    private void OnBatchFinished()
    {
        bool isConveyorFull = m_ConveyorList.All(c => !c.IsEmpty);

        if (isConveyorFull)
        {
            _stashInputQueue.Clear();
            _pendingOverflow.Clear();

            DOVirtual.DelayedCall(1f, () => UIManager.Ins.OpenUI<LoseUI>());

            return;
        }

        if (LevelManager.Ins.currentLevel.Stash.Count == 0)
        {
            bool isConveyorEmpty = m_ConveyorList.All(c => c.IsEmpty);
            bool isQueueEmpty = m_QueueConveyor == null || m_QueueConveyor.All(q => q.IsEmpty);
            bool isPendingEmpty = _pendingOverflow.Count == 0;
            bool isInputEmpty = _stashInputQueue.Count == 0;

            if (isConveyorEmpty && isQueueEmpty && isPendingEmpty && isInputEmpty)
            {
                DOVirtual.DelayedCall(1.5f, () => { UIManager.Ins.OpenUI<WinUI>(); });
                _isProcessing = false;
                return;
            }
        }

        _isProcessing = false;
        TryProcessNextBatch();
    }

    #endregion
}