using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace Abarabone.Draw
{
    using Abarabone.Misc;
    using Abarabone.Utilities;
    using Abarabone.SystemGroup;
    using Abarabone.Character;
    using Abarabone.Structure;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    //[UpdateAfter(typeof())]
    public class StructureSwitchingSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            var linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true);


            this.Entities
                .WithBurst()
                .WithAll<Structure.StructureMainTag, ObjectMain.ObjectMainTag>()
                .WithNone<Structure.SleepingTag>()
                .WithReadOnly(linkedGroups)
                .ForEach(
                    (
                        Entity mainEntity, int entityInQueryIndex,
                        in DrawInstance.ModeLinkData model,
                        //in DrawInstance.mode
                        in ObjectMain.BinderLinkData binderLink,
                        in Structure.SwitchingData switcher
                    ) =>
                    {

                        //if(switcher.IsNear & model.DrawModelEntityCurrent == )

                        var children = linkedGroups[binderLink.BinderEntity];

                        ////foreach (var child in children)
                        //for (var i = 2; i < children.Length; i++)
                        //{
                        //    var child = children[i].Value;

                        //    //if (destructeds.HasComponent(child)) continue;
                        //    if (!locals.HasComponent(child)) continue;

                        //    var local = locals[child];

                        //    var wpos = pos.Value + math.mul(rot.Value, local.Translation);
                        //    var wrot = math.mul(rot.Value, local.Rotation);

                        //    poss[child] = new Translation { Value = wpos };
                        //    rots[child] = new Rotation { Value = wrot };
                        //}

                    }
                )
                .ScheduleParallel();

        }

        void changeToNear
            (
                
                DynamicBuffer<LinkedEntityGroup> children
                //ComponentDataFromEntity<StructurePart.PartData> partData,
            )
        {

            for (var i = 2; i < children.Length; i++)
            {
                var child = children[i].Value;
                //if (!partData.HasComponent(child)) continue;


            }
        }
        void changeToFar()
        {

        }
    }

}
