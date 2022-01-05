using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Day25_SeaCucumber : MonoBehaviour
{
    Cell cell;
    public SimulationState state;
    public string input;
    public string[] lines;
    public int[] image;
    private int iteration = 0;
    private int changeCounter = 0;

    public int width = 0;
    public int height = 0;

    public ComputeShader compute;
    public RenderTexture texture;
    public ComputeBuffer cellChunkBuffer;
    public ComputeBuffer inputBuffer;
    public Material material;

    private int SetCellsKernel;
    private int ClearBuffersKernel;
    private int OneStepKernel;

    // Debug
    public ComputeBuffer counterBuffer;
    public ComputeBuffer argsBuffer;
    uint[] counter = new uint[1];

    void Start () {
        if (height < 1 || width < 1) return;

        OneStepKernel = compute.FindKernel("OneStep");
        ClearBuffersKernel = compute.FindKernel("ClearBuffers");
        SetCellsKernel = compute.FindKernel("SetCells");

        AllocateMemory();
        ClearBuffers();
        LoadInput();

        //
        inputBuffer = new ComputeBuffer(image.Length, sizeof(int));
        inputBuffer.SetData(image);

        //
        compute.SetInt("width", width);
        compute.SetInt("height", height);

        SetCells();
    }

    void Update()
    {
            //Step();
            //iteration++;
            //if (iteration < 60) go = false;
    }

    void AllocateMemory()
    {
        cellChunkBuffer = new ComputeBuffer(width * height, sizeof(uint) * 3);

        texture = new RenderTexture(width, height, 24);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.enableRandomWrite = true;
        texture.filterMode = FilterMode.Point;
        texture.useMipMap = false;
        texture.Create();

        counterBuffer = new ComputeBuffer(1, 4, ComputeBufferType.Counter);
        argsBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
    }

    void SetCells()
    {
        compute.SetTexture(SetCellsKernel, "Result", texture);
        compute.SetBuffer(SetCellsKernel, "CellsA", cellChunkBuffer);
        compute.SetBuffer(SetCellsKernel, "imageBuffer", inputBuffer);
        compute.SetInt("imageWidth", state.width);
        compute.SetInt("imageHeight", state.height);
        compute.DispatchThreads(SetCellsKernel, width, height, 1);
    }

    public void ClearBuffers()
    {
        compute.SetBuffer(ClearBuffersKernel, "cellChunkBuffer", cellChunkBuffer);
        counterBuffer.SetCounterValue(0);
        compute.SetBuffer(ClearBuffersKernel, "counterBuffer", counterBuffer);
        compute.DispatchThreads(ClearBuffersKernel, width, height, 1);
    }

    public void Step()
    {
        compute.SetTexture(OneStepKernel, "texture", texture);
        compute.SetBuffer(OneStepKernel, "cellChunkBuffer", cellChunkBuffer);
        counterBuffer.SetCounterValue(0);
        compute.SetBuffer(OneStepKernel, "counterBuffer", counterBuffer);
        compute.DispatchThreads(OneStepKernel, width, height, 1);

        // Get count from counterBuffer to argsBuffer
        ComputeBuffer.CopyCount(counterBuffer, argsBuffer, 0);
        argsBuffer.GetData(counter);
        Debug.Log("Counter: " + counter[0]);

        material.mainTexture = texture;
    }

    public void LoadInput()
    {
        // Loat input.txt
        input = System.IO.File.ReadAllText(@"C:\Users\Public\Documents\input20.txt");
        // Split input string into an array of strings, separated by empty lines and " "
        lines = input.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);        

        //
        image = new int[(lines.Length-2) * lines[4].Length];
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
        cellChunkBuffer?.Dispose();
        inputBuffer?.Dispose();

        texture?.Release();

        counterBuffer?.Dispose();
        argsBuffer?.Dispose();    
    }

    // On mouse click save texture to .png file
    void OnGUI()
    {
        // get counter value
        GUI.Label(new Rect(10, 50, 100, 30), counter[0].ToString());

        // Print iteration
        GUI.Label(new Rect(10, 70, 100, 30), "Iteration: " + iteration.ToString());
    }    
}
