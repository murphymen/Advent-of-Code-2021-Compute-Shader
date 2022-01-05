using UnityEngine;

static class ComputeHelpers 
{
    // Function sends three ints to the compute shader
    public static void SetInts(this ComputeShader compute, string name, (int x, int y, int z) t)
    => compute.SetInts(name, t.x, t.y, t.z);

    public static void SetInts(this ComputeShader compute, string name, Vector3 v)
    => compute.SetInts(name, (int)v.x, (int)v.y, (int)v.z);

    public static void DispatchThreads(this ComputeShader compute, int kernel, int x, int y, int z)
    {
        uint xc, yc, zc;
        compute.GetKernelThreadGroupSizes(kernel, out xc, out yc, out zc);

        x = (x + (int)xc - 1) / (int)xc;
        y = (y + (int)yc - 1) / (int)yc;
        z = (z + (int)zc - 1) / (int)zc;

        compute.Dispatch(kernel, x, y, z);
    }

    public static void DispatchThreads(this ComputeShader compute, int kernel, (int x, int y, int z) t)
    => DispatchThreads(compute, kernel, t.x, t.y, t.z);

    public static void DispatchThreads(this ComputeShader compute, int kernel, Vector3 v)
    => DispatchThreads(compute, kernel, (int)v.x, (int)v.y, (int)v.z);
}
