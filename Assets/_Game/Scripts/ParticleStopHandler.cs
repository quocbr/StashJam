using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleStopHandler : MonoBehaviour
{
    // Hàm này sẽ tự động chạy khi hiệu ứng Particle System dừng
    private void OnParticleSystemStopped()
    {
        transform.localScale = Vector3.one;
        PoolManager.Ins.Despawn(gameObject);
    }
}