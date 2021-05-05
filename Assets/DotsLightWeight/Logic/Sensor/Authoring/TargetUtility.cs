using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace DotsLite.Targeting
{
    using DotsLite.Arms;
    using DotsLite.Arms.Authoring;
    using DotsLite.Model.Authoring;

    public static class TargetingExtension
    {


        public static void AddTargetCorpsToFunctionUnit
            (this GameObjectConversionSystem gcs, IEnumerable<IFunctionUnitAuthoring> funits, Corps corps)
        {
            var em = gcs.DstEntityManager;

            var qFuEnt =
                from u in funits
                let ent = gcs.GetPrimaryEntity(u as MonoBehaviour)
                select ent
                ;

            foreach (var ent in qFuEnt)
            {
                em.AddComponentData(ent, new CorpsGroup.TargetWithArmsData
                //em.SetComponentData(ent, new CorpsGroup.TargetWithArmsData
                {
                    TargetCorps = corps,
                });
            }

        }


        public static void AddCorpsToAllRigidBody(this GameObjectConversionSystem gcs,
            ModelGroupAuthoring.ModelAuthoringBase top, Corps belongsCorps)
        {

            top.GetComponentsInChildren<Unity.Physics.Authoring.PhysicsBodyAuthoring>()
                //.Do(x => Debug.Log(x.name))
                .Select(x => gcs.GetPrimaryEntity(x))
                .ForEach(x => gcs.DstEntityManager.AddComponentData(x,
                    new CorpsGroup.Data
                    {
                        BelongTo = belongsCorps
                    })
                );

        }

        //public static void SetOrAddComponentData<T>(EntityManager em, Entity ent, T cd)
        //    where T : struct, IComponentData
        //{
        //    if (!em.HasComponent<T>(ent)) em.AddComponent<T>(ent);

        //    em.SetComponentData(ent, cd);
        //}

    }
}
