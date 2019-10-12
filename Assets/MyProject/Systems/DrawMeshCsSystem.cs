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
    [AlwaysUpdateSystem]
    //[DisableAutoCreation]
    public class DrawMeshCsSystem : JobComponentSystem
    {

        // 描画用バッファ
        SimpleComputeBuffer<bone_unit> instanceTransformBuffer;
        SimpleIndirectArgsBuffer instanceArgumentsBuffer;
        InstancingIndirectArguments arguments;


        DrawMeshResourceHolder resourceHolder = new DrawMeshResourceHolder();

        public DrawMeshResourceHolder GetResourceHolder() => this.resourceHolder;



        protected override void OnStartRunning()
        {
            this.instanceArgumentsBuffer.CreateBuffer();
            this.instanceTransformBuffer = new SimpleComputeBuffer<bone_unit>( "bones", 1024 * 4 );
            Debug.Log( this.resourceHolder.Units.Count );
        }
        protected override void OnStopRunning()
        {
            this.instanceTransformBuffer.Dispose();
            this.instanceArgumentsBuffer.Dispose();
        }

        public struct bone_unit
        {
            public float4 pos;
            public quaternion rot;
        }
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            //return inputDeps;

            var unit = this.resourceHolder.Units[ 0 ];
            var mesh = unit.Mesh;
            var mat = unit.Material;
            var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };
            using( var a = new InstancingIndirectArguments( mesh, 1 ) )
                this.instanceArgumentsBuffer.Buffer.SetData( a.Arguments );
            var args = this.instanceArgumentsBuffer;

            var buf = new NativeArray<bone_unit>( mesh.bindposes.Length, Allocator.Temp );
            for( var i = 0; i < mesh.bindposes.Length; i++ )
            {
                buf[ i ] = new bone_unit
                {
                    pos = float4.zero + new float4( i * 0.1f, i*0.1f,0,1),
                    rot = quaternion.identity,
                };
            }
            this.instanceTransformBuffer.Buffer.SetData( buf );
            mat.SetBuffer( this.instanceTransformBuffer );
            mat.SetInt( "boneLength", mesh.bindposes.Length );
            buf.Dispose();

            Graphics.DrawMeshInstancedIndirect( mesh, 0, mat, bounds, args );

            //this.arguments.
            return inputDeps;
        }

    }
}
