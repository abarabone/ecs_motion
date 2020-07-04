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

namespace Abarabone.Authoring
{
    using Abarabone.Geometry;
    using Abarabone.Utilities;
    using Abarabone.Misc;
    using Abarabone.CharacterMotion;
    using Abarabone.Draw;
    using Abarabone.Model;


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
    public class DrawPrefabSettingsAuthoring : MonoBehaviour
    {

        public ConvertToMainCustomPrefabEntityBehaviour[] PrefabGameObjects;

        public Entity[] PrefabEntities { get; private set; }
        

        void Awake()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            
            var drawModelArchetype = em.CreateArchetype(
                typeof( DrawModel.BoneUnitSizeData ),
                typeof( DrawModel.InstanceCounterData ),
                typeof( DrawModel.InstanceOffsetData ),
                typeof( DrawModel.GeometryData ),
                typeof( DrawModel.ComputeArgumentsBufferData )
            );
            
            this.PrefabEntities = this.PrefabGameObjects
                //.Select( prefab => prefab.Convert( em, drawMeshCsResourceHolder, initDrawModelComponents_ ) )
                .Select( prefab => prefab.Convert( em, initDrawModelComponents_ ) )
                .ToArray();
            
            return;


            Entity initDrawModelComponents_( Mesh mesh, Material mat, BoneType BoneType, int boneLength )
            {
                // キャプチャ（暫定）
                var em_ = em;
                var drawModelArchetype_ = drawModelArchetype;
                
                var sys = em.World.GetExistingSystem<DrawBufferManagementSystem>();
                var boneVectorBuffer = sys.GetSingleton<DrawSystem.ComputeTransformBufferData>().Transforms;
                setShaderProps_( mat, mesh, boneVectorBuffer, boneLength );

                return createEntityAndInitComponents_( drawModelArchetype_, boneLength );


                void setShaderProps_( Material mat_, Mesh mesh_, ComputeBuffer boneVectorBuffer_, int boneLength_ )
                {
                    mat_.SetBuffer( "BoneVectorBuffer", boneVectorBuffer_ );
                    mat_.SetInt( "BoneLengthEveryInstance", boneLength_ );
                    //mat_.SetInt( "BoneVectorOffset", 0 );// 毎フレームのセットが必要
                }

                Entity createEntityAndInitComponents_( EntityArchetype drawModelArchetype__, int boneLength_ )
                {
                    //var drawModelArchetype = em.CreateArchetype(
                    //    typeof( DrawModelBoneInfoData ),
                    //    typeof( DrawModelInstanceCounterData ),
                    //    typeof( DrawModelInstanceOffsetData ),
                    //    typeof( DrawModelMeshData ),
                    //    typeof( DrawModelComputeArgumentsBufferData )
                    //);
                    var ent = em.CreateEntity( drawModelArchetype__ );
                    
                    em.SetComponentData( ent,
                        new DrawModel.BoneUnitSizeData
                        {
                            BoneLength = boneLength_,
                            VectorLengthInBone = (int)BoneType,
                        }
                    );
                    em.SetComponentData( ent,
                        new DrawModel.GeometryData
                        {
                            Mesh = mesh,
                            Material = mat,
                        }
                    );
                    em.SetComponentData( ent,
                        new DrawModel.ComputeArgumentsBufferData
                        {
                            InstanceArgumentsBuffer = ComputeShaderUtility.CreateIndirectArgumentsBuffer(),
                        }
                    );

                    return ent;
                }

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
            abstract public Entity Convert
                ( EntityManager em, Func<Mesh, Material, BoneType, int, Entity> initDrawModelComponentsFunc );
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

