using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConwaysGameOfLife : MonoBehaviour
{
    public int width = 0;
    public int height = 0;

    public ComputeShader compute;
    public RenderTexture renderTexture;
    public ComputeBuffer bufferA, bufferB;
    public Material material;

    private int SetCellsKernel;
    private int OneStepKernel;
    private bool pingPong;

    // Debug
    //public Cell[] debugCellsA;
    //private ComputeBuffer debugBufferA;


	// Use this for initialization
	void Start () {
        if (height < 1 || width < 1) return;

        OneStepKernel = compute.FindKernel("OneStep");
        SetCellsKernel = compute.FindKernel("SetCells");

        AllocateMemory();

        pingPong = true;

        compute.SetInt("width", width);
        compute.SetInt("height", height);

        ClearRenderTexture();
        SetCells();
        //GetCells();
    }
	
    void AllocateMemory()
    {
        if (bufferA != null) bufferA.Release();
        if (bufferB != null) bufferB.Release();

        bufferA = new ComputeBuffer(width * height, sizeof(uint) * 2);
        bufferB = new ComputeBuffer(width * height, sizeof(uint) * 2);

        renderTexture = new RenderTexture(width, height, 24);
        renderTexture.wrapMode = TextureWrapMode.Repeat;
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.useMipMap = false;
        renderTexture.Create();

        // Debug
        //debugCellsA = new Cell[width * height]; 
        //debugBufferA = new ComputeBuffer(width * height, sizeof(uint));
    }

    // Clear render texture
    void ClearRenderTexture()
    {
        Graphics.SetRenderTarget(renderTexture);
        GL.Clear(false, false, Color.black);
    }

    void SetCells()
    {
        compute.SetTexture(SetCellsKernel, "Result", renderTexture);
        compute.SetBuffer(SetCellsKernel, "CellsA", bufferA);
        compute.SetBuffer(SetCellsKernel, "CellsB", bufferB);
        compute.Dispatch(SetCellsKernel, width / 8, height / 8, 1);

        material.mainTexture = renderTexture;
    }

    /*
    void GetCells()
    {
        // Debug
        bufferA.GetData(debugCellsA);
    }
    */

    // One step of the cellular automata
    public void Step()
    {
        compute.SetTexture(OneStepKernel, "Result", renderTexture);

        if (true == pingPong)
        {
            compute.SetBuffer(OneStepKernel, "CellsA", bufferA);
            compute.SetBuffer(OneStepKernel, "CellsB", bufferB);
            pingPong = false;
        }
        else
        {
            compute.SetBuffer(OneStepKernel, "CellsA", bufferB);
            compute.SetBuffer(OneStepKernel, "CellsB", bufferA);
            pingPong = true;
        }

        compute.Dispatch(OneStepKernel, width / 8, height / 8, 1);
        material.mainTexture = renderTexture;
    }

	// Update is called once per frame
	void Update ()
	{
        if (height < 1 || width < 1) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Step();
        }

        //Step();
	}

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(material.mainTexture, destination);
    }

    void OnDestroy()
    {
        if (bufferA != null) bufferA.Release();
        if (bufferB != null) bufferB.Release();

        if (renderTexture != null) renderTexture.Release();

        //if (debugBufferA != null) debugBufferA.Release();
    }
}
