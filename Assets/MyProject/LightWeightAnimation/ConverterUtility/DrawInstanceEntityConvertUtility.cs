using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Draw.Authoring
{
    using Abarabone.CharacterMotion;
    using Abarabone.Draw;
    using Abarabone.Character;
    
    using Abarabone.Model.Authoring;
    using Abarabone.Structure.Authoring;//

    using Abarabone.Common.Extension;
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.Geometry;
    using Abarabone.Model;
    using Unity.Physics;

    using Material = UnityEngine.Material;


    [Serializable]
    public struct ObjectAndDistance
    {
        public GameObject objectTop;
        public float LimitDistance;
        public float Margin;
    }

    static public class DrawInstanceEntityConvertUtility
    {

        /// <summary>
        /// DrawModelEntity の生成が必要なメッシュを洗い出す。
        /// また、事前にメッシュ生成が済んでいる場合は、メッシュ辞書から取得する。
        /// ・ＬＯＤが登録されていない場合は、
        /// 　オブジェクトトップから見つかった最初のメッシュを無加工で採用する。（デフォルトメッシュ）
        /// ・ＬＯＤ枠が確保されていて、ＬＯＤにオブジェクトが登録されている場合は、
        /// 　そのオブジェクト以下の結合メッシュを採用する。
        /// ・ＬＯＤ枠が確保されていて、ＬＯＤ登録が Nothing の場合は、
        /// 　オブジェクトトップから見つかった最初のメッシュを無加工で採用する。
        /// </summary>
        static public GameObject[] BuildMeshesForModelEntity
            (
                this GameObjectConversionSystem gcs_,
                GameObject main_, GameObject[] lods_,
                Func<Func<MeshCombinerElements>[]> getMeshCombineFuncs
            )
        {
            var result = new List<GameObject>(lods_.Length);

            if(lods_.Length == 0 || lods_.Where(x => x == null).Any())
            {
                if(!gcs_.IsExistingInMeshDictionary(main_))
                {
                    var mesh = main_.GetComponentInChildren<MeshFilter>().sharedMesh;
                    gcs_.AddToMeshDictionary(main_, mesh);
                }

                result.Add(main_);
            }

            var meshfuncs = getMeshCombineFuncs();

            foreach(var (lod, f) in (lods_, meshfuncs).Zip().Where(x => x.x != null))
            {
                if (!gcs_.IsExistingInMeshDictionary(lod))
                {
                    var mesh = f().CreateMesh();
                    gcs_.AddToMeshDictionary(lod, mesh);
                }

                result.Add(lod);
            }

            return result.ToArray();
        }


        /// <summary>
        /// DrawInstanceEntity のリンクに設定すべき DrawModelEntity を返す。
        /// ＬＯＤ使用時は毎フレーム更新される値なので、初期値は適当でもよい。
        /// </summary>
        static public Entity GetDrawModelEntity
            (
                this GameObjectConversionSystem gcs_,
                GameObject main_, ObjectAndDistance[] lods_
            )
        {
            var model = gcs_.GetFromModelEntityDictionary(main_);
            if (model != Entity.Null) return model;

            return lods_
                .Select(x => x.objectTop)
                .Where(x => x != null)
                .Select(x => gcs_.GetFromModelEntityDictionary(x))
                .First();
        }


        /// <summary>
        /// 必要であれば、DrawInstanceEntity にＬＯＤコンポーネントデータを追加する。
        /// ・ＬＯＤが登録されていない場合は、コンポーネントデータを追加しない。
        /// ・ＬＯＤ枠が確保されていて、ＬＯＤにオブジェクトが登録されている場合は、
        /// 　そのオブジェクト以下の結合メッシュを採用する。
        /// ・ＬＯＤ枠が確保されていて、ＬＯＤ登録が Nothing の場合は、
        /// 　オブジェクトトップから見つかった最初のメッシュを無加工で採用する。
        /// </summary>
        static public void AddLod2ComponentToDrawInstanceEntity
            (
                this GameObjectConversionSystem gcs_,
                Entity drawInstance_, GameObject top_, ObjectAndDistance[] lods_
            )
        {
            if (lods_.Length != 2) return;

            var lod0_ = lods_[0].objectTop ?? top_;
            var lod1_ = lods_[1].objectTop ?? top_;

            var em = gcs_.DstEntityManager;

            em.AddComponentData(drawInstance_,
                new DrawInstance.ModelLod2LinkData
                {
                    DrawModelEntityNear = gcs_.GetFromModelEntityDictionary(lod0_),
                    DrawModelEntityFar = gcs_.GetFromModelEntityDictionary(lod1_),
                    LimitDistanceSqrNear = pow2_(lods_[0].LimitDistance),
                    LimitDistanceSqrFar = pow2_(lods_[1].LimitDistance),
                    MarginDistanceSqrNear = pow2_(lods_[0].LimitDistance + lods_[0].Margin),
                    MarginDistanceSqrFar = pow2_(lods_[1].LimitDistance + lods_[1].Margin),
                }
            );
            
            return;


            float pow2_(float d_) => d_ * d_;
        }

    }

}
