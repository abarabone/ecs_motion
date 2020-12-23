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

    public class StructureAreaAuthoring : MonoBehaviour, IStructureGroupAuthoring, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {

        public Shader ShaderToDraw;

        (GameObject, Mesh)[] objectsAndMeshes;



        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var structurePrefabs = this.GetComponentsInChildren<StructureModelAuthoring>()
                .Select( x => x.MasterPrefab )
                .Distinct()
                .ToArray();

            var structureModelPrefabs = structurePrefabs
                .Select(x => x.GetComponent<StructureModelAuthoring>())
                .ToArray();
            this.objectsAndMeshes = createMeshes(structureModelPrefabs);

            referencedPrefabs.AddRange(structurePrefabs);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            addToMeshDictionary_(this.objectsAndMeshes);

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

        (GameObject, Mesh)[] createMeshes(StructureModelAuthoring[] structureModelPrefabs)
        {

            var qNear =
                from st in structureModelPrefabs.Do(x => Debug.Log(x.NearMeshObject.objectTop.name))
                select st.GetNearMeshFunc()
                ;
            var qFar =
                from st in structureModelPrefabs.Do(x => Debug.Log(x.FarMeshObject.objectTop.name))
                select st.GetFarMeshAndFunc()
                ;
            var qPartAll =
                from st in structureModelPrefabs
                from pt in st.GetComponentsInChildren<StructurePartAuthoring>()
                select pt
                ;
            var qPartDistinct =
                from pt in qPartAll.Distinct(pt => pt.MasterPrefab)
                select pt.GetPartsMeshesAndFuncs()
                ;


            var allMeshFuncs = qNear.Concat(qFar).Concat(qPartDistinct).ToArray();

            var qGoAndTask = allMeshFuncs
                .Where(x => x.f != null)
                .Select(x => (x.go, t: Task.Run(x.f)));

            var qGoAndMesh = allMeshFuncs
                .Where(x => x.mesh != null)
                .Select(x => (x.go, x.mesh));

            var qGoAndMeshFromTask = qGoAndTask
                .Select(x => x.t)
                .WhenAll()
                .Result
                .Select(x => x.CreateMesh())
                .Zip(qGoAndTask, (x, y) => (y.go, mesh: x));


            return qGoAndMesh.Concat(qGoAndMeshFromTask)
                .ToArray();
        }




    }


}