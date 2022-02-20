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

namespace DotsLite.Structure.Authoring
{

    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using Unity.Physics.Authoring;
    using System.Runtime.InteropServices;
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Structure.Authoring;

    public class StructureBuildingGroupAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {

        public Shader ShaderToDraw;



        IEnumerable<IMeshModel> _model = null;

        public override IEnumerable<IMeshModel> QueryModel => _model ??= new MeshModel<UI32, PositionNormalUvVertex>
        {
            objectTop = this.gameObject,
            shader = this.ShaderToDraw,
        }
        .WrapEnumerable();





        StructureBuildingAliasAuthoring[] structures;

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            if (!this.isActiveAndEnabled) return;


            var qStructures = this.GetComponentsInChildren<StructureBuildingAliasAuthoring>();
            var stlist = qStructures.ToArray();

            var qPrefab = stlist
                .Select(x => x.StructureModelPrefab.gameObject)
                .Distinct()
                ;

            referencedPrefabs.AddRange(qPrefab);

            this.structures = stlist;
        }


        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (!this.isActiveAndEnabled) { conversionSystem.DstEntityManager.DestroyEntity(entity); return; }


            this.structures
                .BuildModelToDictionary(conversionSystem);


            var top = this.gameObject;
            var main = top.Children().First();

            //this.QueryModel.CreateMeshAndModelEntitiesWithDictionary(conversionSystem);

            //conversionSystem.CreateDrawInstanceEntities(top, main, bones, this.BoneMode);

            createSpawnInstances_(conversionSystem, this.structures);

            trimEntities_(conversionSystem, this);

            return;


            static void createSpawnInstances_(GameObjectConversionSystem gcs, StructureBuildingAliasAuthoring[] structures)
            {
                var em = gcs.DstEntityManager;

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

            static void trimEntities_(GameObjectConversionSystem gcs, StructureBuildingGroupAuthoring area)
            {
                var em = gcs.DstEntityManager;

                foreach (var obj in area.gameObject.Descendants())
                {
                    var ent = gcs.GetPrimaryEntity(obj);
                    em.DestroyEntity(ent);
                }
            }
        }

    }
}