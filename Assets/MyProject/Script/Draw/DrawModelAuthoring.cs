using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace aaa.Draw.Arthuring
{
    [DisallowMultipleComponent]
    class DrawSkinnedMeshAuthoring : MonoBehaviour
    {

        // 構築前にメッシュ結合墨
        // 同じカテゴリのテクスチャを結合（最初は全部結合でいい）
        // 描画システムにメッシュ、マテリアル、バッファを追加する


        public Shader Shader = null;

        public int MaxInstance = 1000;
        
        //public BoneType BoneType = BoneType.TR;




    }
}