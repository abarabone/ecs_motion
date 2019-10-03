using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Arthuring;
using Abss.Motion;

namespace Abss.Draw
{

    public struct DrawMeshResourceUnit
    {
        public int MeshId;
        public Mesh Mesh;
        public Material Material;
    }

    public class DrawMeshResourceHolder
    {

        public DrawMeshResourceUnit[] Units { get; private set; }


        public void AddDrawMeshResources( IEnumerable<CharactorResourceUnit> resources )
        {

            this.Units = resources
                .Select( ( res, i ) =>
                    new DrawMeshResourceUnit
                    {
                        MeshId = i,
                        Mesh = combineAndConvertMesh( res.SkinnedMesh, res.MotionClip ),
                        Material = res.Material,
                    }
                )
                .ToArray();

            return;


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
