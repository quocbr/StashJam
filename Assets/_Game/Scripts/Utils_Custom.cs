using System;
using System.Collections.Generic;
using UnityEngine;

public class Utils_Custom : MonoBehaviour
{
    public static Vector3 ConvertWorldToUIPosition(Vector3 worldPosition, RectTransform canvasRect)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvasRect.GetComponentInParent<Canvas>().worldCamera,
            out Vector2 uiPosition
        );

        return uiPosition;
    }

    public static string ConvertIntToText(int value)
    {
        if (value < 10000)
        {
            return value.ToString();
        }
        else if (value >= 10000 && value < 1000000)
        {
            return ((float)value / 1000f).ToString(".00") + "K";
        }
        else if (value >= 1000000 && value < 1000000000)
        {
            return ((float)value / 1000000).ToString(".00") + "M";
        }
        else
        {
            return ((float)value / 1000000000).ToString(".00") + "B";
        }
    }

    public static Vector3 GetBorderVector()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return Vector3.zero;
        }

        Rect safeArea = Screen.safeArea;

        Vector3 safeAreaTopRight = new Vector3(safeArea.xMax, safeArea.yMax, 0f);

        Vector3 screenBorderWorldPos = mainCamera.ScreenToWorldPoint(safeAreaTopRight);

        return new Vector3(Mathf.Abs(screenBorderWorldPos.x), Mathf.Abs(screenBorderWorldPos.y), 0f);
    }

    public static Vector3 ConvertUIToWorldPosition(RectTransform uiElement)
    {
        // Get the screen position of the UI element
        Vector3 screenPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, uiElement.position);

        // Convert the screen position to world position
        Vector3 worldPosition =
            Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Camera.main.nearClipPlane));

        return worldPosition;
    }

    public static string ReturnSecondsToMinutes(int seconds)
    {
        if (seconds / 60 > 0) return (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
        else return "00:" + (seconds % 60).ToString("00");
    }

    public static string ConvertSecondsToHours(int seconds)
    {
        int hours = seconds / 3600;
        int remainingSeconds = seconds % 3600;
        int minutes = remainingSeconds / 60;
        remainingSeconds %= 60;

        return $"{hours:D2}:{minutes:D2}:{remainingSeconds:D2}";
    }

    public static string ReturnSecondsToDays(int seconds)
    {
        int days = seconds / 86400;
        seconds %= 86400;
        int hours = seconds / 3600;
        seconds %= 3600;
        int minutes = seconds / 60;
        seconds %= 60;

        if (days > 0)
            return $"{days}d{(hours > 0 ? $"{hours}h" : "")}";
        if (hours > 0)
            return $"{hours}h{(minutes > 0 ? $"{minutes:00}m" : "")}";
        if (minutes > 0)
            return $"{minutes}m";
        if (seconds > 0)
            return $"{seconds:00}s";
        return "0";
    }

    public static void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        System.Random random = new System.Random();

        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // public static void PlayAnimation(SkeletonAnimation skeletonAnimation, string animationName,
    //     Action onStart = null, Action onComplete = null, bool loop = false,
    //     float timeScale = 1f)
    // {
    //     Spine.AnimationState.TrackEntryDelegate completeDelegate = null;
    //     completeDelegate = (trackEntry) =>
    //     {
    //         if (trackEntry.Animation.Name == animationName)
    //         {
    //             onComplete?.Invoke();
    //             skeletonAnimation.AnimationState.Complete -= completeDelegate;
    //         }
    //     };

    //     skeletonAnimation.AnimationState.ClearTracks();
    //     skeletonAnimation.timeScale = timeScale;
    //     skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);
    //     skeletonAnimation.AnimationState.Complete += completeDelegate;
    //     onStart?.Invoke();
    // }

    // public static void PlayAnimationUI(SkeletonGraphic skeletonGraphic, string animationName,
    //     Action onStart = null, Action onComplete = null, bool loop = false,
    //     float timeScale = 1f)
    // {
    //     Spine.AnimationState.TrackEntryDelegate completeDelegate = null;
    //     completeDelegate = (trackEntry) =>
    //     {
    //         if (trackEntry.Animation.Name == animationName)
    //         {
    //             onComplete?.Invoke();
    //             skeletonGraphic.AnimationState.Complete -= completeDelegate;
    //         }
    //     };

    //     skeletonGraphic.AnimationState.ClearTracks();
    //     skeletonGraphic.Skeleton.SetToSetupPose(); // <- thêm dòng này
    //     skeletonGraphic.AnimationState.Apply(skeletonGraphic.Skeleton); // <- và dòng này

    //     skeletonGraphic.timeScale = timeScale;
    //     skeletonGraphic.AnimationState.SetAnimation(0, animationName, loop);
    //     skeletonGraphic.AnimationState.Complete += completeDelegate;
    //     onStart?.Invoke();
    // }

    public static class GenericCache<TKey, TValue>
    {
        private static readonly Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

        // Retrieves the value associated with the key, or default(TValue) if the key does not exist
        public static TValue Get(TKey key)
        {
            return _cache.GetValueOrDefault(key);
        }

        // Adds a new key-value pair to the cache or updates the value if the key already exists
        public static void Add(TKey key, TValue value)
        {
            _cache[key] = value;
        }

        // Checks if the specified key exists in the cache
        public static bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }

        // Checks if the specified key exists in the cache and retrieves the value if it exists
        public static bool ContainsKey(TKey key, out TValue value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public static void Remove(TKey key)
        {
            // if (!_cache.ContainsKey(key))
            // {
            //     return;
            // }

            _cache.Remove(key);
        }

        public static void Clear()
        {
            _cache.Clear();
        }
    }
}