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


        public override Entity Convert( EntityManager em, PrefabSettingsAuthoring.PrefabCreators creators )
        {
            
            var mr = this.GetComponentInChildren<SkinnedMeshRenderer>();

            return creators.Character.CreatePrefab( em );
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


        public Entity CreatePrefab( EntityManager em, Mesh mesh )
        {

        }
    }

}

