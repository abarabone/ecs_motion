using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Burst.Intrinsics;

public class testc : MonoBehaviour
{
    // Start is called before the first frame update
    unsafe void Start()
    {

        //var (x, y) = testcompile.testfunc(0);
        testcompile.testfunc(0, out var x, out var y);//, out var *b);
        Debug.Log(x + y);
        //b.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [BurstCompile]
    static unsafe class testcompile
    {
        // 戻り値あるとエディタが落ちるっぽい？
        //[BurstCompile]
        //public static (int x, int y) testfunc(in int2 a)
        //{
        //    using var arr1 = new NativeArray<float>(1, Allocator.Temp);
        //    using var arr2 = new NativeArray<float>(1, Allocator.Temp);
        //    UnsafeUtility.MemCpy(arr1.GetUnsafePtr(), arr2.GetUnsafePtr(), 4);

        //    return (1, 2);
        //}
        [BurstCompile]
        public static void testfunc(in int2 a, out int x, out int y)//, out NativeArray<float> *b)
        {
            if (X86.Sse2.IsSse2Supported)
            //if (X86.Avx2.IsAvx2Supported)
            {
                using var arr1 = new NativeArray<float>(1, Allocator.Temp);
                using var arr2 = new NativeArray<float>(1, Allocator.Temp);
                UnsafeUtility.MemCpy(arr1.GetUnsafePtr(), arr2.GetUnsafePtr(), 4);

                x = 1;
                y = 2;
            }
            else
            {
                x = 34;
                y = 345;
            }
            //b = new NativeArray<float>(1, Allocator.Temp);
        }
    }

}
