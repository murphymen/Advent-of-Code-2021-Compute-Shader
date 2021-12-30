using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Cell
{
    [SerializeField]
    public uint isAlive;
    [SerializeField]
    public uint type;
}
