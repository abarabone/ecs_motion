using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Physics;
using System;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using UnityEngine;

using Colider = Unity.Physics.Collider;

namespace DotsLite.Structure.Authoring
{
    using DotsLite.Dependency;
    using DotsLite.Model.Authoring;


    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class CompoundColliderInitializeSystem : GameObjectConversionSystem
    {



        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            this.Entities
                .ForEach((AreaAuthoring area) =>
                {
                    var ent = this.GetPrimaryEntity(area);
                    area.Convert(ent, this.DstEntityManager, this);
                });
        }

    }

}
