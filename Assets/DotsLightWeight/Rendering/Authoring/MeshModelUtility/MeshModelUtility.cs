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

        public static IEnumerable<GameObject> Objs(this IEnumerable<IMeshModel> models) =>
            models.Select(x => x.Obj);



        public static void CreateMeshAndModelEntitiesWithDictionary
            (this IEnumerable<IMeshModel> models, GameObjectConversionSystem gcs)
        {
            var meshDict = gcs.GetMeshDictionary();
            var atlasDict = gcs.GetTextureAtlasDictionary();

            models.Objs().PackTextureToDictionary(atlasDict);
            models.CreateModelToDictionary(meshDict, atlasDict);
            models.CreateModelEntities(gcs, meshDict, atlasDict);
        }



        public static void CreateModelToDictionary(
            this IEnumerable<IMeshModel> models,
            Dictionary<GameObject, Mesh> meshDict, TextureAtlasDictionary.Data atlasDict)
        {
            var qMmts =
                from model in models.Do(x => Debug.Log($"srckey {x.SourcePrefabKey} at {x.Obj.name}"))
                select model.QueryMmts
                ;
            using var meshAll = qMmts.QueryMeshDataFromModel();

            var qOfs =
                from x in (meshAll.AsEnumerable, models).Zip()
                    //.Do(x => Debug.Log($"create from {x.src1.Obj?.name} {x.src1.Obj?.GetHashCode()} {meshDict.ContainsKey(x.src1?.Obj)}"))
                let meshsrc = x.src0
                let model = x.src1
                where !meshDict.ContainsKey(model.Obj)
                select model.BuildMeshCombiner(meshsrc, meshDict, atlasDict);
            var ofs = qOfs.ToArray();

            var qMObj = ofs.Select(x => x.obj);
            var qMesh = ofs.Select(x => x.f.ToTask())
                .WhenAll().Result
                .Select(x => x.CreateMesh());
            //var qMesh = ofs.Select(x => x.f().CreateMesh());
            meshDict.AddRange(qMObj, qMesh);
        }



        public static void CreateModelEntities
            (
                this IEnumerable<IMeshModel> models,
                GameObjectConversionSystem gcs,
                Dictionary<GameObject, Mesh> meshDict, TextureAtlasDictionary.Data atlasDict
            )
        {
            foreach (var model in models)
            {
                //Debug.Log($"{model.Obj.name} model ent");
                if (gcs.IsExistsInModelEntityDictionary(model.Obj)) continue;

                var obj = model.Obj;
                //Debug.Log($"create");

                var mesh = meshDict[obj];
                var atlas = atlasDict.objectToAtlas[obj];
                model.CreateModelEntity(gcs, mesh, atlas);
            }
        }


    }
}
