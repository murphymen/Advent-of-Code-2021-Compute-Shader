using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Binary : MonoBehaviour
{
    public int[] binary;
    public int bin;

    // Start is called before the first frame update
    void Start()
    {
        bin = 0; 
        binary = new int[9];
        
        for(int k = 0; k < 9; k++)
            bin |= binary[k] << k;
    }

    // Update is called once per frame
    void Update()
    {
        for(int k = 0; k < 9; k++)
            bin |= binary[k] << k;
    }
}
