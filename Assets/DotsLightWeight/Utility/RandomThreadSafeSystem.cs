using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;

using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class RandomSystem : ComponentSystem
{
    public NativeArray<Random> RandomArray { get; private set; }

    protected override void OnCreate()
    {
        var rnds = new NativeArray<Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);
        var seed = new System.Random();

        for (int i = 0; i < rnds.Length; ++i)
        {
            rnds[i] = new Random((uint)seed.Next());
        }

        this.RandomArray = rnds;

        //this.Enabled = false;
    }

    protected override void OnDestroy()
        => this.RandomArray.Dispose();

    protected override void OnUpdate() { }
}