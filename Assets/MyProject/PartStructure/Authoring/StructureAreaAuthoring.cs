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
                .Select(x => x.StructureModelPrefab);

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

            var qFar =
                from st in structureModelPrefabs
                    .Do(x => Debug.Log(x.FarMeshObject.objectTop.name))
                select st.GetFarMeshAndFunc()
                ;

            var allMeshFuncs = qFar.ToArray();

            var qObjectAndMesh = allMeshFuncs
                .Where(x => x.mesh != null)
                .Select(x => (x.go, x.mesh));

            var qObjectAndTask = allMeshFuncs
                .Where(x => x.f != null)
                .Select(x => (x.go, t: Task.Run(x.f)));
            var qObjectAndMeshFromTask = qObjectAndTask
                .Select(x => x.t)
                .WhenAll()
                .Result
                .Select(x => x.CreateMesh())
                .Zip(qObjectAndTask, (x, y) => (y.go, mesh: x));

            var farObjectsAndMeshes = qObjectAndMesh.Concat(qObjectAndMeshFromTask)
                .ToArray();



        }




    }


}