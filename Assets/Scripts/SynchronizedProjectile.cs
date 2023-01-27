using System;
using Alteruna;
using Alteruna.Trinity;
using UnityEngine;
using Avatar = Alteruna.Avatar;
using Vector3 = UnityEngine.Vector3;

public class SynchronizedProjectile : Synchronizable
{
    [SerializeField, Range(0, 5)] float DeflectionSpeedMultiplier = 1.5f;

    public Spawner _spawner;
    public UInt16 _ownerIndex;

    public Vector3 _direction;
    private Vector3 _oldDirection;

    [SerializeField] private float _speed = 5.0f;
    private float _oldSpeed;
    
    public int damage = 1;

    public void Start()
    {
        Debug.Log("Proj was spawned");
        _spawner = FindObjectOfType<Spawner>();
        _direction = transform.forward;
        
        // register get method
        Multiplayer.RegisterRemoteProcedure("RemoteGetOwnerIndex", RemoteGetOwnerIndex);
    }

    [SynchronizableMethod]
    public void Init(UInt16 fromIndex)
    {
        _ownerIndex = fromIndex;
#if UNITY_EDITOR
        GameManager.Instance.PrintDebug("Projectile owner index: ", _ownerIndex);
#endif

    }
    
    public void RemoteGetOwnerIndex(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        UInt16 fromIndex = parameters.Get("playerIndex", (UInt16) 0);
        _ownerIndex = fromIndex;
#if UNITY_EDITOR
        GameManager.Instance.PrintDebug("Projectile owner index: ", _ownerIndex + " , This Player: " + Multiplayer.Me.Index);
#endif
    }
    
    private void Update()
    {
        if (_oldDirection != _direction | _oldSpeed != _speed)
        {
            Commit();
            _oldDirection = _direction;
            _oldSpeed = _speed;
        }
        
        transform.position += _direction * (_speed * Time.deltaTime);
        SyncUpdate();
    }
    
    public override void AssembleData(Writer writer, byte LOD = 100)
    {
        writer.Write(_direction);
        writer.Write(_speed);
    }

    public override void DisassembleData(Reader reader, byte LOD = 100)
    {
        _direction = reader.ReadVector3();
        _oldDirection = _direction;

        _speed = reader.ReadFloat();
        _oldSpeed = _speed;
    }

    public void OnDeflect(Vector3 newDirection)
    {
        _direction = newDirection;
        _speed *= DeflectionSpeedMultiplier;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var otherGo = other.gameObject;
        if (!otherGo.CompareTag("Player"))
            return;
        
        var damageable = otherGo.GetComponentInChildren<DamageableComponent>();
        Vector3 direction = (otherGo.transform.position - transform.position).normalized;
        if (damageable)
            damageable.OnHit(1, direction * _speed, this.gameObject);
    }
    
    /*
    private void DestroySelf()
    {
        if (Multiplayer.Instance.Me.Index == _ownerIndex)
            _spawner.Despawn(userindex, gameObject);
    }
    */
    
    /*
    private void OnCollisionEnter(Collision collision)
    {
        var damageable = collision.gameObject.GetComponentInChildren<DamageableComponent>();
        if (damageable)
            damageable.OnHit(1, _direction);

        DestroySelf();
    }
    */
}
