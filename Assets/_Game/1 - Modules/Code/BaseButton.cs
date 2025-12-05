//using MoreMountains.Feedbacks;

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class BaseButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Button button;

    [SerializeField] private Image image;
    //[SerializeField] private MMF_Player MMFClick;

    [field: SerializeField] public RectTransform Rect { get; private set; }
    private float initialHoldDelay = 0f;

    private bool isInteractable = true;

    private bool isPressed = false;

    public Action OnPress;
    public Action OnRelease;

    private void OnEnable()
    {
        StopAllCoroutines();
        isPressed = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isPressed = false;
    }

    public void OnPointerClick(PointerEventData eventData) // Animation and sound support when click
    {
        if (!isInteractable) return;

        //MMFClick?.PlayFeedbacks();
        //AudioManager.Ins.PlaySFX(clickSFX);
        SoundManager.Ins.PlaySFX(SoundFX.UI_Click);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isInteractable) return;

        isPressed = true;
        StartCoroutine(HoldPressCoroutine()); // Bắt đầu lặp OnPress khi giữ
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isInteractable) return;

        isPressed = false;
        OnRelease?.Invoke(); // Gọi OnRelease khi thả
        StopAllCoroutines(); // Dừng coroutine nhấn giữ
    }

    public void AddListener(UnityAction action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    public void AddListenerHold(Action action)
    {
        OnPress = action;
    }

    public void AddListenerUp(Action action)
    {
        OnRelease = action;
    }

    public void RemoveListener()
    {
        button.onClick.RemoveAllListeners();
    }

    public void SetInteractable(bool isInteractable, bool needEffect = true)
    {
        this.isInteractable = isInteractable;
        button.interactable = this.isInteractable;

        if (needEffect)
        {
            if (isInteractable)
            {
                image.material = null;
            }
            else
            {
                //image.material = GameSetting.Ins.GrayMaterial;
            }
        }
    }

    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }

    private IEnumerator HoldPressCoroutine()
    {
        // Đợi độ trễ ban đầu trước khi lặp
        yield return new WaitForSeconds(initialHoldDelay);

        // Lặp gọi OnPress khi vẫn giữ nút
        while (isPressed && isInteractable)
        {
            OnPress?.Invoke();
            yield return null;
        }
    }

    [Button]
    private void Fetch()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        Rect = GetComponent<RectTransform>();
    }
}