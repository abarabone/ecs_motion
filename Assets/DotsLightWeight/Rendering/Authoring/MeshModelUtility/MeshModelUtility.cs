using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
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
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;

    public static class MeshModelUtility
    {

        //public static IEnumerable<GameObject> Objs(this IEnumerable<IMeshModel> models) =>
        //    models.Select(x => x.Obj);

        //public static IEnumerable<SourcePrefabKeyUnit> Keys(this IEnumerable<IMeshModel> models) =>
        //    models.Select(x => x.SourcePrefabKey);



        public static void BuildModelToDictionary<T>(this IEnumerable<T> models, GameObjectConversionSystem gcs)
            where T : ModelGroupAuthoring.ModelAuthoringBase
        {

            var meshDict = gcs.GetMeshDictionary();
            var atlasDict = gcs.GetTextureAtlasDictionary();
            
            var meshmodels = models
                .SelectMany(model => model.QueryModel)
                .Distinct(x => x.SourcePrefabKey)
                .ToArray();

            meshmodels.PackTextureToDictionary(atlasDict);

            meshmodels.CreateModelToDictionary(meshDict, atlasDict);

            meshmodels.CreateModelEntitiesToDictionary(gcs, meshDict, atlasDict);
        }

        public static void BuildModelToDictionary(this IEnumerable<IMeshModel> models, GameObjectConversionSystem gcs)
        {
            var meshDict = gcs.GetMeshDictionary();
            var atlasDict = gcs.GetTextureAtlasDictionary();

            var meshmodels = models
                .Distinct(x => x.SourcePrefabKey)
                .ToArray();
            meshmodels.PackTextureToDictionary(atlasDict);
            meshmodels.CreateModelToDictionary(meshDict, atlasDict);
            meshmodels.CreateModelEntitiesToDictionary(gcs, meshDict, atlasDict);
        }



        public static void CreateModelToDictionary(
            this IEnumerable<IMeshModel> models,
            Dictionary<SourcePrefabKeyUnit, Mesh> meshDict, TextureAtlasDictionary.Data atlasDict)
        {
            var qMmts =
                from model in models
                    //.Do(x => Debug.Log($"srckey {x.SourcePrefabKey.Value} at {x.Obj.name}"))
                select model.QueryMmts
                ;
            using var meshAll = qMmts.QueryMeshDataFromModel();

            var qOfs =
                from x in (meshAll.AsEnumerable, models).Zip()
                    //.Do(x => Debug.Log($"create from {x.src1.Obj?.name} {x.src1.Obj?.GetHashCode()} {meshDict.ContainsKey(x.src1?.Obj)}"))
                let meshsrc = x.src0
                let model = x.src1
                where !meshDict.ContainsKey(model.SourcePrefabKey)
                select model.BuildMeshCombiner(meshsrc, meshDict, atlasDict);
            var ofs = qOfs.ToArray();

            var qKey = ofs.Select(x => x.key);
            var qMesh = ofs.Select(x => x.f.ToTask())
                .WhenAll().Result
                .Select(x => x.CreateMesh());
            //var qMesh = ofs.Select(x => x.f().CreateMesh());
            meshDict.AddRange(qKey, qMesh);
        }



        public static void CreateModelEntitiesToDictionary(
            this IEnumerable<IMeshModel> models,
            GameObjectConversionSystem gcs,
            Dictionary<SourcePrefabKeyUnit, Mesh> meshDict, TextureAtlasDictionary.Data atlasDict)
        {
            foreach (var model in models)
            {
                //Debug.Log($"{model.Obj.name} model ent");
                if (gcs.IsExistsInModelEntityDictionary(model.SourcePrefabKey)) continue;
                //Debug.Log($"create {model.Obj.name}");

                var key = model.SourcePrefabKey;

                var mesh = meshDict[key];
                var atlas = atlasDict.srckeyToAtlas[key];
                var modelEntity = model.CreateModelEntity(gcs, mesh, atlas);

                gcs.AddToModelEntityDictionary(key, modelEntity);
            }
        }

    }
}
