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

namespace Abarabone.Geometry
{
    using Abarabone.Model;
    using Abarabone.Model.Authoring;
    using Abarabone.Draw.Authoring;
    using Abarabone.Common.Extension;
    using Abarabone.Utilities;

    public static class MeshModelUtility
    {

        public static IEnumerable<GameObject> Objs(this IEnumerable<IMeshModel> models) =>
            models.Select(x => x.Obj);



        public static void CreateModelEntities
            (this IEnumerable<IMeshModel> models, GameObjectConversionSystem gcs)
        {
            var meshDict = gcs.GetMeshDictionary();
            var atlasDict = gcs.GetTextureAtlasDictionary();

            models.Objs().PackTextureToDictionary(atlasDict);
            models.CreateModelToDictionary(meshDict, atlasDict);
            models.CreateModelEntities(gcs, meshDict, atlasDict);
        }



        public static void CreateModelToDictionary
            (
                this IEnumerable<IMeshModel> models,
                Dictionary<GameObject, Mesh> meshDict, TextureAtlasDictionary.Data atlasDict
            )
        {
            var qMmts =
                from model in models
                select model.Obj.QueryMeshMatsTransform_IfHaving();

            using var meshAll = qMmts.QueryMeshDataFromModel();

            var qOfs =
                from x in (meshAll.AsEnumerable, models).Zip()
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
                var obj = model.Obj;
                Debug.Log($"{obj.name} model ent");

                var mesh = meshDict[obj];
                var atlas = atlasDict.objectToAtlas[obj];
                model.CreateModelEntity(gcs, mesh, atlas);
            }
        }


    }
}
