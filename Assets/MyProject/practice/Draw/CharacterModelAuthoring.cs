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

        //public AvatarMask BoneMask;

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
            var paths = queryBonePath_( skinnedMeshRenderer ).ToArray();

            //paths.ForEach( x => Debug.Log( x ) );
            BoneEntitiesCreator.CreateEntities( conversionSystem, dstManager, this.gameObject, entity, paths );

            return;


            IEnumerable<string> queryBonePath_( SkinnedMeshRenderer smr_ )
            {
                return
                    from bone in smr_.bones
                    where !bone.name.StartsWith( "_" )
                    select bone.gameObject.MakePath( smr_.rootBone.gameObject )
                    ;
            }
        }

    }


    static public class BoneEntitiesCreator
    {


        static public void CreateEntities
        (
            GameObjectConversionSystem gcs, EntityManager em, GameObject mainGameObject,
            Entity drawInstancePrefab, IEnumerable<string> paths
        )
        {
            var postArchetype = em.postureArchetypeCache();
            var boneArchetype = em.boneArchetypeCache();

            var postureEntity = gcs.CreateAdditionalEntity( mainGameObject, em, postArchetype );
            var boneEntities = gcs.CreateAdditionalEntities( mainGameObject, em, boneArchetype, paths.Count() );
            
            em.setBoneId( boneEntities );
            em.setBoneRelationLinks( postureEntity, boneEntities, paths );

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


        //static void setDrawComponet
        //    ( this EntityManager em_, IEnumerable<Entity> boneEntities_, Entity drawInstancePrefab )
        //{
        //    var drawModelLinker = em_.GetComponentData<DrawInstanceModeLinkData>( drawInstancePrefab );

        //    em_.SetComponentData(
        //        boneEntities_,
        //        new DrawTransformLinkData
        //        {
        //            DrawInstanceEntity = drawInstancePrefab,
        //            DrawModelEntity = drawModelLinker.DrawModelEntity,
        //        }
        //    );
        //}

        
        static void setBoneId
            ( this EntityManager em_, IEnumerable<Entity> boneEntities_ )
        {
            var boneLength = boneEntities_.Count();

            em_.SetComponentData( boneEntities_,
                from i in Enumerable.Range(0, boneLength)
                select new DrawTransformIndexData { BoneLength = boneLength, BoneId = i }
            );
        }

        static void setBoneRelationLinks
        (
            this EntityManager em, Entity postureEntity,
            IEnumerable<Entity> boneEntities, IEnumerable<string> paths
        )
        {
            var pathToEntDict =
                (paths, boneEntities).Zip( ( x, y ) => (path: x, ent: y) )
                .Append( (path: "", ent: postureEntity) )
                .Append( (path: "\0", ent: Entity.Null) )
                .ToDictionary( x => x.path, x => x.ent );

            var qParentPath = paths
                .Select( x => x.GetParentPath() );

            var qNextPath = paths
                .Skip( 1 )
                .Append( "\0" );
            
            var qBoneLinker =
                from path in (qParentPath, qNextPath).Zip((x,y) => (parent:x, next:y) )
                select new BoneRelationLinkData
                {
                    ParentBoneEntity = pathToEntDict[path.parent],
                    NextBoneEntity = pathToEntDict[path.next],
                };

            em.SetComponentData( boneEntities, qBoneLinker );
        }
        
    }

}

