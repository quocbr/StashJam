using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public enum TutorialName
{
    None,
    StartTutorial,
    SecondPlaceTutorial
}

[Serializable]
public enum ShapeType
{
    None,
    Square,
    Round,
    HorizontalRetangle,
    VerticleRetangle
}

[Serializable]
public struct TutorialData
{
    public TutorialName Name;
    public ShapeType ShapeType;
    public Vector3 Scale;
    public Vector3 DescriptionPosition;
    public string Description;
}

public class Tutorial : Singleton<Tutorial>
{
    [Header("Hand")] [SerializeField] private Image hand;

    [SerializeField] private Animator handAnim;

    [Header("Area")] [SerializeField] private Image area;

    [SerializeField] private SpriteRenderer areaBG;
    [SerializeField] private SpriteMask square;
    [SerializeField] private SpriteMask round;
    [SerializeField] private SpriteMask horizontalRetangle;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private TutorialData[] datas;

    [Header("Area Button")] [SerializeField]
    private Image buttonArea;

    [SerializeField] private BaseButton tutorialBtn;

    //private Action clickAction = null;
    private TutorialData currentData;

    private Vector3 handRenderRotation;

    private void Awake()
    {
        area.gameObject.SetActive(false);
        hand.gameObject.SetActive(false);
        round.gameObject.SetActive(false);
        square.gameObject.SetActive(false);
        horizontalRetangle.gameObject.SetActive(false);
    }

    public void ButtonAction(Vector3 worldPosition, Action btnAction)
    {
        buttonArea.gameObject.SetActive(true);
        Vector3 UIPosition = Utils_Custom.ConvertWorldToUIPosition(worldPosition, GameManager.Ins.MainCanvasRect);
        tutorialBtn.Rect.anchoredPosition = UIPosition;

        tutorialBtn.AddListener(() => btnAction?.Invoke());
    }

    public void Message(string content)
    {
        tutorialText.gameObject.SetActive(true);
        tutorialText.text = content;
    }

