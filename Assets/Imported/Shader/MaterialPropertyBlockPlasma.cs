using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPropertyBlockPlasma : MonoBehaviour
{
    public Color startColor, EndColor, defaultColor, failedColor;  

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    
    public bool projectileDeflected = false;
    public bool deflectionFailed = false;   

    void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
        _propBlock.SetColor("_Color", startColor);
        _renderer.SetPropertyBlock(_propBlock);
    }

    void Update()
    {
        _renderer.GetPropertyBlock(_propBlock);

        if (projectileDeflected)
            _propBlock.SetColor("_Color", defaultColor);
        else if (deflectionFailed)
            _propBlock.SetColor("_Color", failedColor);
        else
            _propBlock.SetColor("_Color", startColor);

        _renderer.SetPropertyBlock(_propBlock);
    }   

    public void ResetShield()
    {
        projectileDeflected = false;
        deflectionFailed = false;
    }


    //_propBlock.SetColor("_Color", Color.Lerp(startColor, defaultColor, (Mathf.Sin(Time.time * 20f + 0) + 1) / 2f));  
}
