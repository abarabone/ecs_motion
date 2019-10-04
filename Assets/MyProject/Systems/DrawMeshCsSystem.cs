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
    public class DrawMeshCsSystem : JobComponentSystem
    {

        SimpleComputeBuffer<float4> instanceTransformBuffer;
        SimpleIndirectArgsBuffer instanceArgumentsBuffer;
        InstancingIndirectArguments arguments;

        public DrawMeshResourceHolder resourceHolder { private get; set; }


        protected override void OnCreate()
        {
            this.resourceHolder.Units
                .Select( x => )
            this.instanceArgumentsBuffer = new SimpleIndirectArgsBuffer()
        }

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            this.arguments.

        }
        
    }
}
