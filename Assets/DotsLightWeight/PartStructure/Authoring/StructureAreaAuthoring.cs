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
    using Abarabone.Utilities;
    using Abarabone.Structure.Authoring;

    public class StructureAreaAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public Shader ShaderToDraw;



        IEnumerable<IMeshModel> _model = null;

        public override IEnumerable<IMeshModel> QueryModel => _model ??= new MeshModel<UI32, PositionNormalUvVertex>
        (
            this.gameObject,
            this.ShaderToDraw
        )
        .WrapEnumerable();





        StructureBuildingModelAliasAuthoring[] structures;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var qStructures = this.GetComponentsInChildren<StructureBuildingModelAliasAuthoring>();
            this.structures = qStructures.ToArray();

            var qPrefab = this.structures
                .Select(x => x.StructureModelPrefab.gameObject)
                .Distinct()
                ;

            referencedPrefabs.AddRange(qPrefab);
        }


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var top = this.gameObject;
            var main = top.Children().First();

            this.QueryModel.CreateMeshAndModelEntitiesWithDictionary(conversionSystem);

            //conversionSystem.CreateDrawInstanceEntities(top, main, bones, this.BoneMode);

            //createSpawnInstances(conversionSystem, dstManager, this.structures);

            return;


            static void createSpawnInstances(GameObjectConversionSystem gcs, EntityManager em, StructureBuildingModelAliasAuthoring[] structures)
            {
                var arch = em.CreateArchetype(typeof(Spawn.EntryData));
                using var instances = em.CreateEntity(arch, structures.Length, Allocator.Temp);

                var q =
                    from st in structures
                    let tf = st.transform
                    let prefab = gcs.GetPrimaryEntity(st.StructureModelPrefab)
                    select new Spawn.EntryData
                    {
                        prefab = prefab,
                        pos = tf.position,
                        rot = tf.rotation,
                    };

                em.SetComponentData(instances, q);
            }

        }

    }
}