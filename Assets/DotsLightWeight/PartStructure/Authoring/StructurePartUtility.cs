using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Unity.Linq;
using Unity.Entities;
using System;

namespace DotsLite.Structure.Authoring
{

    using DotsLite.Model;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Character;//ObjectMain ÇÕÇ±Ç±Ç…Ç†ÇÈÅAñºëOïœÇ¶ÇÈÇ◊Ç´Ç©ÅH
    using DotsLite.Structure;
    using Unity.Physics;
    using Unity.Transforms;
    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.EntityTrimmer.Authoring;

    using Material = UnityEngine.Material;
    using Unity.Physics.Authoring;
    
    public static class StructurePartUtility
    {


        // ìØÇ∂ÉvÉåÉnÉuÇÇ‹Ç∆ÇﬂÇÈÇ±Ç∆ÇÕÇ≈Ç´Ç»Ç¢ÇæÇÎÇ§Ç©ÅH
        public static Entity CreateDebrisPrefab(this GameObjectConversionSystem gcs, GameObject part, Entity modelEntity)
        {
            var em_ = gcs.DstEntityManager;


            var types = em_.CreateArchetype
            (
                typeof(PartDebris.Data),

                typeof(DrawInstance.MeshTag),
                typeof(DrawInstance.ModelLinkData),
                typeof(DrawInstance.TargetWorkData),

                typeof(PhysicsVelocity),//ébíË
                typeof(PhysicsGravityFactor),//ébíË
                typeof(PhysicsMass),//ébíË
                typeof(PhysicsWorldIndex),//ébíË

                typeof(Marker.Rotation),
                typeof(Marker.Translation),

                //typeof(NonUniformScale),//ébíË
                typeof(Prefab)
            );
            //var prefabEnt = gcs_.CreateAdditionalEntity(part_, types);
            var prefabEnt = em_.CreateEntity(types);


            em_.SetComponentData(prefabEnt,
                new PartDebris.Data
                {
                    LifeTime = 5.0f,
                }
            );

            //em_.SetComponentData(mainEntity,
            //    new NonUniformScale
            //    {
            //        Value = new float3(1, 1, 1)
            //    }
            //);
            em_.SetComponentData(prefabEnt,
                new DrawInstance.ModelLinkData
                {
                    DrawModelEntityCurrent = modelEntity,
                }
            );
            em_.SetComponentData(prefabEnt,
                new DrawInstance.TargetWorkData
                {
                    DrawInstanceId = -1,
                }
            );

            //var mass = part_.GetComponent<PhysicsBodyAuthoring>().CustomMassDistribution;
            //var mass = em_.GetComponentData<PhysicsCollider>(gcs_.GetPrimaryEntity(part_)).MassProperties;
            em_.SetComponentData(prefabEnt,
                //PhysicsMass.CreateDynamic( mass, 1.0f )
                PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1.0f)// ébíËÇæÇ¡ÇØÅH
            );
            em_.SetComponentData(prefabEnt,
                new PhysicsGravityFactor
                {
                    Value = 1.0f,
                }
            );


            em_.SetName_(prefabEnt, $"{part.name} debris");

            return prefabEnt;
        }


        static public void DispatchPartId<T>(this IEnumerable<T> parts)
            where T : IStructurePart
        {
            parts.ForEach((x, i) => x.partId = i);
        }

        //static public void BuildPartModelEntities<T>(this IEnumerable<T> parts, GameObjectConversionSystem gcs)
        //    where T : IStructurePart
        //{
        //    parts.ForEach(x => x.QueryModel.CreateModelEntitiesWithDictionary(gcs));
        //}

        //static public void CombineMeshesWithDictionary<T>(IEnumerable<T> parts, MultiMeshesToDrawModelEntityDictionary dict)
        //    where T : IStructurePart
        //{

        //    var q =
        //        from part in parts
        //        let hash = getPartMeshesHash_(part)
        //        from model in part.QueryModel
        //        select (hash, model.QueryMmts)
        //        ;
        //    foreach (var (hash, mmts) in q)
        //    {
        //        //dict.
        //    }

        //    int getPartMeshesHash_(T part)
        //    {
        //        var qHash =
        //            from model in part.QueryModel
        //            from mmt in model.QueryMmts
        //            select mmt.mesh.GetHashCode()
        //            ;
        //        return qHash
        //            .Select((x, i) => x * 31 ^ i)
        //            .Do(x => Debug.Log($"part {part.partId} hash {x}"))
        //            .Sum();
        //    }
        //}
    }
}

