using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Arthuring;
using Abss.Motion;
using Abss.Cs;

namespace Abss.Draw
{


    public struct DrawMeshResourceUnit
    {
        public int MeshId;
        public Mesh Mesh;
        public Material Material;
        public SimpleComputeBuffer<float4> TransformBuffer;
        public InstancingIndirectArguments Arguments;
    }



    public class DrawMeshResourceHolder
    {


        public List<DrawMeshResourceUnit> Units { get; private set; } = new List<DrawMeshResourceUnit>();



        /// <summary>
        /// 描画用メッシュを登録する
        /// </summary>
        public void AddDrawMeshResources( IEnumerable<CharactorResourceUnit> resources )
        {

            this.Units.AddRange( resources
                .Select( ( res, i ) =>
                    new DrawMeshResourceUnit
                    {
                        MeshId = i,
                        Mesh = combineAndConvertMesh( res.SkinnedMesh, res.MotionClip ),
                        Material = res.Material,
                        TransformBuffer = new SimpleComputeBuffer<float4>("bones",1024*4)
                    }
                )
            );

            return;


            // メッシュを結合する
            Mesh combineAndConvertMesh( Mesh[] meshes, MotionClip motionClip )
            {
                var qCis =
                    from mesh in meshes
                    select new CombineInstance
                    {
                        mesh = mesh
                    }
                    ;

                var dstmesh = new Mesh();

                dstmesh.CombineMeshes( qCis.ToArray(), mergeSubMeshes: true, useMatrices: false );

                return ChMeshConverter.ConvertToChMesh( dstmesh, motionClip );
            }
        }

        //public void AddDrawMeshResources( IEnumerable<> resources )
        //{

        //}
    }

}
