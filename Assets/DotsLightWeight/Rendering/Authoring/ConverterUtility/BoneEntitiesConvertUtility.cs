﻿using System.Collections;
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
    using CharacterMotion;
    using Draw;
    using Character;
    

    using Abarabone.Common.Extension;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Unity.Physics;
    using Unity.Physics.Authoring;

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

    public enum EnBoneType
    {
        reelup_chain,
        jobs_per_depth,
    }

    static public class BoneEntitiesConvertUtility
    {


        // bones の GameObject に関連付けられた Entity に bone 関連コンポーネントを付与し、また親子構造をもとにしたリンク構造のコンポーネントを付加する。
        // またルートボーンは、mainGameObject に関連付けられた Entity を親としてリンクを張る。
        // bones の親子構造は、bones に含まれるエントリの中で構築される。エントリの中から親が見つからない場合はルートボーンとみなされ、親は mainGameObject となる。
        // root 
        static public void InitBoneEntities
            (this GameObjectConversionSystem gcs, GameObject mainGameObject, IEnumerable<Transform> bones, Transform root, EnBoneType boneMode)
        {

            if (boneMode == EnBoneType.reelup_chain)
                gcs.initBoneEntitiesChain(mainGameObject, bones, root);

            if (boneMode == EnBoneType.jobs_per_depth)
                gcs.initBoneEntitiesLeveled(mainGameObject, bones, root);

        }


        // - - - - - - - - - - - - - - - - - - - - -

        static void initBoneEntitiesChain
            ( this GameObjectConversionSystem gcs, GameObject mainGameObject, IEnumerable<Transform> bones, Transform root )
        {
            var em = gcs.DstEntityManager;

            var postureEntity = gcs.GetPrimaryEntity(mainGameObject);
            var boneEntities = addComponentsToBoneEntities( gcs, bones, addtypeComponentsChain_());

            addMainEntityLinkForCollider(gcs, mainGameObject, bones);

            initLocalPosition(em, boneEntities, mainGameObject, bones);

            var paths = queryBonePath_(bones, root);//.Do(x=>Debug.Log($"@ {x}"));
            setBoneRelationLinksChain(em, postureEntity, boneEntities, paths );
        }

        static void initBoneEntitiesLeveled
            (this GameObjectConversionSystem gcs, GameObject mainGameObject, IEnumerable<Transform> bones, Transform root)
        {
            var em = gcs.DstEntityManager;

            var postureEntity = gcs.GetPrimaryEntity(mainGameObject);
            var boneEntities = addComponentsToBoneEntities(gcs, bones, addtypeComponentsLeveled_());

            addMainEntityLinkForCollider(gcs, mainGameObject, bones);

            initLocalPosition(em, boneEntities, mainGameObject, bones);

            var paths = queryBonePath_(bones, root);//.Do(x=>Debug.Log($"@ {x}"));
            setBoneLinksLeveled(em, postureEntity, boneEntities, paths);
        }



        // ----------------------------------------------------------------------------------

        static IEnumerable<string> queryBonePath_(IEnumerable<Transform> bones_, Transform root_)
        {
            return (
                from bone in bones_
                where !bone.name.StartsWith("_")
                select bone.gameObject.MakePath(root_.gameObject)
            )
            ;//.Do(x=>Debug.Log("~=) "+x));
        }


        static void addMainEntityLinkForCollider
            (GameObjectConversionSystem gcs, GameObject main, IEnumerable<Transform> bones)
        {
            var em = gcs.DstEntityManager;
            var mainEntity = gcs.GetPrimaryEntity(main);

            var qBoneWithCollider = bones
                .Where(bone => bone.GetComponent<PhysicsBodyAuthoring>() != null)
                .Select(bone => gcs.TryGetPrimaryEntity(bone));

            em.AddComponentData(qBoneWithCollider,
                new Bone.MainEntityLinkData
                {
                    MainEntity = mainEntity,
                }
            );
        }



        /// <summary>
        /// ゲームオブジェクトとしての子オブジェクトは、変換プロセスにて、すべてエンティティとして生成されるようす。
        /// physics もそれをあてにするようなので、ボーンもそれに乗っかろうと思う。
        /// </summary>
        static Entity[] addComponentsToBoneEntities
            ( GameObjectConversionSystem gcs, IEnumerable<Transform> bones, ComponentTypes addtypes )
        {
            var em = gcs.DstEntityManager;

            return bones
                .Select( bone => gcs.TryGetPrimaryEntity( bone ) )
                //.Select( existsEnt => existsEnt != Entity.Null ? addComponents_(existsEnt) : create_() )
                .Do( ent => em.AddComponents(ent, addtypes) )
                .ToArray();
        }
        static ComponentTypes addtypeComponentsChain_()
        {
            return new ComponentTypes
            (
                //typeof( DrawTransform.LinkData ),
                //typeof( DrawTransform.IndexData ),
                //typeof( DrawTransform.TargetWorkData ),
                typeof(Bone.RelationLinkData),
                typeof(Bone.LocalValueData),// 回転と移動をわけたほうがいいか？
                typeof(Translation),
                typeof(Rotation)
            );
        }
        static ComponentTypes addtypeComponentsLeveled_()
        {
            return new ComponentTypes
            (
                //typeof( DrawTransform.LinkData ),
                //typeof( DrawTransform.IndexData ),
                //typeof( DrawTransform.TargetWorkData ),
                typeof(Bone.LocalValueData),// 回転と移動をわけたほうがいいか？
                typeof(Translation),
                typeof(Rotation)
            );
        }


        static void setBoneRelationLinksChain
        (
            EntityManager em, Entity postureEntity,
            IEnumerable<Entity> boneEntities, IEnumerable<string> paths
        )
        {
            var pathArray = paths.ToArray();
            var boneEntityArray = boneEntities.ToArray();

            var pathToEntDict =
                (pathArray, boneEntityArray).Zip( ( x, y ) => (path: x, ent: y))
                .Where(x => x.path != "")
                //.Do(x => Debug.Log($"x {x.path}"))
                .Append( (path: "", ent: postureEntity) )
                .Append( (path: "\0", ent: Entity.Null) )
                .ToDictionary( x => x.path, x => x.ent );

            var qParentPath = pathArray
                .Select( x => x.GetParentPath() );

            var qNextPath = pathArray
                .Skip( 1 )
                .Append( "\0" );

            var qBoneLinker =
                from path in (qParentPath, qNextPath).Zip( ( x, y ) => (parent: x, next: y) )
                select new Bone.RelationLinkData
                {
                    ParentBoneEntity = pathToEntDict[ path.parent ],
                    NextBoneEntity = pathToEntDict[ path.next ],
                };

            em.SetComponentData( boneEntityArray, qBoneLinker );
        }

        static void setBoneLinksLeveled
        (
            EntityManager em, Entity postureEntity,
            IEnumerable<Entity> boneEntities, IEnumerable<string> paths
        )
        {
            var pathArray = paths.ToArray();
            var boneEntityArray = boneEntities.ToArray();

            var pathToEntDict =
                (pathArray, boneEntityArray).Zip((x, y) => (path: x, ent: y))
                .Where(x => x.path != "")
                //.Do(x => Debug.Log($"x {x.path}"))
                .Append((path: "", ent: postureEntity))
                .Append((path: "\0", ent: Entity.Null))
                .ToDictionary(x => x.path, x => x.ent);

            var qParent = pathArray
                .Select(x => x.GetParentPath())//.Do(x => Debug.Log($"w {x}"))
                .Select(path => pathToEntDict[path]);

            var qPathDepthCount =
                from path in pathArray
                select path.Where(x => x == '/').Count() + 1
                ;

            foreach( var (ent, parent, depth) in (boneEntityArray, qParent, qPathDepthCount).Zip() )
            {
                setLvLinker(ent, parent, depth);
            }

            return;


            void setLvLinker(Entity ent, Entity parent, int lv)
            {
                switch (lv)
                {
                    case 1: em.AddComponentData(ent, new Bone.Lv01LinkData { ParentBoneEntity = parent }); break;
                    case 2: em.AddComponentData(ent, new Bone.Lv02LinkData { ParentBoneEntity = parent }); break;
                    case 3: em.AddComponentData(ent, new Bone.Lv03LinkData { ParentBoneEntity = parent }); break;
                    case 4: em.AddComponentData(ent, new Bone.Lv04LinkData { ParentBoneEntity = parent }); break;
                    case 5: em.AddComponentData(ent, new Bone.Lv05LinkData { ParentBoneEntity = parent }); break;
                    case 6: em.AddComponentData(ent, new Bone.Lv06LinkData { ParentBoneEntity = parent }); break;
                    case 7: em.AddComponentData(ent, new Bone.Lv07LinkData { ParentBoneEntity = parent }); break;
                    case 8: em.AddComponentData(ent, new Bone.Lv08LinkData { ParentBoneEntity = parent }); break;
                    case 9: em.AddComponentData(ent, new Bone.Lv09LinkData { ParentBoneEntity = parent }); break;
                }
            }
        }

        static void initLocalPosition
            (EntityManager em, IEnumerable<Entity> boneEntities, GameObject mainGameObject, IEnumerable<Transform> bones)
        {
            var mtInv = mainGameObject.transform.worldToLocalMatrix;

            var qLocal =
                from bn in bones
                let lpos = bn.transform.localPosition//mtInv.MultiplyPoint(bn.transform.position)
                let lrot = bn.transform.localRotation//bn.transform.rotation * mtInv.rotation
                select new Bone.LocalValueData
                {
                    Position = lpos,
                    Rotation = lrot,
                };

            foreach( var (ent, local) in (boneEntities, qLocal).Zip() )
            {
                em.SetComponentData(ent, local);
            }
        }
    }
}
