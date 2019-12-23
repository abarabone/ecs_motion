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

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Character;

namespace Abss.Arthuring
{

    public struct NameAndEntity
    {
        public string Name;
        public Entity Entity;
        public NameAndEntity( string name, Entity entity )
        {
            this.Name = name;
            this.Entity = entity;
        }
    }

    [DisallowMultipleComponent]
    public class PrefabSettingsAuthoring : MonoBehaviour
    {

        public ConvertToMainCustomPrefabEntityBehaviour[] PrefabGameObjects;

        public Entity[] PrefabEntities { get; private set; }

        public int InstanceCountPerModel = 100;


        List<Entity> ents = new List<Entity>();

        void Awake()
        {
            var em = World.Active.EntityManager;


            var drawMeshCsSystem = em.World.GetExistingSystem<DrawMeshCsSystem>();
            var drawMeshCsResourceHolder = drawMeshCsSystem.GetResourceHolder();

            var sysEnt = initDrawSystemComponents_();

            var drawModelArchetype = em.CreateArchetype(
                typeof( DrawModelBoneInfoData ),
                typeof( DrawModelInstanceCounterData ),
                typeof( DrawModelInstanceOffsetData ),
                typeof( DrawModelMeshData ),
                typeof( DrawModelComputeArgumentsBufferData )
            );

            this.PrefabEntities = this.PrefabGameObjects
                .Select( prefab => prefab.Convert( em, drawMeshCsResourceHolder, initDrawModelComponents_ ) )
                .ToArray();

            initChunkSystemComponent_();


            // モーション設定
            foreach( var model in Enumerable.Range( 0, this.PrefabEntities.Length ) )
            {
                var mlinker = em.GetComponentData<CharacterLinkData>( this.PrefabEntities[ model ] );
                ref var mclip = ref em.GetComponentData<MotionClipData>( mlinker.MainMotionEntity ).ClipData.Value;

                foreach( var i in Enumerable.Range( 0, this.InstanceCountPerModel ) )
                {
                    this.ents.Add( em.Instantiate( this.PrefabEntities[ model ] ) );

                    var chlinker = em.GetComponentData<CharacterLinkData>( this.ents[ this.ents.Count - 1 ] );
                    em.SetComponentData( chlinker.PostureEntity, new Translation { Value = new float3( i * 3, 0, -model * 5 ) } );
                    em.SetComponentData( chlinker.MainMotionEntity, new MotionInitializeData { MotionIndex = i % mclip.Motions.Length } );
                }
            }

            // 先頭キャラのみ
            em.AddComponentData( this.ents[ 0 ], new PlayerTag { } );
            var post = em.GetComponentData<CharacterLinkData>( this.ents[ 0 ] ).PostureEntity;
            em.AddComponentData( post, new PlayerTag { } );//
                                                           //em.AddComponentData( post, new MoveHandlingData { } );//

        
            Entity initDrawModelComponents_( Mesh mesh, Material mat, BoneType boneType )
            {

                var boneVectorBuffer = em.GetComponentData<DrawSystemComputeTransformBufferData>(sysEnt).Transforms;
                mat.SetBuffer( "BoneVectorBuffer", boneVectorBuffer );
                mat.SetInt( "BoneVectorLength", mesh.bindposes.Length * (int)boneType );
                mat.SetInt( "BoneVectorOffset", 0 );// 毎フレームのセットが必要

                //var drawModelArchetype = em.CreateArchetype(
                //    typeof( DrawModelBoneInfoData ),
                //    typeof( DrawModelInstanceCounterData ),
                //    typeof( DrawModelInstanceOffsetData ),
                //    typeof( DrawModelMeshData ),
                //    typeof( DrawModelComputeArgumentsBufferData )
                //);
                var ent = em.CreateEntity( drawModelArchetype );

                em.SetComponentData( ent,
                    new DrawModelBoneInfoData
                    {
                        BoneLength = mesh.bindposes.Length,//
                        VectorLengthInBone = (int)boneType,
                    }
                );
                em.SetComponentData( ent,
                    new DrawModelMeshData
                    {
                        Mesh = mesh,
                        Material = mat,
                    }
                );
                em.SetComponentData( ent,
                    new DrawModelComputeArgumentsBufferData
                    {
                        InstanceArgumentsBuffer = ComputeShaderUtility.CreateIndirectArgumentsBuffer(),
                    }
                );

                return ent;
            }

            Entity initDrawSystemComponents_()
            {
                var drawBufferArchetype = em.CreateArchetype(
                    typeof(DrawSystemComputeTransformBufferData),
                    typeof(DrawSystemNativeTransformBufferData)
                );
                var ent = em.CreateEntity( drawBufferArchetype );

                const int maxBuffer = 1000 * 16 * 2;//
                var stride = Marshal.SizeOf( typeof( float4 ) );
                em.SetComponentData( ent,
                    new DrawSystemComputeTransformBufferData
                    {
                        Transforms = new ComputeBuffer
                            ( maxBuffer, stride, ComputeBufferType.Default, ComputeBufferMode.Immutable ),
                    }
                );

                return ent;
            }

            void initChunkSystemComponent_()
            {
                var q = em.CreateEntityQuery(
                    typeof( DrawModelBoneInfoData ),
                    typeof( DrawModelInstanceCounterData ),
                    typeof( DrawModelInstanceOffsetData ),
                    typeof( DrawModelMeshData ),
                    typeof( DrawModelComputeArgumentsBufferData )
                );
                em.AddChunkComponentData( q, new DrawChunkBufferLinkerData { BufferEntity = sysEnt } );
                q.Dispose();
            }
        }



