using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class UICanvas : MonoBehaviour
{
    //public bool IsAvoidBackKey = false;
    public bool IsDestroyOnClose = false;

    //Custom
    [Header("Open Anim")] [SerializeField] private bool isPopupAnim;

    [SerializeField] private bool isMoveAnim;

    [ShowIf("@isMoveAnim || isPopupAnim")] [SerializeField]
    private CanvasGroup cvGroupBackGround;

    [ShowIf("@isMoveAnim || isPopupAnim")] [SerializeField]
    private RectTransform rectTfMain;

    [Header("Close Anim")] [SerializeField]
    private bool isCloseAnim;

    private float initScale = 1f;
    private Animator m_Animator;

    protected RectTransform m_RectTransform;
    public Action OnClosePopup;

    private void Start()
    {
        OnInit();
    }

    //Init default Canvas
    //khoi tao gia tri canvas
    protected virtual void OnInit()
    {
        m_RectTransform = GetComponent<RectTransform>();
        m_Animator = GetComponent<Animator>();
    }

    //Setup canvas to avoid flash UI
    //set up mac dinh cho UI de tranh truong hop bi nhay' hinh
    public virtual void Setup()
    {
        UIManager.Ins.AddBackUI(this);
        UIManager.Ins.PushBackAction(this, BackKey);
    }

    //back key in android device
    //back key danh cho android
    public virtual void BackKey()
    {
    }

    //Open canvas
    //mo canvas
    public virtual void Open()
    {
        OnClosePopup = null;
        gameObject.SetActive(true);
        UIManager.Ins.CurrentTopUI = this;
        transform.SetAsLastSibling();

        if (isPopupAnim)
            PopUpAnim();
        else if (isMoveAnim)
            MoveAnim();
    }

    //close canvas directly
    //dong truc tiep, ngay lap tuc
    public virtual void CloseDirectly()
    {
        OnClosePopup?.Invoke();
        UIManager.Ins.RemoveBackUI(this);
        gameObject.SetActive(false);
        if (IsDestroyOnClose)
        {
            Destroy(gameObject);
        }
    }

    //close canvas with delay time, used to anim UI action
    //dong canvas sau mot khoang thoi gian delay
    public virtual void Close(float delayTime)
    {
        if (isCloseAnim)
        {
            if (delayTime == 0)
            {
                delayTime = 0.5f;
            }

            CloseAnim();
        }

        Invoke(nameof(CloseDirectly), delayTime);
    }

    private void PopUpAnim()
    {
        cvGroupBackGround.DOKill();
        rectTfMain.DOKill();

        if (cvGroupBackGround != null)
        {
            cvGroupBackGround.alpha = 0f;
            cvGroupBackGround.DOFade(1, 0.1f).SetEase(Ease.Linear).SetUpdate(true);
        }

        rectTfMain.localScale = Vector2.zero;
        rectTfMain.DOScale(1, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void MoveAnim()
    {
        cvGroupBackGround.DOKill();
        rectTfMain.DOKill();

        if (cvGroupBackGround != null)
        {
            cvGroupBackGround.alpha = 0f;
            cvGroupBackGround.DOFade(1f, 0.1f).SetEase(Ease.Linear).SetUpdate(true);
        }

        rectTfMain.anchoredPosition = new Vector2(0, -Screen.height);
        rectTfMain.DOAnchorPos(Vector2.zero, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void CloseAnim()
    {
        cvGroupBackGround.DOKill();
        rectTfMain.DOKill();

        if (cvGroupBackGround != null)
        {
            cvGroupBackGround.alpha = 1f;
            cvGroupBackGround.DOFade(0f, 0.25f).SetEase(Ease.Linear);
        }

        if (isMoveAnim)
        {
            rectTfMain.DOAnchorPos(new Vector2(0, -Screen.height), 0.25f)
                .SetEase(Ease.OutBack)
                .From(rectTfMain.anchoredPosition);
        }
        else if (isPopupAnim)
        {
            rectTfMain.DOScale(0, 0.25f)
                .SetEase(Ease.InBack);
        }
    }

    protected void PauseGame()
    {
        Time.timeScale = 0.0001f;
    }

    protected void ContinueGame()
    {
        Time.timeScale = 1f;
    }
}