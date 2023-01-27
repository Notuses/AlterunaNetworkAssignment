using System;
using Alteruna;
using Alteruna.Trinity;
using UnityEngine;

public class PlayerActions : AttributesSync
{
    // TODO: add charge up on projectiles
    
    private Alteruna.Avatar avatar;
    [SerializeField]private Spawner spawner;
    
    // Attack
    [SerializeField] private float attackCoolDown = 0.5f;
    private bool canAttack = true;
    private float curAttackCoolDown;
    
    // Deflect
    [SerializeField] private BoxCollider deflectArea;
    public SynchronizedProjectile curDeflectable = null;
    
    [SerializeField] private float deflectCoolDown = 5.0f;
    private float curDeflectCoolDown;
    private float curShieldUptime;
    private bool canDeflect = true;   



    public delegate void ShootDelegate();
    public event ShootDelegate OnShoot;

    public delegate void DeflectDelegate();
    public event DeflectDelegate OnTryDeflect;

    public delegate void TauntDelegate();
    public event TauntDelegate OnTaunt;

    public GameObject deflectShield;
    private PlayerStateSync playerState;
    private Light shieldLight;
    private MaterialPropertyBlockPlasma shieldMaterialPropertyBlock;

    Vector3 hiddenShieldLocation;
    Vector3 currentShieldLocation;
    Vector3 lastShieldLocation;
    Quaternion currentShieldRotation;
    Quaternion lastShieldRotation;
    [SynchronizableField] private bool deflecting = false;
    [SynchronizableField] private bool deflectSuccess = false;


    private void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
    }

    private void Start()
    {
        playerState = transform.parent.GetComponentInChildren<PlayerStateSync>();

        curAttackCoolDown = attackCoolDown;
        curDeflectCoolDown = deflectCoolDown;
        curShieldUptime = deflectCoolDown;
        avatar = gameObject.GetComponentInParent(typeof(Alteruna.Avatar)) as Alteruna.Avatar;

        Multiplayer.RegisterRemoteProcedure("ShootRemote", ShootRemote);
        Multiplayer.RegisterRemoteProcedure("DeflectRemote", DeflectRemote);
        Multiplayer.RegisterRemoteProcedure("TauntRemote", TauntRemote);

        hiddenShieldLocation = new Vector3(-500, -500, 0);
        deflectShield = Instantiate(deflectShield);        
        deflectShield.transform.position = hiddenShieldLocation;

       
        shieldLight = deflectShield.GetComponentInChildren<Light>();
        shieldMaterialPropertyBlock = deflectShield.GetComponent<MaterialPropertyBlockPlasma>();
    }
    
    private void Update()
    {
        ShowReflectVFX();
        currentShieldLocation = transform.parent.position;
        currentShieldRotation = transform.parent.rotation;
        if (!deflecting)
        {
            lastShieldLocation = currentShieldLocation + transform.parent.forward * 2 * Time.deltaTime;
            lastShieldRotation = currentShieldRotation;
        }

        if (!avatar.IsMe || !playerState.isAlive)
            return;       

        if (Input.GetMouseButtonDown(0))
            if (canAttack)
                Shoot();
        
        if (Input.GetMouseButtonDown(1) && canDeflect)
        {
            deflecting = true;

            if (CheckDeflectable())
            {
                deflectSuccess = true;
                OnDeflectSuccess();
            }                
            else
            {
                deflectSuccess = false;                
                OnDeflectMiss();
            }               
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Taunt();
            Multiplayer.InvokeRemoteProcedure("TauntRemote", UserId.All);
        }

       


        curShieldUptime -= Time.deltaTime;
        if (curShieldUptime <= deflectCoolDown - 1.5f)
        {
            deflecting = false;
            curShieldUptime = deflectCoolDown;            
        }

        

        if (!canAttack)
        {
            curAttackCoolDown -= Time.deltaTime;
            if (curAttackCoolDown <= 0)
            {
                canAttack = true;
                curAttackCoolDown = attackCoolDown;
            }
        }
        
        if (!canDeflect)
        {            
            curDeflectCoolDown -= Time.deltaTime;
            if (curDeflectCoolDown <= 0)
            {
                shieldMaterialPropertyBlock.ResetShield();
                canDeflect = true;
                curDeflectCoolDown = deflectCoolDown;                                
            }    
        }    
    } 

    void Taunt()
    {
        OnTaunt?.Invoke();
    }

    void TauntRemote(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        OnTaunt?.Invoke();
    }

    bool CheckDeflectable()
    {
        if (curDeflectable)
            return true;
        
        return false;
    }

    void OnDeflectSuccess()
    {
        OnTryDeflect?.Invoke(); //This is kind of borked
        Multiplayer.InvokeRemoteProcedure("DeflectRemote", UserId.All);
        
        Vector3 direction = transform.parent.forward;
        curDeflectable.OnDeflect(direction.normalized);
        curDeflectable = null;
        canDeflect = true;       
    }
    
    void OnDeflectMiss()
    {
        OnTryDeflect?.Invoke(); //This is kind of borked
        Multiplayer.InvokeRemoteProcedure("DeflectRemote", UserId.All);
        curDeflectable = null;
        canDeflect = false;             
    }
    
    private void Shoot()
    {
        OnShoot?.Invoke();
        Multiplayer.InvokeRemoteProcedure("ShootRemote", UserId.All);
        
        GameObject proj = spawner.Spawn(0, transform.position + transform.forward * 2f, transform.rotation);
        
        canAttack = false;
    }

    private void ShootRemote(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        OnShoot?.Invoke();
    }

    private void DeflectRemote(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        OnTryDeflect?.Invoke();    
    }


    private void ShowReflectVFX()
    {
        if (deflectSuccess)
        {
            shieldLight.enabled = true;
            shieldLight.color = (Color.cyan);
            shieldMaterialPropertyBlock.projectileDeflected = true;
        }
        else
        {
            shieldLight.enabled = true;
            shieldLight.color = (Color.red);
            shieldMaterialPropertyBlock.deflectionFailed = true;
        }

        if (deflecting)
        {
            deflectShield.transform.rotation = lastShieldRotation;
            deflectShield.transform.position = lastShieldLocation;
        }
        else
        {
            deflectShield.transform.position = hiddenShieldLocation;
            shieldMaterialPropertyBlock.ResetShield();
            shieldLight.enabled = false;
        }
        
       
           
    }    
}
