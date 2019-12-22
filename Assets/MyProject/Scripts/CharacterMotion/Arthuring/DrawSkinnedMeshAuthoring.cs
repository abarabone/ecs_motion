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

namespace Abss.Arthuring
{
    [DisallowMultipleComponent]
    class DrawSkinnedMeshAuthoring : MonoBehaviour
    {

        // 構築前にメッシュ結合墨
        // 同じカテゴリのテクスチャを結合（最初は全部結合でいい）
        // 描画システムにメッシュ、マテリアル、バッファを追加する
        
        
        public Shader Shader = null;
        public int MaxInstance = 1000;
        public BoneType BoneType = BoneType.TR;
        

        public Entity Convert( EntityManager em, DrawMeshResourceHolder drawres, Action<Mesh,Material,BoneType> initDrawModelComponentsAction )
        {
            
            var mrs = this.GetComponentsInChildren<SkinnedMeshRenderer>();
            var motionClip = this.GetComponent<MotionAuthoring>().MotionClip;//
            var mesh = combineAndConvertMesh( mrs, motionClip );//

            var mat = new Material( mrs[0].sharedMaterial );
            mat.shader = this.Shader;// ちゃんとした材質生成が必要
            mat.enableInstancing = true;

            var drawIndex = drawres.AddDrawMeshResource( mesh, mat, this.BoneType, this.MaxInstance );

            return DrawMeshPrefabCreator.CreatePrefab( em, drawIndex, mesh.bindposes.Length );


            // メッシュを結合する
            Mesh combineAndConvertMesh( IEnumerable<SkinnedMeshRenderer> smrs_, MotionClip motionClip_ )
            {
                var qCis =
                    from mr in smrs_
                    select new CombineInstance
                    {
                        mesh = mr.sharedMesh
                    };

                //return ChMeshConverter.ConvertToChMesh( smrs_.ElementAt( 0 ).sharedMesh, smrs_.ElementAt(0).bones );

                var dstmesh = new Mesh();
                var boneLength = smrs_.First().bones.Length;

                // 後でちゃんとした結合に差し替えよう
                dstmesh.CombineMeshes( qCis.ToArray(), mergeSubMeshes: true, useMatrices: false );
                dstmesh.boneWeights = (
                    from w in dstmesh.boneWeights
                    select new BoneWeight
                    {
                        boneIndex0 = w.boneIndex0 % boneLength,
                        boneIndex1 = w.boneIndex1 % boneLength,
                        boneIndex2 = w.boneIndex2 % boneLength,
                        boneIndex3 = w.boneIndex3 % boneLength,
                        weight0 = w.weight0,
                        weight1 = w.weight1,
                        weight2 = w.weight2,
                        weight3 = w.weight3,
                    }
                )
                .ToArray();

                return ChMeshConverter.ConvertToChMesh( dstmesh, smrs_.ElementAt( 0 ).bones );
            }
        }

    }

    

    static public class DrawMeshPrefabCreator
    {
        

        static EntityArchetypeCache archetypeCache = new EntityArchetypeCache
        (
            em => em.CreateArchetype
            (
                typeof( DrawModelIndexData ),
                typeof( DrawInstanceTargetWorkData ),
                typeof( Prefab )
            )
        );


        static public Entity CreatePrefab( EntityManager em, int modelIndex, int boneLength )
        {
            var archetype = archetypeCache.GetOrCreateArchetype( em );

            var ent = em.CreateEntity( archetype );

            em.SetComponentData( ent,
                new DrawModelIndexData
                {
                    ModelIndex = modelIndex,
                    BoneLength = boneLength,
                }
            );

            em.SetComponentData( ent,
                new DrawInstanceTargetWorkData
                {
                    InstanceIndex = -1,
                }
            );

            return ent;
        }
    }

}

