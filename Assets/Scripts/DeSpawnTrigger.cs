using System;
using Alteruna;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DeSpawnTrigger : MonoBehaviour
{
    private Spawner _spawner;

    private void Awake()
    {
        _spawner = FindObjectOfType<Spawner>();
        if (!_spawner)
            throw new NullReferenceException("_spawner is null!");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Projectile")) return;
        if (!other.gameObject.TryGetComponent(out SynchronizedProjectile projectile)) return;
        
        if (projectile._ownerIndex == Multiplayer.Instance.Me.Index)
            _spawner.Despawn(other.gameObject);
    }
}
