﻿using System.Collections;
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


    //[DisableAutoCreation]
    public class TransformStructurePartsSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            var poss = this.GetComponentDataFromEntity<Translation>();
            var rots = this.GetComponentDataFromEntity<Rotation>();
            var links = this.GetComponentDataFromEntity<Structure.PartLinkData>(isReadOnly: true);
            var locals = this.GetComponentDataFromEntity<StructurePart.LocalPositionData>(isReadOnly: true);


            this.Entities
                .WithBurst()
                .WithAll<ObjectMain.ObjectMainTag>()
                .WithNone<Structure.SleepingTag>()
                .WithNativeDisableParallelForRestriction(poss)
                .WithNativeDisableParallelForRestriction(rots)
                .WithReadOnly(locals)
                .WithReadOnly(links)
                .ForEach(
                    (
                        in Structure.PartLinkData link,
                        in Translation pos,
                        in Rotation rot
                    ) =>
                    {
                        
                        for( var ptent = link.NextEntity; ptent != Entity.Null; ptent = links[ptent].NextEntity )
                        {
                            var local = locals[ptent];

                            var wpos = pos.Value + math.mul(rot.Value, local.Translation);
                            var wrot = math.mul(rot.Value, local.Rotation);

                            poss[ptent] = new Translation { Value = wpos };
                            rots[ptent] = new Rotation { Value = wrot };
                        }

                    }
                )
                .Schedule();

        }

    }

}