    public void WorldClick(Vector3 worldPosition, Vector3 rotation, float offset)
    {
        transform.SetAsLastSibling();
        hand.gameObject.SetActive(true);
        handAnim.Play("Click");

        hand.rectTransform.DOKill();
        hand.rectTransform.localRotation = Quaternion.Euler(rotation);

        Vector3 startPosition =
            Utils_Custom.ConvertWorldToUIPosition(worldPosition, GameManager.Ins.MainCanvasRect);
        Vector3 end = startPosition - hand.rectTransform.up * offset;
        hand.rectTransform.localPosition = end;
        hand.rectTransform.DOLocalMove(startPosition, 0.5f)
            .From(end)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear);
    }

    public void AreaTutorial(TutorialName name, Action btnAction, Vector3 worldPosition, bool isFullRect = true)
    {
        //tutorialBtn.SetInteractable(false);
        transform.SetAsLastSibling();
        buttonArea.gameObject.SetActive(true);
        tutorialBtn.Rect.sizeDelta = new Vector2(Screen.width, Screen.height);
        tutorialBtn.AddListener(() =>
        {
            btnAction?.Invoke();
            //tutorialBtn.SetInteractable(false);
        });

        if (name == TutorialName.None)
        {
            DOVirtual.DelayedCall(0.01f, () => tutorialBtn.SetInteractable(true));
            return;
        }

        foreach (TutorialData data in datas)
            if (data.Name == name)
                currentData = data;

        float delay = 0f;
        if (!area.gameObject.activeInHierarchy)
        {
            area.gameObject.SetActive(true);
            areaBG.DOKill();
            tutorialBtn.DOKill();
            areaBG.DOFade(0.9f, 0.5f).From(0);
            delay = 0.5f;
        }

        Vector3 UIPosition = Utils_Custom.ConvertWorldToUIPosition(worldPosition, GameManager.Ins.MainCanvasRect);

        if (currentData.ShapeType == ShapeType.Square)
        {
            square.gameObject.SetActive(true);
            square.GetComponent<RectTransform>().anchoredPosition = UIPosition;
            square.transform.localScale = Vector3.zero;
            square.transform.DOKill();
            square.transform.DOScale(currentData.Scale, 0.5f)
                .From(currentData.Scale + Vector3.one * 100)
                .SetDelay(delay)
                .OnComplete(() => tutorialBtn.SetInteractable(true));
        }
        else if (currentData.ShapeType == ShapeType.Round)
        {
            round.gameObject.SetActive(true);
            round.GetComponent<RectTransform>().anchoredPosition = UIPosition;
            round.transform.localScale = Vector3.zero;
            round.transform.DOKill();
            round.transform.DOScale(currentData.Scale, 0.5f)
                .From(currentData.Scale + Vector3.one * 100)
                .SetDelay(delay)
                .OnComplete(() => tutorialBtn.SetInteractable(true));
        }
        else if (currentData.ShapeType == ShapeType.HorizontalRetangle)
        {
            horizontalRetangle.gameObject.SetActive(true);
            horizontalRetangle.GetComponent<RectTransform>().anchoredPosition = UIPosition;
            horizontalRetangle.transform.localScale = Vector3.zero;
            horizontalRetangle.transform.DOKill();
            horizontalRetangle.transform.DOScale(currentData.Scale, 0.5f)
                .From(currentData.Scale + Vector3.one * 100)
                .SetDelay(delay)
                .OnComplete(() => tutorialBtn.SetInteractable(true));
        }

        tutorialText.text = currentData.Description;
        tutorialText.gameObject.SetActive(true);
        tutorialText.GetComponent<RectTransform>().anchoredPosition = UIPosition + currentData.DescriptionPosition;

        if (!isFullRect)
        {
            tutorialBtn.Rect.sizeDelta = Vector2.one * 400;
            tutorialBtn.Rect.anchoredPosition = UIPosition;
        }
    }


    public void WorldDrag(Vector3 starPosition, Vector3 endPosition, Vector3 rotation, float moveTime = 2)
    {
        hand.DOKill();
        hand.gameObject.SetActive(true);
        Vector3 start = Utils_Custom.ConvertWorldToUIPosition(starPosition, GameManager.Ins.MainCanvasRect);
        Vector3 end = Utils_Custom.ConvertWorldToUIPosition(endPosition, GameManager.Ins.MainCanvasRect);

        hand.rectTransform.localPosition = start;
        hand.rectTransform.localRotation = Quaternion.Euler(rotation);
        hand.rectTransform.DOLocalMove(end, moveTime)
            .From(start)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

    public void UIClick(Vector3 sourcePosition, float offset, Vector3 rotation, Transform parent = null)
    {
        hand.gameObject.SetActive(true);
        hand.DOKill();
        hand.rectTransform.localRotation = Quaternion.Euler(rotation);
        hand.rectTransform.localPosition = sourcePosition - hand.rectTransform.up * offset;
        hand.rectTransform.DOLocalMove(hand.rectTransform.localPosition + hand.rectTransform.up * offset, 0.5f)
            .From(hand.rectTransform.localPosition)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear);
    }


    public void UIDrag(Vector3 starPosition, Vector3 endPosition, Vector3 rotation, float moveTime = 2,
        Transform parent = null)
    {
        hand.DOKill();
        hand.gameObject.SetActive(true);
        hand.rectTransform.localPosition = starPosition;
        hand.rectTransform.rotation = Quaternion.Euler(rotation);
        hand.rectTransform.DOLocalMove(endPosition, moveTime)
            .From(starPosition)
            .SetLoops(-1, LoopType.Restart);
    }

    public void Off()
    {
        OffHand();
        OffArea();

        buttonArea.gameObject.SetActive(false);
        tutorialText.gameObject.SetActive(false);
    }

    public void OffHand()
    {
        if (hand.gameObject.activeInHierarchy)
        {
            hand.rectTransform.DOKill();
            handAnim.Play("Idle");
            hand.rectTransform.localRotation = Quaternion.identity;
            hand.gameObject.SetActive(false);
        }
    }

    public void OffArea()
    {
        tutorialBtn.RemoveListener();
        if (area.gameObject.activeInHierarchy)
        {
            areaBG.DOKill();
            areaBG.DOFade(0f, 0.15f).From(0.5f).OnComplete(() =>
            {
                area.gameObject.SetActive(false);
                round.gameObject.SetActive(false);
                square.gameObject.SetActive(false);
                horizontalRetangle.gameObject.SetActive(false);
            });
        }
    }
}