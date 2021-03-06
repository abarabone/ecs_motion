﻿using System.Collections;
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

namespace Abarabone.Structure.Authoring
{
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;
    using Abarabone.Structure.Authoring;
    using Abarabone.Utilities;
    using Abarabone.Common.Extension;
    using Abarabone.Misc;
    using Abarabone.Model.Authoring;

    [Serializable]
    public class PartModel<TIdx, TVtx> : MeshModel<TIdx, TVtx>
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {

        public PartModel(GameObject obj, Shader shader) : base(obj, shader)
        { }


        public void SetObject(GameObject obj) => this.objectTop = obj;


        public override IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts
        {
            get
            {
                var part = this.Obj;
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

    }

}