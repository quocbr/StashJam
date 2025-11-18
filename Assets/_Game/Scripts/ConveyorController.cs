using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class ConveyorController : Singleton<ConveyorController>
{
    [SerializeField] private List<ConveyorItem> m_ConveyorList;
    [SerializeField] private List<ConveyorItem> m_Queue;
    [SerializeField] private List<Vector3> m_ListPos;
    [SerializeField] private float moveDuration = 1f;

    private void Start()
    {
        m_ListPos.Clear();

        for (int i = 0; i < m_ConveyorList.Count; i++)
        {
            m_ListPos.Add(m_ConveyorList[i].transform.position);
            m_ConveyorList[i].SetIndex(i);
        }

        Test();
    }

    private void OnEnable()
    {
        EventManager.AddListener<OnStashPick>(OnStashPickCallBack);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener<OnStashPick>(OnStashPickCallBack);
    }

    [Button]
    private void Test()
    {
        StartCoroutine(RunAnim());
    }

    private IEnumerator RunAnim()
    {
        if (m_ConveyorList == null || m_ConveyorList.Count == 0 ||
            m_ListPos == null || m_ListPos.Count == 0)
        {
            yield break;
        }

        int currentIndex = 0;

        while (true)
        {
            int nextIndex = (currentIndex + 1) % m_ListPos.Count;

            float elapsed = 0f;
            Vector3[] startPositions = new Vector3[m_ConveyorList.Count];

            for (int i = 0; i < m_ConveyorList.Count; i++)
            {
                startPositions[i] = m_ConveyorList[i].transform.position;
            }

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveDuration;

                for (int i = 0; i < m_ConveyorList.Count; i++)
                {
                    int targetPosIndex = (i + nextIndex) % m_ListPos.Count;
                    m_ConveyorList[i].transform.position = Vector3.Lerp(
                        startPositions[i],
                        m_ListPos[targetPosIndex],
                        t
                    );
                }

                yield return null;
            }

            // Gán lại vị trí cuối cùng và cập nhật index cho từng item
            for (int i = 0; i < m_ConveyorList.Count; i++)
            {
                int targetPosIndex = (i + nextIndex) % m_ListPos.Count;
                m_ConveyorList[i].transform.position = m_ListPos[targetPosIndex];

                m_ConveyorList[i].SetIndex(targetPosIndex);
            }

            currentIndex = nextIndex;
        }
    }

    private void OnStashPickCallBack(OnStashPick cb)
    {
        if (cb.listItem == null || cb.listItem.Count == 0)
            return;

        // Sắp xếp conveyors theo thứ tự Index
        var orderedConveyors = m_ConveyorList
            .OrderBy(c => c.Index)
            .ToList();

        int itemIndex = 0;

        // Chèn các item vào conveyors
        for (int i = 0; i < orderedConveyors.Count && itemIndex < cb.listItem.Count; i++)
        {
            var conv = orderedConveyors[i];
            if (conv != null && conv.IsEmpty)
            {
                var item = cb.listItem[itemIndex];

                conv.SetItem(item);
                // dùng Index thật của conveyor để lấy đúng vị trí
                conv.transform.position = m_ListPos[conv.Index];

                itemIndex++;
            }
        }

        // Nếu còn dư item -> đẩy vào Queue
        if (itemIndex < cb.listItem.Count && m_Queue != null)
        {
            int indexQueue = 0;
            for (int i = itemIndex; i < cb.listItem.Count && indexQueue < m_Queue.Count; i++)
            {
                if (m_Queue[indexQueue] != null && m_Queue[indexQueue].IsEmpty)
                {
                    m_Queue[indexQueue].SetItem(cb.listItem[i]);
                    indexQueue++;
                }
                else
                {
                    indexQueue++;
                    i--; // thử lại item này với slot queue tiếp theo
                }
            }
        }

        CheckMath();
    }

    /// <summary>
    /// Đổ items từ queue lên các conveyor trống.
    /// </summary>
    private void Con(List<ConveyorItem> items)
    {
        if (items == null || items.Count == 0)
            return;

        // Lấy list các item thật sự còn trong queue (không null, không empty)
        var queuedItems = items
            .Where(q => q != null && !q.IsEmpty && q.CurrentItem != null)
            .ToList();

        if (queuedItems.Count == 0)
            return;

        // Sắp xếp conveyors theo index
        var orderedConveyors = m_ConveyorList
            .OrderBy(c => c.Index)
            .ToList();

        int srcIndex = 0;

        foreach (var conv in orderedConveyors)
        {
            if (srcIndex >= queuedItems.Count)
                break;

            if (conv == null || !conv.IsEmpty)
                continue;

            var queueSlot = queuedItems[srcIndex];

            // Gán item từ queue sang conveyor
            conv.SetItem(queueSlot.CurrentItem);
            conv.transform.position = m_ListPos[conv.Index];

            // Clear slot trong queue
            queueSlot.SetItem(null);

            srcIndex++;
        }
    }

    private void CheckMath()
    {
        Dictionary<int, List<ConveyorItem>> itemGroups = new Dictionary<int, List<ConveyorItem>>();

        // Gom nhóm trên băng chuyền
        for (int i = 0; i < m_ConveyorList.Count; i++)
        {
            var conv = m_ConveyorList[i];
            if (conv != null && !conv.IsEmpty && conv.CurrentItem != null)
            {
                int itemId = conv.CurrentItem.ID;

                if (!itemGroups.ContainsKey(itemId))
                    itemGroups[itemId] = new List<ConveyorItem>();

                itemGroups[itemId].Add(conv);
            }
        }

        // Gom thêm từ queue
        if (m_Queue != null)
        {
            foreach (var q in m_Queue)
            {
                if (q == null || q.IsEmpty || q.CurrentItem == null)
                    continue;

                int itemId = q.CurrentItem.ID;

                if (!itemGroups.ContainsKey(itemId))
                    itemGroups[itemId] = new List<ConveyorItem>();

                itemGroups[itemId].Add(q);
            }
        }

        int countTriples = 0;
        bool isEarn = false;

        foreach (var group in itemGroups.Values)
        {
            int tripleCount = group.Count / 3;

            if (tripleCount <= 0)
                continue;

            countTriples += tripleCount;
            int itemsToProcess = tripleCount * 3;

            for (int i = 0; i < itemsToProcess && i < group.Count; i++)
            {
                // group[i] có thể là Conveyor trên băng hoặc trong queue
                if (group[i] != null && !group[i].IsEmpty)
                {
                    group[i].Earn(); // giả sử Earn() sẽ clear item
                    isEarn = true;
                }
            }
        }

        Debug.Log($"Có {countTriples} bộ 3 item giống nhau");

        if (isEarn)
        {
            // Sau khi ăn, delay 1 chút rồi pack lại queue lên conveyor
            DOVirtual.DelayedCall(0.5f, () => { Con(m_Queue); });
        }
    }
}