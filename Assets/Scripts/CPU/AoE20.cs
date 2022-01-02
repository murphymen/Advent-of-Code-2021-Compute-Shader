using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoE20 : MonoBehaviour
{
    public SimulationState state;
    public string input;
    public string[] lines;
    public int[] enchanceTable;
    public int[] image;

    public int width = 0;
    public int height = 0;

    public ComputeShader compute;
    public RenderTexture renderTexture;
    public ComputeBuffer bufferA, bufferB;
    public ComputeBuffer imageBuffer;
    public Material material;

    private int SetCellsKernel;
    private int OneStepKernel;
    private bool pingPong;

    void Start () {
        if (height < 1 || width < 1) return;

        OneStepKernel = compute.FindKernel("OneStep");
        SetCellsKernel = compute.FindKernel("SetCells");

        AllocateMemory();

        pingPong = true;

        compute.SetInt("width", width);
        compute.SetInt("height", height);

        //SetCells();
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

        LoadInput();
    }

    void SetCells()
    {
        compute.SetTexture(SetCellsKernel, "Result", renderTexture);
        compute.SetBuffer(SetCellsKernel, "CellsA", bufferA);
        compute.SetBuffer(SetCellsKernel, "CellsB", bufferB);
        compute.Dispatch(SetCellsKernel, width / 8, height / 8, 1);

        material.mainTexture = renderTexture;
    }

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

    public void LoadInput()
    {
        // Loat input.txt
        input = System.IO.File.ReadAllText(@"C:\Users\Public\Documents\input20.txt");
        // Split input string into an array of strings, separated by empty lines and " "
        lines = input.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        // Split the first line into an array of integers (enchanceTable) where '.' is 0 and '#' is 1
        enchanceTable = new int[lines[0].Length];
        for (int i = 0; i < lines[0].Length; i++)
        {
            if (lines[0][i] == '.')
                enchanceTable[i] = 0;
            else
                enchanceTable[i] = 1;
        }

        // from line 2 to last line, split each line into an array of integers (image) where '.' is 0 and '#' is 1
        // add each integer to image array
        image = new int[lines.Length * lines[4].Length];
        for (int i = 1; i < lines.Length; i++)
        {
            for (int j = 0; j < lines[i].Length; j++)
            {
                if (lines[i][j] == '.')
                    image[i + lines[i].Length * j] = 0;
                else
                    image[i + lines[i].Length * j] = 1;
            }
        }

        // Create a new SimulationState object and pass the all image data to object.cells.isAlive
        state = new SimulationState();
        state.cells = new Cell[image.Length];
        state.width = lines[5].Length;
        state.height = lines.Length - 2;
        for (int i = 0; i < image.Length; i++)
        {
            state.cells[i] = new Cell();
            state.cells[i].isAlive = (uint)image[i];
        }
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

    // On mouse click save texture to .png file
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Save")
            & (renderTexture != null))
        {
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = null;

            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(@"C:\Users\Public\Documents\output.png", bytes);
        }

        if (GUI.Button(new Rect(10, 90, 100, 30), "Step"))
        {
            Step();
        }
    }
}