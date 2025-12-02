using System;
using HellTap.PoolKit;
using UnityEngine;
using Object = UnityEngine.Object;

public class PoolName
{
    public const string LockFX = "Lock";
    public const string SplashFX = "Water_Splash_02_air";
}

public class PoolManager : Singleton<PoolManager>
{
    [SerializeField] private Pool myPoolKit;

    public Pool MyPoolKit => myPoolKit;

    /// <summary>
    /// Spawn một prefab từ pool theo tên pool.
    /// </summary>
    /// <param name="poolName">Tên của pool (đặt trong PoolKit)</param>
    /// <param name="position">Vị trí spawn</param>
    /// <param name="rotation">Rotation spawn</param>
    /// <returns>Transform của instance vừa spawn hoặc null nếu không có pool hợp lệ.</returns>
    public T Spawn<T>(string poolName, Vector3 position, Quaternion rotation, Transform parent = null)
        where T : Component
    {
        Transform t = myPoolKit.Spawn(poolName, position, rotation, parent);
        if (t == null)
        {
            Debug.LogWarning(
                $"[PoolManagerService] Không thể spawn từ pool '{poolName}' — có thể chưa setup hoặc đã hết instance!");
            return null;
        }

        T comp = t.GetComponent<T>();
        if (comp == null)
        {
            Debug.LogWarning(
                $"[PoolManagerService] Instance spawn từ pool '{poolName}' không có component kiểu {typeof(T).Name}!");
        }

        return comp;
    }

    // Spawn với parent và trả về T
    public T Spawn<T>(string poolName, Transform parent)
        where T : Component
    {
        Transform t = myPoolKit.Spawn(poolName, Vector3.zero, Quaternion.identity, parent);
        if (t == null)
        {
            Debug.LogWarning(
                $"[PoolManagerService] Không thể spawn từ pool '{poolName}' — có thể chưa setup hoặc đã hết instance!");
            return null;
        }

        T comp = t.GetComponent<T>();
        if (comp == null)
        {
            Debug.LogWarning(
                $"[PoolManagerService] Instance spawn từ pool '{poolName}' không có component kiểu {typeof(T).Name}!");
        }

        return comp;
    }

    /// <summary>
    /// Despawn một object về pool.
    /// </summary>
    /// <param name="obj">Transform hoặc GameObject muốn despawn.</param>
    public void Despawn(Object obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("[PoolManagerService] Không thể despawn object, tham số null.");
            return;
        }

        Transform t = null;

        switch (obj)
        {
            case Transform tr:
                t = tr;
                break;
            case GameObject go:
                t = go.transform;
                break;
            case Component c:
                t = c.transform;
                break;
        }

        if (t != null)
        {
            myPoolKit.Despawn(t);
        }
        else
        {
            Debug.LogWarning(
                $"[PoolManagerService] Không thể despawn object ({obj}), kiểu không hợp lệ."
            );
        }
    }

    /// <summary>
    /// Despawn component cụ thể T — tiện cho code generic.
    /// </summary>
    public void Despawn<T>(T component) where T : Component
    {
        if (component == null)
        {
            Debug.LogWarning("[PoolManagerService] Không thể despawn component, tham số null.");
            return;
        }

        myPoolKit.Despawn(component.transform);
    }

    /// <summary>
    /// Despawn tất cả instance trong tất cả pool hiện có.
    /// </summary>
    public void DespawnAll()
    {
        // Nếu có PoolKit instance, gọi API despawn all pools
        if (myPoolKit != null)
        {
            myPoolKit.DespawnAll();
        }
    }

    /// <summary>
    /// Despawn tất cả instance trong một pool cụ thể.
    /// </summary>
    /// <param name="poolName">Tên pool cần despawn toàn bộ instance.</param>
    public void DespawnAll(string poolName)
    {
        Pool pool = PoolKit.Find(poolName);
        if (pool != null)
        {
            pool.DespawnAll();
        }
    }
}