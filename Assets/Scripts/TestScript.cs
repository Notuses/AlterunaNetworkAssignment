using System.Collections;
using System.Collections.Generic;
using Alteruna;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public void DebugHello()
    {
        if (Multiplayer.Instance.Me.Index != 0)
        {
            Debug.Log("Index not zero");
        }
    }
}
