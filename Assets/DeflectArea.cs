using UnityEngine;

public class DeflectArea : MonoBehaviour
{
    [SerializeField] private PlayerActions player;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.TryGetComponent(out SynchronizedProjectile proj))
            return;
        
        player.curDeflectable = proj;
    }

    private void OnTriggerExit(Collider other)
    {
        player.curDeflectable = null;
    }
}
