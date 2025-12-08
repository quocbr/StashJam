using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = System.Object;

public class TSSceneLoader : MonoBehaviour
{
    public Image progressBar;
    private bool isSDKInitialized = false;
    private bool isTimeCompleted = false;

    private void Start()
    {
        progressBar.fillAmount = 0f;
        progressBar.DOFillAmount(1f, 3f)
            .SetEase(Ease.Linear)
            .OnComplete(() => { isTimeCompleted = true; })
            .SetTarget(this);
        TinySauce.SubscribeOnInitFinishedEvent(OnTinySauceInit);
        StartCoroutine(LoadSceneRoutine());
    }

    private void OnTinySauceInit(bool adConsent, bool trackingConsent)
    {
        isSDKInitialized = true;
    }

    private IEnumerator LoadSceneRoutine()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(1);
        asyncOperation.allowSceneActivation = false;
        yield return new WaitUntil(() => isTimeCompleted && isSDKInitialized);
        while (asyncOperation.progress < 0.9f)
        {
            yield return null;
        }

        asyncOperation.allowSceneActivation = true;
    }
}