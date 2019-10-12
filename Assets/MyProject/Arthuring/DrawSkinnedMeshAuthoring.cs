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
        

        public Shader Shader;

        public override Entity Convert( EntityManager em, DrawMeshResourceHolder drawres, PrefabSettingsAuthoring.PrefabCreators creators )
        {
            
            var mrs = this.GetComponentsInChildren<SkinnedMeshRenderer>();
            var motionClip = this.GetComponentInChildren<MotionAuthoring>().MotionClip;//
            var mesh = combineAndConvertMesh( mrs, motionClip );//

            var mat = new Material( mrs[0].sharedMaterial );
            mat.shader = this.Shader;
            mat.enableInstancing = true;

            var drawIndex = drawres.Units.Count;
            drawres.AddDrawMeshResource( mesh, mat );

            return creators.Draw.CreatePrefab( em, drawIndex );




            // メッシュを結合する
            Mesh combineAndConvertMesh( IEnumerable<SkinnedMeshRenderer> mrs_, MotionClip motionClip_ )
            {
                var qCis =
                    from mr in mrs_
                    select new CombineInstance
                    {
                        mesh = mr.sharedMesh
                    }
                    ;

                //return ChMeshConverter.ConvertToChMesh( mrs_.ElementAt( 0 ).sharedMesh, motionClip_ );

                var dstmesh = new Mesh();

                dstmesh.CombineMeshes( qCis.ToArray(), mergeSubMeshes: true, useMatrices: false );
                dstmesh.boneWeights = (
                    from w in dstmesh.boneWeights
                    select new BoneWeight
                    {
                        boneIndex0 = w.boneIndex0 % motionClip_.StreamPaths.Length,
                        boneIndex1 = w.boneIndex1 % motionClip_.StreamPaths.Length,
                        boneIndex2 = w.boneIndex2 % motionClip_.StreamPaths.Length,
                        boneIndex3 = w.boneIndex3 % motionClip_.StreamPaths.Length,
                        weight0 = w.weight0,
                        weight1 = w.weight1,
                        weight2 = w.weight2,
                        weight3 = w.weight3,
                    }
                )
                .ToArray();

                return ChMeshConverter.ConvertToChMesh( dstmesh, motionClip_ );
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
                    typeof( DrawModelIndexData ),
                    typeof( Prefab )
                );
        }


        public Entity CreatePrefab( EntityManager em, int index )
        {
            var ent = em.CreateEntity( this.prefabArchetype );

            em.SetComponentData( ent,
              new DrawModelIndexData
              {
                  modelIndex = index,
                  instanceIndex = 0,
              }
            );

            return ent;
        }
    }

}

