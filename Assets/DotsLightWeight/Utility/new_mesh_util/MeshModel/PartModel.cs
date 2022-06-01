//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using System;
//using System.Linq;
//using UnityEngine;
//using Unity.Entities;
//using Unity.Collections;
//using Unity.Transforms;
//using Unity.Mathematics;
//using Unity.Linq;

//namespace DotsLite.Structure.Authoring
//{
//    using DotsLite.Model;
//    using DotsLite.Draw;
//    using DotsLite.Draw.Authoring;
//    using DotsLite.Geometry;
//    using DotsLite.Structure.Authoring;
//    using DotsLite.Utilities;
//    using DotsLite.Common.Extension;
//    using DotsLite.Misc;
//    using DotsLite.Model.Authoring;

//    [Serializable]
//    public class PartModel<TIdx, TVtx> : PartModelBase<TIdx, TVtx, StructureBuildingPartAuthoring>
//        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
//        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
//    {

//    }

//    [Serializable]
//    public class AreaPartModel<TIdx, TVtx> : PartModelBase<TIdx, TVtx, StructureAreaPartAuthoring>
//        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
//        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
//    {

//    }

//    [Serializable]
//    public abstract class PartModelBase<TIdx, TVtx, TPart> : MeshModel<TIdx, TVtx>
//        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
//        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
//        where TPart : IStructurePart
//    {


//        public virtual void SetObject(GameObject obj) => this.objectTop = obj;


//        [SerializeField]
//        protected ColorPaletteAsset colorPalette;


//        public override IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> QueryMmts
//        {
//            get
//            {
//                var part = this.Obj;
//                var children = queryPartBodyObjects_Recursive_(part);//.ToArray();

//                return children.QueryMeshMatsTransform_IfHaving();


//                static IEnumerable<GameObject> queryPartBodyObjects_Recursive_(GameObject go)
//                {
//                    var q =
//                        from child in go.Children()
//                        where child.GetComponent<TPart>() == null
//                        from x in queryPartBodyObjects_Recursive_(child)
//                        select x
//                        ;
//                    return q.Prepend(go);
//                }
//            }
//        }

//    }

//}