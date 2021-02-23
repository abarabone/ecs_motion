using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using System.Threading.Tasks;
using Unity.Linq;
using UnityEditor;

namespace Abarabone.Structure.Authoring
{

    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Model.Authoring;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;
    using Unity.Physics.Authoring;
    using System.Runtime.InteropServices;
    using Abarabone.Common.Extension;
    using Abarabone.Structure.Authoring;

    public class StructureAreaAuthoring : MonoBehaviour, IConvertGameObjectToEntity//, IStructureGroupAuthoring
    {

        public Material MaterialToDraw;


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var modelsFromDirect = this.GetComponentsInChildren<StructureBuildingModelAuthoring>();
            var qModelFromAlias = this.GetComponentsInChildren<StructureBuildingModelAliasAuthoring>()
                .Select(x => x.StructureModelPrefab)
                .Distinct();

            var models = modelsFromDirect
                .Concat(qModelFromAlias)
                .ToArray();
            
            var objectsAndMeshes = createMeshes(models);

            addToMeshDictionary_(objectsAndMeshes);

            return;


            void addToMeshDictionary_((GameObject, Mesh)[] objectsAndMeshes_)
            {
                foreach (var (go, mesh) in objectsAndMeshes_)
                {
                    Debug.Log($"to dict {go.name} - {mesh.name}");
                    conversionSystem.AddToMeshDictionary(go, mesh);
                }
            }
        }

        (GameObject, Mesh)[] createMeshes(StructureBuildingModelAuthoring[] structureModelPrefabs)
        {
            return default;

            //var qFar =
            //    from st in structureModelPrefabs
            //        .Do(x => Debug.Log(x.FarMeshObject.objectTop.name))
            //    select st.GetFarMeshAndFunc()
            //    ;

            //var farObjectsAndMeshes = qFar.QueryObjectAndMesh()
            //    .ToArray();

            //var qFarObject = farObjectsAndMeshes
            //    .Select(x => x.go);
            //var farMeshFunc = MeshCombiner.BuildStructureMeshElements(qFarObject, this.transform);

            //var fars = (this.gameObject, farMeshFunc, null as Mesh);


            //var qNear =
            //    from st in structureModelPrefabs
            //        .Do(x => Debug.Log(x.NearMeshObject.objectTop.name))
            //    select st.GetNearMeshFunc()
            //    ;
            //var qPartAll =
            //    from st in structureModelPrefabs
            //    from pt in st.GetComponentsInChildren<StructurePartAuthoring>()
            //    select pt
            //    ;
            //var qPartDistinct =
            //    from pt in qPartAll.Distinct(pt => pt.MasterPrefab)
            //    select pt.GetPartsMeshesAndFuncs()
            //    ;

            //var qAllMeshFuncs = qNear
            //    .Concat(qPartDistinct)
            //    .Append(fars);
            
            //return qAllMeshFuncs.QueryObjectAndMesh()
            //    .ToArray();
            
        }




    }



    static class StructureMeshCreateUtility
    {

        static public IEnumerable<(GameObject go, Mesh mesh)> QueryObjectAndMesh
            (this IEnumerable<(GameObject go, Func<MeshCombinerElements> f, Mesh mesh)> qObject_And_MeshOrFunc)
        {
            var objects_And_meshesOrFuncs = qObject_And_MeshOrFunc.ToArray();

            var qObjectAndMesh = objects_And_meshesOrFuncs
                .Where(x => x.mesh != null)
                .Select(x => (x.go, x.mesh));

            var qObjectAndTask = objects_And_meshesOrFuncs
                .Where(x => x.f != null)
                .Select(x => (x.go, t: Task.Run(x.f)));
            var qObjectAndMeshFromTask = qObjectAndTask
                .Select(x => x.t)
                .WhenAll()
                .Result
                .Select(x => x.CreateMesh())
                .Zip(qObjectAndTask, (x, y) => (y.go, mesh: x));

            return qObjectAndMesh.Concat(qObjectAndMeshFromTask);
        }
    }

}