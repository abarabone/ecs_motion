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
using UnityEditor;

namespace DotsLite.Model.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Structure.Authoring;
    using DotsLite.Utilities;
    using DotsLite.Common.Extension;
    using DotsLite.Misc;

    // 違うプレハブでも同じモデルを使いまわせるようにしたいんだけど
    // モデルだけ独立させるか…？
    // 保留中

    [CreateAssetMenu(fileName="mesh model asset", menuName= "Custom/Mesh Model")]
    public class MeshModelAsset : ScriptableObject
    {

        [SerializeField]
        public MeshModel<UI32, PositionNormalUvVertex> MeshModel =
            new MeshModel<UI32, PositionNormalUvVertex>();

    }

    [CreateAssetMenu(fileName = "mesh model asset", menuName = "Custom/Mesh Model2")]
    public class MeshModelAsset2 : ScriptableObject
    {

        [SerializeField]
        public MeshModel<UI32, PositionNormalUvVertex> MeshModel =
            new MeshModel<UI32, PositionNormalUvVertex>();

    }

    //[Serializable]
    //public class LodMeshModel<TIdx, TVtx> : MeshModel<TIdx, TVtx>, IMeshModelLod
    //    where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
    //    where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    //{
    //    //public LodMeshModel(GameObject obj, Shader shader) : base(obj, shader)
    //    //{ }

    //    [SerializeField]
    //    public float limitDistance;

    //    [SerializeField]
    //    public float margin;


    //    public float LimitDistance => this.limitDistance;
    //    public float Margin => this.margin;
    //}
}