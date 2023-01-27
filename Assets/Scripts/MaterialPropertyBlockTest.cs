using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MaterialPropertyBlockTest : MonoBehaviour
{
    public Color defaultColor, deletedTile1, deletedTile2, pending1, pending2;
    [SerializeField] private float pendingSpeed = 2;
    [SerializeField] private float outOfScopeSpeed = 1;
    private readonly float offset;


    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    private bool isPending = false;
    private bool isOutOfBound = false;


    void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
    }

    void Update()
    {
        _renderer.GetPropertyBlock(_propBlock);

        if (isPending)   
            _propBlock.SetColor("_Color", Color.Lerp(pending1, pending2, (Mathf.Sin(Time.time * pendingSpeed + offset) + 1) / 2f));        
        else if(isOutOfBound)
            _propBlock.SetColor("_Color", Color.Lerp(deletedTile1, deletedTile2, (Mathf.Sin(Time.time * outOfScopeSpeed + offset) + 1) / 2f));
        else
            _propBlock.SetColor("_Color", defaultColor);

        _renderer.SetPropertyBlock(_propBlock);

    }

    public void PendingTileOutOfBound()
    {
        isPending = true;
    }

    public void TileOutOfBound()
    {
        isPending = false;
        isOutOfBound = true;
    }

    public void ResetPropertyBlock()
    {
        isPending = false;
        isOutOfBound = false;
    }
}