        //void Update()
        //{
        //    if( !Input.GetMouseButtonDown( 0 ) ) return;
        //    if( this.ents.Count == 0 ) return;

        //    var em = World.Active.EntityManager;

        //    var ent = this.ents.Last();
        //    em.DestroyEntity( ent );
        //    this.ents.Remove( ent );
        //}


        public abstract class ConvertToMainCustomPrefabEntityBehaviour : ConvertToCustomPrefabEntityBehaviour
        { }

        public abstract class ConvertToCustomPrefabEntityBehaviour : MonoBehaviour
        {
            abstract public Entity Convert( EntityManager em, DrawMeshResourceHolder drawres, Func<Mesh, Material, BoneType, Entity> initDrawModelComponentsAction );
        }
    }


    /// <summary>
    /// アーキタイプを EntityManager ごとにキャッシュする。
    /// もしかすると、CreateArchetype() 自体にそういった仕組みがあるかもしれない、その場合は不要となる。
    /// </summary>
    public class EntityArchetypeCache
    {

        Dictionary<EntityManager, EntityArchetype> archetypeCache;

        Func<EntityManager, EntityArchetype> createFunc;


        public EntityArchetypeCache( Func<EntityManager, EntityArchetype> createFunc )
        {
            this.archetypeCache = new Dictionary<EntityManager, EntityArchetype>();
            this.createFunc = createFunc;
        }

        public EntityArchetype GetOrCreateArchetype( EntityManager em )
        {
            if( this.archetypeCache.TryGetValue( em, out var archetype ) ) return archetype;

            archetype = this.createFunc( em );

            archetypeCache.Add( em, archetype );

            return archetype;
        }
    }
    
    /// <summary>
    /// アーキタイプを EntityManager ごとキャッシュする。
    /// もしかすると、CreateArchetype() 自体にそういった仕組みがあるかもしれない、その場合は不要となる。
    /// </summary>
    static public class EntityArchetypeCache2
    {

        static Dictionary<(EntityManager,ComponentType[]), EntityArchetype> archetypeCache;
        static EntityArchetypeCache2() =>
            EntityArchetypeCache2.archetypeCache = new Dictionary<(EntityManager, ComponentType[]), EntityArchetype>();


        static public EntityArchetype GetOrCreate( this ComponentType[] componentDataTypes, EntityManager em )
            => EntityArchetypeCache2.GetOrCreateArchetype( em, componentDataTypes );


        static EntityArchetype GetOrCreateArchetype( EntityManager em, ComponentType[] componentDataTypes )
        {
            if( EntityArchetypeCache2.archetypeCache.TryGetValue( (em,componentDataTypes), out var archetype ) )
                return archetype;

            archetype = em.CreateArchetype( componentDataTypes );
            archetypeCache.Add( (em, componentDataTypes), archetype );

            return archetype;
        }
    }
    
}

