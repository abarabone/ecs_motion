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

    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class MarchingCubesResourceBufferManagementSystem : SystemBase
    {


        protected override void OnStartRunning()
        {
            this.Enabled = false;


            var init = this.GetSingleton<Resource.Initialize>();
            var res = this.GetSingleton<Resource.DrawResourceData>();

            //var buf = Resource.CreateDrawBufferData(init.Asset, init.MaxGridLengthInShader);
            var buf = new Resource.DrawBufferData { DrawResources = new mc.DrawResources(init.Asset, init.MaxGridLengthInShader) };
            this.SetSingleton(buf);

            setDrawResources_(buf, res);

            return;


            void setDrawResources_(Resource.DrawBufferData buf_, Resource.DrawResourceData res_)
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

                var bufs = buf_.DrawResources;

                res_.CubeMaterial.SetConstantBuffer("normals", bufs.NormalBuffer);
                res_.CubeMaterial.SetConstantBuffer("cube_patterns", bufs.CubePatternBuffer);
                res_.CubeMaterial.SetConstantBuffer("cube_vtxs", bufs.CubeVertexBuffer);
                res_.CubeMaterial.SetConstantBuffer_("grids", bufs.GridBuffer);
                //res_.CubeMaterial.SetVectorArray( "grids", new Vector4[ 512 * 2 ] );// res.GridBuffer );

                res_.CubeMaterial.SetBuffer("cube_instances", bufs.CubeInstancesBuffer);
                res_.CubeMaterial.SetTexture("grid_cubeids", bufs.GridCubeIdBuffer);
                //res_.CubeMaterial.SetBuffer( "grid_cubeids", res.GridCubeIdBuffer );


                res_.GridCubeIdSetShader.SetBuffer(0, "src_instances", bufs.CubeInstancesBuffer);
                res_.GridCubeIdSetShader.SetTexture(0, "dst_grid_cubeids", bufs.GridCubeIdBuffer);
                //res_.SetGridCubeIdShader.SetBuffer( 0, "dst_grid_cubeids", res_.GridCubeIdBuffer );
            }

        }


        protected override void OnUpdate()
        { }


        protected override void OnDestroy()
        {

            var resbuf = this.GetSingleton<Resource.DrawBufferData>();

            resbuf.DrawResources.Dispose();


            base.OnDestroy();
        }

    }

}
