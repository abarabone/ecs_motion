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
                (Dictionary<GameObject, Mesh> meshDictionary = null, TextureAtlasAndParameter tex = default)
                where TIdx : struct, IIndexUnit<TIdx>
                where TVtx : struct, IVertexUnit<TVtx>
            =>
                (null, null);

            public virtual IEnumerable<GameObject> QueryMeshTopObjects() => new GameObject[0];
        }


        public ModelAuthoringBase[] ModelPrefabs;

        //public bool MakeTexutreAtlus;


        public void DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
        {
            referencedPrefabs.AddRange( this.ModelPrefabs.Select(x => x.gameObject) );
        }


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {
            var meshDict = conversionSystem.GetMeshDictionary();
            var atlasDict = conversionSystem.GetTextureAtlasDictionary();

            //var prefabsDistinct = this.ModelPrefabs
            //    .Select(x => x.gameObject)
            //    .Distinct()
            //    .ToArray();

            //var prefabModelDisticts = this.ModelPrefabs
            //    .Distinct()
            //    .Where(x => !meshDict.ContainsKey(x.gameObject))
            //    .ToArray();

            var qObj =
                from model in this.ModelPrefabs.Distinct()
                from objtop in model.QueryMeshTopObjects()
                where !meshDict.ContainsKey(objtop)
                select objtop
                ;
            var objs = qObj.ToArray();


            // atlas
            var qMat =
                from obj in objs
                from r in obj.GetComponentsInChildren<Renderer>()
                from mat in r.sharedMaterials
                select mat
                ;

            var tex = qMat.QueryUniqueTextures().ToAtlasAndParameter();

            atlasDict.texHashToUvRect[tex.texhashes] = tex.uvRects;


            // mesh
            var qSrc =
                from obj in objs
                let mmt = obj.QueryMeshMatsTransform_IfHaving()
                select (obj, mmt)
                ;
            var srcs = qSrc.ToArray();

            var qMeshSrc =
                from src in srcs
                where !src.mmt.IsSingle()
                select src.mmt.BuildCombiner<UI32, PositionNormalUvVertex>(src.obj.transform, tex).ToTask()
                ;
            var qMesh = qMeshSrc
                .WhenAll().Result
                .Select(x => x.CreateMesh())
                .ToArray();

            foreach (var obj in qObj)
            {
                atlasDict.objectToAtlas[obj] = tex.atlas;
            }
            foreach (var (obj, mesh) in (qObj, qMesh).Zip())
            {
                meshDict[obj] = mesh;
            }


            // モデルグループ自体にはエンティティは不要
            dstManager.DestroyEntity(entity);

            return;

        }
        

    }


}
