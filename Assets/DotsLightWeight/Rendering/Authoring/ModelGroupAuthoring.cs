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
    using Abarabone.Draw.Authoring;


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
        public class ModelAuthoringBase : MonoBehaviour
        { }


        public ModelAuthoringBase[] ModelPrefabs;

        //public bool MakeTexutreAtlus;


        public void DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
        {
            referencedPrefabs.AddRange( this.ModelPrefabs.Select(x => x.gameObject) );
        }


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            var prefabsDistinct = this.ModelPrefabs
                .Select(x => x.gameObject)
                .Distinct()
                .ToArray();


            var qMmt =
                from prefab in prefabsDistinct
                select
                    prefab.GetComponentsInChildren<Transform>()
                        .Select(x => x.gameObject)
                        .QueryMeshMatsTransform_IfHaving();
            var mmtss = qMmt.ToArray();


            var tex = mmtss
                .SelectMany()
                .SelectMany(x => x.mats)
                .QueryUniqueTextures().PackTextureAndQueryHashAndUvRect();

            var holder = conversionSystem.GetTextureAtlasHolder();
            foreach (var prefab in prefabsDistinct)
            {
                holder.objectToAtlas.Add(prefab, tex.atlas);
            }
            foreach (var (hash, rect) in (tex.texhashes, tex.uvRects).Zip())
            {
                holder.texHashToUvRect[hash.atlas, hash.part] = rect;
            }


            var qMeshfunc =
                from mmts in mmtss
                select mmts.BuildCombiner<UI32, PositionUvVertex>(mmts.First().tf, tex).ToTask()
                ;

            var meshes =
                from meshelement in qMeshfunc.WhenAll().Result
                select meshelement.CreateMesh()
                ;

            var src = (prefabsDistinct, meshes);
            foreach (var (go, mesh) in src.Zip())
            {
                conversionSystem.AddToMeshDictionary(go, mesh);
            }


            // モデルグループ自体にはエンティティは不要
            dstManager.DestroyEntity(entity);

            return;

        }
        

    }


}
