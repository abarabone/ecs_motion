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

    public class StructureGroupAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {

        //public bool IsCombineMesh = true;
        //public bool IsPackTexture = true;


        public StructureModelAuthoring[] StructureModelPrefabs;

        (GameObject, Mesh)[] objectsAndMeshes;
        //GameObject[] partMasterPrefabs;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var structurePrefabs = this.StructureModelPrefabs.Select( x => x.gameObject );
            referencedPrefabs.AddRange(structurePrefabs);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            dstManager.DestroyEntity(entity);

            addToMeshDictionary_(this.objectsAndMeshes);

            //stantiateMasterPrefab_ForConversion_(conversionSystem, this.partMasterPrefabs);

            return;


            void addToMeshDictionary_((GameObject, Mesh)[] objectsAndMeshes_)
            {
                foreach (var (go, mesh) in objectsAndMeshes_)
                {
                    conversionSystem.AddToMeshDictionary(go, mesh);
                }
            }

            //void stantiateMasterPrefab_ForConversion_
            //    (GameObjectConversionSystem gcs_, IEnumerable<GameObject> partMasterPrefabs_)
            //{
            //    foreach( var pt in partMasterPrefabs_.Select(prefab => Instantiate(prefab)) )
            //    {
            //        var em = gcs_.DstEntityManager;

            //        Debug.Log("pt "+pt.name);
            //        pt.AddComponent<PhysicsBodyAuthoring>();

            //        var flag = GameObjectConversionUtility.ConversionFlags.AssignName;
            //        var settings = new GameObjectConversionSettings(em.World, flag, gcs_.BlobAssetStore);
            //        var ent = GameObjectConversionUtility.ConvertGameObjectHierarchy(pt, settings);
            //        // このコンバートが走るたびに、専用の変換ワールドが作られて消えるみたい、よくないね…
            //        //GameObjectConversionUtility.ConvertGameObjectsToEntitiesField
            //        //    (gcs_, new GameObject[] { pt.gameObject}, out var ents);

            //        var addtype = new ComponentTypes
            //        (
            //            typeof(BinderTrimBlankLinkedEntityGroupTag),
            //            typeof(Prefab)
            //        );
            //        em.AddComponents(ent, addtype);
            //        //em.AddComponents(ents.First(), addtype);

            //        Destroy(pt);
            //    }
            //}
        }

        void Awake()
        {
            Debug.Log("aaa");
            create();
        }


        private void create()
        {

            var structureFarPrefabs = this.StructureModelPrefabs
                .Select(st => st.FarMeshObject.objectTop)
                .Do(x => Debug.Log($"st gr far {x.name}"))
                .ToArray();

            var structureNearPrefabs = this.StructureModelPrefabs
                .Select(st => st.NearMeshObject.objectTop)
                .Do(x => Debug.Log($"st gr near {x.name}"))
                .ToArray();

            var partMasterPrefabs = this.StructureModelPrefabs
                .SelectMany(st => st.GetComponentsInChildren<StructurePartAuthoring>())
                //.Select(pt => PrefabUtility.GetCorrespondingObjectFromOriginalSource(pt.gameObject))
                .Select(pt => pt.MasterPrefab)
                .Distinct()
                .Do(x => Debug.Log($"st gr part {x.name}"))
                .ToArray();
            //this.partMasterPrefabs = partMasterPrefabs;


            combineMeshes_(structureFarPrefabs, structureNearPrefabs, partMasterPrefabs);

            return;


            void combineMeshes_( GameObject[] farPrefabs_, GameObject[] nearPrefabs_, GameObject[] partMasterPrefabs_ )
            {

                var q =
                    from st in this.StructureModelPrefabs
                    let nearf = st.GetNearMeshFunc()
                    let far = st.GetFarMeshAndFunc()
                    let parts =
                        from pt in st.GetComponentsInChildren<StructurePartAuthoring>()
                        select pt.GetPartsMeshesAndFuncs()
                    let tasks = Enumerable.Empty<Task<MeshElements>>().Append(Task.Run(far.f)).Append(Task.Run(nearf)).Concat(parts.Select(pt=>Task.Run(pt.f)))
                    let meshes = Enumerable.Empty<Mesh>().Append(far.mesh).Concat(parts.Select(pt=>pt.mesh))
                    select
                        Enumerable.Empty<Task<MeshElements>>()
                            .Concat(far.sele)
                            .Concat(qNearElement)
                            .Concat(qPartElement_multi)
                            .WhenAll()
                            .Result
                            .Select(elm => elm.CreateMesh())
                            .Concat(qFarMesh_single)
                            .Concat(qPartMesh_single)
                    ;
                    




                var qObject = nearPrefabs_
                    .Concat(qPart_multi)
                    .Concat(qPart_single);

                this.objectsAndMeshes = (qObject, meshes).Zip().ToArray();

                return;


                IEnumerable<Task<MeshElements>> queryNear_(GameObject[] nears_)
                {

                    var qNearElement_ =
                        from x in nears_
                        let objects = x.DescendantsAndSelf()
                        let f = MeshCombiner.BuildStructureWithPalletMeshElements(objects, x.transform)
                        select Task.Run(f)
                        ;

                    return qNearElement_;
                }

                (
                    IEnumerable<Task<MeshElements>>, IEnumerable<Mesh>,
                    IEnumerable<GameObject>, IEnumerable<GameObject>
                )
                    queryFar_(GameObject[] fars_)
                {

                    var qFarChildren =
                        from x in fars_
                        let children = x.DescendantsAndSelf().Where(child => child.GetComponent<MeshFilter>() != null)
                        select children.ToArray()
                        ;
                    var farChildren = qFarChildren;

                    var q =
                        from x in farChildren
                        where x.SingleOrDefault() != null
                        select x
                        ;
                    var isFarSingle = farChildren.Length == 1 && farChildren.First() == qFarChildren.First();
                    var qFar_multi = !isFarSingle ? farChildren : Enumerable.Empty<GameObject>();
                    var qFar_single = isFarSingle ? farChildren : Enumerable.Empty<GameObject>();

                    var qFarElement_multi_ =
                        from x in qFar_multi
                        let objects = x.DescendantsAndSelf()
                        let f = MeshCombiner.BuildNormalMeshElements(objects, x.transform)
                        select Task.Run(f)
                        ;
                    var qFarMesh_single_ =
                        from x in qFar_single
                        let mesh = x.GetComponent<MeshFilter>().sharedMesh
                        select mesh
                        ;

                    return (qFarElement_multi_, qFarMesh_single_);
                }

                (
                    IEnumerable<Task<MeshElements>>, IEnumerable<Mesh>,
                    IEnumerable<GameObject>, IEnumerable<GameObject>
                )
                    queryPart_(GameObject[] parts_)
                {

                    var qPartChildren =
                        from pt in parts_
                        select (pt, children: pt.QueryPartBodyObjects_Recursive_().ToArray())
                        ;
                    var partChildrens = qPartChildren.ToArray();

                    var qPartAndChildren_multi_ =
                        from x in partChildrens
                        where x.children.Length > 1
                        select x
                        ;
                    var qPartAndChildren_single_ =
                        from x in partChildrens
                        where x.children.Length == 1
                        select x
                        ;

                    var qPartElement_multi_ =
                        from x in qPartAndChildren_multi_
                        let f = MeshCombiner.BuildNormalMeshElements(x.children, x.pt.transform)
                        select Task.Run(f)
                        ;
                    var qPartMesh_single_ =
                        from x in qPartAndChildren_single_
                        let mesh = x.children.First().GetComponent<MeshFilter>().sharedMesh
                        select mesh
                        ;
                    
                    var qPart_multi_ = qPartAndChildren_multi_.Select(x => x.pt);
                    var qPart_single_ = qPartAndChildren_single_.Select(x => x.pt);

                    return (qPartElement_multi_, qPartMesh_single_, qPart_multi_, qPart_single_);
                }

            }

        }


    }

    static class StructureConversionExtension
    {

        /// <summary>
        /// 
        /// </summary>
        static public Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks) =>
            Task.WhenAll(tasks);

    }

}