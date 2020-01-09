﻿using System.Collections;
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
    public class DrawPrefabSettingsAuthoring : MonoBehaviour
    {

        public ConvertToMainCustomPrefabEntityBehaviour[] PrefabGameObjects;

        public Entity[] PrefabEntities { get; private set; }
        

        void Awake()
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;

            
            var drawModelArchetype = em.CreateArchetype(
                typeof( DrawModelBoneUnitSizeData ),
                typeof( DrawModelInstanceCounterData ),
                typeof( DrawModelInstanceOffsetData ),
                typeof( DrawModelGeometryData ),
                typeof( DrawModelComputeArgumentsBufferData )
            );
            
            this.PrefabEntities = this.PrefabGameObjects
                //.Select( prefab => prefab.Convert( em, drawMeshCsResourceHolder, initDrawModelComponents_ ) )
                .Select( prefab => prefab.Convert( em, initDrawModelComponents_ ) )
                .ToArray();
            
            return;


            Entity initDrawModelComponents_( Mesh mesh, Material mat, BoneType boneType )
            {
                // キャプチャ（暫定）
                var em_ = em;
                var drawModelArchetype_ = drawModelArchetype;
                
                var sys = em.World.GetExistingSystem<DrawBufferManagementSystem>();
                var boneVectorBuffer = sys.GetSingleton<DrawSystemComputeTransformBufferData>().Transforms;
                setShaderProps_( mat, mesh, boneVectorBuffer );

                return createEntityAndInitComponents_( drawModelArchetype_ );


                void setShaderProps_( Material mat_, Mesh mesh_, ComputeBuffer boneVectorBuffer_ )
                {
                    mat_.SetBuffer( "BoneVectorBuffer", boneVectorBuffer_ );
                    mat_.SetInt( "BoneLengthEveryInstance", mesh_.bindposes.Length );
                    //mat_.SetInt( "BoneVectorOffset", 0 );// 毎フレームのセットが必要
                }

                Entity createEntityAndInitComponents_( EntityArchetype drawModelArchetype__ )
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
                        new DrawModelBoneUnitSizeData
                        {
                            BoneLength = mesh.bindposes.Length > 0 ? mesh.bindposes.Length : 1,// より正確なものに変える
                            VectorLengthInBone = (int)boneType,
                        }
                    );
                    em.SetComponentData( ent,
                        new DrawModelGeometryData
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
                ( EntityManager em, Func<Mesh, Material, BoneType, Entity> initDrawModelComponentsFunc );
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

