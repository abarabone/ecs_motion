using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using Unity.Collections.LowLevel.Unsafe;

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;

namespace Abss.Draw
{

    [UpdateAfter(typeof( DrawMeshCsSystem ) )]
    [UpdateInGroup( typeof( DrawSystemGroup ) )]
    public class DrawResourceHoderSystem : ComponentSystem
    {



        public int MaxInstance = 10000;


        public DrawComputeInstanceBufferHolder ComputeBuffers { get; } = new DrawComputeInstanceBufferHolder();

        public DrawNativeInstanceBufferHolder NativeBuffers { get; } = new DrawNativeInstanceBufferHolder();
        
        public DrawMeshResourceHolder DrawResources { get; } = new DrawMeshResourceHolder();
        



        protected override void OnStartRunning()
        {
            var chAuthor = GameObject.Find("draw setting");//

            this.ComputeBuffers.Initialize( this.DrawResources );
            this.NativeBuffers.Initialize( this.DrawResources );
        }

        protected override void OnStopRunning()
        {
            this.NativeBuffers.Dispose();
            this.ComputeBuffers.Dispose();
            this.DrawResources.Dispose();
        }




        protected override void OnCreate()
        {

        }
        



        public



        protected override void OnCreate()
        {
            this.Enabled = false;
        }



        protected override void OnUpdate()
        { }

    }

}
