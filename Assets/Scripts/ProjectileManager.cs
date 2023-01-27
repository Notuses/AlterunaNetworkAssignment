using System;
using System.Collections;
using System.Collections.Generic;
using Alteruna;
using Alteruna.Trinity;
using UnityEngine;

// vill vi ha en ProjetileManager

public class ProjectileManager : AttributesSync
{
    [SerializeField ]private Alteruna.Avatar avatar;
    [SerializeField] private GameObject projectilePrefab;

    private Dictionary<int, GameObject> projectileDict;
    private List<GameObject> projectilePoolList;

    [SynchronizableField]
    private byte currentProjectilePoolIndex;

    private List<GameObject> projectilesInRoom;

    private Spawner spawner;

    private void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
    }

    private void Start()
    {
        projectileDict = new Dictionary<int, GameObject>();
        projectilePoolList = new List<GameObject>();

        /*
        for (int i = 0; i < 100; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
            if (projectile.TryGetComponent(out Projectile proj))
                proj.localId = i;

            projectilePoolList.Add(projectile);

        }
        */
        

        Multiplayer.RegisterRemoteProcedure("SpawnProjectileRemote", SpawnProjectileRemote);
        Multiplayer.RegisterRemoteProcedure("OnPlayerDeflectProjectileRemote", OnPlayerDeflectProjectileRemote);
    }
    
    public void SpawnProjectileLocal(Vector3 spawnPos, Quaternion rotation)
    {
        //GameObject projectile = Instantiate(projectilePrefab, spawnPos, rotation);
        GameObject projectile = spawner.Spawn(0, spawnPos, rotation);
        int id = projectile.GetInstanceID();
        projectileDict.Add(id, projectile);

        if (projectile.TryGetComponent(out SynchronizedProjectile proj))
        {
            //proj.localId = id;
        }

        //Get object from object pool and get the right data from it
        //GameObject obj = projectilePoolList[currentProjectilePoolIndex];
        //Projectile proj = obj.GetComponent<Projectile>();
        //int id = proj.localId;

        //Set psotion
        //Transform objTrans = obj.transform;
        //objTrans.position = spawnPos;
        //objTrans.rotation = rotation;

        //Go to next index in pool
        //currentProjectilePoolIndex++;


        ProcedureParameters parameters = new ProcedureParameters();
        
        parameters.Set("id", id);
        parameters.Set("spawnPosX", spawnPos.x);
        parameters.Set("spawnPosY", spawnPos.y);
        parameters.Set("spawnPosZ", spawnPos.z);
        parameters.Set("yawRot", rotation.eulerAngles.y); 

        
        
        //Multiplayer.InvokeRemoteProcedure("SpawnProjectileRemote", UserId.All, parameters);
        
        //Debug.Log("SPAWN LOCAL, ID: " + id);
    }
    
    void SpawnProjectileRemote(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        float posX = parameters.Get("spawnPosX", 0.0f); //The default value HAS to be specifically a float to pass a float
        float posY = parameters.Get("spawnPosY", 0.0f); //The default value HAS to be specifically a float to pass a float
        float posZ = parameters.Get("spawnPosZ", 0.0f); //The default value HAS to be specifically a float to pass a float
        float yaw = parameters.Get("yawRot", 0.0f);

        int id = parameters.Get("id", 0);

        Vector3 spawnPos = new Vector3(posX, posY, posZ);
        
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        //GameObject projectile = spawner.Spawn(0, spawnPos, rotation);
        projectile.transform.Rotate(Vector3.up, yaw);
        
        projectileDict.Add(id, projectile);
        if (projectile.TryGetComponent(out SynchronizedProjectile proj))
        { 
            //proj.localId = id;
        }

        //GameObject obj = projectilePoolList[currentProjectilePoolIndex];
        //Projectile proj = obj.GetComponent<Projectile>();
        //id = proj.localId;

        //Debug.Log("SPAWN REMOTE, ID: " + id);
    }

    public void OnPlayerDeflectProjectile(int projectileId)
    {
        if (projectileDict.TryGetValue(projectileId, out var go))
        {
            var proj = go.GetComponent<SynchronizedProjectile>();
            //proj.OnDeflect();
        }
        else
        {
            //Debug.Log("LOCAL, PROJECTILE NOT FOUND IN DICT");
        }
        
        ProcedureParameters parameters = new ProcedureParameters();
        parameters.Set("projectileId", projectileId);
        Multiplayer.InvokeRemoteProcedure("OnPlayerDeflectProjectileRemote", UserId.All, parameters);
    }
    
    public void OnPlayerDeflectProjectileRemote(ushort fromUser, ProcedureParameters parameters, uint callId, ITransportStreamReader processor)
    {
        int projectileId = parameters.Get("projectileId", 0);
        
        if (projectileDict.TryGetValue(projectileId, out var go))
        {
            var proj = go.GetComponent<SynchronizedProjectile>();
            //proj.OnDeflect();
        }
        else
        {
            //Debug.Log("REMOTE, PROJECTILE NOT FOUND IN DICT");
        }
    }
}
