using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HumanSort
{
    public class MarketingHand : MonoBehaviour
    {
        private const string ClickDown = "ClickDown";
        private const string ClickUp = "ClickUp";
        [SerializeField] private Animator animator;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }


        private void Update()
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0; // hoặc giữ nguyên layer của object
            transform.position = Vector3.Lerp(transform.position, worldPos, 5f);

            if (Input.GetMouseButtonDown(0))
            {
                animator.Play(ClickDown);
                //AudioManager.Ins.PlaySFX(AudioManager.MouseClick);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                animator.Play(ClickUp);
            }
        }
    }
}