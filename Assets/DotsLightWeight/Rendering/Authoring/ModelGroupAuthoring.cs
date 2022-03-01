using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine.Serialization;

namespace DotsLite.Model.Authoring
{
    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.Common.Extension;
    using DotsLite.Draw.Authoring;


    /// <summary>
    /// モデル描画をまとめる工夫を行う。
    /// テクスチャアトラスとか。
    /// できればシェーダーでも描画順をまとめいたいけど、unity はマテリアル単位の設定なので無理かな…？
    /// unity が自動的にやってくれてる可能性もある。
    /// </summary>
    //[UpdateAfter(typeof(GameObjectBeforeConversionGroup))]
    [UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    public class ModelGroupAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {

        /// <summary>
        /// これを継承しないと、モデルビヘイビアを要求するものに登録できない。
        /// </summary>
        public abstract class ModelAuthoringBase : MonoBehaviour
        {
            //[FormerlySerializedAs("OriginId")]
            //public int SourcePrefabKey;
            //public void SetOridinId() => this.SourcePrefabKey = new System.Random(this.name.GetHashCode() + this.GetHashCode()).Next();
            protected void Reset()
            {
#if UNITY_EDITOR
                var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromOriginalSource(this);
                (prefab ?? this).QueryModel.ForEach(m => m.GenerateSourcePrefabKey());
#endif
            }

            public virtual IEnumerable<IMeshModel> QueryModel => throw new NotImplementedException();
        }


        public ModelAuthoringBase[] ModelPrefabs;

        //public bool MakeTexutreAtlus;


        public void DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
        {
            if (!this.isActiveAndEnabled) return;

            referencedPrefabs.AddRange(this.ModelPrefabs.Select(x => x.gameObject));
        }


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {
            if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


            this.ModelPrefabs
                .Distinct()
                .BuildModelToDictionary(conversionSystem);


            // モデルグループ自体にはエンティティは不要
            dstManager.DestroyEntity(entity);

        }


    }


}
