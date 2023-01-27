using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

public class WorldManager : MonoBehaviour
{
    
    //total hex grid size
    [SerializeField] private int worldWidth = 20;
    [SerializeField] private int worldHeight = 20;

    //hex offset when spawning to make them match
    private readonly float xOffset = 1.8f;
    private readonly float zOffset = 1.6f;

    //playfield and safezone. Cuts out spheres shape from hexgrid.
    [SerializeField] float playfieldStartSize = 14;
    private float playfieldSize = 100f;
    private float safeZoneRadius = 100f;

    
    [SerializeField] float levelMinSize = 5;
    [SerializeField] float shrinkAmount = 2;    
    [SerializeField] float shrinkRepeatTimer = 10;

    //list with all hexes currently in scene.
    private readonly List<GameObject> hexList = new();
    //hexes that are marked out of scape to deal damage.
    private readonly List<GameObject> hexMarkedForDeletion = new();
    //temp for resetting grid
    private readonly List<GameObject> hexReset = new();
    private bool isGameRunning = false;

    [SerializeField] private GameObject hexPrefab;
    private static WorldManager instance;
    public static WorldManager Instance
    {
        get
        {
            if (instance == null)
            {
                throw new NullReferenceException("instance is null!");
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }



    void Start()
    {
        GenerateHexGrid();
       // StartShrinkGrid();  
       CreateWorldBounds();
    }

    public void StartShrinkGrid()
    {
        isGameRunning = true;
        playfieldSize = playfieldStartSize;
        safeZoneRadius = worldWidth;        

        SetHexShape(); //cuts the sphere into square hexgrid. 
        InvokeRepeating(nameof(ShrinkGrid), shrinkRepeatTimer, shrinkRepeatTimer);        
    }


    private void CreateWorldBounds()
    {
        BoxCollider boxCol = gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;
        if (!boxCol)
            return;
        
        boxCol.isTrigger = true;
        boxCol.size = new Vector3(worldWidth * 2, 40, worldHeight * 2);
        
        gameObject.AddComponent<DeSpawnTrigger>();
    }

    private void GenerateHexGrid()
    {
        float gridXMin = -worldWidth / 2;
        float gridXMax = worldWidth / 2;
        float gridZMin = -worldHeight / 2;
        float gridZMax = worldHeight / 2;

        for (float x = gridXMin; x < gridXMax; x++)
        {
            for (float z = gridZMin; z < gridZMax; z++)
            {
                GameObject tempGameObject = Instantiate(hexPrefab);
                Vector3 gridPosition;

                if (z % 2 == 0)
                {
                    gridPosition = new Vector3(x * xOffset, 0, z * zOffset);

                }
                else
                {
                    gridPosition = new Vector3(x * xOffset + xOffset / 2, 0, z * zOffset);
                }


                tempGameObject.transform.position = gridPosition;
                tempGameObject.name = "Hex_" + x + "," + z;
                tempGameObject.transform.parent = transform;
                
                hexList.Add(tempGameObject);
                hexReset.Add(tempGameObject);
            }
        }
    }

    private void ShrinkGrid()
    {
        
        if (playfieldSize > levelMinSize)
                playfieldSize -= shrinkAmount;       
        
        SetHexShape();        
    }

    private void SetHexShape()
    {
        for (int j = 0; j < 10; j++)
        {
            for (int i = 0; i < hexList.Count; i++)
            {
                if (Vector3.Distance(hexList[i].transform.position, transform.position) > playfieldSize)
                {
                    hexList[i].GetComponent<MaterialPropertyBlockTest>().PendingTileOutOfBound();

                    hexMarkedForDeletion.Add(hexList[i]);
                    hexList.RemoveAt(i);
                    StartCoroutine(DestroyHexOutOfRange());
                }
            }
        }
    }

   
    IEnumerator DestroyHexOutOfRange()
    {
        yield return new WaitForSeconds(shrinkRepeatTimer / 2);

        //avoids cutting a hole in grid after resetting the map.
        if(isGameRunning)
        {
            for (int i = 0; i < hexMarkedForDeletion.Count; i++)
            {
                hexMarkedForDeletion[i].GetComponent<MaterialPropertyBlockTest>().TileOutOfBound();
            }
            
            safeZoneRadius = playfieldSize;
        }        
               
        hexMarkedForDeletion.Clear();
       
    }


    public bool TakeWorldDamage(Vector3 playerPosition)
    {
        if (Vector3.Distance(transform.position, playerPosition) > safeZoneRadius)
            return true;
        else
            return false;
    }

    public void ResetHexGrid()
    {
        //stops the grid from shrinking
        CancelInvoke();
        isGameRunning = false;

        foreach (var hex in hexReset)
        {
            hexList.Add(hex);
        }

        for (int i = 0; i < hexList.Count; i++)
        {
            hexList[i].GetComponent<MaterialPropertyBlockTest>().ResetPropertyBlock();
        }
    }


}

