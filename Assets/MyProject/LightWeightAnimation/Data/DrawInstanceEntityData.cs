using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Abarabone.Geometry;
using System.Runtime.InteropServices;
using System;

namespace Abarabone.Draw
{
    using Abarabone.Utilities;


    public struct DrawInstanceEntity :
        ITypedEntity<
            DrawInstance.ModeLinkData,
            DrawInstance.TargetWorkData
        >
    {
        public Entity Entity { get; set; }

        static public implicit operator DrawInstanceEntity( Entity ent )
            => new DrawInstanceEntity { Entity = ent };
    }



    static public partial class DrawInstance
    {

        /// <summary>
        /// 
        /// </summary>
        public struct MeshTag : IComponentData
        { }


        /// <summary>
        /// 描画モデルの種類情報
        /// </summary>
        public struct ModeLinkData : IComponentData
        {
            public Entity DrawModelEntityCurrent;
        }
        public struct PostureLinkData : IComponentData
        {
            public Entity PostureEntity;
        }
        public struct BoneLinkData : IComponentData
        {
            public Entity BoneRelationTop;
        }



        public struct TargetWorkData : IComponentData
        {
            public int DrawInstanceId;   // -1 なら描画しない
        }

        public struct ModelLod2LinkData : IComponentData
        {
            public Entity DrawModelEntity0;
            public Entity DrawModelEntity1;
            public float SqrDistance0;
            public float SqrDistance1;
        }




        ///// <summary>
        ///// カリング用オブジェクトＡＡＢＢ
        ///// </summary>
        //public struct TargetAabb : IComponentData
        //{
        //    public float4 min;
        //    public float4 max;
        //}

        ///// <summary>
        ///// カリング用オブジェクト球データ
        ///// </summary>
        //public struct TargetSphere : IComponentData
        //{
        //    public float center;
        //    public float radius;
        //}



    }

}
