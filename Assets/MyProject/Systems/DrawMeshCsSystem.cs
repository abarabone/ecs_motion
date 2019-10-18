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
using Abss.Misc;
using Abss.SystemGroup;

namespace Abss.Draw
{

    [UpdateAfter(typeof( BoneToDrawInstanceSystem ) )]
    [UpdateInGroup(typeof(DrawSystemGroup))]
    public class BeginDrawCsBarier : EntityCommandBufferSystem
    { }


    /// <summary>
    /// メッシュをインスタンシングバッファを使用してインスタンシング描画する
    /// </summary>
    [AlwaysUpdateSystem]
    //[DisableAutoCreation]
    [UpdateAfter(typeof( BeginDrawCsBarier ) )]
    [UpdateInGroup(typeof(DrawSystemGroup))]
    public class DrawMeshCsSystem : JobComponentSystem
    {

        public int MaxInstance = 1024 * 100;



        // 描画用バッファ
        SimpleComputeBuffer<bone_unit> instanceTransformBuffer;
        SimpleIndirectArgsBuffer instanceArgumentsBuffer;
        InstancingIndirectArguments arguments;


        DrawMeshResourceHolder resourceHolder = new DrawMeshResourceHolder();

        public DrawMeshResourceHolder GetResourceHolder() => this.resourceHolder;


        NativeArray<float4> instanceBoneVectors;
        NativeArray<NativeSlice<float4>> instanceBoneVectorEveryModels;

        public NativeArray<NativeSlice<float4>> GetInstanceBoneVectorEveryModels() =>
            this.instanceBoneVectorEveryModels;


        NativeArray<ThreadSafeCounter<Persistent>> instanceCounters;

        public NativeArray<ThreadSafeCounter<Persistent>> GetInstanceCounters() =>
            this.instanceCounters;


        protected override void OnStartRunning()
        {
            createBuffers();
            allocVectors();
            allocInstanceCounters();

            return;


            void createBuffers()
            {
                this.instanceArgumentsBuffer.CreateBuffer();
                this.instanceTransformBuffer = new SimpleComputeBuffer<bone_unit>( "bones", 4 * 16 * this.MaxInstance );
            }

            void allocVectors()
            {
                var vectorLength = 4;
                var instanceMax = this.MaxInstance;
                var arrayLengths = this.resourceHolder.Units
                    .Select( x => vectorLength * x.Mesh.bindposes.Length * instanceMax )
                    .ToArray();

                this.instanceBoneVectorEveryModels =
                    new NativeArray<NativeSlice<float4>>( arrayLengths.Length, Allocator.Persistent );
                
                this.instanceBoneVectors =
                    new NativeArray<float4>( arrayLengths.Sum(), Allocator.Persistent );
                
                var start = 0;
                for( var i = 0; i < arrayLengths.Length; i++ )
                {
                    this.instanceBoneVectorEveryModels[ i ] = this.instanceBoneVectors.Slice( start, arrayLengths[ i ] );
                    start += arrayLengths[ i ];
                }
            }

            void allocInstanceCounters()
            {
                this.instanceCounters =
                    new NativeArray<ThreadSafeCounter<Persistent>>( this.resourceHolder.Units.Count, Allocator.Persistent );

                for( var i = 0; i < this.instanceCounters.Length; i++ )
                {
                    this.instanceCounters[ i ] = new ThreadSafeCounter<Persistent>( 0 );
                }
            }
        }
        protected override void OnStopRunning()
        {
            if( this.instanceCounters.IsCreated )
            {
                for( var i = 0; i < this.instanceCounters.Length; i++ )
                {
                    this.instanceCounters[ i ].Dispose();
                }
                this.instanceCounters.Dispose();
            }

            if( this.instanceBoneVectors.IsCreated ) this.instanceBoneVectors.Dispose();
            if( this.instanceBoneVectorEveryModels.IsCreated ) this.instanceBoneVectorEveryModels.Dispose();

            this.instanceTransformBuffer.Dispose();
            this.instanceArgumentsBuffer.Dispose();

            this.resourceHolder.Dispose();
        }

        public struct bone_unit
        {
            public float4 pos;
            public quaternion rot;
        }
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {
            //var unit = this.resourceHolder.Units[ 0 ];
            //var mesh = unit.Mesh;
            //var mat = unit.Material;
            //var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };
            //using( var a = new InstancingIndirectArguments( mesh, 1 ) )
            //    this.instanceArgumentsBuffer.Buffer.SetData( a.Arguments );
            //var args = this.instanceArgumentsBuffer;

            //var buf = new NativeArray<bone_unit>( mesh.bindposes.Length, Allocator.Temp );
            //for( var i = 0; i < mesh.bindposes.Length; i++ )
            //{
            //    buf[ i ] = new bone_unit
            //    {
            //        pos = float4.zero + new float4( i * 0.1f, i*0.1f,0,1),
            //        rot = quaternion.identity,
            //    };
            //}
            //this.instanceTransformBuffer.Buffer.SetData( buf );
            //mat.SetBuffer( this.instanceTransformBuffer );
            //mat.SetInt( "boneLength", mesh.bindposes.Length );
            //buf.Dispose();

            //Graphics.DrawMeshInstancedIndirect( mesh, 0, mat, bounds, args );


            var unit = this.resourceHolder.Units[ 0 ];
            var mesh = unit.Mesh;
            var mat = unit.Material;
            var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };
            var args = this.instanceArgumentsBuffer;

            using( var a = new InstancingIndirectArguments( mesh, (uint)this.instanceCounters[ 0 ].Count ) )
                args.Buffer.SetData( a.Arguments );
            
            this.instanceTransformBuffer.Buffer.SetData( this.instanceBoneVectors, 0, 0, this.instanceCounters[0].Count * 16 * 2 );
            mat.SetBuffer( this.instanceTransformBuffer );
            //Debug.Log( this.instanceCounters[ 0 ].Count );
            mat.SetInt( "boneLength", mesh.bindposes.Length );
            //Debug.Log( mesh.bindposes.Length );

            //for( var i=0; i<32; i++ )
            //Debug.Log( $"{i} {this.instanceBoneVectors[i]}" );
            
            Graphics.DrawMeshInstancedIndirect( mesh, 0, mat, bounds, args );
            
            
            return inputDeps;
        }

    }
}
