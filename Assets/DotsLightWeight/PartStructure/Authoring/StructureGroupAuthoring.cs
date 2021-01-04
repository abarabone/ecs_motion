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

    public class StructureGroupAuthoring : MonoBehaviour, IStructureGroupAuthoring, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {

        //public bool IsCombineMesh = true;
        //public bool IsPackTexture = true;


        public StructureBuildingModelAuthoring[] StructureModelPrefabs;

        (GameObject, Mesh)[] objectsAndMeshes;
        //GameObject[] partMasterPrefabs;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            this.objectsAndMeshes = createMeshes(this.StructureModelPrefabs);

            var structurePrefabs = this.StructureModelPrefabs.Select( x => x.gameObject );
            referencedPrefabs.AddRange(structurePrefabs);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            dstManager.DestroyEntity(entity);

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

        (GameObject, Mesh)[] createMeshes(StructureBuildingModelAuthoring[] structureModelPrefabs)
        {

            var qNear =
                from st in structureModelPrefabs
                    .Do(x => Debug.Log(x.NearMeshObject.objectTop.name))
                select st.GetNearMeshFunc()
                ;
            var qFar =
                from st in structureModelPrefabs
                    .Do(x => Debug.Log(x.FarMeshObject.objectTop.name))
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


            var allMeshFuncs = qNear
                .Concat(qFar)
                .Concat(qPartDistinct)
                .ToArray();

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


            return qObjectAndMesh.Concat(qObjectAndMeshFromTask)
                .ToArray();
        }

        (Texture2D, Dictionary<Mesh, Mesh>[]) convertNearTextureAndMesh
            (IEnumerable<(GameObject, Mesh)> objectAndMeshPairs, StructureBuildingModelAuthoring[] structureModelPrefabs)
        {
            
        }


    }

}