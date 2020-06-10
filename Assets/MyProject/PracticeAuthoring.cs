using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Linq;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;

using Abarabone.Geometry;
using Abarabone.Utilities;
using Abarabone.Misc;
using Abarabone.Motion;
using Abarabone.Draw;
using Abarabone.Character;

namespace Abarabone.Arthuring
{

    [DisallowMultipleComponent]
    public class PracticeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        //public CharacterAuthoring ch;

        public void Convert
            ( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            //ch.Convert(dstManager, )

            //Debug.Log("aaa");

            var em = dstManager;// World.DefaultGameObjectInjectionWorld.EntityManager;
            em.AddComponentData( entity, new DrawSystemNativeTransformBufferData { } );

            //var drawModelArchetype = em.CreateArchetype(
            //    typeof( DrawModelBoneUnitSizeData ),
            //    typeof( DrawModelInstanceCounterData ),
            //    typeof( DrawModelInstanceOffsetData ),
            //    typeof( DrawModelGeometryData ),
            //    typeof( DrawModelComputeArgumentsBufferData )
            //);

            //initDrawSystemComponents_( em );

            //this.PrefabEntities = this.PrefabGameObjects
            //    //.Select( prefab => prefab.Convert( em, drawMeshCsResourceHolder, initDrawModelComponents_ ) )
            //    .Select( prefab => prefab.Convert( em, initDrawModelComponents_ ) )
            //    .ToArray();

            return;


            //void initDrawSystemComponents_( EntityManager em_ )
            //{
            //    var ent = entity;


            //    const int maxBufferLength = 1000 * 16 * 2;//

            //    var stride = Marshal.SizeOf( typeof( float4 ) );

            //    em_.AddComponentData( ent,
            //        new DrawSystemComputeTransformBufferData
            //        {
            //            Transforms = new ComputeBuffer
            //                ( maxBufferLength, stride, ComputeBufferType.Default, ComputeBufferMode.Immutable ),
            //        }
            //    );
            //    em_.AddComponentData( ent, new DrawSystemNativeTransformBufferData { } );
            //}

            //Entity initDrawModelComponents_( Mesh mesh, Material mat, BoneType boneType )
            //{
            //    // キャプチャ（暫定）
            //    var em_ = em;
            //    var drawModelArchetype_ = drawModelArchetype;
            //    var sysEnt_ = entity;

            //    var drawModelEnt = createEntityAndInitComponents_( drawModelArchetype_ );

            //    var boneVectorBuffer = em_.GetComponentData<DrawSystemComputeTransformBufferData>( sysEnt_ ).Transforms;
            //    setShaderProps_( mat, mesh, boneVectorBuffer );

            //    return drawModelEnt;


            //    void setShaderProps_( Material mat_, Mesh mesh_, ComputeBuffer boneVectorBuffer_ )
            //    {
            //        mat_.SetBuffer( "BoneVectorBuffer", boneVectorBuffer_ );
            //        mat_.SetInt( "BoneLengthEveryInstance", mesh_.bindposes.Length );
            //        //mat_.SetInt( "BoneVectorOffset", 0 );// 毎フレームのセットが必要
            //    }

            //    Entity createEntityAndInitComponents_( EntityArchetype drawModelArchetype__ )
            //    {
            //        //var drawModelArchetype = em.CreateArchetype(
            //        //    typeof( DrawModelBoneInfoData ),
            //        //    typeof( DrawModelInstanceCounterData ),
            //        //    typeof( DrawModelInstanceOffsetData ),
            //        //    typeof( DrawModelMeshData ),
            //        //    typeof( DrawModelComputeArgumentsBufferData )
            //        //);
            //        var ent = em.CreateEntity( drawModelArchetype__ );

            //        em.SetComponentData( ent,
            //            new DrawModelBoneUnitSizeData
            //            {
            //                BoneLength = mesh.bindposes.Length,// より正確なものに変える
            //                VectorLengthInBone = (int)boneType,
            //            }
            //        );
            //        em.SetComponentData( ent,
            //            new DrawModelGeometryData
            //            {
            //                Mesh = mesh,
            //                Material = mat,
            //            }
            //        );
            //        em.SetComponentData( ent,
            //            new DrawModelComputeArgumentsBufferData
            //            {
            //                InstanceArgumentsBuffer = ComputeShaderUtility.CreateIndirectArgumentsBuffer(),
            //            }
            //        );

            //        return ent;
            //    }

            //}
        }
    }
}
