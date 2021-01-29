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
        {
            public virtual (GameObject[] objs, Func<MeshElements<TIdx, TVtx>>[] fs) BuildMeshCombiners<TIdx, TVtx>
                (Dictionary<GameObject, Mesh> meshDictionary = null, TextureAtlasParameter tex = default)
                where TIdx : struct, IIndexUnit<TIdx>
                where TVtx : struct, IVertexUnit<TVtx>
            =>
                (null, null);
        }


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


            var qMat =
                from prefab in prefabsDistinct
                from r in prefab.GetComponentsInChildren<Renderer>()
                from mat in r.sharedMaterials
                select mat
                ;

            var tex = qMat.QueryUniqueTextures().PackTextureAndQueryHashAndUvRect();

            var holder = conversionSystem.GetTextureAtlasHolder();
            //foreach (var prefab in prefabsDistinct)
            //{
            //    holder.objectToAtlas.Add(prefab, tex.atlas);
            //}
            foreach (var (hash, rect) in (tex.texhashes, tex.uvRects).Zip())
            {
                holder.texHashToUvRect[hash.atlas, hash.part] = rect;
            }


            var meshDict = conversionSystem.GetMeshDictionary();

            var qMeshSrc =
                from prefab in this.ModelPrefabs
                from x in prefab.BuildMeshCombiners<UI32, PositionNormalUvVertex>(meshDict, tex).Zip()
                select (obj: x.src0, f: x.src1)
                ;
            var meshsrcs = qMeshSrc.ToArray();
            var qObj = meshsrcs.Select(x => x.obj);
            var qMesh = meshsrcs
                .Select(x => x.f.ToTask())
                .WhenAll().Result
                .Select(x => x.CreateMesh());

            foreach (var obj in qObj)
            {
                holder.objectToAtlas.Add(obj, tex.atlas);
            }
            foreach (var (obj, mesh) in (qObj, qMesh).Zip())
            {
                conversionSystem.AddToMeshDictionary(obj, mesh);
            }


            // モデルグループ自体にはエンティティは不要
            dstManager.DestroyEntity(entity);

            return;

        }
        

    }


}
