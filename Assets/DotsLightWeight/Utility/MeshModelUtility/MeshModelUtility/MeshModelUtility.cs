using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Linq;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.Geometry
{
    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;

    public static class MeshModelUtility
    {

        //public static IEnumerable<GameObject> Objs(this IEnumerable<IMeshModel> models) =>
        //    models.Select(x => x.Obj);

        //public static IEnumerable<SourcePrefabKeyUnit> Keys(this IEnumerable<IMeshModel> models) =>
        //    models.Select(x => x.SourcePrefabKey);



        /// <summary>
        /// �O���[�v�I�[�T�����O����
        /// </summary>
        public static void BuildModelToDictionary<T>(
            this IEnumerable<T> models, GameObjectConversionSystem gcs)
            where T : ModelGroupAuthoring.ModelAuthoringBase
        =>
            models
                .SelectMany(model => model.QueryModel)
                .BuildModelToDictionary(gcs);

        /// <summary>
        /// ���f���I�[�T�����O����
        /// </summary>
        public static void BuildModelToDictionary(
            this IEnumerable<IMeshModel> models, GameObjectConversionSystem gcs)
        {
            var meshDict = gcs.GetMeshDictionary();
            var atlasDict = gcs.GetTextureAtlasDictionary();

            var meshmodels = models
                .Distinct(x => x)
                .ToArray();
            meshmodels.PackTextureToDictionary(atlasDict);
            meshmodels.CreateMeshesToDictionary(meshDict, atlasDict);
            meshmodels.InitModelEntities(gcs, meshDict, atlasDict);
        }





        public static void CreateMeshesToDictionary(
            this IEnumerable<IMeshModel> models,
            Dictionary<IMeshModel, Mesh> meshDict, TextureAtlasDictionary.Data atlasDict)
        {
            var qMmts =
                from model in models
                    .Do(x => Debug.Log($"t {x.Obj.name}"))
                select model.QueryMmts
                ;
            using var meshAll = qMmts.ToArray().QueryMeshDataFromModel();

            var qOfs =
                from x in (meshAll.AsEnumerable, models).Zip()
                    .Do(x => Debug.Log($"create from {x.src1.Obj?.name} {meshDict.ContainsKey(x.src1)}"))
                let meshsrc = x.src0
                let model = x.src1
                where !meshDict.ContainsKey(model)
                select (
                    builder: model.BuildMeshCombiner(meshsrc, meshDict, atlasDict),
                    model: model as MonoBehaviour// �����Ƃ��悤
                );
            var ofs = qOfs.ToArray();

            var qModel = ofs.Select(x => x.model as IMeshModel);
            var qMesh = ofs.Select(x => x.builder.ToTask())
                .WhenAll().Result
                .Select(x => x.CreateMesh());
            meshDict.AddRange(qModel, qMesh);
            //var qModel = ofs.Select(x => x.model as IMeshModel);
            //var qMesh = ofs
            //    .Select(x => x.model.allo)
            //    .Select(x => x.builder.ToTask())
            //    .WhenAll().Result
            //    .Select(x => x.CreateMesh());
            //meshDict.AddRange(qModel, qMesh);
        }

        /// <summary>
        /// 
        /// </summary>
        public static Task<Mesh.MeshDataArray> ToTask(this Func<Mesh.MeshDataArray> f) =>
            Task.Run(f);

        //public static Task<IMeshElements> ToTask(this Func<IMeshElements> f) =>
        //    Task.Run(f);

        //public static Task<(TIdx[], TVtx[])> ToTask<TIdx, TVtx>(this Func<(TIdx[], TVtx[])> f)
        //    where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        //    where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        //=>
        //    Task.Run(f);





        public static void InitModelEntities(
            this IEnumerable<IMeshModel> models,
            GameObjectConversionSystem gcs,
            Dictionary<IMeshModel, Mesh> meshDict, TextureAtlasDictionary.Data atlasDict)
        {
            var em = gcs.DstEntityManager;

            foreach (var model in models)
            {
                //Debug.Log($"{model.Obj.name} model ent");
                var ent = gcs.GetPrimaryEntity(model.AsGameObject);
                if (em.HasComponent<Draw.DrawModel.GeometryData>(ent)) continue;
                //Debug.Log($"create {model.Obj.name}");

                var mesh = meshDict[model];
                var atlas = atlasDict.modelToAtlas[model];
                model.InitModelEntity(gcs, mesh, atlas);
            }
        }

    }
}