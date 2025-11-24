using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector; // Cần DOTween

public class AutoCenterLayout : MonoBehaviour
{
    [Header("Settings")] [SerializeField] private float spacing = 1.5f;

    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private Ease moveEase = Ease.OutBack;

    [Header("Spawn Animation")] [SerializeField]
    private float spawnYDistance = 5f; // Khoảng cách từ dưới bay lên

    [SerializeField] private Ease spawnEase = Ease.OutBack;
    [SerializeField] private Ease despawnEase = Ease.InBack;

    [Header("Debug")] [SerializeField] private List<BoxSoldOut> boxes = new List<BoxSoldOut>();
    [SerializeField] private BoxSoldOut boxPrefab;

    public void AddBox(BoxSoldOut newBox)
    {
        newBox.transform.SetParent(transform);
        int newCount = boxes.Count + 1;
        int newIndex = boxes.Count;
        float centerOffset = (newCount - 1) / 2f;
        float targetX = (newIndex - centerOffset) * spacing;
        newBox.transform.localPosition = new Vector3(targetX, -spawnYDistance, 0);
        boxes.Add(newBox);
        RecalculatePositions();
    }

    public void RemoveBox(BoxSoldOut boxToRemove)
    {
        if (boxes.Contains(boxToRemove))
        {
            boxes.Remove(boxToRemove);
            boxToRemove.transform.DOScale(0.3f, moveDuration).SetDelay(0.2f);
            boxToRemove.transform
                .DOLocalMoveX(-spawnYDistance, moveDuration)
                .SetEase(despawnEase)
                .SetDelay(0.2f)
                .OnComplete(() =>
                {
                    if (boxToRemove != null) Destroy(boxToRemove.gameObject);
                });
        }
    }

    public void RecalculatePositions()
    {
        if (boxes.Count == 0) return;

        int count = boxes.Count;
        float centerOffset = (count - 1) / 2f;

        int remove = 0;

        for (int i = 0; i < count; i++)
        {
            float newX = (i - centerOffset + remove) * spacing;
            Vector3 targetLocalPos = new Vector3(newX, 0, 0);
            boxes[i].transform.DOLocalMove(targetLocalPos, moveDuration).SetEase(moveEase);
        }
    }
}