using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

public class testc : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        testcompile.testfunc(0);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [BurstCompile]
    static unsafe class testcompile
    { 
        [BurstCompile]
        public static void testfunc(in int2 a)
        {
            using var arr1 = new NativeArray<float>(1, Allocator.Temp);
            using var arr2 = new NativeArray<float>(1, Allocator.Temp);
            UnsafeUtility.MemCpy(arr1.GetUnsafePtr(), arr2.GetUnsafePtr(), 4);
        }
    }

}
