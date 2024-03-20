using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;

public class Testing : MonoBehaviour
{
    public List<string> Sorts = new List<string>();

    private void OnValidate()
    {
        Sorts.Sort((x, y) => string.Compare(y, x));
    }
}