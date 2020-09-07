using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes
{
    using MarchingCubes;
    using Abarabone.Draw;

    [DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    public class MarchingCubesResourceBufferManagementSystem : SystemBase
    {


        protected override void OnCreate()
        {
            base.OnCreate();

            this.Enabled = false;


            var res = this.GetSingleton<Resource.DrawResourceData>();
            var gridbuf = this.GetSingleton<Grid.GridBufferData>();
            var resbuf = this.GetSingleton<Resource.DrawBufferData>();//new Resource.DrawBufferData();//
            var gridinfo = this.GetSingleton<Grid.GridInfoData>();

            gridbuf.gridData = new NativeList<CubeUtility.GridInstanceData>(gridinfo.maxDrawGridLength, Allocator.Persistent);
            gridbuf.cubeInstances = new NativeList<CubeInstance>(gridinfo.maxCubeInstances, Allocator.Persistent);
            //gridbuf.cubeInstances = new NativeQueue<CubeInstance>( Allocator.Persistent );

            //res.meshResources = new MeshResources(this.MarchingCubeAsset, this.maxDrawGridLength);
            this.SetSingleton(res);

            setResources_(resbuf, res);
            //initCubes_();
            //createHitMesh_();

            return;


            void setResources_(Resource.DrawBufferData resbuf_, Resource.DrawResourceData res_)
            {
                //uint4 cube_patterns[ 254 ][2];
                // [0] : vertex posision index { x: tri0(i0>>0 | i1>>8 | i2>>16)  y: tri1  z: tri2  w: tri3 }
                // [1] : vertex normal index { x: (i0>>0 | i1>>8 | i2>>16 | i3>>24)  y: i4|5|6|7  z:i8|9|10|11 }

                //uint4 cube_vtxs[ 12 ];
                // x: near vertex index (x>>0 | y>>8 | z>>16)
                // y: near vertex index offset prev (left >>0 | up  >>8 | front>>16)
                // z: near vertex index offset next (right>>0 | down>>8 | back >>16)
                // w: pos(x>>0 | y>>8 | z>>16)

                //uint3 grids[ 512 ][2];
                // [0] : position as float3
                // [1] : near grid id
                // { x: prev(left>>0 | up>>9 | front>>18)  y: next(right>>0 | down>>9 | back>>18)  z: current }

                res_.CubeMaterial.SetConstantBuffer("normals", resbuf_.NormalBuffer);
                res_.CubeMaterial.SetConstantBuffer("cube_patterns", resbuf_.CubePatternBuffer);
                res_.CubeMaterial.SetConstantBuffer("cube_vtxs", resbuf_.CubeVertexBuffer);
                res_.CubeMaterial.SetConstantBuffer_("grids", resbuf_.GridBuffer);
                //res_.CubeMaterial.SetVectorArray( "grids", new Vector4[ 512 * 2 ] );// res.GridBuffer );

                res_.CubeMaterial.SetBuffer("cube_instances", resbuf_.CubeInstancesBuffer);
                res_.CubeMaterial.SetTexture("grid_cubeids", resbuf_.GridCubeIdBuffer);
                //res_.CubeMaterial.SetBuffer( "grid_cubeids", res.GridCubeIdBuffer );


                res_.SetGridCubeIdShader.SetBuffer(0, "src_instances", resbuf_.CubeInstancesBuffer);
                res_.SetGridCubeIdShader.SetTexture(0, "dst_grid_cubeids", resbuf_.GridCubeIdBuffer);
                //res_.SetGridCubeIdShader.SetBuffer( 0, "dst_grid_cubeids", res_.GridCubeIdBuffer );
            }

        }


        protected override void OnUpdate()
        { }


        protected override void OnDestroy()
        {

            //var gridbuf = this.GetSingleton<Grid.GridBufferData>();
            var resbuf = this.GetSingleton<Resource.DrawBufferData>();

            resbuf.Dispose();
            //gridbuf.Dispose();


            base.OnDestroy();
        }

    }

}
