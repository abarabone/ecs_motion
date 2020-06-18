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
    using Abarabone.Misc;
    using Abarabone.Utilities;


    /// <summary>
    /// 
    /// </summary>
    public class CharacterModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity
    {

        public Material Material;

        public AvatarMask BoneMask;

        //public Transform[] BoneRoots;


        // ボーン
        // ・ボーンＩＤは、SkinnedMeshRenderer.bones の並び順
        // ・ボーン名が _ で始まるものは除外
        // ・除外したうえでＩＤを 0 から振りなおし
        // ・モーションストリームはボーンに対応するようにソートされている
        // ・ボーンとマスクの並び順は同じだと思われるが、念のためボーン名で取得する

        // いずれは…
        // ・ボーンは、BoneRoots に登録したものから辿れるすべて
        // ・BoneRoots.Length が 0 なら、最初に取得できた SkinnedMeshRenderer.bones から取得する
        // ・.RootBone からがいいか？
        // ・ボーンＩＤは、パスをソートした順番

        public EnBoneType Mode;
        public enum EnBoneType
        {
            reel_a_chain,
            in_deep_order,
        }


        /// <summary>
        /// 
        /// </summary>
        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {
            var skinnedMeshRenderer = this.GetComponentInChildren<SkinnedMeshRenderer>();
            var (paths, enablePaths) = queryEnabledsAndPaths_( skinnedMeshRenderer );

            paths.ForEach( x => Debug.Log( x ) );
            //BoneEntitiesCreator.CreateEntities( conversionSystem, dstManager, this.gameObject, entity, isEnableds, paths );

            return;


            (string[] paths, string[] enablePaths) queryEnabledsAndPaths_( SkinnedMeshRenderer smr, AvatarMask boneMask )
            {

                var bonePaths = queryBonePath( smr ).ToArray();

                var isBoneEnableds = ( this.BoneMask != null )
                    ? queryBoneIsEnabled(this.BoneMask, bonePaths).ToArray()
                    : bonePaths.Select(x=>true).ToArray();

                var enablePaths = makeEnabledBoneHashSet_( this.)

                return (isBoneEnableds, bonePaths);


                IEnumerable<string> queryBonePath( SkinnedMeshRenderer smr_ )
                {
                    var rootPath = smr_.rootBone.gameObject.MakePath();
                    var pathOffset = rootPath.Length;

                    return
                        from bone in smr_.bones
                        where !bone.name.StartsWith( "_" )
                        select bone.gameObject.MakePath().Substring( pathOffset )
                        ;
                }

                HashSet<string> makeEnabledBoneHashSet_( AvatarMask boneMask, IEnumerable<string> bonePaths_ )
                {
                    var qEnabledBonePaths =
                        from id in Enumerable.Range( 0, boneMask.transformCount )
                        let isEnabled = this.BoneMask.GetTransformActive( id )
                        let path = boneMask.GetTransformPath( id )
                        select path
                        ;

                    return new HashSet<string>( qEnabledBonePaths );
                }

                IEnumerable<bool> queryBoneIsEnabled( AvatarMask boneMask, IEnumerable<string> bonePaths_ )
                {
                    var qIsEnabledAndPathOnMask =
                        from id in Enumerable.Range( 0, boneMask.transformCount )
                        let isEnabled = this.BoneMask.GetTransformActive( id )
                        let path = boneMask.GetTransformPath( id )
                        select (isEnabled, path)
                        ;

                    var qIsEnabled =
                        from bpath in bonePaths_
                        join x in qIsEnabledAndPathOnMask
                            on bpath equals x.path
                        select x.isEnabled
                        ;

                    return qIsEnabled;
                }
            }
        }

    }


    static public class BoneEntitiesCreator
    {


        static public void CreateEntities
        (
            GameObjectConversionSystem gcs, EntityManager em, GameObject mainGameObject,
            Entity drawInstancePrefab, IEnumerable<bool> isEnableds, IEnumerable<string> paths
        )
        {
            var postArchetype = em.postureArchetypeCache();
            var boneArchetype = em.boneArchetypeCache();

            var postureEntity = gcs.CreateAdditionalEntity( mainGameObject, em, postArchetype );
            var boneEntities = gcs.CreateAdditionalEntities( mainGameObject, em, boneArchetype, paths.Count() );

            //em.setDrawComponet( boneEntities, drawInstancePrefab );
            em.setBoneId( boneEntities );
            em.setBoneRelationLinks( postureEntity, boneEntities, paths, isEnableds );
            em.removeBoneRelationLinks( boneEntities, isEnableds );

            em.SetComponentData( postureEntity, new PostureLinkData { BoneRelationTop = boneEntities[ 0 ] } );
            em.SetComponentData( postureEntity, new Rotation { Value = quaternion.identity } );
            em.SetComponentData( postureEntity, new Translation { Value = float3.zero } );
        }


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
                typeof( DrawTransformLinkData ),
                typeof( DrawTransformIndexData ),
                typeof( DrawTransformTargetWorkData ),
                typeof( BoneRelationLinkData ),
                typeof( BoneLocalValueData ),// どうしようか
                typeof( Translation ),
                typeof( Rotation )
            );


        static void setDrawComponet
            ( this EntityManager em_, IEnumerable<Entity> bonePrefabs, Entity drawInstancePrefab )
        {
            var drawModelLinker = em_.GetComponentData<DrawInstanceModeLinkData>( drawInstancePrefab );

            em_.SetComponentData(
                bonePrefabs,
                new DrawTransformLinkData
                {
                    DrawInstanceEntity = drawInstancePrefab,
                    DrawModelEntity = drawModelLinker.DrawModelEntity,
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
            ( this EntityManager em_, IEnumerable<Entity> bonePreafabs_ )
        {
            var boneLength = bonePreafabs_.Count();

            em_.SetComponentData( bonePreafabs_,
                from i in Enumerable.Range(0, boneLength)
                select new DrawTransformIndexData { BoneLength = boneLength, BoneId = i }
            );
        }

        static void setBoneRelationLinks(
            this EntityManager em,
            Entity postureEntity,
            IEnumerable<Entity> boneEntities,
            IEnumerable<string> paths,
            IEnumerable<bool> isEnableds
        )
        {
            var qEnabledParentName =
                from x in (isEnableds, paths).Zip( (x,y) => (isEnabled:x, path:y) )
                where x.isEnabled
                let parentPath = System.IO.Path.GetDirectoryName( x.path )
                let parentName = System.IO.Path.GetFileName( parentPath )
                select parentName
                ;

            var qEnabledEntity =
                from x in (isEnableds, boneEntities).Zip( ( x, y ) => (isEnabled: x, ent: y) )
                where x.isEnabled
                select x.ent
                ;

            var qNameAndEntity =
                from x in (paths, boneEntities).Zip( ( x, y ) => (path: x, ent: y) )
                let name = System.IO.Path.GetFileName( x.path )
                select (name, x.ent)
                ;
            qNameAndEntity = qNameAndEntity.Prepend( ("", postureEntity) );// ルートを追加


            var qParentEnt =
                from parentName in qEnabledParentName
                join bone in qNameAndEntity
                    on parentName equals bone.name
                select bone.ent
                ;

            var qNextEnt = qEnabledEntity
                .Skip( 1 )
                .Append( Entity.Null );


            var qBoneLinker =
                from x in (qParentEnt, qNextEnt).Zip((x,y) => (parent:x, next:y) )
                select new BoneRelationLinkData
                {
                    ParentBoneEntity = x.parent,
                    NextBoneEntity = x.next,
                };

            em.SetComponentData( boneEntities, qBoneLinker );
        }

        // チャンクが別になるから、消さないほうがいい可能性もあり
        static void removeBoneRelationLinks
            ( this EntityManager em, IEnumerable<Entity> boneEntities, IEnumerable<bool> isEnableds )
        {
            var qDisEnables =
                from x in (boneEntities, isEnableds).Zip( (x,y) => (bone:x, isEnable:y) )
                where !x.isEnable
                select x.bone
                ;
            foreach( var x in qDisEnables )
            {
                em.RemoveComponent<BoneRelationLinkData>( x );
                em.RemoveComponent<BoneStream0LinkData>( x );
            }

        }

        
    }

}

