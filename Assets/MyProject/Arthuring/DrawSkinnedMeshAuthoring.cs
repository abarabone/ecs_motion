using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Charactor;

namespace Abss.Arthuring
{
    class DrawSkinnedMeshAuthoring : PrefabSettingsAuthoring.ConvertToMainCustomPrefabEntityBehaviour
    {

        // 構築前にメッシュ結合墨
        // 同じカテゴリのテクスチャを結合（最初は全部結合でいい）
        // 描画システムにメッシュ、マテリアル、バッファを追加する

        public MotionAuthoring MotionAuthor;


        public override Entity Convert( EntityManager em, DrawMeshResourceHolder drawres, PrefabSettingsAuthoring.PrefabCreators creators )
        {
            
            var mrs = this.GetComponentsInChildren<SkinnedMeshRenderer>();

            var mesh = combineAndConvertMesh( mrs, this.MotionAuthor.MotionClip );//
            var mat = mrs[0].sharedMaterial;

            drawres.AddDrawMeshResource( mesh, mat );

            return creators.Draw.CreatePrefab( em );




            // メッシュを結合する
            Mesh combineAndConvertMesh( IEnumerable<SkinnedMeshRenderer> mrs_, MotionClip motionClip )
            {
                var qCis =
                    from mr in mrs_
                    select new CombineInstance
                    {
                        mesh = mr.sharedMesh
                    }
                    ;

                var dstmesh = new Mesh(); Debug.Log( mrs_.Count() );

                dstmesh.CombineMeshes( qCis.ToArray(), mergeSubMeshes: true, useMatrices: false );
                dstmesh.boneWeights = (
                    from w in dstmesh.boneWeights
                    select new BoneWeight
                    {
                        boneIndex0 = w.boneIndex0 % motionClip.StreamPaths.Length,
                        boneIndex1 = w.boneIndex1 % motionClip.StreamPaths.Length,
                        boneIndex2 = w.boneIndex2 % motionClip.StreamPaths.Length,
                        boneIndex3 = w.boneIndex3 % motionClip.StreamPaths.Length,
                        weight0 = w.weight0,
                        weight1 = w.weight1,
                        weight2 = w.weight2,
                        weight3 = w.weight3,
                    }
                )
                .ToArray();

                return ChMeshConverter.ConvertToChMesh( dstmesh, motionClip );
            }
        }

    }

    

    public class DrawMeshPrefabCreator
    {
        
        EntityArchetype prefabArchetype;



        public DrawMeshPrefabCreator( EntityManager em )
        {
            
            this.prefabArchetype = em.CreateArchetype
                (
                    
                    typeof( Prefab )
                );
        }


        public Entity CreatePrefab( EntityManager em )
        {
            return Entity.Null;
        }
    }

}

