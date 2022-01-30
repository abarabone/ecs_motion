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
    using DotsLite.EntityTrimmer.Authoring;

    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    [UpdateBefore(typeof(RemoveTransformAllConversion))]
    public class ExecuteAreaAuthoringConversion : GameObjectConversionSystem
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

            this.Entities
                .ForEach((StructureBone bone) =>
                {
                    var ent = this.GetPrimaryEntity(bone);

                    var ress = em.GetBuffer<PartBone.PartDestructionResourceData>(ent);
                    var info = em.GetComponentData<PartBone.PartInfoData>(ent);
                    var link = em.GetComponentData<PartBone.LinkToMainData>(ent);

                    var destructions = em.GetComponentData<Main.PartDestructionData>(link.MainEntity);

                    em.SetComponentData(ent, ress.BuildCompoundCollider(info, destructions));
                });
        }

    }

}
