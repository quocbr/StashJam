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
        if (m_ConveyorList == null || m_ConveyorList.Count == 0 || m_ListPos == null || m_ListPos.Count == 0)
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

                // Gán lại index tại vị trí đó
                m_ConveyorList[i].SetIndex(targetPosIndex);
            }

            currentIndex = nextIndex;
        }
    }

    private void OnStashPickCallBack(OnStashPick cb)
    {
        if (cb.listItem == null || cb.listItem.Count == 0)
        {
            return;
        }

        // Sắp xếp conveyors theo thứ tự index
        var orderedConveyors = m_ConveyorList
            .OrderBy(c => c.Index)
            .ToList();

        int itemIndex = 0;

        // Chèn các item vào conveyors
        for (int i = 0; i < orderedConveyors.Count && itemIndex < cb.listItem.Count; i++)
        {
            if (orderedConveyors[i] != null && orderedConveyors[i].IsEmpty)
            {
                // Gán item vào conveyor
                orderedConveyors[i].SetItem(cb.listItem[itemIndex]);
                orderedConveyors[i].transform.position = m_ListPos[i];
                itemIndex++;
            }
        }

        // Kiểm tra nếu còn item chưa được chèn
        if (itemIndex < cb.listItem.Count)
        {
            int indexQueue = 0;
            for (int i = itemIndex; i < cb.listItem.Count; i++)
            {
                m_Queue[indexQueue].SetItem(cb.listItem[i]);
                indexQueue++;
            }
        }

        CheckMath();
    }

    private void Con(List<ConveyorItem> items)
    {
        // Sắp xếp conveyors theo thứ tự index
        var orderedConveyors = m_ConveyorList
            .OrderBy(c => c.Index)
            .ToList();

        int itemIndex = 0;

        // Chèn các item vào conveyors
        for (int i = 0; i < orderedConveyors.Count && itemIndex < items.Count; i++)
        {
            if (orderedConveyors[i] != null && orderedConveyors[i].IsEmpty)
            {
                // Gán item vào conveyor
                orderedConveyors[i].SetItem(items[itemIndex].CurrentItem);
                orderedConveyors[i].transform.position = m_ListPos[i];
                itemIndex++;
            }
        }
    }

    private void CheckMath()
    {
        Dictionary<int, List<ConveyorItem>> itemGroups = new Dictionary<int, List<ConveyorItem>>();

        // Kiểm tra m_ConveyorList
        for (int i = 0; i < m_ConveyorList.Count; i++)
        {
            if (m_ConveyorList[i] != null && !m_ConveyorList[i].IsEmpty)
            {
                int itemId = m_ConveyorList[i].CurrentItem.ID;

                if (!itemGroups.ContainsKey(itemId))
                {
                    itemGroups[itemId] = new List<ConveyorItem>();
                }

                itemGroups[itemId].Add(m_ConveyorList[i]);
            }
        }

        if (m_Queue != null)
        {
            foreach (var item in m_Queue)
            {
                if (item == null || item.IsEmpty)
                {
                    continue;
                }

                int itemId = item.CurrentItem.ID;

                if (!itemGroups.ContainsKey(itemId))
                {
                    itemGroups[itemId] = new List<ConveyorItem>();
                }

                itemGroups[itemId].Add(item);
            }
        }

        int countTriples = 0;
        bool isEarn = false;
        foreach (var group in itemGroups.Values)
        {
            int tripleCount = group.Count / 3;

            if (tripleCount > 0)
            {
                countTriples += tripleCount;
                int itemsToProcess = tripleCount * 3;
                for (int i = 0; i < itemsToProcess; i++)
                {
                    group[i].Earn();
                    isEarn = true;
                }
            }
        }

        Debug.Log($"Có {countTriples} loại items với đúng 3 cặp giống nhau");
        DOVirtual.DelayedCall(0.5f, () => { Con(m_Queue); });
    }
}