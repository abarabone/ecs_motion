using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Model.Authoring
{
    using Motion;
    using Draw;
    using Character;
    using Abarabone.Authoring;

    using Abarabone.Common.Extension;


    /// <summary>
    /// 
    /// </summary>
    public class CharacterModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {

        public Material Material;


        /// <summary>
        /// 
        /// </summary>
        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            

        }

    }


    static public class BoneEntitiesCreator
    {

        static EntityArchetype postureArchetypeCache( this EntityManager em ) =>
            em.CreateArchetype
            (
                typeof( PostureNeedTransformTag ),
                typeof( PostureLinkData ),
                typeof( Translation ),
                typeof( Rotation )
            );

        static EntityArchetype boneArchetypeCache( this EntityManager em ) =>
            em.CreateArchetype
            (
                typeof( BoneRelationLinkData ),
                typeof( DrawTransformLinkData ),
                typeof( DrawTransformIndexData ),
                typeof( DrawTransformTargetWorkData ),
                typeof( BoneLocalValueData ),// どうしようか
                typeof( Translation ),
                typeof( Rotation )
            );


        static public (NameAndEntity[] bonePrefabs, Entity posturePrefab) CreateEntities
        (
            ConvertToEntitySystem cs, EntityManager em,
            Entity drawInstancePrefab,
            float4x4[] mtBones,
            bool[] isEnableds,
            string[] paths
        )
        {
            var postArchetype = em.postureArchetypeCache();
            var boneArchetype = em.boneArchetypeCache();

            var qName =
                from x in paths
                let name = System.IO.Path.GetFileName( x )
                select name
                ;

            var posturePrefab = em.CreateEntity( postArchetype );

            var boneNameAndPrefabs = createNameAndBonePrefabs( em, qName, boneArchetype );
            var bonePrefabs = ( from x in boneNameAndPrefabs select x.Entity ).ToArray();

            em.setDrawComponet( bonePrefabs, drawInstancePrefab );
            em.setBoneId( bonePrefabs, bonePrefabs.Length );
            em.setBoneRelationLinks( posturePrefab, boneNameAndPrefabs, enabledsAndPaths );
            em.removeBoneRelationLinks( bonePrefabs, qBoneMasks );

            em.SetComponentData( posturePrefab, new PostureLinkData { BoneRelationTop = bonePrefabs[ 0 ] } );
            em.SetComponentData( posturePrefab, new Rotation { Value = quaternion.identity } );
            em.SetComponentData( posturePrefab, new Translation { Value = float3.zero } );
            
            return (boneNameAndPrefabs, posturePrefab);
        }

        static void setDrawComponet
            ( this EntityManager em_, IEnumerable<Entity> bonePrefabs, Entity drawInstancePrefab )
        {
            em_.SetComponentData(
                bonePrefabs,
                new DrawTransformLinkData
                {
                    DrawInstanceEntity = drawInstancePrefab,
                    DrawModelEntity = em_.GetComponentData<DrawInstanceModeLinkData>( drawInstancePrefab ).DrawModelEntity,
                }
            );
        }


        static NameAndEntity[] createNameAndBonePrefabs
            ( this EntityManager em_, IEnumerable<string> qName, EntityArchetype archetype )
        {
            using( var bonePrefabs = new NativeArray<Entity>( qName.Count(), Allocator.Temp ) )
            {
                em_.CreateEntity( archetype, bonePrefabs );

                var q =
                    from x in (qName, bonePrefabs).Zip()
                    select new NameAndEntity( x.x, x.y )
                    ;
                return q.ToArray();
            }
        }

        static void setBoneId
            ( this EntityManager em_, IEnumerable<Entity> bonePreafabs_, int boneLength )
        {
            em_.SetComponentData( bonePreafabs_,
                from x in Enumerable.Range( 0, bonePreafabs_.Count() )
                select new DrawTransformIndexData { BoneLength = boneLength, BoneId = x }
            );
        }

        static void setBoneRelationLinks(
            this EntityManager em_,
            Entity posturePrefab_, NameAndEntity[] bonePrefabs_,
            //(bool isEnabled, string path)[] enabledsAndPaths_

        )
        {
            var qParentEnt =
                from parentName in
                    from src in enabledsAndPaths_
                    where src.isEnabled
                    let parentPath = System.IO.Path.GetDirectoryName( src.path )
                    select System.IO.Path.GetFileName( parentPath )
                join bone in bonePrefabs_
                    on parentName equals bone.Name
                into x
                from bone in x.DefaultIfEmpty( new NameAndEntity( "", posturePrefab_ ) )
                select bone.Entity
                ;

            var qNextEnt = (bonePrefabs_, enabledsAndPaths_).Zip( ( x, y ) => (x.Entity, y.isEnabled) )
                .Where( x => x.isEnabled )
                .Select( x => x.Entity )
                .Append( Entity.Null )
                .Skip( 1 );

            var qBoneLinker =
                from x in (qParentEnt, qNextEnt).Zip()
                select new BoneRelationLinkData
                {
                    ParentBoneEntity = x.x,
                    NextBoneEntity = x.y,
                };

            em_.SetComponentData( from x in bonePrefabs_ select x.Entity, qBoneLinker );

        }

        // チャンクが別になるから、消さないほうがいい可能性もあり
        static void removeBoneRelationLinks
            ( this EntityManager em_, IEnumerable<Entity> bonePrefabs_, IEnumerable<bool> boneMasks_ )
        {
            var qDisEnables =
                from x in (bonePrefabs_, boneMasks_).Zip()
                let boneEnt = x.x
                let isEnable = x.y
                where !isEnable
                select boneEnt
                ;
            foreach( var x in qDisEnables )
            {
                em_.RemoveComponent<BoneRelationLinkData>( x );
                em_.RemoveComponent<BoneStream0LinkData>( x );
            }

        }

        
    }

}

