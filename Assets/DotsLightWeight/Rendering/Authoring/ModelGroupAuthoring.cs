using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Abarabone.Model.Authoring
{
    using Abarabone.Geometry;
    using Abarabone.Draw;
    using Abarabone.Utilities;
    using Abarabone.Common.Extension;


    /// <summary>
    /// モデル描画をまとめる工夫を行う。
    /// テクスチャアトラスとか。
    /// できればシェーダーでも描画順をまとめいたいけど、unity はマテリアル単位の設定なので無理かな…？
    /// unity が自動的にやってくれてる可能性もある。
    /// </summary>
    [UpdateAfter(typeof(GameObjectBeforeConversionGroup))]
    public class ModelGroupAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {

        /// <summary>
        /// これを継承しないと、モデルビヘイビアを要求するものに登録できない。
        /// </summary>
        public class ModelAuthoringBase : MonoBehaviour
        { }


        public ModelAuthoringBase[] ModelPrefabs;

        public bool MakeTexutreAtlus;


        public void DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
        {
            referencedPrefabs.AddRange( this.ModelPrefabs.Select(x => x.gameObject) );
        }


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            if (this.MakeTexutreAtlus)
            {
                var tex =
                    this.GetComponentsInChildren<Transform>()
                    .Select(x => x.gameObject)
                    .PackTextureAndToHashToUvRectDict();

                var holder = conversionSystem.GetTextureAtlasHolder();
                foreach (var prefab in this.ModelPrefabs)
                {
                    holder.objectToAtlas.Add(prefab.gameObject, tex.atlas);
                }

            }

            // モデルグループ自体にはエンティティは不要
            dstManager.DestroyEntity( entity );

            return;


            void setAtlasForObject_(IEnumerable<ModelAuthoringBase> prefabs, Texture2D atlas)
            {
                var holder = conversionSystem.GetTextureAtlasHolder();
                foreach (var prefab in prefabs)
                {
                    holder.objectToAtlas.Add(prefab.gameObject, atlas);
                }
            }
            void addUvRectsToDictionary_(IEnumerable<ModelAuthoringBase> prefabs, Texture2D atlas)
            {
                var holder = conversionSystem.GetTextureAtlasHolder();
                foreach (var prefab in prefabs)
                {
                    holder.objectToAtlas.Add(prefab.gameObject, atlas);
                }
            }
        }
        

    }


}
