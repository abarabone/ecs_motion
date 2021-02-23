using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Linq;

namespace Abarabone.Structure.Aurthoring
{
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;
    using Abarabone.Structure.Authoring;
    using Abarabone.Utilities;
    using Abarabone.Common.Extension;
    using Abarabone.Misc;

    public class PartModel<TIdx, TVtx> : IMeshModel
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {
        public PartModel(GameObject obj, Shader shader)
        {
            this.objectTop = obj;
            this.shader = shader;
        }

        [SerializeField]
        GameObject objectTop;

        [SerializeField]
        Shader shader;



        public GameObject Obj => this.objectTop;
        public Transform TfRoot => this.objectTop.transform;
        public Transform[] Bones => null;




        public IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts
        {
            get
            {
                var part = this.objectTop;
                var children = queryPartBodyObjects_Recursive_(part);//.ToArray();

                return children.QueryMeshMatsTransform_IfHaving();


                static IEnumerable<GameObject> queryPartBodyObjects_Recursive_(GameObject go)
                {
                    var q =
                        from child in go.Children()
                        where child.GetComponent<StructurePartAuthoring>() == null
                        from x in queryPartBodyObjects_Recursive_(child)
                        select x
                        ;
                    return q.Prepend(go);
                }
            }
        }


        public void CreateModelEntity
            (GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            const BoneType BoneType = BoneType.TR;
            var boneLength = 1;

            gcs.CreateDrawModelEntityComponents(this.Obj, mesh, mat, BoneType, boneLength);
        }

        public (GameObject obj, Func<IMeshElements> f) BuildMeshCombiner
            (
                SrcMeshesModelCombinePack meshpack,
                Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary
            )
        {
            var atlas = atlasDictionary.objectToAtlas[this.Obj].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            return (
                this.Obj,
                meshpack.BuildCombiner<TIdx, TVtx>(this.TfRoot, part => texdict[atlas, part], this.Bones)
            );
        }
    }

}