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
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Abss.Geometry;
using System.Runtime.InteropServices;
using System;

namespace Abss.Draw
{

	/// <summary>
	/// カリング用オブジェクトＡＡＢＢ
	/// </summary>
	public struct DrawInstanceTargetAabb : IComponentData
	{
		public float4	min;
		public float4	max;
	}

	/// <summary>
	/// カリング用オブジェクト球データ
	/// </summary>
	public struct DrawInstanceTargetSphere : IComponentData
	{
		public float	center;
		public float	radius;
	}

    /// <summary>
    /// 描画モデルの種類情報
    /// </summary>
    public struct DrawInstanceIndexOfModelData : IComponentData
    {
        public Entity DrawModelEntity;
    }
    public struct DrawInstanceTargetWorkData : IComponentData
    {
        public int DrawInstanceId;   // -1 なら描画しない
	}

    //public struct DrawBoneRelationLinkData : IComponentData
    //{
    //    public Entity BoneEntityTop;
    //}

    
    public interface aaa
    {
        Entity Entity { set; }
    }
    public struct DrawInstance : aaa
    {
        
        public Entity Entity { get; set; }

        public ComponentDataAccessor<DrawInstanceIndexOfModelData> IndexOfModel;

    }

    public struct ComponentDataAccessor<T> where T : struct, IComponentData
    {
        public T this[EntityManager em, Entity ent]
        {
            get => em.GetComponentData<T>( ent );
            set => em.SetComponentData( ent, value );
        }
    }


    public interface Iaa<T> where T : Iaa<T>
    {
        T Init( EntityManager em, Entity entity );
    }

    static public class ComponentDataExtension
    {
        static public T GetEntityAccessor<T>( this EntityManager em, Entity entity ) where T : Iaa<T>, new() =>
            new T().Init( em, entity );

        static public T aaaaa<T>( this EntityManager em, Entity ent, T e ) where T : aaa, new()
        {
            var c = new T();
            var a = new DrawInstance();
            a.IndexOfModel[em,ent].
        }

    }
}
