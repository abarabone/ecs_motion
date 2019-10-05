using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

using Abss.Cs;
using Abss.Arthuring;

namespace Abss.Draw
{
    /// <summary>
    /// メッシュをインスタンシングバッファを使用してインスタンシング描画する
    /// </summary>
    public class DrawMeshCsSystem : JobComponentSystem
    {

        // 描画用バッファ
        SimpleComputeBuffer<float4>     instanceTransformBuffer;
        SimpleIndirectArgsBuffer        instanceArgumentsBuffer;
        InstancingIndirectArguments     arguments;


        public DrawMeshResourceHolder resourceHolder;
        


        protected override void OnCreate()
        {

        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            //this.arguments.
            return inputDeps;
        }
        
    }
}
