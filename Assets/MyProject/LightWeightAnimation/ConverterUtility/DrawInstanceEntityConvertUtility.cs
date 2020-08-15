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
    using Abarabone.Authoring;
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
        public float distance;
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
        static public (GameObject go, Mesh mesh)[] GetMeshesToCreateModelEntity
            (
                this GameObjectConversionSystem gcs_,
                GameObject main_, GameObject[] lods_,
                Func<(GameObject, Func<MeshElements>)[]> getMeshCombineFuncs
            )
        {
            var preBakedMeshes = new List<(GameObject go, Mesh mesh)>(2);
            if (lods_.Length >= 1) (lods_[0], gcs_.GetFromStructureMeshDictionary(lods_[0])).AddTo(preBakedMeshes);
            if (lods_.Length >= 2) (lods_[1], gcs_.GetFromStructureMeshDictionary(lods_[1])).AddTo(preBakedMeshes);
            if (lods_.Length == 0) (main_, gcs_.GetFromStructureMeshDictionary(main_)).AddTo(preBakedMeshes);

            var vailedMeshes = preBakedMeshes
                .Where(x => x.mesh != null)
                .ToArray();

            if (vailedMeshes.Length > 0) return vailedMeshes;


            var lodCombineFuncs = getMeshCombineFuncs();

            var qBakedMesh = lodCombineFuncs
                .Select(x => x().CreateMesh());

            var isNoNeedDefaultMesh =
                lodCombineFuncs.Length != 0
                &&
                lodCombineFuncs.Length == lods_.Length;
            if (isNoNeedDefaultMesh) return qBakedMesh.ToArray();

            return qBakedMesh
                .Append(main_.GetComponentInChildren<MeshFilter>().sharedMesh)
                .ToArray();
        }
        static void AddTo(this (GameObject, Mesh) x, List<(GameObject, Mesh)> list) => list.Add(x);


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
            if (model != null) return model;

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
        static public void AddLodComponentToDrawInstanceEntity
            (
                this GameObjectConversionSystem gcs_,
                Entity drawInstance_, GameObject main_, ObjectAndDistance[] lods_
            )
        {
            if (lods_.Length == 0) return;

            var lod0_ = (lods_.Length >= 1 ? lods_[0].objectTop : null) ?? main_;
            var lod1_ = (lods_.Length >= 2 ? lods_[1].objectTop : null) ?? main_;

            var em = gcs_.DstEntityManager;

            em.AddComponentData(drawInstance_,
                new DrawInstance.ModelLod2LinkData
                {
                    DrawModelEntity0 = gcs_.GetFromModelEntityDictionary(lod0_),
                    DrawModelEntity1 = gcs_.GetFromModelEntityDictionary(lod1_),
                    SqrDistance0 = lods_[0].distance,
                    SqrDistance1 = lods_[1].distance,
                }
            );
        }

    }

}
