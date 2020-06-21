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

    static public class BoneEntitiesConvertUtility
    {

        static public void CreateEntities
            ( this GameObjectConversionSystem gcs, EntityManager em, GameObject mainGameObject, IEnumerable<Transform> bones )
        {
            var postArchetype = em.postureArchetype();
            var boneArchetype = em.boneArchetype();

            var postureEntity = gcs.CreateAdditionalEntity( em, mainGameObject, postArchetype );
            var boneEntities = gcs.getOrCreateBoneEntities( em, mainGameObject, bones, boneArchetype );

            var paths = queryBonePath_( bones ).ToArray();
            em.setBoneRelationLinks( postureEntity, boneEntities, paths );

            em.setPostureValue( postureEntity, boneEntities.First() );
        }

        static Entity[] getOrCreateBoneEntities
            ( this GameObjectConversionSystem gcs, EntityManager em, GameObject main, IEnumerable<Transform> bones, EntityArchetype arche )
        {
            return bones
                .Select( bone => gcs.TryGetPrimaryEntity( bone ) )
                .Select( existsEnt => existsEnt != Entity.Null ? addComponents_( existsEnt ) : create_() )
                .ToArray()
                ;

            Entity addComponents_( Entity exists_ )
            {
                var addtype = new ComponentTypes( typeof( BoneRelationLinkData ), typeof( BoneLocalValueData ) );
                em.AddComponents( exists_, addtype );
                return exists_;
            }
            Entity create_() => gcs.CreateAdditionalEntity( em, main, arche );
        }


        static EntityArchetype postureArchetype( this EntityManager em ) =>
            em.CreateArchetype
            (
                typeof( PostureNeedTransformTag ),
                typeof( PostureLinkData ),
                typeof( Translation ),
                typeof( Rotation )
            );

        static EntityArchetype boneArchetype( this EntityManager em ) =>
            em.CreateArchetype
            (
                //typeof( DrawTransformLinkData ),
                //typeof( DrawTransformIndexData ),
                //typeof( DrawTransformTargetWorkData ),
                typeof( BoneRelationLinkData ),
                typeof( BoneLocalValueData ),// 回転と移動をわけたほうがいいか？
                typeof( Translation ),
                typeof( Rotation )
            );


        static IEnumerable<string> queryBonePath_( IEnumerable<Transform> bones )
        {
            var root = bones.First();

            return
                from bone in bones
                where !bone.name.StartsWith( "_" )
                select bone.gameObject.MakePath( root.gameObject )
                ;
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
                from path in (qParentPath, qNextPath).Zip( ( x, y ) => (parent: x, next: y) )
                select new BoneRelationLinkData
                {
                    ParentBoneEntity = pathToEntDict[ path.parent ],
                    NextBoneEntity = pathToEntDict[ path.next ],
                };

            em.SetComponentData( boneEntities, qBoneLinker );
        }

        static void setPostureValue( this EntityManager em, Entity postureEntity, Entity boneTopEntity )
        {
            em.SetComponentData( postureEntity, new PostureLinkData { BoneRelationTop = boneTopEntity } );
            em.SetComponentData( postureEntity, new Rotation { Value = quaternion.identity } );
            em.SetComponentData( postureEntity, new Translation { Value = float3.zero } );
        }

    }
}
