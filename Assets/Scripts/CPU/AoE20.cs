using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoE20 : MonoBehaviour
{
    Cell cell;
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
    public ComputeBuffer enchanceTableBuffer;
    public Material material;

    private int SetCellsKernel;
    private int DrawCellsAKernel;
    private int DrawCellsBKernel;
    private int OneStepKernel;
    private bool pingPong;

    // Debug
    public Cell[] cellsADebug;
    public Cell[] cellsBDebug;

    void Start () {
        if (height < 1 || width < 1) return;

        OneStepKernel = compute.FindKernel("OneStep");
        DrawCellsAKernel = compute.FindKernel("DrawCellsA");
        DrawCellsBKernel = compute.FindKernel("DrawCellsB");
        SetCellsKernel = compute.FindKernel("SetCells");

        AllocateMemory();
        LoadInput();

        enchanceTableBuffer = new ComputeBuffer(enchanceTable.Length, sizeof(int));
        enchanceTableBuffer.SetData(enchanceTable);
        imageBuffer = new ComputeBuffer(image.Length, sizeof(int));
        imageBuffer.SetData(image);

        pingPong = true;

        compute.SetInt("width", width);
        compute.SetInt("height", height);

        SetCells();
        //GetCells();
    }
	
    void AllocateMemory()
    {
        if (bufferA != null) bufferA.Release();
        if (bufferB != null) bufferB.Release();

        bufferA = new ComputeBuffer(width * height, sizeof(uint) * 3);
        bufferB = new ComputeBuffer(width * height, sizeof(uint) * 3);

        renderTexture = new RenderTexture(width, height, 24);
        renderTexture.wrapMode = TextureWrapMode.Repeat;
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.useMipMap = false;
        renderTexture.Create();

        cellsADebug = new Cell[width * height];
        cellsBDebug = new Cell[width * height];
    }

    void SetCells()
    {
        compute.SetTexture(SetCellsKernel, "Result", renderTexture);
        compute.SetBuffer(SetCellsKernel, "CellsA", bufferA);
        compute.SetBuffer(SetCellsKernel, "CellsB", bufferB);
        compute.SetBuffer(SetCellsKernel, "enchanceTableBuffer", enchanceTableBuffer);
        compute.SetBuffer(SetCellsKernel, "imageBuffer", imageBuffer);
        compute.SetInt("imageWidth", state.width);
        compute.SetInt("imageHeight", state.height);
        enchanceTableBuffer.SetData(enchanceTable);
        compute.Dispatch(SetCellsKernel, 1, 1, 1);
        //material.mainTexture = renderTexture;
    }

    void GetCells()
    {
        bufferA.GetData(cellsADebug);
        bufferB.GetData(cellsBDebug);
    }

    void DrawCellsA()
    {
        compute.SetTexture(DrawCellsAKernel, "Result", renderTexture);
        compute.SetBuffer(DrawCellsAKernel, "CellsA", bufferA);
        compute.Dispatch(DrawCellsAKernel, width / 8, height / 8, 1);

        material.mainTexture = renderTexture;
    }

    void DrawCellsB()
    {
        compute.SetTexture(DrawCellsBKernel, "Result", renderTexture);
        compute.SetBuffer(DrawCellsBKernel, "CellsB", bufferB);
        compute.Dispatch(DrawCellsBKernel, width / 8, height / 8, 1);        

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

        compute.SetBuffer(OneStepKernel, "enchanceTableBuffer", enchanceTableBuffer);
        compute.Dispatch(OneStepKernel, width / 8, height / 8, 1);
    
        material.mainTexture = renderTexture;
        GetCells();
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
        image = new int[5 * lines[4].Length];
        for (int i = 2; i < lines.Length; i++)
        {
            for (int j = 0; j < lines[i].Length; j++)
            {
                int index = (i - 2) * lines[i].Length + j;
                if (lines[i][j] == '.')
                    image[index] = 0;
                else
                    image[index] = 1;
            }
        }

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
        if (imageBuffer != null) imageBuffer.Release();
        if (enchanceTableBuffer != null) enchanceTableBuffer.Release();

        if (renderTexture != null) renderTexture.Release();

        //if (debugBufferA != null) debugBufferA.Release();
    }

    // On mouse click save texture to .png file
    void OnGUI()
    {
        /*
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
        }*/

        if (GUI.Button(new Rect(10, 90, 100, 30), "Step"))
        {
            Step();
        }

        // Button to draw cellsA
        if (GUI.Button(new Rect(10, 50, 100, 30), "Draw A"))
        {
            DrawCellsA();
        }

        // Button to draw cellsB
        if (GUI.Button(new Rect(10, 70, 100, 30), "Draw B"))
        {
            DrawCellsB();
        }
    }    
}
