using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class ConveyorController : Singleton<ConveyorController>
{
    private Tween _checkMathTween;

    private Tween _refillTween; // Map: ItemID -> Index đích đến

    // Runtime Variables
    private bool isRunning = false;
    private List<Vector3> m_ListPos = new List<Vector3>(); // Danh sách điểm chi tiết (sau khi làm mềm)
    private Dictionary<int, int> targetIndices = new Dictionary<int, int>();

    #region MOVEMENT LOGIC

    private void MoveItemContinuous(ConveyorItem item)
    {
        if (item == null) return;
        int id = item.GetInstanceID();
        if (!targetIndices.ContainsKey(id)) return;

        int targetIndex = targetIndices[id];
        Vector3 targetPos = m_ListPos[targetIndex];

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
            }

            targetIndices[id] = nextTarget;
        }
    }

    #endregion

    #region CONFIGURATION

    [Header("--- Movement Settings ---")] [SerializeField]
    private float moveSpeed = 2.0f;

    [SerializeField] private float rotationSpeed = 15f; // Tốc độ xoay (càng lớn xoay càng nhanh)
    [SerializeField] private bool rotateWithMovement = true; // Bật/tắt tính năng xoay

    [SerializeField] private bool isLinearPath = true; // True: Teleport về đầu. False: Chạy vòng tròn.

    [Header("--- Curve Settings ---")] [SerializeField]
    private bool smoothCorners = true; // Bật làm mềm góc

    [SerializeField] private float cornerRadius = 0.5f; // Độ rộng góc cua
    [SerializeField] private int cornerResolution = 10; // Độ mượt

    [Header("--- Visuals ---")] [SerializeField]
    private SpriteRenderer trayPrefab; // Prefab nền khay

    [SerializeField] private Sprite[] traySprites; // Bộ 16 sprite bitmask

    #endregion

    #region REFERENCES

    [Header("--- References ---")] [SerializeField]
    private List<ConveyorItem> m_ConveyorList; // Các slot trên băng chuyền

    [SerializeField] private List<ConveyorItem> m_Queue; // Các slot hàng chờ
    [SerializeField] private List<Transform> m_Waypoints; // Các điểm neo đường đi

    #endregion

    #region UNITY EVENTS

    private void Start()
    {
        InitPositions();

        //GenerateTrays();

        InitItems();

        isRunning = true;
    }

    private void OnEnable()
    {
        EventManager.AddListener<OnStashPick>(OnStashPickCallBack);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnStashPick>(OnStashPickCallBack);
    }

    private void FixedUpdate()
    {
        if (!isRunning || m_ListPos.Count < 2) return;

        // Loop di chuyển liên tục
        for (int i = 0; i < m_ConveyorList.Count; i++)
        {
            MoveItemContinuous(m_ConveyorList[i]);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && m_ListPos.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < m_ListPos.Count - 1; i++)
                Gizmos.DrawLine(m_ListPos[i], m_ListPos[i + 1]);

            if (!isLinearPath) Gizmos.DrawLine(m_ListPos[m_ListPos.Count - 1], m_ListPos[0]);
        }
    }

    #endregion

    #region INITIALIZATION (PATH & VISUALS)

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

        // Rải đều item lên đường đi
        int totalPathPoints = m_ListPos.Count;
        int totalItems = m_ConveyorList.Count;

        for (int i = 0; i < totalItems; i++)
        {
            var item = m_ConveyorList[i];
            // Map index item sang index đường đi
            int pathIndex = Mathf.RoundToInt((float)i / totalItems * totalPathPoints) % totalPathPoints;

            item.transform.position = m_ListPos[pathIndex];
            item.SetIndex(pathIndex);

            int next = (pathIndex + 1) % totalPathPoints;
            targetIndices[item.GetInstanceID()] = next;
        }
    }

    private bool HasNeighbor(Vector3 center, Vector3 dir, float dist)
    {
        foreach (var wp in m_Waypoints)
        {
            if (wp == null) continue;
            if (Vector3.Distance(center + dir, wp.position) < 0.5f) return true;
        }

        return false;
    }

    #endregion

    #region GAMEPLAY LOGIC (INPUT & MATCHING)

    private void OnStashPickCallBack(OnStashPick cb)
    {
        if (cb.listItem == null) return;
        ProcessInputItems(cb.listItem);
    }

    private void ProcessInputItems(List<Item> items)
    {
        int itemIndex = 0;

        // 1. Điền vào Băng chuyền trước
        var emptyConveyorSlots = m_ConveyorList.Where(c => c.IsEmpty).ToList();
        foreach (var slot in emptyConveyorSlots)
        {
            if (itemIndex >= items.Count) break;
            slot.SetItem(items[itemIndex]);
            itemIndex++;
        }

        // 2. Điền vào Queue sau
        if (itemIndex < items.Count && m_Queue != null)
        {
            var emptyQueueSlots = m_Queue.Where(q => q.IsEmpty).ToList();
            foreach (var qSlot in emptyQueueSlots)
            {
                if (itemIndex >= items.Count) break;
                qSlot.SetItem(items[itemIndex]);
                itemIndex++;
            }
        }

        CheckMath();
    }

    private void CheckMath()
    {
        // 1. DEBOUNCE: Nếu đang có lệnh chờ check, hủy nó đi để dùng lệnh mới nhất
        if (_checkMathTween != null && _checkMathTween.IsActive())
        {
            _checkMathTween.Kill();
        }

        // 2. Gọi lệnh delay mới
        _checkMathTween = DOVirtual.DelayedCall(0.5f, () =>
        {
            // --- CHUYỂN TOÀN BỘ LOGIC TÍNH TOÁN VÀO TRONG NÀY ---

            // Gom nhóm item theo ID (Tính toán tại thời điểm thực thi)
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
            if (m_Queue != null) m_Queue.ForEach(AddToGroup);

            bool isEarn = false;

            foreach (var group in itemGroups.Values)
            {
                int matches = group.Count / 3;
                if (matches > 0)
                {
                    for (int i = 0; i < matches * 3; i++)
                    {
                        if (i < group.Count && group[i] != null)
                        {
                            group[i].Earn();
                            isEarn = true;
                        }
                    }
                }
            }

            // Nếu có ăn điểm -> Gọi Refill
            if (isEarn)
            {
                CallRefillDelayed();
            }
        });
    }

    private void CallRefillDelayed()
    {
        // Hủy lệnh refill cũ nếu có (tránh việc gọi refill 2 lần liên tiếp quá nhanh)
        if (_refillTween != null && _refillTween.IsActive())
        {
            _refillTween.Kill();
        }

        _refillTween = DOVirtual.DelayedCall(0.5f, RefillFromQueue);
    }

    private void RefillFromQueue()
    {
        if (m_Queue == null || m_Queue.Count == 0) return;

        var validSourceItems = m_Queue.Where(q => !q.IsEmpty).ToList();
        if (validSourceItems.Count == 0) return;

        // OrderBy Index để lấp đầy từ đầu đến cuối
        var emptyTargets = m_ConveyorList.Where(c => c.IsEmpty).OrderBy(c => c.Index).ToList();

        int sourceIdx = 0;
        foreach (var target in emptyTargets)
        {
            if (sourceIdx >= validSourceItems.Count) break;

            var source = validSourceItems[sourceIdx];

            // Chuyển data
            target.SetItem(source.CurrentItem);
            source.SetItem(null);

            // (Nếu bạn muốn thêm lại hiệu ứng bay Visual ở đây thì thêm vào, code hiện tại là chuyển tức thì)

            sourceIdx++;
        }

        // Nếu có sự thay đổi (có item mới lấp vào), check math lại lần nữa
        if (sourceIdx > 0)
        {
            CheckMath();
        }
    }

    #endregion
}